namespace FileSystem
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.FileSystemTV = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新建文件夹ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新建文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.属性ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.展开所有ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadImgBtn = new System.Windows.Forms.Button();
            this.FormatImgBtn = new System.Windows.Forms.Button();
            this.CloseFileBtn = new System.Windows.Forms.Button();
            this.NowOpenLb = new System.Windows.Forms.Label();
            this.NowOpenTB = new System.Windows.Forms.TextBox();
            this.ReadWriteGB = new System.Windows.Forms.GroupBox();
            this.WriteFileBtn = new System.Windows.Forms.Button();
            this.ReadFileBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PointerLocNUD = new System.Windows.Forms.NumericUpDown();
            this.WriteContentGB = new System.Windows.Forms.GroupBox();
            this.WriteByteNUD = new System.Windows.Forms.NumericUpDown();
            this.RWAllCB = new System.Windows.Forms.CheckBox();
            this.ChosenFileTB = new System.Windows.Forms.TextBox();
            this.ChooseWriteFileBtn = new System.Windows.Forms.Button();
            this.WriteFileRB = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.WriteCharRB = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.RWLengthNUD = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.FileSystemGB = new System.Windows.Forms.GroupBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.CreateImgBtn = new System.Windows.Forms.Button();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SpaceUsageTSSL = new System.Windows.Forms.ToolStripStatusLabel();
            this.SpaceUsageTSPB = new System.Windows.Forms.ToolStripProgressBar();
            this.SpaceUsageRatioTSSL = new System.Windows.Forms.ToolStripStatusLabel();
            this.NodeUsageTSSL = new System.Windows.Forms.ToolStripStatusLabel();
            this.NodeUsageCountTSSL = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTB = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.ReadWriteGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PointerLocNUD)).BeginInit();
            this.WriteContentGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WriteByteNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RWLengthNUD)).BeginInit();
            this.FileSystemGB.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FileSystemTV
            // 
            this.FileSystemTV.ContextMenuStrip = this.contextMenuStrip1;
            this.FileSystemTV.Location = new System.Drawing.Point(8, 46);
            this.FileSystemTV.Name = "FileSystemTV";
            this.FileSystemTV.Size = new System.Drawing.Size(316, 522);
            this.FileSystemTV.TabIndex = 0;
            this.FileSystemTV.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.FileSystemTV_BeforeExpand);
            this.FileSystemTV.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.FileSystemTV_NodeMouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建文件夹ToolStripMenuItem,
            this.新建文件ToolStripMenuItem,
            this.删除ToolStripMenuItem,
            this.属性ToolStripMenuItem,
            this.展开所有ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(158, 136);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // 新建文件夹ToolStripMenuItem
            // 
            this.新建文件夹ToolStripMenuItem.Name = "新建文件夹ToolStripMenuItem";
            this.新建文件夹ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.新建文件夹ToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.新建文件夹ToolStripMenuItem.Text = "新建文件夹";
            this.新建文件夹ToolStripMenuItem.Click += new System.EventHandler(this.新建文件夹ToolStripMenuItem_Click);
            // 
            // 新建文件ToolStripMenuItem
            // 
            this.新建文件ToolStripMenuItem.Name = "新建文件ToolStripMenuItem";
            this.新建文件ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.新建文件ToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.新建文件ToolStripMenuItem.Text = "新建文件";
            this.新建文件ToolStripMenuItem.Click += new System.EventHandler(this.新建文件ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // 属性ToolStripMenuItem
            // 
            this.属性ToolStripMenuItem.Name = "属性ToolStripMenuItem";
            this.属性ToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.属性ToolStripMenuItem.Text = "属性";
            this.属性ToolStripMenuItem.Click += new System.EventHandler(this.属性ToolStripMenuItem_Click);
            // 
            // 展开所有ToolStripMenuItem
            // 
            this.展开所有ToolStripMenuItem.Name = "展开所有ToolStripMenuItem";
            this.展开所有ToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.展开所有ToolStripMenuItem.Text = "展开所有";
            this.展开所有ToolStripMenuItem.Click += new System.EventHandler(this.展开所有ToolStripMenuItem_Click);
            // 
            // LoadImgBtn
            // 
            this.LoadImgBtn.Location = new System.Drawing.Point(12, 65);
            this.LoadImgBtn.Name = "LoadImgBtn";
            this.LoadImgBtn.Size = new System.Drawing.Size(203, 45);
            this.LoadImgBtn.TabIndex = 1;
            this.LoadImgBtn.Text = "载入文件卷";
            this.LoadImgBtn.UseVisualStyleBackColor = true;
            this.LoadImgBtn.Click += new System.EventHandler(this.LoadImgBtn_Click);
            // 
            // FormatImgBtn
            // 
            this.FormatImgBtn.Enabled = false;
            this.FormatImgBtn.Location = new System.Drawing.Point(12, 116);
            this.FormatImgBtn.Name = "FormatImgBtn";
            this.FormatImgBtn.Size = new System.Drawing.Size(203, 45);
            this.FormatImgBtn.TabIndex = 3;
            this.FormatImgBtn.Text = "格式化文件卷";
            this.FormatImgBtn.UseVisualStyleBackColor = true;
            this.FormatImgBtn.Click += new System.EventHandler(this.FormatImgBtn_Click);
            // 
            // CloseFileBtn
            // 
            this.CloseFileBtn.Location = new System.Drawing.Point(6, 325);
            this.CloseFileBtn.Name = "CloseFileBtn";
            this.CloseFileBtn.Size = new System.Drawing.Size(186, 36);
            this.CloseFileBtn.TabIndex = 5;
            this.CloseFileBtn.Text = "关闭文件";
            this.CloseFileBtn.UseVisualStyleBackColor = true;
            this.CloseFileBtn.Click += new System.EventHandler(this.CloseFileBtn_Click);
            // 
            // NowOpenLb
            // 
            this.NowOpenLb.AutoSize = true;
            this.NowOpenLb.Location = new System.Drawing.Point(11, 18);
            this.NowOpenLb.Name = "NowOpenLb";
            this.NowOpenLb.Size = new System.Drawing.Size(53, 12);
            this.NowOpenLb.TabIndex = 1;
            this.NowOpenLb.Text = "当前打开";
            // 
            // NowOpenTB
            // 
            this.NowOpenTB.Location = new System.Drawing.Point(70, 15);
            this.NowOpenTB.Name = "NowOpenTB";
            this.NowOpenTB.ReadOnly = true;
            this.NowOpenTB.Size = new System.Drawing.Size(254, 21);
            this.NowOpenTB.TabIndex = 0;
            // 
            // ReadWriteGB
            // 
            this.ReadWriteGB.Controls.Add(this.CloseFileBtn);
            this.ReadWriteGB.Controls.Add(this.WriteFileBtn);
            this.ReadWriteGB.Controls.Add(this.ReadFileBtn);
            this.ReadWriteGB.Controls.Add(this.label5);
            this.ReadWriteGB.Controls.Add(this.label4);
            this.ReadWriteGB.Controls.Add(this.PointerLocNUD);
            this.ReadWriteGB.Controls.Add(this.WriteContentGB);
            this.ReadWriteGB.Controls.Add(this.label2);
            this.ReadWriteGB.Controls.Add(this.RWLengthNUD);
            this.ReadWriteGB.Controls.Add(this.label1);
            this.ReadWriteGB.Location = new System.Drawing.Point(15, 167);
            this.ReadWriteGB.Name = "ReadWriteGB";
            this.ReadWriteGB.Size = new System.Drawing.Size(200, 367);
            this.ReadWriteGB.TabIndex = 5;
            this.ReadWriteGB.TabStop = false;
            this.ReadWriteGB.Text = "文件操作";
            // 
            // WriteFileBtn
            // 
            this.WriteFileBtn.Location = new System.Drawing.Point(6, 283);
            this.WriteFileBtn.Name = "WriteFileBtn";
            this.WriteFileBtn.Size = new System.Drawing.Size(186, 36);
            this.WriteFileBtn.TabIndex = 11;
            this.WriteFileBtn.Text = "写入文件";
            this.WriteFileBtn.UseVisualStyleBackColor = true;
            this.WriteFileBtn.Click += new System.EventHandler(this.WriteFileBtn_Click);
            // 
            // ReadFileBtn
            // 
            this.ReadFileBtn.Location = new System.Drawing.Point(6, 241);
            this.ReadFileBtn.Name = "ReadFileBtn";
            this.ReadFileBtn.Size = new System.Drawing.Size(186, 36);
            this.ReadFileBtn.TabIndex = 10;
            this.ReadFileBtn.Text = "读取文件";
            this.ReadFileBtn.UseVisualStyleBackColor = true;
            this.ReadFileBtn.Click += new System.EventHandler(this.ReadFileBtn_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(183, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "B";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "读写指针";
            // 
            // PointerLocNUD
            // 
            this.PointerLocNUD.Location = new System.Drawing.Point(67, 51);
            this.PointerLocNUD.Maximum = new decimal(new int[] {
            1215752191,
            23,
            0,
            0});
            this.PointerLocNUD.Name = "PointerLocNUD";
            this.PointerLocNUD.Size = new System.Drawing.Size(113, 21);
            this.PointerLocNUD.TabIndex = 8;
            // 
            // WriteContentGB
            // 
            this.WriteContentGB.Controls.Add(this.WriteByteNUD);
            this.WriteContentGB.Controls.Add(this.RWAllCB);
            this.WriteContentGB.Controls.Add(this.ChosenFileTB);
            this.WriteContentGB.Controls.Add(this.ChooseWriteFileBtn);
            this.WriteContentGB.Controls.Add(this.WriteFileRB);
            this.WriteContentGB.Controls.Add(this.label3);
            this.WriteContentGB.Controls.Add(this.WriteCharRB);
            this.WriteContentGB.Location = new System.Drawing.Point(5, 78);
            this.WriteContentGB.Name = "WriteContentGB";
            this.WriteContentGB.Size = new System.Drawing.Size(189, 157);
            this.WriteContentGB.TabIndex = 7;
            this.WriteContentGB.TabStop = false;
            this.WriteContentGB.Text = "写入内容";
            // 
            // WriteByteNUD
            // 
            this.WriteByteNUD.Location = new System.Drawing.Point(114, 42);
            this.WriteByteNUD.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.WriteByteNUD.Name = "WriteByteNUD";
            this.WriteByteNUD.Size = new System.Drawing.Size(69, 21);
            this.WriteByteNUD.TabIndex = 10;
            // 
            // RWAllCB
            // 
            this.RWAllCB.AutoSize = true;
            this.RWAllCB.Location = new System.Drawing.Point(62, 130);
            this.RWAllCB.Name = "RWAllCB";
            this.RWAllCB.Size = new System.Drawing.Size(72, 16);
            this.RWAllCB.TabIndex = 9;
            this.RWAllCB.Text = "全部读写";
            this.RWAllCB.UseVisualStyleBackColor = true;
            this.RWAllCB.CheckedChanged += new System.EventHandler(this.RWAllCB_CheckedChanged);
            // 
            // ChosenFileTB
            // 
            this.ChosenFileTB.Cursor = System.Windows.Forms.Cursors.Default;
            this.ChosenFileTB.Location = new System.Drawing.Point(14, 98);
            this.ChosenFileTB.Name = "ChosenFileTB";
            this.ChosenFileTB.ReadOnly = true;
            this.ChosenFileTB.Size = new System.Drawing.Size(161, 21);
            this.ChosenFileTB.TabIndex = 8;
            this.ChosenFileTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ChooseWriteFileBtn
            // 
            this.ChooseWriteFileBtn.Location = new System.Drawing.Point(114, 69);
            this.ChooseWriteFileBtn.Name = "ChooseWriteFileBtn";
            this.ChooseWriteFileBtn.Size = new System.Drawing.Size(69, 23);
            this.ChooseWriteFileBtn.TabIndex = 7;
            this.ChooseWriteFileBtn.Text = "选择文件";
            this.ChooseWriteFileBtn.UseVisualStyleBackColor = true;
            this.ChooseWriteFileBtn.Click += new System.EventHandler(this.ChooseWriteFileBtn_Click);
            // 
            // WriteFileRB
            // 
            this.WriteFileRB.AutoSize = true;
            this.WriteFileRB.Location = new System.Drawing.Point(7, 72);
            this.WriteFileRB.Name = "WriteFileRB";
            this.WriteFileRB.Size = new System.Drawing.Size(107, 16);
            this.WriteFileRB.TabIndex = 6;
            this.WriteFileRB.Text = "从指定文件写入";
            this.WriteFileRB.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "写入字节(0-255）";
            // 
            // WriteCharRB
            // 
            this.WriteCharRB.AutoSize = true;
            this.WriteCharRB.Checked = true;
            this.WriteCharRB.Location = new System.Drawing.Point(6, 20);
            this.WriteCharRB.Name = "WriteCharRB";
            this.WriteCharRB.Size = new System.Drawing.Size(95, 16);
            this.WriteCharRB.TabIndex = 4;
            this.WriteCharRB.TabStop = true;
            this.WriteCharRB.Text = "写入固定字节";
            this.WriteCharRB.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "B";
            // 
            // RWLengthNUD
            // 
            this.RWLengthNUD.Location = new System.Drawing.Point(67, 15);
            this.RWLengthNUD.Maximum = new decimal(new int[] {
            1215752191,
            23,
            0,
            0});
            this.RWLengthNUD.Name = "RWLengthNUD";
            this.RWLengthNUD.Size = new System.Drawing.Size(113, 21);
            this.RWLengthNUD.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "读写长度";
            // 
            // FileSystemGB
            // 
            this.FileSystemGB.Controls.Add(this.FileSystemTV);
            this.FileSystemGB.Controls.Add(this.NowOpenTB);
            this.FileSystemGB.Controls.Add(this.NowOpenLb);
            this.FileSystemGB.Location = new System.Drawing.Point(221, 12);
            this.FileSystemGB.Name = "FileSystemGB";
            this.FileSystemGB.Size = new System.Drawing.Size(330, 573);
            this.FileSystemGB.TabIndex = 6;
            this.FileSystemGB.TabStop = false;
            this.FileSystemGB.Text = "文件系统视图";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // CreateImgBtn
            // 
            this.CreateImgBtn.Location = new System.Drawing.Point(12, 14);
            this.CreateImgBtn.Name = "CreateImgBtn";
            this.CreateImgBtn.Size = new System.Drawing.Size(203, 45);
            this.CreateImgBtn.TabIndex = 7;
            this.CreateImgBtn.Text = "新建文件卷";
            this.CreateImgBtn.UseVisualStyleBackColor = true;
            this.CreateImgBtn.Click += new System.EventHandler(this.CreateImgBtn_Click);
            // 
            // CloseBtn
            // 
            this.CloseBtn.Location = new System.Drawing.Point(12, 540);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(203, 45);
            this.CloseBtn.TabIndex = 8;
            this.CloseBtn.Text = "保存并退出";
            this.CloseBtn.UseVisualStyleBackColor = true;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SpaceUsageTSSL,
            this.SpaceUsageTSPB,
            this.SpaceUsageRatioTSSL,
            this.NodeUsageTSSL,
            this.NodeUsageCountTSSL});
            this.statusStrip1.Location = new System.Drawing.Point(0, 595);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(563, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SpaceUsageTSSL
            // 
            this.SpaceUsageTSSL.Name = "SpaceUsageTSSL";
            this.SpaceUsageTSSL.Size = new System.Drawing.Size(80, 17);
            this.SpaceUsageTSSL.Text = "空间使用情况";
            // 
            // SpaceUsageTSPB
            // 
            this.SpaceUsageTSPB.Name = "SpaceUsageTSPB";
            this.SpaceUsageTSPB.Size = new System.Drawing.Size(100, 16);
            // 
            // SpaceUsageRatioTSSL
            // 
            this.SpaceUsageRatioTSSL.Name = "SpaceUsageRatioTSSL";
            this.SpaceUsageRatioTSSL.Size = new System.Drawing.Size(66, 17);
            this.SpaceUsageRatioTSSL.Text = "0% 已使用";
            // 
            // NodeUsageTSSL
            // 
            this.NodeUsageTSSL.Name = "NodeUsageTSSL";
            this.NodeUsageTSSL.Size = new System.Drawing.Size(64, 17);
            this.NodeUsageTSSL.Text = "  文件数量";
            // 
            // NodeUsageCountTSSL
            // 
            this.NodeUsageCountTSSL.Name = "NodeUsageCountTSSL";
            this.NodeUsageCountTSSL.Size = new System.Drawing.Size(48, 17);
            this.NodeUsageCountTSSL.Text = "0/8192";
            // 
            // StatusTB
            // 
            this.StatusTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StatusTB.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StatusTB.Location = new System.Drawing.Point(403, 599);
            this.StatusTB.Name = "StatusTB";
            this.StatusTB.ReadOnly = true;
            this.StatusTB.Size = new System.Drawing.Size(160, 16);
            this.StatusTB.TabIndex = 11;
            this.StatusTB.Text = "就绪";
            this.StatusTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 617);
            this.Controls.Add(this.StatusTB);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.CreateImgBtn);
            this.Controls.Add(this.FileSystemGB);
            this.Controls.Add(this.ReadWriteGB);
            this.Controls.Add(this.FormatImgBtn);
            this.Controls.Add(this.LoadImgBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "文件系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ReadWriteGB.ResumeLayout(false);
            this.ReadWriteGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PointerLocNUD)).EndInit();
            this.WriteContentGB.ResumeLayout(false);
            this.WriteContentGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WriteByteNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RWLengthNUD)).EndInit();
            this.FileSystemGB.ResumeLayout(false);
            this.FileSystemGB.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView FileSystemTV;
        private System.Windows.Forms.Button LoadImgBtn;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 新建文件夹ToolStripMenuItem;
        private System.Windows.Forms.Button FormatImgBtn;
        private System.Windows.Forms.Button CloseFileBtn;
        private System.Windows.Forms.Label NowOpenLb;
        private System.Windows.Forms.TextBox NowOpenTB;
        private System.Windows.Forms.GroupBox ReadWriteGB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown RWLengthNUD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox FileSystemGB;
        private System.Windows.Forms.Button WriteFileBtn;
        private System.Windows.Forms.Button ReadFileBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown PointerLocNUD;
        private System.Windows.Forms.GroupBox WriteContentGB;
        private System.Windows.Forms.Button ChooseWriteFileBtn;
        private System.Windows.Forms.RadioButton WriteFileRB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton WriteCharRB;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button CreateImgBtn;
        private System.Windows.Forms.ToolStripMenuItem 新建文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.Button CloseBtn;
        private System.Windows.Forms.TextBox ChosenFileTB;
        private System.Windows.Forms.CheckBox RWAllCB;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SpaceUsageTSSL;
        private System.Windows.Forms.ToolStripProgressBar SpaceUsageTSPB;
        private System.Windows.Forms.ToolStripStatusLabel SpaceUsageRatioTSSL;
        private System.Windows.Forms.ToolStripStatusLabel NodeUsageTSSL;
        private System.Windows.Forms.ToolStripStatusLabel NodeUsageCountTSSL;
        private System.Windows.Forms.TextBox StatusTB;
        private System.Windows.Forms.NumericUpDown WriteByteNUD;
        private System.Windows.Forms.ToolStripMenuItem 属性ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 展开所有ToolStripMenuItem;
    }
}

