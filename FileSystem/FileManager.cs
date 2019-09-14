using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    class DirectoryEntry
    {
        public const int NULL_NO = 0;    //无效Inode编号
        public const int DIR_SIZE = 32;  //目录结构长度
        public const int NAME_SIZE = 28; //路径名最多占用空间
        public const int NAME_MAX_LENGTH = NAME_SIZE / sizeof(char); //路径名最多字符数量

        public int InodeNo;
        public char[] FileName;

        public DirectoryEntry()
        {
            FileName = new char[NAME_MAX_LENGTH];
        }
    }
    class FileManager
    {
        public const int NULL_NO = -1;                                                              //代表无效的编号
        public const int BLOCK_SIZE = 512;                                                          //物理块大小
        public const int INODE_PER_BLOCK = BLOCK_SIZE / DiskInode.INODE_SIZE;                       //每个物理块能存放的INODE数量
        public const int MAX_INODE = 8192;                                                          //最多DiskInode数量
        public const int MAX_DATA_BLOCK = 261118;                                                   //最多一般数据块数量
        public const int FREE_BLOCK_GROUP_SIZE = 128;                                               //空闲盘块组每组数量
        public const int INODE_BEGIN = SuperBlock.FULL_SIZE;                                        //INode区起始物理地址
        public const int DATA_BEGIN = INODE_BEGIN + MAX_INODE * DiskInode.INODE_SIZE;               //数据区起始物理地址，即Inode区起始地址加上Inode区所占空间
        public const int IMAGE_SIZE = BLOCK_SIZE * MAX_DATA_BLOCK + DATA_BEGIN;                     //默认文件卷大小,当前设置下为128MB

        private FileStream CurOpenFileStream;                                       //当前打开文件卷的流
        private int FilePtr;                                                        //文件读写指针
        private DiskInode CurFileInode;                                             //当前打开的文件对应的Inode
        private int CurFileInodeNo;                                                 //当前打开文件Inode在Inode区的编号

        public SuperBlock SuperBlockInstance;                                       //内存中的超级块实例

        private int AllocBlock() //获取一个空闲数据块
        {
            if (SuperBlockInstance.FreeBlockIndex[SuperBlockInstance.FreeBlockCount - 1] == SuperBlock.NULL_NO)  //若即将分配的盘块是代表无效的盘块，说明所有空闲盘块已被分配完，抛出异常
                throw new InvalidOperationException("磁盘无空余空间！");
            int ReturnValue = SuperBlockInstance.FreeBlockIndex[--SuperBlockInstance.FreeBlockCount]; //分配的盘块为表尾盘块，表长度-1
            if (SuperBlockInstance.FreeBlockCount == 0)  //若当前表已空，将即将分配空闲块内的表载入超级块
            {
                byte[] ReadBuffer = new byte[FREE_BLOCK_GROUP_SIZE * sizeof(int)];
                CurOpenFileStream.Seek(ReturnValue * BLOCK_SIZE + DATA_BEGIN, SeekOrigin.Begin);
                CurOpenFileStream.Read(ReadBuffer, 0, FREE_BLOCK_GROUP_SIZE * sizeof(int));
                for (int i = 0; i < FREE_BLOCK_GROUP_SIZE; i++)
                    SuperBlockInstance.FreeBlockIndex[i] = BitConverter.ToInt32(ReadBuffer, i * sizeof(int));
                SuperBlockInstance.FreeBlockCount = FREE_BLOCK_GROUP_SIZE; //载入完毕，设置直接管理的空闲块为一整组的数量
            }
            SuperBlockInstance.BlockCount--;
            return ReturnValue; //返回该空闲块
        }
        private void FreeBlock(int block_no) //释放指定数据块
        {
            if (block_no <= 0) //0号数据块用于存储根目录文件，无法被释放
                return;
            CurOpenFileStream.Seek(DATA_BEGIN + block_no * BLOCK_SIZE, SeekOrigin.Begin);
            for (int i = 0; i < BLOCK_SIZE; i++) //用0覆写数据块
                CurOpenFileStream.WriteByte(0x0);
            if (SuperBlockInstance.FreeBlockCount == FREE_BLOCK_GROUP_SIZE)  //若索引表已满，将索引表存入释放的数据块，并将超级块索引表设置为只有新释放的块
            {
                long WriteOffset = DATA_BEGIN + block_no * BLOCK_SIZE; //设置写入开始位点
                for (int i = 0; i < SuperBlockInstance.FreeBlockIndex.Length; i++)
                {
                    CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
                    CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.FreeBlockIndex[i]), 0, sizeof(int));
                    WriteOffset += sizeof(int);
                    SuperBlockInstance.FreeBlockIndex[i] = 0; //空闲的索引表位置赋值为0
                }
                SuperBlockInstance.FreeBlockIndex[0] = block_no; //将索引表0号位置设置为新释放的块
                SuperBlockInstance.FreeBlockCount = 1; //超级块直接管理1个空闲块
            }
            else //否则，在索引表尾加入该数据块即可
            {
                SuperBlockInstance.FreeBlockIndex[SuperBlockInstance.FreeBlockCount] = block_no;
                SuperBlockInstance.FreeBlockCount++;
            }
            SuperBlockInstance.BlockCount++;
        }
        private int AllocInode() //获取一个空闲Inode
        {
            if (SuperBlockInstance.FreeInodeCount == 0)  //若空闲Inode索引表已空，需要去Inode区寻找散落的空闲Inode并置入索引表
            {
                for (int i = 0; i < MAX_INODE; i++) //逐个遍历Inode区的Inode
                {
                    CurOpenFileStream.Seek(INODE_BEGIN + i * DiskInode.INODE_SIZE, SeekOrigin.Begin);
                    if (!Convert.ToBoolean(CurOpenFileStream.ReadByte())) //从Inode头读入Occupied变量的字节，若为否，则Inode未被占用
                        SuperBlockInstance.FreeInodeIndex[SuperBlockInstance.FreeInodeCount++] = i;
                    if (SuperBlockInstance.FreeInodeCount == SuperBlock.INODE_INDEX_SIZE) //若索引表已满，结束搜索
                        break;
                }
            }
            if (SuperBlockInstance.FreeInodeCount == 0) //若仍无空闲Inode,说明整个系统已经没有空闲Inode，抛出异常
                throw new InvalidOperationException("文件数量已到达上限！");
            SuperBlockInstance.InodeCount--;
            return SuperBlockInstance.FreeInodeIndex[--SuperBlockInstance.FreeInodeCount]; //返回索引最后一项，表长自减
        }
        private void FreeInode(int inode_no) //释放指定Inode
        {
            if (SuperBlockInstance.FreeInodeCount < SuperBlock.INODE_INDEX_SIZE) //若索引表不满，将其置入索引表。否则任由其散落在Inode区
            {
                SuperBlockInstance.FreeInodeIndex[SuperBlockInstance.FreeInodeCount] = inode_no;
                SuperBlockInstance.FreeInodeCount++;
            }
            SuperBlockInstance.InodeCount++;
        }
        private int Find(int index_inode, string name) //在指定inode对应的目录文件内寻找指定名字的文件或目录，返回其inode编号
        {
            DiskInode Inode = ReadDiskInode(index_inode);
            if (!Inode.Occupied)
                throw new ArgumentException("该inode为无效inode");
            if (Inode._FileType != DiskInode.FileType.Index)
                throw new ArgumentException("该inode对应文件不为目录文件");
            byte[] ReadBuffer = new byte[DirectoryEntry.DIR_SIZE];
            DiskInode OriginInode = CurFileInode; //备份当前Inode
            int OriginFilePtr = FilePtr;          //备份文件指针
            int OriginNo = CurFileInodeNo;        //备份当前Inode编号
            CurFileInode = Inode; //将当前打开文件临时设置为该目录文件，以方便调用ReadFile方法
            FilePtr = 0; //设置文件指针为从头开始读取
            for (int i = 0; i < Inode.Size / DirectoryEntry.DIR_SIZE; i++)   //遍历当前目录下所有目录项，找出目标路径
            {
                Seek(i * DirectoryEntry.DIR_SIZE);
                ReadFile(ReadBuffer, 0, DirectoryEntry.DIR_SIZE);
                /* 读取文件名，需要转码成Unicode-16字符串，并将结尾的\0字符去除 */
                string CurName = Encoding.Unicode.GetString(ReadBuffer, sizeof(int), DirectoryEntry.NAME_SIZE);
                CurName = CurName.Remove(CurName.IndexOf('\0'));
                if (CurName == name)   //若找到目标文件，还原Inode和文件指针并返回
                {
                    CurFileInode = OriginInode;
                    FilePtr = OriginFilePtr;
                    CurFileInodeNo = OriginNo;
                    return BitConverter.ToInt32(ReadBuffer, 0);
                }
            }
            /* 找完所有目录项均未找到，还原Inode和文件指针并抛出异常 */
            CurFileInode = OriginInode;
            FilePtr = OriginFilePtr;
            CurFileInodeNo = OriginNo;
            throw new FileNotFoundException("找不到目标文件");
        }
        private DiskInode ReadDiskInode(int inode_no) //读出指定位置的inode
        {
            byte[] ReadBuffer = new byte[DiskInode.INODE_SIZE];
            CurOpenFileStream.Seek(INODE_BEGIN + inode_no * DiskInode.INODE_SIZE, SeekOrigin.Begin);
            CurOpenFileStream.Read(ReadBuffer, 0, DiskInode.INODE_SIZE);
            int BufferOffset = 0;
            DiskInode ReturnInode = new DiskInode();
            ReturnInode.Occupied = BitConverter.ToBoolean(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(bool);
            ReturnInode._FileType = (DiskInode.FileType)ReadBuffer[BufferOffset];
            BufferOffset += sizeof(DiskInode.FileType);
            ReturnInode._FileLengthType = (DiskInode.FileLengthType)BitConverter.ToInt16(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(DiskInode.FileLengthType);
            ReturnInode.LinkCount = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            ReturnInode.Size = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            for (int i = 0; i < ReturnInode.IndexTable.Length; i++)
            {
                ReturnInode.IndexTable[i] = BitConverter.ToInt32(ReadBuffer, BufferOffset);
                BufferOffset += sizeof(int);
            }
            ReturnInode.LastModifyTime = BitConverter.ToInt64(ReadBuffer, BufferOffset);
            return ReturnInode;
        }
        private void WriteDiskInode(DiskInode inode, int inode_no) //将inode写入到inode区指定位置
        {
            CurOpenFileStream.Seek(INODE_BEGIN + inode_no * DiskInode.INODE_SIZE, SeekOrigin.Begin); //设置文件流初始位置
            CurOpenFileStream.Write(BitConverter.GetBytes(inode.Occupied), 0, sizeof(bool));
            CurOpenFileStream.Write(BitConverter.GetBytes((byte)inode._FileType), 0, sizeof(byte));
            CurOpenFileStream.Write(BitConverter.GetBytes((short)inode._FileLengthType), 0, sizeof(short));
            CurOpenFileStream.Write(BitConverter.GetBytes(inode.LinkCount), 0, sizeof(int));
            CurOpenFileStream.Write(BitConverter.GetBytes(inode.Size), 0, sizeof(int));
            for (int i = 0; i < inode.IndexTable.Length; i++)
                CurOpenFileStream.Write(BitConverter.GetBytes(inode.IndexTable[i]), 0, sizeof(int));
            CurOpenFileStream.Write(BitConverter.GetBytes(inode.LastModifyTime), 0, sizeof(long));
            CurOpenFileStream.Flush();
        }
        public void LoadSuperBlockInstance() //从磁盘卷载入SuperBlock到内存
        {
            byte[] ReadBuffer = new byte[SuperBlock.TRUE_SIZE];
            CurOpenFileStream.Seek(0, SeekOrigin.Begin);
            CurOpenFileStream.Read(ReadBuffer, 0, SuperBlock.TRUE_SIZE); //从卷头读入SuperBlock类长度的字节
            SuperBlockInstance = new SuperBlock();
            int BufferOffset = SuperBlock.OFFSET;
            /* 将缓冲区内的字节赋值给SuperBlock实例 */
            SuperBlockInstance.InodeCount = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            SuperBlockInstance.BlockCount = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            SuperBlockInstance.FreeBlockCount = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            for (int i = 0; i < SuperBlockInstance.FreeBlockIndex.Length; i++)
            {
                SuperBlockInstance.FreeBlockIndex[i] = BitConverter.ToInt32(ReadBuffer, BufferOffset);
                BufferOffset += sizeof(int);
            }
            SuperBlockInstance.FreeInodeCount = BitConverter.ToInt32(ReadBuffer, BufferOffset);
            BufferOffset += sizeof(int);
            for (int i = 0; i < SuperBlockInstance.FreeInodeIndex.Length; i++)
            {
                SuperBlockInstance.FreeInodeIndex[i] = BitConverter.ToInt32(ReadBuffer, BufferOffset);
                BufferOffset += sizeof(int);
            }
        }
        private int GetPhyBlockNo(int logic_no) //给定一个逻辑块号，获取当前打开文件中对应的物理块号
        {
            if (logic_no * BLOCK_SIZE > CurFileInode.Size) //若逻辑块号超出文件大小范围，抛出异常
                throw new ArgumentOutOfRangeException();
            if (logic_no < DiskInode.MAX_SMALL_FILE) //对于小型文件，返回索引表内值
                return CurFileInode.IndexTable[logic_no];
            if (logic_no < DiskInode.MAX_MEDIUM_FILE)  //对于中型文件，需要间接索引一次
            {
                byte[] ReadBuffer = new byte[sizeof(int)];
                int SingleIndirectNo = CurFileInode.IndexTable[((logic_no - DiskInode.MAX_SMALL_FILE) / DiskInode.INDEX_PER_BLOCK) + DiskInode.SINGLE_INDIRECT_BEGIN];
                int BlockIndex = (logic_no - DiskInode.MAX_SMALL_FILE) % DiskInode.INDEX_PER_BLOCK; //索引在间接索引块内第几个
                CurOpenFileStream.Seek(DATA_BEGIN + SingleIndirectNo * BLOCK_SIZE + BlockIndex * sizeof(int), SeekOrigin.Begin);
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                return BitConverter.ToInt32(ReadBuffer, 0);
            }
            if (logic_no < DiskInode.MAX_LARGE_FILE)  //对于大型文件，需要间接索引两次
            {
                byte[] ReadBuffer = new byte[sizeof(int)];
                int DoubleIndirectNo = CurFileInode.IndexTable[((logic_no - DiskInode.MAX_MEDIUM_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) + DiskInode.DOUBLE_INDIRECT_BEGIN];
                int DoubleBlockIndex = ((logic_no - DiskInode.MAX_MEDIUM_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                int SingleBlockIndex = (logic_no - DiskInode.MAX_MEDIUM_FILE) % (DiskInode.INDEX_PER_BLOCK); //物理块号在一次间接索引块内位置
                CurOpenFileStream.Seek(DATA_BEGIN + DoubleIndirectNo * BLOCK_SIZE + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                int SingleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                CurOpenFileStream.Seek(DATA_BEGIN + SingleIndirectNo * BLOCK_SIZE + SingleBlockIndex * sizeof(int), SeekOrigin.Begin);
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                return BitConverter.ToInt32(ReadBuffer, 0);
            }
            else //对于超大型文件，需要索引三次 
            {
                byte[] ReadBuffer = new byte[sizeof(int)];
                int TripleBlockIndex = (logic_no - DiskInode.MAX_LARGE_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK);
                int DoubleBlockIndex = ((logic_no - DiskInode.MAX_LARGE_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                int SingleBlockIndex = (logic_no - DiskInode.MAX_LARGE_FILE) % (DiskInode.INDEX_PER_BLOCK); //物理块号在一次间接索引块内位置
                CurOpenFileStream.Seek(DATA_BEGIN + CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN] * BLOCK_SIZE + TripleBlockIndex * sizeof(int), SeekOrigin.Begin); //读出二次间接索引块号
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                CurOpenFileStream.Seek(DATA_BEGIN + BitConverter.ToInt32(ReadBuffer, 0) * BLOCK_SIZE + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin); //读出一次间接索引块号
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                CurOpenFileStream.Seek(DATA_BEGIN + BitConverter.ToInt32(ReadBuffer, 0) * BLOCK_SIZE + SingleBlockIndex * sizeof(int), SeekOrigin.Begin); //读出物理块号
                CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                return BitConverter.ToInt32(ReadBuffer, 0);
            }
        }
        private void ExpandFile() //当文件所有数据块空间已经用尽时，为当前打开文件扩容一个数据块
        {
            if (CurFileInode.Size % BLOCK_SIZE != 0) //若仍有剩余空间，抛出异常
                throw new NotSupportedException("当前文件占用数据块空间尚未用尽！");
            int LogicBlockNo = CurFileInode.Size / BLOCK_SIZE; //新数据块的逻辑块号
            int NewBlockNo; //新数据块的块号
            /* 尝试获取新数据块以存放数据 */
            try
            {
                NewBlockNo = AllocBlock();
            }
            catch
            {
                throw;
            }
            if (LogicBlockNo < DiskInode.MAX_SMALL_FILE)   //若新块属于直接索引，数据块直接置入索引表
                CurFileInode.IndexTable[LogicBlockNo] = NewBlockNo;
            else if (LogicBlockNo < DiskInode.MAX_MEDIUM_FILE)  //若新块属于一次间接索引，找到一次间接索引块并置入
            {
                int SingleIndirectNo; //一次间接索引块号
                if (((LogicBlockNo - DiskInode.MAX_SMALL_FILE) % DiskInode.INDEX_PER_BLOCK) == 0) //若新块位于一次间接索引表的第一项，需要再申请一个数据块作为间接索引块
                {
                    try
                    {
                        SingleIndirectNo = AllocBlock();
                    }
                    catch //若获取间接索引块失败，将之前获取的数据块释放
                    {
                        FreeBlock(NewBlockNo);
                        throw;
                    }
                    CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_SMALL_FILE) / DiskInode.INDEX_PER_BLOCK + DiskInode.SINGLE_INDIRECT_BEGIN] = SingleIndirectNo; //将一次间接索引块置入索引表
                }
                else //否则直接从索引表获取一次间接索引块号
                    SingleIndirectNo = CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_SMALL_FILE) / DiskInode.INDEX_PER_BLOCK + DiskInode.SINGLE_INDIRECT_BEGIN];
                int SingleIndirectIndex = (LogicBlockNo - DiskInode.MAX_SMALL_FILE) % DiskInode.INDEX_PER_BLOCK; //索引在索引块的位置
                CurOpenFileStream.Seek(DATA_BEGIN + SingleIndirectNo * BLOCK_SIZE + SingleIndirectIndex * sizeof(int), SeekOrigin.Begin); //设置要写入索引的位置
                CurOpenFileStream.Write(BitConverter.GetBytes(NewBlockNo), 0, sizeof(int)); //写入新获取的数据块到相应索引位置
                CurOpenFileStream.Flush();
            }
            else if (LogicBlockNo < DiskInode.MAX_LARGE_FILE)  //若新块属于二次间接索引
            {
                int SingleIndirectNo; //一次间接索引块号
                int DoubleIndirectNo = CurFileInode.IndexTable[((LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) + DiskInode.DOUBLE_INDIRECT_BEGIN]; //二次间接索引块号,默认从索引表取
                int DoubleBlockIndex = ((LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                int SingleBlockIndex = (LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) % (DiskInode.INDEX_PER_BLOCK); //物理块号在一次间接索引块内位置
                if (SingleBlockIndex == 0) //若新块位于一次间接索引表的第一项，需要再申请一个数据块作为一次间接索引块
                {
                    try
                    {
                        SingleIndirectNo = AllocBlock();
                    }
                    catch //若获取间接索引块失败，将之前获取的数据块释放
                    {
                        FreeBlock(NewBlockNo);
                        throw;
                    }
                    if (DoubleBlockIndex == 0)  //若新建的一次间接索引块同时还位于二次间接索引块的第一个，需要再申请一个数据块作为二次间接索引块
                    {
                        try
                        {
                            DoubleIndirectNo = AllocBlock();
                        }
                        catch //若获取间接索引块失败，将之前获取的数据块和一次间接索引块释放
                        {
                            FreeBlock(NewBlockNo);
                            FreeBlock(SingleIndirectNo);
                            throw;
                        }
                        /* 将二次间接索引块置入索引表 */
                        CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK) + DiskInode.DOUBLE_INDIRECT_BEGIN] = DoubleIndirectNo;
                    }
                    /* 将一次间接索引块号写入二次间接索引块 */
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Write(BitConverter.GetBytes(SingleIndirectNo), 0, sizeof(int));
                    CurOpenFileStream.Flush();
                }
                else //否则从二次间接索引块获取一次间接索引块号
                {
                    byte[] ReadBuffer = new byte[sizeof(int)];
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    SingleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                }
                /* 将新数据块号写入到一次间接索引块 */
                CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * SingleIndirectNo + SingleBlockIndex * sizeof(int), SeekOrigin.Begin);
                CurOpenFileStream.Write(BitConverter.GetBytes(NewBlockNo), 0, sizeof(int));
                CurOpenFileStream.Flush();
            }
            else //若新块属于三次间接索引
            {
                int SingleIndirectNo; //一次间接索引块号
                int DoubleIndirectNo; //二次间接索引块号
                int TripleIndirectNo = CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN]; //三次间接索引块号，默认直接从索引表取
                int TripleBlockIndex = (LogicBlockNo - DiskInode.MAX_LARGE_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK); //二次间接索引块在三次间接索引块内位置
                int DoubleBlockIndex = ((LogicBlockNo - DiskInode.MAX_LARGE_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                int SingleBlockIndex = (LogicBlockNo - DiskInode.MAX_LARGE_FILE) % (DiskInode.INDEX_PER_BLOCK); //物理块号在一次间接索引块内位置
                if (SingleBlockIndex == 0) //若新块位于一次间接索引表的第一项，需要再申请一个数据块作为一次间接索引块
                {
                    try
                    {
                        SingleIndirectNo = AllocBlock();
                    }
                    catch //若获取间接索引块失败，将之前获取的数据块释放
                    {
                        FreeBlock(NewBlockNo);
                        throw;
                    }
                    if (DoubleBlockIndex == 0)  //若新建的一次间接索引块同时还位于二次间接索引块的第一个，需要再申请一个数据块作为二次间接索引块
                    {
                        try
                        {
                            DoubleIndirectNo = AllocBlock();
                        }
                        catch //若获取间接索引块失败，将之前获取的数据块和一次间接索引块释放
                        {
                            FreeBlock(NewBlockNo);
                            FreeBlock(SingleIndirectNo);
                            throw;
                        }
                        if (TripleBlockIndex == 0)  //若新建的二次间接索引块位于三次间接索引第一个，需要再申请一个数据块作为三次间接索引块
                        {
                            try
                            {
                                TripleIndirectNo = AllocBlock();
                            }
                            catch //若获取失败，将之前获取的数据块和一、二次间接索引块释放
                            {
                                FreeBlock(NewBlockNo);
                                FreeBlock(SingleIndirectNo);
                                FreeBlock(DoubleIndirectNo);
                                throw;
                            }
                            CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN] = TripleIndirectNo; //将三次间接索引块号置入索引表
                        }
                        /* 将新建的二次间接索引块号写入三次间接索引块 */
                        CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * TripleIndirectNo + TripleBlockIndex * sizeof(int), SeekOrigin.Begin);
                        CurOpenFileStream.Write(BitConverter.GetBytes(DoubleIndirectNo), 0, sizeof(int));
                        CurOpenFileStream.Flush();
                    }
                    else //否则直接从三次间接索引块获取二次间接索引块号
                    {
                        byte[] ReadBuffer = new byte[sizeof(int)];
                        CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * TripleIndirectNo + TripleBlockIndex * sizeof(int), SeekOrigin.Begin);
                        CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                        DoubleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                    }
                    /* 将新建的一次间接索引块号写入二次间接索引块 */
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Write(BitConverter.GetBytes(SingleIndirectNo), 0, sizeof(int));
                    CurOpenFileStream.Flush();
                }
                else //否则先从三次间接索引块取得二次间接索引块，再从二次间接索引块取得一次间接索引块
                {
                    byte[] ReadBuffer = new byte[sizeof(int)];
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * TripleIndirectNo + TripleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    DoubleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    SingleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                }
                /* 将新数据块号写入到一次间接索引块 */
                CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * SingleIndirectNo + SingleBlockIndex * sizeof(int), SeekOrigin.Begin);
                CurOpenFileStream.Write(BitConverter.GetBytes(NewBlockNo), 0, sizeof(int));
                CurOpenFileStream.Flush();
            }
        }
        private void AbsorbFile() //若当前文件末尾的数据块已不再被占用，将其释放。若有间接索引块因此不再被占用，将其一并释放
        {
            if (CurFileInode.Size % BLOCK_SIZE != 0) //要缩小的文件大小必然是数据块大小的整数倍
                throw new InvalidOperationException();
            int LogicBlockNo = CurFileInode.Size / BLOCK_SIZE;
            FreeBlock(GetPhyBlockNo(LogicBlockNo));
            byte[] ReadBuffer = new byte[sizeof(int)];
            int DoubleIndirectNo; //二次间接索引块号
            int TripleIndirectNo = CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN]; //三次间接索引块号，默认直接从索引表取
            if (LogicBlockNo >= DiskInode.MAX_LARGE_FILE)  //若释放的数据块属于三次间接索引
            {
                int TripleBlockIndex = (LogicBlockNo - DiskInode.MAX_LARGE_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK); //二次间接索引块在三次间接索引块内位置
                int DoubleBlockIndex = ((LogicBlockNo - DiskInode.MAX_LARGE_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                if ((CurFileInode.Size - DiskInode.MAX_LARGE_FILE * BLOCK_SIZE) % (BLOCK_SIZE * DiskInode.INDEX_PER_BLOCK) == 0)  //一次间接索引块空，将其释放
                {
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * TripleIndirectNo + TripleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    DoubleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    FreeBlock(BitConverter.ToInt32(ReadBuffer, 0));
                }
                if ((CurFileInode.Size - DiskInode.MAX_LARGE_FILE * BLOCK_SIZE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE) == 0) //二次间接索引块空，将其释放
                {
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * TripleIndirectNo + TripleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    FreeBlock(BitConverter.ToInt32(ReadBuffer, 0));
                }
                if (CurFileInode.Size == DiskInode.MAX_LARGE_FILE) //文件现大小恰好为大型文件最大值，说明三次间接索引块空，将其释放
                {
                    CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN] = DiskInode.NULL_NO;
                    FreeBlock(TripleIndirectNo);
                }
            }
            else if (LogicBlockNo >= DiskInode.MAX_MEDIUM_FILE) //若释放的数据块属于二次间接索引
            {
                DoubleIndirectNo = CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK) + DiskInode.DOUBLE_INDIRECT_BEGIN]; //二次间接索引块号,默认从索引表取
                int DoubleBlockIndex = ((LogicBlockNo - DiskInode.MAX_MEDIUM_FILE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK)) / DiskInode.INDEX_PER_BLOCK; //物理块对应一次间接索引块号在二次间接索引块内位置
                if ((CurFileInode.Size - DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE) % (BLOCK_SIZE * DiskInode.INDEX_PER_BLOCK) == 0)  //一次间接索引块空，将其释放
                {
                    CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * DoubleIndirectNo + DoubleBlockIndex * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                    FreeBlock(BitConverter.ToInt32(ReadBuffer, 0));
                }
                if ((CurFileInode.Size - DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE) % (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE) == 0) //二次间接索引块空，将其释放
                {
                    CurFileInode.IndexTable[(CurFileInode.Size - DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE) / (DiskInode.INDEX_PER_BLOCK * DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE) + DiskInode.DOUBLE_INDIRECT_BEGIN] = DiskInode.NULL_NO;
                    FreeBlock(DoubleIndirectNo);
                }
            }
            else if (LogicBlockNo >= DiskInode.MAX_SMALL_FILE) //若释放的数据库属于一次间接索引
            {
                if ((CurFileInode.Size - DiskInode.MAX_SMALL_FILE * BLOCK_SIZE) % (DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE) == 0) //若一次间接索引块空，将其释放
                {
                    FreeBlock(CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_SMALL_FILE) / DiskInode.INDEX_PER_BLOCK + DiskInode.SINGLE_INDIRECT_BEGIN]);
                    CurFileInode.IndexTable[(LogicBlockNo - DiskInode.MAX_SMALL_FILE) / DiskInode.INDEX_PER_BLOCK + DiskInode.SINGLE_INDIRECT_BEGIN] = DiskInode.NULL_NO;
                }
            }
            else //否则直接将索引表这项设置为NUll_NO即可
                CurFileInode.IndexTable[LogicBlockNo] = DiskInode.NULL_NO;
        }       
        private void UpdateSuperBlock() //将内存中的SuperBlock更新到磁盘卷中
        {
            long WriteOffset = 0;
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlock.MAGIC_NUM), 0, sizeof(int));
            WriteOffset += sizeof(int);
            CurOpenFileStream.Flush();
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.InodeCount), 0, sizeof(int));
            WriteOffset += sizeof(int);
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.BlockCount), 0, sizeof(int));
            WriteOffset += sizeof(int);
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.FreeBlockCount), 0, sizeof(int));
            WriteOffset += sizeof(int);
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            for (int i = 0; i < SuperBlockInstance.FreeBlockIndex.Length; i++) 
            {
                CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
                CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.FreeBlockIndex[i]), 0, sizeof(int));
                WriteOffset += sizeof(int);
            }
            CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
            CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.FreeInodeCount), 0, sizeof(int));
            WriteOffset += sizeof(int);
            for (int i = 0; i < SuperBlockInstance.FreeInodeIndex.Length; i++)
            {
                CurOpenFileStream.Seek(WriteOffset, SeekOrigin.Begin);
                CurOpenFileStream.Write(BitConverter.GetBytes(SuperBlockInstance.FreeInodeIndex[i]), 0, sizeof(int));
                WriteOffset += sizeof(int);
            }
            CurOpenFileStream.Flush();
        }
        public void LoadImage(string path)
        {
            if (CurOpenFileStream != null) //若已经有打开文件流，需要将其关闭并保存更改
            {
                SaveAllChanges();
                CurOpenFileStream.Close();
            }
            CurOpenFileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); //以读写方式打开文件卷，并拒绝其他进程
        }
        public bool ImageFormatted() //返回当前读取的文件卷是否已被格式化
        {
            byte[] CheckBuffer = new byte[sizeof(int)];
            try //尝试从文件卷头读取魔数，若读取失败，则没有被格式化
            {
                CurOpenFileStream.Seek(0, SeekOrigin.Begin);
                CurOpenFileStream.Read(CheckBuffer, 0, sizeof(int));
            }
            catch
            {
                return false;
            }
            if (BitConverter.ToInt32(CheckBuffer, 0) == SuperBlock.MAGIC_NUM) //若魔数正确，则文件卷已被格式化
                return true;
            return false; //否则没有被格式化
        }
        public void FormatImage() //格式化文件卷
        {
            CurOpenFileStream.Seek(0, SeekOrigin.Begin);
            CurOpenFileStream.Write(new byte[IMAGE_SIZE], 0, IMAGE_SIZE); //用0覆写整个文件卷
            SuperBlockInstance = new SuperBlock(); //重新创建一个SuperBlock
            SuperBlockInstance.InodeCount = 0;
            SuperBlockInstance.BlockCount = 0;
            /* 先设置空闲数据块数量为1，并且索引表的第一项为NULL，然后逐个释放0#以外所有数据块，0#为根目录文件 */
            SuperBlockInstance.FreeBlockCount = 1;
            SuperBlockInstance.FreeBlockIndex[0] = SuperBlock.NULL_NO;
            for (int i = 1; i < MAX_DATA_BLOCK; i++)
                FreeBlock(i);
            /* 对Inode做同样操作，但0#Inode不释放，该Inode用作根目录文件Inode */
            SuperBlockInstance.FreeInodeCount = 0;
            for (int i = 1; i < MAX_INODE; i++)
                FreeInode(i);
            UpdateSuperBlock(); //新SuperBlock初始化完毕，将其写入文件卷
            /* 设置0#Inode内容并写入磁盘 */
            DiskInode inode = new DiskInode();
            inode.Occupied = true;
            inode._FileType = DiskInode.FileType.Index;
            inode.LinkCount = 1;
            inode.IndexTable[0] = 0; //设置索引表0#位置指向0#数据块
            inode.LastModifyTime = DateTime.Now.Ticks;
            WriteDiskInode(inode, 0);
            /* 重置文件打开信息 */
            CurFileInode = null;
            CurFileInodeNo = 0;
            FilePtr = 0;
        }
        public int OpenFile(string path) //打开指定路径的文件，路径用\分隔，从左到右依次是各级目录。返回其上级目录文件的Inode编号。
        {
            int CurInodeNo = 0; //从0#Inode，也就是根目录文件开始找
            int ParentInodeNo = NULL_NO; //上级目录Inode编号
            /* 用队列保存路径中的各层 */
            Queue<string> PathQueue = new Queue<string>();
            char[] SplitChar = { '\\' };
            foreach (string s in path.Split(SplitChar))
            {
                if (s != MainForm.ROOT_INDEX_NAME) //将根目录以外的目录加入队列
                    PathQueue.Enqueue(s);
            }
            while (PathQueue.Count > 0)  //当队列不为空时，逐层向下搜索
            {
                try //尝试搜索本层路径
                {
                    if (PathQueue.Count == 1)  //当处于上级目录时，记录其Inode
                        ParentInodeNo = CurInodeNo;
                    CurInodeNo = Find(CurInodeNo, PathQueue.Dequeue());
                }
                catch //若搜索失败，将抓取到的异常传递给上层
                {
                    throw;
                }
            }
            if (CurFileInode != null && CurFileInode.Occupied)  //若之前已经打开了文件且该文件没有被删除
                WriteDiskInode(CurFileInode, CurFileInodeNo); //将之前打开的文件Inode存回磁盘
            CurFileInode = ReadDiskInode(CurInodeNo); //将当前打开的文件Inode读入内存。当路径为空时，读入的文件便是根目录文件
            FilePtr = 0; //设置文件读写指针到文件头
            CurFileInodeNo = CurInodeNo; //设置当前Inode编号
            return ParentInodeNo; //返回上级目录文件Inode编号
        }
        public void CloseFile() //关闭当前打开文件
        {
            if (CurFileInode != null)
            {
                WriteDiskInode(CurFileInode, CurFileInodeNo); //保存当前打开Inode
                /* 清空打开文件信息 */
                CurFileInode = null;
                CurFileInodeNo = 0;
                FilePtr = 0;
            }
        }
        public void Seek(int offset) //设定当前文件读写指针
        {
            if (CurFileInode != null)
            {
                if (offset > CurFileInode.Size) //若指针越界，抛出参数错误异常
                    throw new ArgumentOutOfRangeException("文件指针", "指针位置超出文件大小范围！");
                FilePtr = offset; //若无误，设置文件指针
            }
            else
                throw new InvalidOperationException("当前没有打开文件");
        }
        public int ReadFile(byte[] read_buffer, int offset, int need_count) //从当前打开文件当前的文件指针处读出指定数量的字节,并从缓冲区的指定位置开始存放,返回实际读出的字节数
        {
            int RealCount = need_count;
            if (FilePtr + RealCount > CurFileInode.Size)  //若要读取的字节超出范围，计算实际能读出的字节数
                RealCount = CurFileInode.Size - FilePtr;
            int LogicBlockNo = FilePtr / BLOCK_SIZE;    //设置起始读写逻辑块号
            int BlockStartIndex = FilePtr % BLOCK_SIZE; //起始读写块内位置
            int NeedToRead = RealCount; //剩余需读写字节数量
            CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * GetPhyBlockNo(LogicBlockNo) + BlockStartIndex, SeekOrigin.Begin); //将文件流读写指针调整到起始位置
            if (BLOCK_SIZE - BlockStartIndex > RealCount)  //若要读出的字节数量没有超出起始数据块，读出需要的数据即可
            {
                CurOpenFileStream.Read(read_buffer, offset, RealCount);
                NeedToRead = 0;
            }
            else //否则读完这个数据块
            {
                CurOpenFileStream.Read(read_buffer, offset, BLOCK_SIZE - BlockStartIndex);
                NeedToRead-= BLOCK_SIZE - BlockStartIndex;
            }
            while (NeedToRead > 0)  //不断读取数据块直至读完
            {
                LogicBlockNo++;  //逻辑块号+1
                int pno = GetPhyBlockNo(LogicBlockNo);
                CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * GetPhyBlockNo(LogicBlockNo), SeekOrigin.Begin); //设置读写指针到物理块开头
                if (NeedToRead < BLOCK_SIZE) //若剩余需读取的数据不足一块，读取剩余数据
                {
                    CurOpenFileStream.Read(read_buffer, RealCount - NeedToRead + offset, NeedToRead);
                    NeedToRead = 0;
                }
                else //否则将整块读取
                {
                    CurOpenFileStream.Read(read_buffer, RealCount - NeedToRead + offset, BLOCK_SIZE);
                    NeedToRead -= BLOCK_SIZE;
                }
            }
            FilePtr += RealCount; //修改文件指针位置
            return RealCount;
        }
        public int WriteFile(byte[] write_buffer, int offset, int need_count) //从当前打开文件当前的文件指针处写入缓冲区内从指定位置开始指定数量的字节,返回实际写入的字节数
        {
            int WrittenCount = 0;
            int LogicBlockNo = FilePtr / BLOCK_SIZE;    //设置起始读写逻辑块号
            int BlockStartIndex = FilePtr % BLOCK_SIZE; //起始读写块内位置
            if (FilePtr == CurFileInode.Size && FilePtr % BLOCK_SIZE == 0)   //若文件开始写入的位置是一个新的数据块且在文件末尾，需要先扩容文件
                ExpandFile();
            while (WrittenCount < need_count) //循环写入直至写完
            {
                int WriteCount = Math.Min(BLOCK_SIZE - BlockStartIndex, need_count - WrittenCount); //在本逻辑块可剩余可写字节数和还需写入字节数中选取最小值作为本次写入的字节数量
                int pno = GetPhyBlockNo(LogicBlockNo);
                CurOpenFileStream.Seek(DATA_BEGIN + BLOCK_SIZE * GetPhyBlockNo(LogicBlockNo) + BlockStartIndex, SeekOrigin.Begin); //将文件流调整到本次写入的起始位置
                CurOpenFileStream.Write(write_buffer, offset + WrittenCount, WriteCount); //写入本次应写入的字节
                CurOpenFileStream.Flush();
                WrittenCount += WriteCount; //已写入的字节增加本次写入的字节数
                if (FilePtr + WrittenCount > CurFileInode.Size)  //若当前位置已经超出原文件长度，修改文件长度
                {
                    CurFileInode.Size = FilePtr + WrittenCount;
                    if (CurFileInode.Size == DiskInode.MAX_FILE * BLOCK_SIZE) //当文件大小到达系统支持的最大值时，结束写入并返回
                    {
                        FilePtr += WrittenCount;
                        return WrittenCount;
                    }
                    /* 若文件长度超出了当前文件长度类型的最大长度，需要修改文件长度类型 */
                    if (CurFileInode.Size > DiskInode.MAX_LARGE_FILE * BLOCK_SIZE)
                        CurFileInode._FileLengthType = DiskInode.FileLengthType.Huge;
                    else if (CurFileInode.Size > DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE)
                        CurFileInode._FileLengthType = DiskInode.FileLengthType.Large;
                    else if (CurFileInode.Size > DiskInode.MAX_SMALL_FILE * BLOCK_SIZE)
                        CurFileInode._FileLengthType = DiskInode.FileLengthType.Medium;
                    if (WrittenCount < need_count)  
                    {
                        try //若仍有数据需要写入，尝试为该文件进行扩容
                        {
                            ExpandFile();
                        }
                        catch //若无法进行扩容，终止写入并返回已写入的字节数量
                        {
                            CurFileInode.LastModifyTime = DateTime.Now.Ticks; //设置文件最后修改时刻
                            WriteDiskInode(CurFileInode, CurFileInodeNo); //将修改后的DiskInode写回磁盘
                            FilePtr += WrittenCount;
                            return WrittenCount;
                        }
                    }
                }
                LogicBlockNo++;  //逻辑块号+1
                BlockStartIndex = 0; //除第一次以外，均从数据块的开头开始写入
            }
            CurFileInode.LastModifyTime = DateTime.Now.Ticks; //设置文件最后修改时刻
            WriteDiskInode(CurFileInode, CurFileInodeNo); //将修改后的DiskInode写回磁盘
            FilePtr += WrittenCount;
            return WrittenCount; //写入结束，返回
        }
        public void CreateFile(string path, bool allow_overlay, DiskInode.FileType type) //创建指定路径名的文件，路径用\分隔
        {
            if (path.Substring(path.LastIndexOf('\\')).Length > DirectoryEntry.NAME_MAX_LENGTH)  //若名称过长，抛出异常
                throw new ArgumentException("文件名不能超过14个字符！");
            int NewInode;
            string CreateFileName = path.Substring(path.LastIndexOf('\\') + 1); //取出要创建的文件名
            DiskInode OriginInode = CurFileInode; //备份当前打开Inode
            int OriginFilePtr = FilePtr; //备份当前文件指针
            int OriginNo = CurFileInodeNo; //备份当前Inode在Inode区编号
            try //尝试打开上级目录文件，若出错，抛出抓取到的异常
            {
                OpenFile(path.Substring(0, path.Length - CreateFileName.Length - 1)); //将不含文件名的目录打开，同时也不含最后一个\  
            }
            catch
            {
                throw;
            }
            if (CurFileInode.Size == DiskInode.MAX_FILE * BLOCK_SIZE) //若上级目录文件大小已达上限，抛出异常
                throw new InvalidOperationException("目录内无更多空间！");
            if (CurFileInode._FileType != DiskInode.FileType.Index) //若上级文件不是目录文件，抛出异常
                throw new ArgumentException("无法在文件下创建文件！");
            bool FileDuplicate = true; //默认有重复
            try //尝试在上级目录下寻找这个文件，若找不到，则创建该文件。若找到，则根据是否允许覆盖决定是否覆盖
            {
                int DupFileInode = Find(CurFileInodeNo, CreateFileName);
                if (ReadFileType(DupFileInode) == DiskInode.FileType.Index)
                    throw new InvalidOperationException("已有同名文件夹存在！");
            }
            catch(FileNotFoundException)
            {
                FileDuplicate = false; //若找不到文件，说明不重复
            }
            if (!FileDuplicate || allow_overlay)   //若文件不重复或文件允许覆盖，创建文件
            {
                try //尝试获取一个新空闲Inode位置,若失败，抛出异常
                {
                    NewInode = AllocInode();
                }
                catch
                {
                    throw;
                }
                if (FileDuplicate) //若文件重复
                    DeleteFile(path); //删除该文件
                Seek(CurFileInode.Size); //将读写指针移动到目录文件末尾
                try
                {
                    WriteFile(BitConverter.GetBytes(NewInode), 0, sizeof(int)); //写入Inode号到目录
                    WriteFile(Encoding.Unicode.GetBytes(CreateFileName), 0, CreateFileName.Length * sizeof(char)); //紧接Inode号写入文件名
                    WriteFile(new byte[DirectoryEntry.NAME_SIZE - CreateFileName.Length * sizeof(char)], 0, DirectoryEntry.NAME_SIZE - CreateFileName.Length * sizeof(char)); //用0填充剩余部分
                }
                catch //若写入失败，说明无空余空间，释放Inode并抛出异常
                {
                    FreeInode(NewInode);
                    throw;
                }
                /* 配置新文件的DiskInode */
                DiskInode CreateFileInode = new DiskInode();
                CreateFileInode.Occupied = true;
                CreateFileInode._FileType = type;
                CreateFileInode.LastModifyTime = DateTime.Now.Ticks;
                WriteDiskInode(CreateFileInode, NewInode); //写入到Inode区
            }
            /* 若Inode不同，还原原先的Inode、编号和文件指针 */
            if (CurFileInodeNo != OriginNo)
            {
                CurFileInode = OriginInode;
                CurFileInodeNo = OriginNo;
                FilePtr = OriginFilePtr;
            }
            if (FileDuplicate && !allow_overlay)
            {
                throw new ArgumentException("文件名重复！");
            }
        }
        public void DeleteFile(string path) //删除指定路径名的文件
        {
            DiskInode OriginInode = CurFileInode; //备份当前打开Inode
            int OriginFilePtr = FilePtr; //备份当前文件指针
            int OriginNo = CurFileInodeNo; //备份当前Inode在Inode区编号
            int ParentInodeNo; //上级目录文件编号
            try //先尝试打开该文件
            {
                ParentInodeNo = OpenFile(path);
            }
            catch
            {
                throw;
            }
            if (CurFileInode._FileType == DiskInode.FileType.Index && CurFileInode.Size != 0) //不允许删除仍有目录项的目录文件，因此还原现场并抛出异常
            {
                CurFileInode = OriginInode;
                CurFileInodeNo = OriginNo;
                FilePtr = OriginFilePtr;
                throw new ArgumentException("该目录下仍然存在文件！");
            }
            /* 开始删除文件 */
            for (int i = 0; i < Math.Ceiling((double)CurFileInode.Size / BLOCK_SIZE); i++)  //逐个释放本文件占用的所有数据块
                FreeBlock(GetPhyBlockNo(i));
            if (CurFileInode.Size > DiskInode.MAX_SMALL_FILE * BLOCK_SIZE)   //对于大于小型文件的文件，释放一次间接索引块
            {
                for (int i = DiskInode.SINGLE_INDIRECT_BEGIN; i < DiskInode.DOUBLE_INDIRECT_BEGIN; i++)
                    FreeBlock(CurFileInode.IndexTable[i]);
            }
            if (CurFileInode.Size > DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE)   //对于大于中型文件的文件，释放一次和二次间接索引块
            {
                byte[] ReadBuffer = new byte[sizeof(int)];
                int BlockToFree;
                for (int i = 0; i < Math.Ceiling((double)(CurFileInode.Size - DiskInode.MAX_MEDIUM_FILE * BLOCK_SIZE) / (DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE)); i++)  //逐个释放一次间接索引块 
                {
                    if (i >= (DiskInode.TRIPLE_INDIRECT_BEGIN - DiskInode.DOUBLE_INDIRECT_BEGIN) * DiskInode.INDEX_PER_BLOCK) //控制释放的一次间接索引块不能超出中型文件的最大数量
                        break;
                    CurOpenFileStream.Seek(DATA_BEGIN + CurFileInode.IndexTable[DiskInode.DOUBLE_INDIRECT_BEGIN + i / DiskInode.INDEX_PER_BLOCK] * BLOCK_SIZE + (i % DiskInode.INDEX_PER_BLOCK) * sizeof(int), SeekOrigin.Begin);
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int)); //读入下一个一次间接索引块
                    BlockToFree = BitConverter.ToInt32(ReadBuffer, 0);
                    FreeBlock(BlockToFree); //释放一次间接索引块
                }
                for (int i = DiskInode.DOUBLE_INDIRECT_BEGIN; i < DiskInode.TRIPLE_INDIRECT_BEGIN; i++) //释放二次间接索引块
                    FreeBlock(CurFileInode.IndexTable[i]);
            }
            if (CurFileInode.Size > DiskInode.MAX_LARGE_FILE * BLOCK_SIZE)   //对于大于大型文件的文件，释放一次、二次和三次间接索引块
            {
                byte[] ReadBuffer = new byte[sizeof(int)];
                int TripleIndirectNo = CurFileInode.IndexTable[DiskInode.TRIPLE_INDIRECT_BEGIN]; //获取三次间接索引块号
                int TripleBlockIndex = 0; //二次间接索引块在三次间接索引的位置
                int DoubleIndirectNo = 0; //当前正在处理的二次间接索引块号
                int BlockToFree;
                for (int i = 0; i < Math.Ceiling((double)(CurFileInode.Size - DiskInode.MAX_LARGE_FILE * BLOCK_SIZE) / (DiskInode.INDEX_PER_BLOCK * BLOCK_SIZE)); i++)  //逐个释放一、二次间接索引块
                {
                    if (i % DiskInode.INDEX_PER_BLOCK == 0) //每遍历完一个二次间接索引块，释放该块并重新设置二次间接索引块
                    {
                        if (i != 0) //除第一次以外，释放上一个二次间接索引块
                            FreeBlock(DoubleIndirectNo);
                        CurOpenFileStream.Seek(DATA_BEGIN + TripleIndirectNo * BLOCK_SIZE + TripleBlockIndex * sizeof(int), SeekOrigin.Begin); //从三次间接索引块中读出一个新的二次间接索引块号
                        CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int));
                        DoubleIndirectNo = BitConverter.ToInt32(ReadBuffer, 0);
                        TripleBlockIndex++;
                    }
                    CurOpenFileStream.Seek(DATA_BEGIN + DoubleIndirectNo * BLOCK_SIZE + (i % DiskInode.INDEX_PER_BLOCK) * sizeof(int), SeekOrigin.Begin); //将读写指针调整二次间接索引块相应位置
                    CurOpenFileStream.Read(ReadBuffer, 0, sizeof(int)); //读入下一个一次间接索引块
                    BlockToFree = BitConverter.ToInt32(ReadBuffer, 0);
                    FreeBlock(BlockToFree); //释放一次间接索引块
                }
                FreeBlock(DoubleIndirectNo); //释放最后的二次间接索引块
                FreeBlock(TripleIndirectNo); //释放三次间接索引块
            }
            /* 设置本文件的Inode未被占用并保存，然后释放该Inode */
            CurFileInode.Occupied = false; 
            WriteDiskInode(CurFileInode, CurFileInodeNo);
            FreeInode(CurFileInodeNo);
            /* 打开上级目录，并从目录里删去这个目录项 */
            int DeleteInodeNo = CurFileInodeNo; //记录要删除目录项Inode号
            CurFileInode = ReadDiskInode(ParentInodeNo);
            CurFileInodeNo = ParentInodeNo;
            FilePtr = 0;
            byte[] IndexReadBuffer = new byte[DirectoryEntry.DIR_SIZE];
            for (int i = 0; i < CurFileInode.Size / DirectoryEntry.DIR_SIZE; i++) //遍历所有目录项
            {
                ReadFile(IndexReadBuffer, 0, DirectoryEntry.DIR_SIZE);
                if (BitConverter.ToInt32(IndexReadBuffer, 0) == DeleteInodeNo)  //若找到了要删除的目录项,删除该项，若不是最后一项目录项，从末尾取一个目录项补到这个位置
                {
                    if (i != CurFileInode.Size / DirectoryEntry.DIR_SIZE - 1)
                    {
                        FilePtr = (CurFileInode.Size / DirectoryEntry.DIR_SIZE - 1) * DirectoryEntry.DIR_SIZE; //移动文件指针到末尾的目录项
                        ReadFile(IndexReadBuffer, 0, DirectoryEntry.DIR_SIZE);  //读出末尾的目录项
                        FilePtr = i * DirectoryEntry.DIR_SIZE;
                        WriteFile(IndexReadBuffer, 0, DirectoryEntry.DIR_SIZE); //将末尾的目录项写回要删除的目录项
                    }
                    Seek(CurFileInode.Size - DirectoryEntry.DIR_SIZE);
                    WriteFile(new byte[DirectoryEntry.DIR_SIZE], 0, DirectoryEntry.DIR_SIZE); //用0覆写末尾的目录项
                    CurFileInode.Size -= DirectoryEntry.DIR_SIZE; //修改文件大小
                    if (CurFileInode.Size % BLOCK_SIZE == 0) //若一整个目录文件块为空，收缩文件
                        AbsorbFile();
                    CurFileInode.LastModifyTime = DateTime.Now.Ticks; //修改上级目录的最后修改时间
                    WriteDiskInode(CurFileInode, CurFileInodeNo); //将上级目录文件Inode存回磁盘
                    break; //目录删除完成，结束循环
                }
            }
            /* 文件删除完毕，若删除文件的目录不是原先打开的文件，还原现场 */
            if (CurFileInodeNo != OriginNo)
            {
                CurFileInode = OriginInode;
                CurFileInodeNo = OriginNo;
                FilePtr = OriginFilePtr;
            }
        }
        public DiskInode.FileType ReadFileType(int inode_no) //获取指定文件的文件类型
        {
            byte[] ReadBuffer = new byte[sizeof(DiskInode.FileType)];
            CurOpenFileStream.Seek(INODE_BEGIN + inode_no * DiskInode.INODE_SIZE + sizeof(bool), SeekOrigin.Begin); //移动文件流到该Inode存储文件类型的变量位置
            CurOpenFileStream.Read(ReadBuffer, 0, sizeof(DiskInode.FileType));
            return (DiskInode.FileType)ReadBuffer[0];
        }
        public DiskInode GetInodeInstance() //获取当前打开Inode内存实例
        {
            return CurFileInode;
        }
        public void SaveAllChanges() //保存所有更改
        {
            if (CurOpenFileStream != null && ImageFormatted())
            {
                UpdateSuperBlock(); //保存超级块
                if (CurFileInode != null) //若有打开文件
                    WriteDiskInode(CurFileInode, CurFileInodeNo); //保存当前打开文件的DiskInode
                CurOpenFileStream.Flush();
            }
        }
    }
}
