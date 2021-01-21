
namespace DeviceTester.Wl
{
    partial class VbWlMixView
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
            this.dMsg = new System.Windows.Forms.RichTextBox();
            this.dPorts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.dCmd = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // dMsg
            // 
            this.dMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dMsg.Location = new System.Drawing.Point(13, 13);
            this.dMsg.Name = "dMsg";
            this.dMsg.Size = new System.Drawing.Size(849, 569);
            this.dMsg.TabIndex = 0;
            this.dMsg.Text = "";
            // 
            // dPorts
            // 
            this.dPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dPorts.FormattingEnabled = true;
            this.dPorts.Location = new System.Drawing.Point(932, 13);
            this.dPorts.Name = "dPorts";
            this.dPorts.Size = new System.Drawing.Size(121, 20);
            this.dPorts.TabIndex = 1;
            this.dPorts.SelectedValueChanged += new System.EventHandler(this.dPorts_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(881, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "串口:";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(978, 48);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(978, 559);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // dCmd
            // 
            this.dCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dCmd.FormattingEnabled = true;
            this.dCmd.ItemHeight = 12;
            this.dCmd.Location = new System.Drawing.Point(883, 91);
            this.dCmd.Name = "dCmd";
            this.dCmd.Size = new System.Drawing.Size(170, 436);
            this.dCmd.TabIndex = 4;
            // 
            // VbWlMixView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 594);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.dCmd);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dPorts);
            this.Controls.Add(this.dMsg);
            this.Name = "VbWlMixView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VbWlMix";
            this.Load += new System.EventHandler(this.VbWlMixView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox dMsg;
        private System.Windows.Forms.ComboBox dPorts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ListBox dCmd;
    }
}