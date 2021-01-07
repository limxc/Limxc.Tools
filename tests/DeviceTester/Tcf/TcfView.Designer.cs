
namespace DeviceTester.Tcf
{
    partial class TcfView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dReceived = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.dAutoConnect = new System.Windows.Forms.CheckBox();
            this.dId = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dAge = new System.Windows.Forms.NumericUpDown();
            this.dWeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dHeight = new System.Windows.Forms.NumericUpDown();
            this.dGenderMale = new System.Windows.Forms.RadioButton();
            this.dGenderFemale = new System.Windows.Forms.RadioButton();
            this.dResult = new System.Windows.Forms.ListBox();
            this.dMsg = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dAge)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dWeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // dReceived
            // 
            this.dReceived.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dReceived.Location = new System.Drawing.Point(12, 33);
            this.dReceived.Name = "dReceived";
            this.dReceived.Size = new System.Drawing.Size(281, 473);
            this.dReceived.TabIndex = 0;
            this.dReceived.Text = "";
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(709, 483);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // dAutoConnect
            // 
            this.dAutoConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dAutoConnect.AutoSize = true;
            this.dAutoConnect.Checked = true;
            this.dAutoConnect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dAutoConnect.Location = new System.Drawing.Point(712, 251);
            this.dAutoConnect.Name = "dAutoConnect";
            this.dAutoConnect.Size = new System.Drawing.Size(72, 16);
            this.dAutoConnect.TabIndex = 5;
            this.dAutoConnect.Text = "自动重连";
            this.dAutoConnect.UseVisualStyleBackColor = true;
            // 
            // dId
            // 
            this.dId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.dId, 2);
            this.dId.Enabled = false;
            this.dId.Location = new System.Drawing.Point(58, 5);
            this.dId.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.dId.Name = "dId";
            this.dId.Size = new System.Drawing.Size(119, 21);
            this.dId.TabIndex = 6;
            this.dId.Text = "123456789012345678";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.Controls.Add(this.dName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dAge, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.dWeight, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.dId, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.dHeight, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.dGenderMale, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.dGenderFemale, 2, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(594, 33);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(190, 196);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // dName
            // 
            this.dName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.dName, 2);
            this.dName.Enabled = false;
            this.dName.Location = new System.Drawing.Point(58, 37);
            this.dName.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.dName.Name = "dName";
            this.dName.Size = new System.Drawing.Size(119, 21);
            this.dName.TabIndex = 18;
            this.dName.Text = "张三";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "姓名:";
            // 
            // dAge
            // 
            this.dAge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dAge.Location = new System.Drawing.Point(58, 133);
            this.dAge.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.dAge.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.dAge.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.dAge.Name = "dAge";
            this.dAge.Size = new System.Drawing.Size(62, 21);
            this.dAge.TabIndex = 14;
            this.dAge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.dAge.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // dWeight
            // 
            this.dWeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dWeight.DecimalPlaces = 1;
            this.dWeight.Enabled = false;
            this.dWeight.Location = new System.Drawing.Point(58, 101);
            this.dWeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.dWeight.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.dWeight.Name = "dWeight";
            this.dWeight.Size = new System.Drawing.Size(62, 21);
            this.dWeight.TabIndex = 13;
            this.dWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "身高:";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "ID:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "体重:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "年龄:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "性别:";
            // 
            // dHeight
            // 
            this.dHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dHeight.DecimalPlaces = 1;
            this.dHeight.Location = new System.Drawing.Point(58, 69);
            this.dHeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.dHeight.Maximum = new decimal(new int[] {
            220,
            0,
            0,
            0});
            this.dHeight.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.dHeight.Name = "dHeight";
            this.dHeight.Size = new System.Drawing.Size(62, 21);
            this.dHeight.TabIndex = 12;
            this.dHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.dHeight.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            // 
            // dGenderMale
            // 
            this.dGenderMale.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dGenderMale.AutoSize = true;
            this.dGenderMale.Checked = true;
            this.dGenderMale.Location = new System.Drawing.Point(67, 170);
            this.dGenderMale.Name = "dGenderMale";
            this.dGenderMale.Size = new System.Drawing.Size(35, 16);
            this.dGenderMale.TabIndex = 15;
            this.dGenderMale.TabStop = true;
            this.dGenderMale.Text = "男";
            this.dGenderMale.UseVisualStyleBackColor = true;
            // 
            // dGenderFemale
            // 
            this.dGenderFemale.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dGenderFemale.AutoSize = true;
            this.dGenderFemale.Location = new System.Drawing.Point(137, 170);
            this.dGenderFemale.Name = "dGenderFemale";
            this.dGenderFemale.Size = new System.Drawing.Size(35, 16);
            this.dGenderFemale.TabIndex = 16;
            this.dGenderFemale.Text = "女";
            this.dGenderFemale.UseVisualStyleBackColor = true;
            // 
            // dResult
            // 
            this.dResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dResult.FormattingEnabled = true;
            this.dResult.ItemHeight = 12;
            this.dResult.Location = new System.Drawing.Point(299, 33);
            this.dResult.Name = "dResult";
            this.dResult.Size = new System.Drawing.Size(285, 472);
            this.dResult.TabIndex = 8;
            // 
            // dMsg
            // 
            this.dMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dMsg.Location = new System.Drawing.Point(592, 291);
            this.dMsg.Name = "dMsg";
            this.dMsg.Size = new System.Drawing.Size(189, 161);
            this.dMsg.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 11F);
            this.label7.Location = new System.Drawing.Point(297, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 15);
            this.label7.TabIndex = 10;
            this.label7.Text = "分析结果:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 11F);
            this.label8.Location = new System.Drawing.Point(10, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 15);
            this.label8.TabIndex = 11;
            this.label8.Text = "数据接收:";
            // 
            // TcfView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.dMsg);
            this.Controls.Add(this.dResult);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.dAutoConnect);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.dReceived);
            this.Name = "TcfView";
            this.Size = new System.Drawing.Size(797, 511);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dAge)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dWeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox dReceived;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.CheckBox dAutoConnect;
        private System.Windows.Forms.TextBox dId;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.NumericUpDown dAge;
        private System.Windows.Forms.NumericUpDown dWeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown dHeight;
        private System.Windows.Forms.RadioButton dGenderFemale;
        private System.Windows.Forms.RadioButton dGenderMale;
        private System.Windows.Forms.ListBox dResult;
        private System.Windows.Forms.TextBox dName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label dMsg;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}