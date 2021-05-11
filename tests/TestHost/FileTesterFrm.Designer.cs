
namespace TestHost
{
    partial class FileTesterFrm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtbTxt = new System.Windows.Forms.RichTextBox();
            this.btnLoadFileAsync = new System.Windows.Forms.Button();
            this.btnSaveFileAsync = new System.Windows.Forms.Button();
            this.cbFileAppend = new System.Windows.Forms.CheckBox();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.btnWrite3s = new System.Windows.Forms.Button();
            this.btnRead3s = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(23, 75);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(91, 23);
            this.btnLoadFile.TabIndex = 0;
            this.btnLoadFile.Text = "Load";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRead3s);
            this.groupBox1.Controls.Add(this.btnWrite3s);
            this.groupBox1.Controls.Add(this.rtbTxt);
            this.groupBox1.Controls.Add(this.btnLoadFileAsync);
            this.groupBox1.Controls.Add(this.btnSaveFileAsync);
            this.groupBox1.Controls.Add(this.cbFileAppend);
            this.groupBox1.Controls.Add(this.btnSaveFile);
            this.groupBox1.Controls.Add(this.btnLoadFile);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 325);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "文件读写";
            // 
            // rtbTxt
            // 
            this.rtbTxt.Location = new System.Drawing.Point(23, 144);
            this.rtbTxt.Name = "rtbTxt";
            this.rtbTxt.Size = new System.Drawing.Size(198, 168);
            this.rtbTxt.TabIndex = 5;
            this.rtbTxt.Text = "";
            // 
            // btnLoadFileAsync
            // 
            this.btnLoadFileAsync.Location = new System.Drawing.Point(23, 104);
            this.btnLoadFileAsync.Name = "btnLoadFileAsync";
            this.btnLoadFileAsync.Size = new System.Drawing.Size(91, 23);
            this.btnLoadFileAsync.TabIndex = 4;
            this.btnLoadFileAsync.Text = "LoadAsync";
            this.btnLoadFileAsync.UseVisualStyleBackColor = true;
            this.btnLoadFileAsync.Click += new System.EventHandler(this.btnLoadFileAsync_Click);
            // 
            // btnSaveFileAsync
            // 
            this.btnSaveFileAsync.Location = new System.Drawing.Point(133, 104);
            this.btnSaveFileAsync.Name = "btnSaveFileAsync";
            this.btnSaveFileAsync.Size = new System.Drawing.Size(88, 23);
            this.btnSaveFileAsync.TabIndex = 3;
            this.btnSaveFileAsync.Text = "SaveAsync";
            this.btnSaveFileAsync.UseVisualStyleBackColor = true;
            this.btnSaveFileAsync.Click += new System.EventHandler(this.btnSaveFileAsync_Click);
            // 
            // cbFileAppend
            // 
            this.cbFileAppend.AutoSize = true;
            this.cbFileAppend.Location = new System.Drawing.Point(86, 33);
            this.cbFileAppend.Name = "cbFileAppend";
            this.cbFileAppend.Size = new System.Drawing.Size(73, 21);
            this.cbFileAppend.TabIndex = 2;
            this.cbFileAppend.Text = "Append";
            this.cbFileAppend.UseVisualStyleBackColor = true;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(133, 75);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(88, 23);
            this.btnSaveFile.TabIndex = 1;
            this.btnSaveFile.Text = "Save";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // btnWrite3s
            // 
            this.btnWrite3s.Location = new System.Drawing.Point(227, 193);
            this.btnWrite3s.Name = "btnWrite3s";
            this.btnWrite3s.Size = new System.Drawing.Size(88, 23);
            this.btnWrite3s.TabIndex = 6;
            this.btnWrite3s.Text = "Write3s";
            this.btnWrite3s.UseVisualStyleBackColor = true;
            this.btnWrite3s.Click += new System.EventHandler(this.btnWrite3s_Click);
            // 
            // btnRead3s
            // 
            this.btnRead3s.Location = new System.Drawing.Point(227, 235);
            this.btnRead3s.Name = "btnRead3s";
            this.btnRead3s.Size = new System.Drawing.Size(88, 23);
            this.btnRead3s.TabIndex = 7;
            this.btnRead3s.Text = "Read3s";
            this.btnRead3s.UseVisualStyleBackColor = true;
            this.btnRead3s.Click += new System.EventHandler(this.btnRead3s_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 478);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.CheckBox cbFileAppend;
        private System.Windows.Forms.Button btnLoadFileAsync;
        private System.Windows.Forms.Button btnSaveFileAsync;
        private System.Windows.Forms.RichTextBox rtbTxt;
        private System.Windows.Forms.Button btnRead3s;
        private System.Windows.Forms.Button btnWrite3s;
    }
}

