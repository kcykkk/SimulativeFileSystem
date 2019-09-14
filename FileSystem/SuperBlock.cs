using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    class SuperBlock
    {
        public const int NULL_NO = -1;              //代表无效的块号
        public const int OFFSET = 4;                //类存储结构开始位置的偏移量
        public const int TRUE_SIZE = 1024;          //类实际大小
        public const int PADDING_SIZE = 0;          //类尾填充大小
        public const int FULL_SIZE = 1024;          //类总大小
        public const int INODE_INDEX_SIZE = 123;    //空闲Inode索引表大小
        public const int MAGIC_NUM = 0x12345678;    //用于表示文件系统已经格式化的魔数
        public int InodeCount;                      //可用Inode盘块总数
        public int BlockCount;                      //可用一般数据盘块总数
        public int FreeBlockCount;                  //直接管理的空闲盘块数量
        public int[] FreeBlockIndex;                //空闲盘块索引
        public int FreeInodeCount;                  //直接管理的空闲Inode数量
        public int[] FreeInodeIndex;                //空闲Inode索引 
        public SuperBlock()
        {
            /* 设置空闲盘块和空闲Inode索引表大小 */
            FreeBlockIndex = new int[FileManager.FREE_BLOCK_GROUP_SIZE];
            FreeInodeIndex = new int[INODE_INDEX_SIZE];
        }               
    }
}
