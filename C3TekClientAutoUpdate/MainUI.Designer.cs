namespace C3TekClientAutoUpdate
{
    partial class MainUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainUI));
            this.txtProductName = new System.Windows.Forms.Label();
            this.txtVersionName = new System.Windows.Forms.Label();
            this.pb = new System.Windows.Forms.ProgressBar();
            this.lblDownloadInfo = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtProductName
            // 
            this.txtProductName.BackColor = System.Drawing.Color.SkyBlue;
            this.txtProductName.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtProductName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProductName.ForeColor = System.Drawing.Color.Black;
            this.txtProductName.Location = new System.Drawing.Point(0, 0);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(434, 25);
            this.txtProductName.TabIndex = 2;
            this.txtProductName.Text = "C3TEK CLIENT";
            this.txtProductName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtVersionName
            // 
            this.txtVersionName.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtVersionName.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVersionName.Location = new System.Drawing.Point(0, 25);
            this.txtVersionName.Name = "txtVersionName";
            this.txtVersionName.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.txtVersionName.Size = new System.Drawing.Size(434, 33);
            this.txtVersionName.TabIndex = 4;
            this.txtVersionName.Text = "VERSION";
            this.txtVersionName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb
            // 
            this.pb.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pb.ForeColor = System.Drawing.Color.Gold;
            this.pb.Location = new System.Drawing.Point(0, 166);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(434, 23);
            this.pb.Step = 1;
            this.pb.TabIndex = 5;
            this.pb.Tag = "";
            // 
            // lblDownloadInfo
            // 
            this.lblDownloadInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDownloadInfo.Location = new System.Drawing.Point(0, 143);
            this.lblDownloadInfo.Name = "lblDownloadInfo";
            this.lblDownloadInfo.Size = new System.Drawing.Size(434, 23);
            this.lblDownloadInfo.TabIndex = 6;
            this.lblDownloadInfo.Text = "Đang tải bản cập nhật 0%";
            this.lblDownloadInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.SystemColors.Control;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(0, 58);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(434, 85);
            this.txtDescription.TabIndex = 7;
            this.txtDescription.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 189);
            this.ControlBox = false;
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDownloadInfo);
            this.Controls.Add(this.pb);
            this.Controls.Add(this.txtVersionName);
            this.Controls.Add(this.txtProductName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "C3Tek Client Auto Update";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label txtProductName;
        private System.Windows.Forms.Label txtVersionName;
        private System.Windows.Forms.ProgressBar pb;
        private System.Windows.Forms.Label lblDownloadInfo;
        private System.Windows.Forms.RichTextBox txtDescription;
    }
}

