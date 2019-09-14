using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class Utility //工具类
    {
        public static Stack<string> GetFileNameStack(string path) //将用反斜杠分割的路径转换成字符串的栈
        {
            Stack<string> Path = new Stack<string>();
            char[] SplitChar = { '\\' };
            foreach (string s in path.Split(SplitChar))
            {
                if (s != MainForm.ROOT_INDEX_NAME)  //向保存路径名的栈压入除根目录以外所有路径
                    Path.Push(s);
            }
            return Path;
        }
    }
}
