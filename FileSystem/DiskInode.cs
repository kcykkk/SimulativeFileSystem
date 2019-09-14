using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    class DiskInode
    {
        public const int NULL_NO = -1;                  //无效块号，表示索引表该位置未被使用
        public const int INODE_SIZE = 64;               //DiskINode长度
        public const int INDEX_TABLE_SIZE = 11;         //索引表长度
        public const int SINGLE_INDIRECT_BEGIN = 6;     //一次间接索引在索引表开始位置
        public const int DOUBLE_INDIRECT_BEGIN = 8;     //二次间接索引在索引表开始位置
        public const int TRIPLE_INDIRECT_BEGIN = 10;    //三次间接索引在索引表开始位置

        public const int INDEX_PER_BLOCK = FileManager.BLOCK_SIZE / sizeof(int);  //每个间接索引块能存放的索引数量
        /* 小、中、大和巨大文件最多能占用的数据块数 */
        public const int MAX_SMALL_FILE = SINGLE_INDIRECT_BEGIN;
        public const int MAX_MEDIUM_FILE = MAX_SMALL_FILE + (DOUBLE_INDIRECT_BEGIN - SINGLE_INDIRECT_BEGIN) * INDEX_PER_BLOCK;
        public const int MAX_LARGE_FILE = MAX_MEDIUM_FILE + (TRIPLE_INDIRECT_BEGIN - DOUBLE_INDIRECT_BEGIN) * INDEX_PER_BLOCK * INDEX_PER_BLOCK;
        public const int MAX_FILE = MAX_LARGE_FILE + (INDEX_TABLE_SIZE - TRIPLE_INDIRECT_BEGIN) * INDEX_PER_BLOCK * INDEX_PER_BLOCK * INDEX_PER_BLOCK;
        public enum FileType: byte
        {
            Data,
            Index
        }
        public enum FileLengthType: short
        {
            Small,
            Medium,
            Large,
            Huge
        }
        public bool Occupied;                                //该Inode是否被占用
        public FileType _FileType;                           //文件类型
        public FileLengthType _FileLengthType;               //文件长度类型
        public int LinkCount;                                //路径名数量
        public int Size;                                     //文件大小
        public int[] IndexTable;                             //索引表 
        public long LastModifyTime;                          //最后修改时间                
        public DiskInode()
        {
            IndexTable = new int[INDEX_TABLE_SIZE];
            for (int i = 0; i < INDEX_TABLE_SIZE; i++)  //设置索引表所有位置均未被使用
                IndexTable[i] = NULL_NO;
        }   
    }
}
