using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSystem
{
    public partial class InputNameForm : Form
    {
        public InputNameForm()
        {
            InitializeComponent();
        }
        public string FileName;
        private void OkBtn_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length>DirectoryEntry.NAME_MAX_LENGTH) //若文件名过长，提示
            {
                MessageBox.Show("文件名不能超过" + Convert.ToString(DirectoryEntry.NAME_MAX_LENGTH + "个字符！"));
                textBox1.Focus();
            }
            else if(textBox1.Text.Length==0)
            {
                MessageBox.Show("文件名不能为空！");
                textBox1.Focus();
            }
            else
            {
                FileName = textBox1.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
