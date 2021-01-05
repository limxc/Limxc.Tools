
namespace DeviceTester.Tcf
{
    partial class FrmTcfView
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
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.dSerialPort = new System.Windows.Forms.ComboBox();
            this.viewModelControlHost1 = new ReactiveUI.Winforms.ViewModelControlHost();
            this.SuspendLayout();
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(161, 9);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 16;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "串口:";
            // 
            // dSerialPort
            // 
            this.dSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dSerialPort.FormattingEnabled = true;
            this.dSerialPort.Location = new System.Drawing.Point(57, 12);
            this.dSerialPort.Name = "dSerialPort";
            this.dSerialPort.Size = new System.Drawing.Size(83, 20);
            this.dSerialPort.TabIndex = 14;
            // 
            // viewModelControlHost1
            // 
            this.viewModelControlHost1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.viewModelControlHost1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.viewModelControlHost1.CacheViews = false;
            this.viewModelControlHost1.DefaultContent = null;
            this.viewModelControlHost1.Location = new System.Drawing.Point(7, 43);
            this.viewModelControlHost1.Name = "viewModelControlHost1";
            this.viewModelControlHost1.Size = new System.Drawing.Size(1131, 653);
            this.viewModelControlHost1.TabIndex = 17;
            this.viewModelControlHost1.ViewLocator = null;
            this.viewModelControlHost1.ViewModel = null;
            // 
            // FrmTcfView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1145, 702);
            this.Controls.Add(this.viewModelControlHost1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.dSerialPort);
            this.Name = "FrmTcfView";
            this.Text = "FrmTcfView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox dSerialPort;
        private ReactiveUI.Winforms.ViewModelControlHost viewModelControlHost1;
    }
}