
namespace C3TekClient
{
    partial class AppSettingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSettingForm));
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
            this.btnGetProxy = new DevExpress.XtraEditors.SimpleButton();
            this.lblDieProxyCount = new System.Windows.Forms.Label();
            this.btnCheckProxy = new DevExpress.XtraEditors.SimpleButton();
            this.lblLiveProxyCount = new System.Windows.Forms.Label();
            this.txtProxyList = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.cbLanguage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnSaveSetting = new DevExpress.XtraEditors.SimpleButton();
            this.groupControl8 = new DevExpress.XtraEditors.GroupControl();
            this.settingTimeEndAnswer = new System.Windows.Forms.NumericUpDown();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.groupSettingAuto = new DevExpress.XtraEditors.GroupControl();
            this.ckListBoxAutoModeSetting = new DevExpress.XtraEditors.CheckedListBoxControl();
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
            this.groupControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbLanguage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl8)).BeginInit();
            this.groupControl8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.settingTimeEndAnswer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupSettingAuto)).BeginInit();
            this.groupSettingAuto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ckListBoxAutoModeSetting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            resources.ApplyResources(this.panelControl1, "panelControl1");
            this.panelControl1.Controls.Add(this.groupControl3);
            this.panelControl1.Controls.Add(this.groupControl1);
            this.panelControl1.Controls.Add(this.btnSaveSetting);
            this.panelControl1.Controls.Add(this.groupControl8);
            this.panelControl1.Controls.Add(this.groupSettingAuto);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.panelControl1_Paint);
            // 
            // groupControl3
            // 
            resources.ApplyResources(this.groupControl3, "groupControl3");
            this.groupControl3.Controls.Add(this.btnGetProxy);
            this.groupControl3.Controls.Add(this.lblDieProxyCount);
            this.groupControl3.Controls.Add(this.btnCheckProxy);
            this.groupControl3.Controls.Add(this.lblLiveProxyCount);
            this.groupControl3.Controls.Add(this.txtProxyList);
            this.groupControl3.Controls.Add(this.label2);
            this.groupControl3.Controls.Add(this.label1);
            this.groupControl3.Name = "groupControl3";
            this.groupControl3.Paint += new System.Windows.Forms.PaintEventHandler(this.groupControl3_Paint);
            // 
            // btnGetProxy
            // 
            resources.ApplyResources(this.btnGetProxy, "btnGetProxy");
            this.btnGetProxy.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnGetProxy.ImageOptions.Image")));
            this.btnGetProxy.Name = "btnGetProxy";
            this.btnGetProxy.Click += new System.EventHandler(this.btnCheckProxy_Click);
            // 
            // lblDieProxyCount
            // 
            resources.ApplyResources(this.lblDieProxyCount, "lblDieProxyCount");
            this.lblDieProxyCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblDieProxyCount.Name = "lblDieProxyCount";
            this.lblDieProxyCount.Click += new System.EventHandler(this.lblDieProxyCount_Click);
            // 
            // btnCheckProxy
            // 
            resources.ApplyResources(this.btnCheckProxy, "btnCheckProxy");
            this.btnCheckProxy.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnCheckProxy.ImageOptions.Image")));
            this.btnCheckProxy.Name = "btnCheckProxy";
            this.btnCheckProxy.Click += new System.EventHandler(this.btnCheckProxy_Click_1);
            // 
            // lblLiveProxyCount
            // 
            resources.ApplyResources(this.lblLiveProxyCount, "lblLiveProxyCount");
            this.lblLiveProxyCount.ForeColor = System.Drawing.Color.Green;
            this.lblLiveProxyCount.Name = "lblLiveProxyCount";
            // 
            // txtProxyList
            // 
            resources.ApplyResources(this.txtProxyList, "txtProxyList");
            this.txtProxyList.Name = "txtProxyList";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupControl1
            // 
            resources.ApplyResources(this.groupControl1, "groupControl1");
            this.groupControl1.Controls.Add(this.simpleButton1);
            this.groupControl1.Controls.Add(this.cbLanguage);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.groupControl1_Paint);
            // 
            // simpleButton1
            // 
            resources.ApplyResources(this.simpleButton1, "simpleButton1");
            this.simpleButton1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.ImageOptions.Image")));
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // cbLanguage
            // 
            resources.ApplyResources(this.cbLanguage, "cbLanguage");
            this.cbLanguage.Name = "cbLanguage";
            this.cbLanguage.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("cbLanguage.Properties.Buttons"))))});
            this.cbLanguage.Properties.Items.AddRange(new object[] {
            resources.GetString("cbLanguage.Properties.Items"),
            resources.GetString("cbLanguage.Properties.Items1"),
            resources.GetString("cbLanguage.Properties.Items2"),
            resources.GetString("cbLanguage.Properties.Items3")});
            // 
            // btnSaveSetting
            // 
            resources.ApplyResources(this.btnSaveSetting, "btnSaveSetting");
            this.btnSaveSetting.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveSetting.ImageOptions.Image")));
            this.btnSaveSetting.Name = "btnSaveSetting";
            this.btnSaveSetting.Click += new System.EventHandler(this.btnSaveSetting_Click);
            // 
            // groupControl8
            // 
            resources.ApplyResources(this.groupControl8, "groupControl8");
            this.groupControl8.Controls.Add(this.settingTimeEndAnswer);
            this.groupControl8.Controls.Add(this.labelControl4);
            this.groupControl8.Controls.Add(this.labelControl3);
            this.groupControl8.Controls.Add(this.labelControl2);
            this.groupControl8.Name = "groupControl8";
            // 
            // settingTimeEndAnswer
            // 
            resources.ApplyResources(this.settingTimeEndAnswer, "settingTimeEndAnswer");
            this.settingTimeEndAnswer.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.settingTimeEndAnswer.Name = "settingTimeEndAnswer";
            this.settingTimeEndAnswer.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // labelControl4
            // 
            resources.ApplyResources(this.labelControl4, "labelControl4");
            this.labelControl4.Appearance.FontStyleDelta = ((System.Drawing.FontStyle)(resources.GetObject("labelControl4.Appearance.FontStyleDelta")));
            this.labelControl4.Appearance.Options.UseFont = true;
            this.labelControl4.Name = "labelControl4";
            // 
            // labelControl3
            // 
            resources.ApplyResources(this.labelControl3, "labelControl3");
            this.labelControl3.Name = "labelControl3";
            // 
            // labelControl2
            // 
            resources.ApplyResources(this.labelControl2, "labelControl2");
            this.labelControl2.Name = "labelControl2";
            // 
            // groupSettingAuto
            // 
            resources.ApplyResources(this.groupSettingAuto, "groupSettingAuto");
            this.groupSettingAuto.Controls.Add(this.ckListBoxAutoModeSetting);
            this.groupSettingAuto.Name = "groupSettingAuto";
            this.groupSettingAuto.Paint += new System.Windows.Forms.PaintEventHandler(this.groupSettingAuto_Paint);
            // 
            // ckListBoxAutoModeSetting
            // 
            resources.ApplyResources(this.ckListBoxAutoModeSetting, "ckListBoxAutoModeSetting");
            this.ckListBoxAutoModeSetting.CheckOnClick = true;
            this.ckListBoxAutoModeSetting.HotTrackSelectMode = DevExpress.XtraEditors.HotTrackSelectMode.SelectItemOnClick;
            this.ckListBoxAutoModeSetting.Items.AddRange(new DevExpress.XtraEditors.Controls.CheckedListBoxItem[] {
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(resources.GetString("ckListBoxAutoModeSetting.Items"), ((System.Windows.Forms.CheckState)(resources.GetObject("ckListBoxAutoModeSetting.Items1")))),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(resources.GetString("ckListBoxAutoModeSetting.Items2"), ((System.Windows.Forms.CheckState)(resources.GetObject("ckListBoxAutoModeSetting.Items3")))),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(resources.GetString("ckListBoxAutoModeSetting.Items4")),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(resources.GetString("ckListBoxAutoModeSetting.Items5")),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(((object)(resources.GetObject("ckListBoxAutoModeSetting.Items6"))), resources.GetString("ckListBoxAutoModeSetting.Items7"))});
            this.ckListBoxAutoModeSetting.Name = "ckListBoxAutoModeSetting";
            this.ckListBoxAutoModeSetting.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.ckListBoxAutoModeSetting.SelectedIndexChanged += new System.EventHandler(this.ckListBoxAutoModeSetting_SelectedIndexChanged);
            // 
            // AppSettingForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelControl1);
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("AppSettingForm.IconOptions.SvgImage")));
            this.Name = "AppSettingForm";
            this.Load += new System.EventHandler(this.AppSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
            this.groupControl3.ResumeLayout(false);
            this.groupControl3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbLanguage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl8)).EndInit();
            this.groupControl8.ResumeLayout(false);
            this.groupControl8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.settingTimeEndAnswer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupSettingAuto)).EndInit();
            this.groupSettingAuto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ckListBoxAutoModeSetting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnSaveSetting;
        private DevExpress.XtraEditors.GroupControl groupControl8;
        private System.Windows.Forms.NumericUpDown settingTimeEndAnswer;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.GroupControl groupSettingAuto;
        private DevExpress.XtraEditors.CheckedListBoxControl ckListBoxAutoModeSetting;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.ComboBoxEdit cbLanguage;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private System.Windows.Forms.Label lblLiveProxyCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDieProxyCount;
        private DevExpress.XtraEditors.GroupControl groupControl3;
        private DevExpress.XtraEditors.SimpleButton btnCheckProxy;
        private System.Windows.Forms.RichTextBox txtProxyList;
        public DevExpress.XtraEditors.SimpleButton btnGetProxy;
    }
}