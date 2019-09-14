using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSystem
{
    public partial class MainForm : Form
    {
        public const string ROOT_INDEX_NAME = "根目录";

        private const string FILE_TAG = "file";
        private const string FOLDER_TAG = "folder";
        private const string FAKE_TAG = "fake";
        private const string NULL_TEXT = "<空>";
        private const string NO_FILE_CHOSEN_TEXT = "当前没有选择文件";
        private const string READY_TEXT = "就绪";

        private FileManager FileManagerInstance;
        private string ChosenFileFullPath;
        private TreeNode OpenNode;
        public MainForm()
        {
            InitializeComponent();
        }
        private void ListFile(TreeNode node) //ls命令，读出指定目录下的所有文件和文件夹并显示
        {
            if (node == null)  //目录结点为空时，清空TreeView并将根结点置入TreeView
            {
                FileSystemTV.Nodes.Clear();
                TreeNode RootNode = new TreeNode(ROOT_INDEX_NAME);
                RootNode.Tag = FOLDER_TAG;
                TreeNode FakeNode = new TreeNode();
                FakeNode.Tag = FAKE_TAG;
                RootNode.Nodes.Add(FakeNode);
                FileSystemTV.Nodes.Add(RootNode);
            }
            else //否则读入目录文件，并将读到的文件和文件夹制作成结点置入输入节点的子结点集
            {
                node.Nodes.Clear(); //首先将目录下所有结点清除
                try //尝试读入目录文件
                {
                    FileManagerInstance.OpenFile(node.FullPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                byte[] ReadBuffer = new byte[DirectoryEntry.DIR_SIZE];
                Queue<TreeNode> FolderSet = new Queue<TreeNode>();
                Queue<TreeNode> FileSet = new Queue<TreeNode>();
                while (FileManagerInstance.ReadFile(ReadBuffer, 0, DirectoryEntry.DIR_SIZE) != 0) //不断读入目录项直到读完
                {
                    TreeNode NewNode = new TreeNode(Encoding.Unicode.GetString(ReadBuffer, sizeof(int), DirectoryEntry.NAME_SIZE));
                    if (NewNode.Text.Contains("\0"))
                        NewNode.Text = NewNode.Text.Substring(0, NewNode.Text.IndexOf('\0')); //无视0x0000转换出的字符
                    if (FileManagerInstance.ReadFileType(BitConverter.ToInt32(ReadBuffer, 0)) == DiskInode.FileType.Index)  //若目录指向的文件是文件夹，需要添加一个结点
                    {
                        TreeNode FakeNode = new TreeNode();
                        FakeNode.Tag = FAKE_TAG;
                        NewNode.Nodes.Add(FakeNode);
                        NewNode.Tag = FOLDER_TAG;
                        FolderSet.Enqueue(NewNode);
                    }
                    else
                    {
                        NewNode.Tag = FILE_TAG;
                        FileSet.Enqueue(NewNode);
                    }
                }
                foreach (TreeNode tn in FolderSet) //先添加文件夹结点
                    node.Nodes.Add(tn);
                foreach (TreeNode tn in FileSet) //再添加文件节点
                    node.Nodes.Add(tn);
                if (node.Nodes.Count == 0) //若该目录下没有文件，添加一个空文件作为填充
                {
                    TreeNode NullNode = new TreeNode(NULL_TEXT);
                    NullNode.Tag = FAKE_TAG;
                    node.Nodes.Add(NullNode);
                }
                OpenNode = node;
                NowOpenTB.Text = node.FullPath;
            }
        }
        private void RemoveFile(TreeNode node) //递归删除该节点下所有文件
        {
            if ((string)node.Tag == FAKE_TAG) //若节点不代表一个文件或文件夹，返回
                return;
            if ((string)node.Tag == FILE_TAG) //若节点代表一个文件，直接删除
            {
                string PathBriefStr = node.FullPath;
                if (PathBriefStr.Length > 10)
                    PathBriefStr = ".." + PathBriefStr.Substring(PathBriefStr.Length - 10);
                StatusTB.Text = "正在删除" + PathBriefStr;
                Refresh();
                FileManagerInstance.DeleteFile(node.FullPath);
            }
            else if ((string)node.Tag == FOLDER_TAG) //若节点代表一个文件夹，递归
            {
                foreach (TreeNode tn in node.Nodes)
                    RemoveFile(tn);
                string PathBriefStr = node.FullPath;
                if (PathBriefStr.Length > 10)
                    PathBriefStr = ".." + PathBriefStr.Substring(PathBriefStr.Length - 10);
                StatusTB.Text = "正在删除" + PathBriefStr;
                Refresh();
                FileManagerInstance.DeleteFile(node.FullPath); //最后本删除节点
            }
        }
        private void CheckSpaceUsage() //检查空间使用情况并反馈
        {
            double SpaceUsageRatio = 1 - (double)FileManagerInstance.SuperBlockInstance.BlockCount / FileManager.MAX_DATA_BLOCK;
            double NodeUsage = FileManager.MAX_INODE - (double)FileManagerInstance.SuperBlockInstance.InodeCount;
            SpaceUsageTSPB.Value = (int)Math.Round(SpaceUsageRatio * 100);
            SpaceUsageRatioTSSL.Text = Math.Round(SpaceUsageRatio * 100, 1).ToString() + "% 已使用";
            NodeUsageCountTSSL.Text = NodeUsage.ToString() + '/' + FileManager.MAX_INODE.ToString();
        }
        private void LoadImgBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "文件卷|*.img";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileManagerInstance.SaveAllChanges();
                FileManagerInstance.LoadImage(openFileDialog1.FileName); //读取指定路径的文件卷
                FormatImgBtn.Enabled = true;
                if (FileManagerInstance.ImageFormatted())  //若文件卷已被格式化，载入超级块并读取根目录
                {
                    FileManagerInstance.LoadSuperBlockInstance();
                    ListFile(null); //读出根目录
                    CheckSpaceUsage();
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FileManagerInstance = new FileManager();
            ChosenFileTB.Text = NO_FILE_CHOSEN_TEXT;
        }

        private void CreateImgBtn_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "NewDisk.img";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
            {
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                fs.Close();
                if (MessageBox.Show("是否要载入并格式化文件卷？", "是否格式化", MessageBoxButtons.YesNo) == DialogResult.Yes) 
                {
                    StatusTB.Text = "正在格式化";
                    Refresh();
                    FileManagerInstance.SaveAllChanges();
                    FileManagerInstance.LoadImage(saveFileDialog1.FileName);
                    FileManagerInstance.FormatImage();
                    ListFile(null); //读出根目录
                    CheckSpaceUsage();
                    FormatImgBtn.Enabled = true;
                    StatusTB.Text = READY_TEXT;
                }

            }
        }

        private void FormatImgBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要格式化文件卷？这会导致所有数据被删除。", "确定格式化", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                StatusTB.Text = "正在格式化";
                Refresh();
                ListFile(null); //由于文件都被清空，需要清空treeview并读入根目录
                FileManagerInstance.FormatImage();
                StatusTB.Text = READY_TEXT;
            }
            CheckSpaceUsage();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileManagerInstance.SaveAllChanges();
        }

        private void 新建文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileSystemTV.SelectedNode == null) 
            {
                MessageBox.Show("没有选中文件夹！");
                return;
            }
            InputNameForm NewForm = new InputNameForm();
            if (NewForm.ShowDialog() == DialogResult.OK) 
            {
                string CreateFilePath = FileSystemTV.SelectedNode.FullPath + '\\' + NewForm.FileName;
                try
                {
                    FileManagerInstance.CreateFile(CreateFilePath, false, DiskInode.FileType.Data);
                    ListFile(FileSystemTV.SelectedNode); //创建完成，刷新目录
                }
                catch (ArgumentException ex)
                {
                    if (ex.Message == "文件名重复！")
                    {
                        if (MessageBox.Show("发现已有同名文件，是否覆盖?", "是否覆盖", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            try
                            {
                                FileManagerInstance.CreateFile(CreateFilePath, true, DiskInode.FileType.Data);
                            }
                            catch (Exception _ex)
                            {
                                MessageBox.Show(_ex.Message);
                            }
                            ListFile(FileSystemTV.SelectedNode); //创建完成，刷新目录
                        }
                    }
                    else
                        MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            CheckSpaceUsage();
        }

        private void FileSystemTV_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            ListFile(e.Node);
        }

        private void 新建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileSystemTV.SelectedNode == null)
            {
                MessageBox.Show("没有选中文件夹！");
                return;
            }
            InputNameForm NewForm = new InputNameForm();
            if (NewForm.ShowDialog() == DialogResult.OK)
            {
                string CreateFilePath = FileSystemTV.SelectedNode.FullPath + '\\' + NewForm.FileName;
                try
                {
                    FileManagerInstance.CreateFile(CreateFilePath, false, DiskInode.FileType.Index); //文件夹不允许覆盖
                    ListFile(FileSystemTV.SelectedNode); //创建完成，刷新目录
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            CheckSpaceUsage();
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((string)FileSystemTV.SelectedNode.Tag == FOLDER_TAG)  
            {
                if (MessageBox.Show("删除文件夹会一并删除文件夹内所有文件，是否删除？", "确认删除", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;   
            }
            TreeNode ToDelete = FileSystemTV.SelectedNode;
            try //尝试递归删除
            {
                RecurseExpand(ToDelete); //必须先列出所有文件
                RemoveFile(ToDelete);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                StatusTB.Text = READY_TEXT;
            }
            ListFile(ToDelete.Parent); //删除完毕，重新列出上级目录下的文件和文件夹
            CheckSpaceUsage();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (FileSystemTV.SelectedNode == null)
            {
                foreach (ToolStripMenuItem item in contextMenuStrip1.Items)
                    item.Enabled = false;
            }
            else
            {
                foreach (ToolStripMenuItem item in contextMenuStrip1.Items)
                    item.Enabled = true;
                if (FileSystemTV.SelectedNode.Text == "根目录")
                    删除ToolStripMenuItem.Enabled = false;
            }
        }

        private void FileSystemTV_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((string)e.Node.Tag == FOLDER_TAG)  //双击文件夹节点时ls
            {
                ListFile(e.Node);
                OpenNode = e.Node;
            }
            else if ((string)e.Node.Tag == FILE_TAG)  //双击文件时打开文件
            {
                try
                {
                    FileManagerInstance.OpenFile(e.Node.FullPath);
                    NowOpenTB.Text = e.Node.FullPath;
                    OpenNode = e.Node;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CloseFileBtn_Click(object sender, EventArgs e)
        {
            FileManagerInstance.CloseFile();
            NowOpenTB.Text = "";
            OpenNode = null;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            ChosenFileFullPath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            ChosenFileTB.Text = ChosenFileFullPath.Substring(ChosenFileFullPath.LastIndexOf("\\") + 1); //仅对用户显示文件名，不显示全部路径
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ChooseWriteFileBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "任意文件|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK) 
            {
                ChosenFileFullPath = openFileDialog1.FileName;
                ChosenFileTB.Text = ChosenFileFullPath.Substring(ChosenFileFullPath.LastIndexOf("\\") + 1); //仅对用户显示文件名，不显示全部路径
            }
        }

        private void WriteFileBtn_Click(object sender, EventArgs e)
        {

            if (OpenNode == null || (string)OpenNode.Tag != FILE_TAG)
            {
                MessageBox.Show("没有打开一个要进行写入的数据文件！");
                return;
            }
            if (WriteFileRB.Checked) //若从文件写入
            {
                if (ChosenFileTB.Text == NO_FILE_CHOSEN_TEXT) //若当前没有选中文件
                {
                    MessageBox.Show("没有选中文件！");
                    return;
                }
                StatusTB.Text = "正在向" + OpenNode.Text + "写入数据"; //修改系统状态信息
                Refresh();
                FileInfo Info = new FileInfo(ChosenFileFullPath);
                long WriteCount = RWAllCB.Checked ? Info.Length : (long)RWLengthNUD.Value;
                if (WriteCount > int.MaxValue) //若文件过长
                {
                    MessageBox.Show("长度超出限制！");
                    return;
                }
                int WriteCount32 = Convert.ToInt32(WriteCount);
                FileStream ReadStream = new FileStream(ChosenFileFullPath, FileMode.Open);
                byte[] WriteBuffer = new byte[WriteCount32];
                ReadStream.Read(WriteBuffer, 0, WriteCount32); //先从文件中读出指定长度的字节
                ReadStream.Close();
                int RealWriteCount = 0;
                try
                {
                    FileManagerInstance.Seek(Convert.ToInt32(PointerLocNUD.Value));
                    RealWriteCount = FileManagerInstance.WriteFile(WriteBuffer, 0, WriteCount32);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    StatusTB.Text = READY_TEXT;
                }
                MessageBox.Show("写入完成，需写入" + WriteCount32.ToString() + "字节,实际写入" + RealWriteCount.ToString() + "字节");
                PointerLocNUD.Value += RealWriteCount;
            }
            else //若写入固定字符
            {
                if ((int)RWLengthNUD.Value == 0)
                    return;
                StatusTB.Text = "正在向" + OpenNode.Text + "写入数据"; //修改系统状态信息
                Refresh();
                int WriteCount = (int)RWLengthNUD.Value;
                int RealWriteCount = 0;
                try
                {
                    FileManagerInstance.Seek((int)PointerLocNUD.Value);
                    byte[] WriteBuffer = new byte[WriteCount];
                    for (int i = 0; i < WriteCount; i++)
                        WriteBuffer[i] = (byte)WriteByteNUD.Value;
                    RealWriteCount = FileManagerInstance.WriteFile(WriteBuffer, 0, WriteCount);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    StatusTB.Text = READY_TEXT;
                }
                MessageBox.Show("写入完成，需写入" + WriteCount.ToString() + "字节,实际写入" + RealWriteCount.ToString() + "字节");
                PointerLocNUD.Value += RealWriteCount;
            }
            CheckSpaceUsage();
            
        }

        private void ReadFileBtn_Click(object sender, EventArgs e)
        {
            if (OpenNode == null || (string)OpenNode.Tag != FILE_TAG)
            {
                MessageBox.Show("没有打开一个要进行读取的数据文件！");
                return;
            }
            StatusTB.Text = "正在从" + OpenNode.Text + "读取数据";
            Refresh();
            int ReadCount = RWAllCB.Checked ? FileManagerInstance.GetInodeInstance().Size : (int)RWLengthNUD.Value;
            byte[] ReadBuffer = new byte[ReadCount];
            int RealReadCount = 0;
            try
            {
                FileManagerInstance.Seek(Convert.ToInt32(PointerLocNUD.Value));
                RealReadCount = FileManagerInstance.ReadFile(ReadBuffer, 0, ReadCount);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                StatusTB.Text = READY_TEXT;
            }
            MessageBox.Show("读取完成，需读取" + ReadCount.ToString() + "字节,实际读取" + RealReadCount.ToString() + "字节。请选择要将数据保存到的文件夹。");
            saveFileDialog1.FileName = OpenNode.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
            {
                FileStream WriteStream = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                WriteStream.Write(ReadBuffer, 0, RealReadCount);
                WriteStream.Close();
            }
            RWLengthNUD.Value += RealReadCount;
        }

        private void RWAllCB_CheckedChanged(object sender, EventArgs e)
        {
            RWLengthNUD.Enabled = !RWAllCB.Checked;
        }

        private void 属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileManagerInstance.OpenFile(FileSystemTV.SelectedNode.FullPath);
            string PropertyStr = "路径：" + FileSystemTV.SelectedNode.FullPath + Environment.NewLine;
            int Size = FileManagerInstance.GetInodeInstance().Size;
            if (Size > 1024 * 1024)
                PropertyStr += "大小：" + Size / (1024 * 1024) + "MB";
            else if (Size > 1024)
                PropertyStr += "大小：" + Size / 1024 + "KB";
            else
                PropertyStr += "大小：" + Size + "B";
            PropertyStr += Environment.NewLine;
            PropertyStr += "最后修改时间：" + new DateTime(FileManagerInstance.GetInodeInstance().LastModifyTime).ToString();
            MessageBox.Show(PropertyStr);
            FileManagerInstance.OpenFile(OpenNode.FullPath);
        }

        private void 装满文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < FileManager.MAX_INODE; i++)  
            {
                StatusTB.Text = "正在创建" + FileSystemTV.SelectedNode.FullPath + '\\' + i.ToString();
                Refresh();
                try //不断尝试创建文件直至异常
                {
                    FileManagerInstance.CreateFile(FileSystemTV.SelectedNode.FullPath + '\\' + i.ToString(), false, DiskInode.FileType.Data);
                }
                catch
                {
                    break;
                }
            }
            StatusTB.Text = READY_TEXT;
            CheckSpaceUsage();
        }

        private void RecurseCreate(string path,int layer)
        {
            if (layer >= 5) //最多创建5层
                return;
            for (int i = 0; i < 5; i++) 
            {
                try
                {
                    FileManagerInstance.CreateFile(path + "\\folder" + i.ToString(), false, DiskInode.FileType.Index);
                }
                catch
                {
                    break;
                }
                RecurseCreate(path + "\\folder" + i.ToString(), layer + 1); //对创建的每个文件夹再次执行本函数
            }
        }

        private void RecurseExpand(TreeNode node)
        {
            if (node.Nodes.Count > 0)
                node.Expand();
            foreach (TreeNode tn in node.Nodes)
                RecurseExpand(tn);
        }
        private void 创建五层文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecurseCreate(FileSystemTV.SelectedNode.FullPath, 0);
            CheckSpaceUsage();
        }

        private void 展开所有ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecurseExpand(FileSystemTV.SelectedNode);
        }
    }
}
