
namespace C3TekClient
{
    partial class ListImeiForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListImeiForm));
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.imeiList = new System.Windows.Forms.RichTextBox();
            this.btnSaveImei = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // labelControl2
            // 
            resources.ApplyResources(this.labelControl2, "labelControl2");
            this.labelControl2.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("labelControl2.Appearance.Font")));
            this.labelControl2.Appearance.Options.UseFont = true;
            this.labelControl2.Name = "labelControl2";
            // 
            // labelControl4
            // 
            resources.ApplyResources(this.labelControl4, "labelControl4");
            this.labelControl4.Appearance.FontStyleDelta = ((System.Drawing.FontStyle)(resources.GetObject("labelControl4.Appearance.FontStyleDelta")));
            this.labelControl4.Appearance.Options.UseFont = true;
            this.labelControl4.Name = "labelControl4";
            // 
            // imeiList
            // 
            resources.ApplyResources(this.imeiList, "imeiList");
            this.imeiList.Name = "imeiList";
            // 
            // btnSaveImei
            // 
            resources.ApplyResources(this.btnSaveImei, "btnSaveImei");
            this.btnSaveImei.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveImei.ImageOptions.Image")));
            this.btnSaveImei.Name = "btnSaveImei";
            this.btnSaveImei.Click += new System.EventHandler(this.btnSaveImei_Click);
            // 
            // ListImeiForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSaveImei);
            this.Controls.Add(this.imeiList);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.labelControl2);
            this.Name = "ListImeiForm";
            this.Load += new System.EventHandler(this.ListImeiForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private System.Windows.Forms.RichTextBox imeiList;
        private DevExpress.XtraEditors.SimpleButton btnSaveImei;
    }
}