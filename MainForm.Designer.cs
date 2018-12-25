namespace VRCLogViewer
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.listview = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imagelist = new System.Windows.Forms.ImageList(this.components);
            this.checkbox = new System.Windows.Forms.CheckBox();
            this.label_author = new System.Windows.Forms.Label();
            this.textbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 500;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // listview
            // 
            this.listview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listview.FullRowSelect = true;
            this.listview.GridLines = true;
            this.listview.HideSelection = false;
            this.listview.Location = new System.Drawing.Point(9, 40);
            this.listview.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.listview.Name = "listview";
            this.listview.Size = new System.Drawing.Size(366, 512);
            this.listview.StateImageList = this.imagelist;
            this.listview.TabIndex = 0;
            this.listview.UseCompatibleStateImageBehavior = false;
            this.listview.View = System.Windows.Forms.View.Details;
            this.listview.DoubleClick += new System.EventHandler(this.listview_DoubleClick);
            this.listview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listview_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Log Text";
            this.columnHeader2.Width = 230;
            // 
            // imagelist
            // 
            this.imagelist.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagelist.ImageStream")));
            this.imagelist.TransparentColor = System.Drawing.Color.Transparent;
            this.imagelist.Images.SetKeyName(0, "map.png");
            this.imagelist.Images.SetKeyName(1, "status_online.png");
            this.imagelist.Images.SetKeyName(2, "status_offline.png");
            // 
            // checkbox
            // 
            this.checkbox.AutoSize = true;
            this.checkbox.Location = new System.Drawing.Point(9, 14);
            this.checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.checkbox.Name = "checkbox";
            this.checkbox.Size = new System.Drawing.Size(63, 16);
            this.checkbox.TabIndex = 2;
            this.checkbox.Text = "Update";
            this.checkbox.UseVisualStyleBackColor = true;
            this.checkbox.CheckedChanged += new System.EventHandler(this.checkbox_CheckedChanged);
            // 
            // label_author
            // 
            this.label_author.Cursor = System.Windows.Forms.Cursors.Cross;
            this.label_author.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label_author.Location = new System.Drawing.Point(155, 9);
            this.label_author.Margin = new System.Windows.Forms.Padding(0);
            this.label_author.Name = "label_author";
            this.label_author.Size = new System.Drawing.Size(220, 24);
            this.label_author.TabIndex = 3;
            this.label_author.Text = "DCinside VRChat Minor Gallery\r\nby mina#5656";
            this.label_author.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_author.DoubleClick += new System.EventHandler(this.label_author_DoubleClick);
            // 
            // textbox
            // 
            this.textbox.Location = new System.Drawing.Point(175, 9);
            this.textbox.Margin = new System.Windows.Forms.Padding(0);
            this.textbox.Name = "textbox";
            this.textbox.Size = new System.Drawing.Size(200, 21);
            this.textbox.TabIndex = 4;
            this.textbox.Visible = false;
            this.textbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textbox_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 561);
            this.Controls.Add(this.textbox);
            this.Controls.Add(this.checkbox);
            this.Controls.Add(this.label_author);
            this.Controls.Add(this.listview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRCLogViewer v0.02";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ListView listview;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imagelist;
        private System.Windows.Forms.CheckBox checkbox;
        private System.Windows.Forms.Label label_author;
        private System.Windows.Forms.TextBox textbox;
    }
}

