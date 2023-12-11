
namespace C3TekClient.GSM
{
    partial class WSCallOutUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WSCallOutUI));
            DevExpress.XtraGrid.GridFormatRule gridFormatRule1 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleExpression formatConditionRuleExpression1 = new DevExpress.XtraEditors.FormatConditionRuleExpression();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule2 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleExpression formatConditionRuleExpression2 = new DevExpress.XtraEditors.FormatConditionRuleExpression();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule3 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleExpression formatConditionRuleExpression3 = new DevExpress.XtraEditors.FormatConditionRuleExpression();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule4 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleExpression formatConditionRuleExpression4 = new DevExpress.XtraEditors.FormatConditionRuleExpression();
            this.Status = new DevExpress.XtraGrid.Columns.GridColumn();
            this.txtDelaySecond = new System.Windows.Forms.NumericUpDown();
            this.svgImageBox1 = new DevExpress.XtraEditors.SvgImageBox();
            this.separatorControl1 = new DevExpress.XtraEditors.SeparatorControl();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPrepareData = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.pbSendProcess = new DevExpress.XtraEditors.ProgressBarControl();
            this.rgCallMode = new DevExpress.XtraEditors.RadioGroup();
            this.txtPhoneTo = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gridSMS = new DevExpress.XtraGrid.GridControl();
            this.viewSMS = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.RealPort = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AppPort = new DevExpress.XtraGrid.Columns.GridColumn();
            this.PhoneNumber = new DevExpress.XtraGrid.Columns.GridColumn();
            this.PhoneTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.lblCompleReport = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.btnImportExcel = new DevExpress.XtraEditors.SimpleButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ckPlayAudio = new DevExpress.XtraEditors.CheckEdit();
            this.btnExcelSample = new DevExpress.XtraEditors.SimpleButton();
            this.txtDuration = new System.Windows.Forms.NumericUpDown();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.btnRetry = new DevExpress.XtraEditors.SimpleButton();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.txtDelaySecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgCallMode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPhoneTo.Properties)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewSMS)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ckPlayAudio.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // Status
            // 
            resources.ApplyResources(this.Status, "Status");
            this.Status.FieldName = "Status";
            this.Status.MinWidth = 25;
            this.Status.Name = "Status";
            this.Status.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
            // 
            // txtDelaySecond
            // 
            resources.ApplyResources(this.txtDelaySecond, "txtDelaySecond");
            this.txtDelaySecond.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.txtDelaySecond.Name = "txtDelaySecond";
            this.txtDelaySecond.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtDelaySecond.ValueChanged += new System.EventHandler(this.txtDelaySecond_ValueChanged);
            // 
            // svgImageBox1
            // 
            resources.ApplyResources(this.svgImageBox1, "svgImageBox1");
            this.svgImageBox1.Name = "svgImageBox1";
            this.svgImageBox1.SizeMode = DevExpress.XtraEditors.SvgImageSizeMode.Squeeze;
            this.svgImageBox1.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("svgImageBox1.SvgImage")));
            // 
            // separatorControl1
            // 
            resources.ApplyResources(this.separatorControl1, "separatorControl1");
            this.separatorControl1.LineOrientation = System.Windows.Forms.Orientation.Vertical;
            this.separatorControl1.Name = "separatorControl1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnPrepareData
            // 
            resources.ApplyResources(this.btnPrepareData, "btnPrepareData");
            this.btnPrepareData.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnPrepareData.ImageOptions.SvgImage")));
            this.btnPrepareData.Name = "btnPrepareData";
            this.btnPrepareData.Click += new System.EventHandler(this.btnPrepareData_Click);
            // 
            // labelControl6
            // 
            resources.ApplyResources(this.labelControl6, "labelControl6");
            this.labelControl6.Name = "labelControl6";
            // 
            // labelControl5
            // 
            resources.ApplyResources(this.labelControl5, "labelControl5");
            this.labelControl5.Name = "labelControl5";
            // 
            // pbSendProcess
            // 
            resources.ApplyResources(this.pbSendProcess, "pbSendProcess");
            this.pbSendProcess.Name = "pbSendProcess";
            this.pbSendProcess.Properties.AutoHeight = ((bool)(resources.GetObject("pbSendProcess.Properties.AutoHeight")));
            this.pbSendProcess.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            this.pbSendProcess.Properties.ShowTitle = true;
            this.pbSendProcess.ShowProgressInTaskBar = true;
            // 
            // rgCallMode
            // 
            resources.ApplyResources(this.rgCallMode, "rgCallMode");
            this.rgCallMode.Name = "rgCallMode";
            this.rgCallMode.Properties.AllowFocused = false;
            this.rgCallMode.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(((object)(resources.GetObject("rgCallMode.Properties.Items"))), resources.GetString("rgCallMode.Properties.Items1")),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(((object)(resources.GetObject("rgCallMode.Properties.Items2"))), resources.GetString("rgCallMode.Properties.Items3"))});
            // 
            // txtPhoneTo
            // 
            resources.ApplyResources(this.txtPhoneTo, "txtPhoneTo");
            this.txtPhoneTo.Name = "txtPhoneTo";
            // 
            // labelControl3
            // 
            resources.ApplyResources(this.labelControl3, "labelControl3");
            this.labelControl3.Name = "labelControl3";
            // 
            // labelControl1
            // 
            resources.ApplyResources(this.labelControl1, "labelControl1");
            this.labelControl1.Name = "labelControl1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.gridSMS);
            this.panel1.Name = "panel1";
            // 
            // gridSMS
            // 
            resources.ApplyResources(this.gridSMS, "gridSMS");
            this.gridSMS.EmbeddedNavigator.AccessibleDescription = resources.GetString("gridSMS.EmbeddedNavigator.AccessibleDescription");
            this.gridSMS.EmbeddedNavigator.AccessibleName = resources.GetString("gridSMS.EmbeddedNavigator.AccessibleName");
            this.gridSMS.EmbeddedNavigator.AllowHtmlTextInToolTip = ((DevExpress.Utils.DefaultBoolean)(resources.GetObject("gridSMS.EmbeddedNavigator.AllowHtmlTextInToolTip")));
            this.gridSMS.EmbeddedNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gridSMS.EmbeddedNavigator.Anchor")));
            this.gridSMS.EmbeddedNavigator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gridSMS.EmbeddedNavigator.BackgroundImage")));
            this.gridSMS.EmbeddedNavigator.BackgroundImageLayout = ((System.Windows.Forms.ImageLayout)(resources.GetObject("gridSMS.EmbeddedNavigator.BackgroundImageLayout")));
            this.gridSMS.EmbeddedNavigator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gridSMS.EmbeddedNavigator.ImeMode")));
            this.gridSMS.EmbeddedNavigator.MaximumSize = ((System.Drawing.Size)(resources.GetObject("gridSMS.EmbeddedNavigator.MaximumSize")));
            this.gridSMS.EmbeddedNavigator.TextLocation = ((DevExpress.XtraEditors.NavigatorButtonsTextLocation)(resources.GetObject("gridSMS.EmbeddedNavigator.TextLocation")));
            this.gridSMS.EmbeddedNavigator.ToolTip = resources.GetString("gridSMS.EmbeddedNavigator.ToolTip");
            this.gridSMS.EmbeddedNavigator.ToolTipIconType = ((DevExpress.Utils.ToolTipIconType)(resources.GetObject("gridSMS.EmbeddedNavigator.ToolTipIconType")));
            this.gridSMS.EmbeddedNavigator.ToolTipTitle = resources.GetString("gridSMS.EmbeddedNavigator.ToolTipTitle");
            this.gridSMS.MainView = this.viewSMS;
            this.gridSMS.Name = "gridSMS";
            this.gridSMS.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewSMS});
            // 
            // viewSMS
            // 
            resources.ApplyResources(this.viewSMS, "viewSMS");
            this.viewSMS.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.RealPort,
            this.AppPort,
            this.PhoneNumber,
            this.PhoneTo,
            this.Status});
            gridFormatRule1.Column = this.Status;
            gridFormatRule1.ColumnApplyTo = this.Status;
            gridFormatRule1.Name = "Format0";
            formatConditionRuleExpression1.Appearance.BackColor = System.Drawing.Color.Green;
            formatConditionRuleExpression1.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("resource.Font")));
            formatConditionRuleExpression1.Appearance.ForeColor = System.Drawing.Color.White;
            formatConditionRuleExpression1.Appearance.Options.UseBackColor = true;
            formatConditionRuleExpression1.Appearance.Options.UseFont = true;
            formatConditionRuleExpression1.Appearance.Options.UseForeColor = true;
            formatConditionRuleExpression1.Expression = "[Status] = \'Thành công\' Or [Status] = \'Success\'";
            gridFormatRule1.Rule = formatConditionRuleExpression1;
            gridFormatRule2.Column = this.Status;
            gridFormatRule2.ColumnApplyTo = this.Status;
            gridFormatRule2.Name = "Format1";
            formatConditionRuleExpression2.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            formatConditionRuleExpression2.Appearance.ForeColor = System.Drawing.Color.White;
            formatConditionRuleExpression2.Appearance.Options.UseBackColor = true;
            formatConditionRuleExpression2.Appearance.Options.UseForeColor = true;
            formatConditionRuleExpression2.Expression = "[Status] = \'\' Or Contains([Status], \'BUSY OR NO CARRIER\')";
            gridFormatRule2.Rule = formatConditionRuleExpression2;
            gridFormatRule3.Column = this.Status;
            gridFormatRule3.ColumnApplyTo = this.Status;
            gridFormatRule3.Name = "Format2";
            formatConditionRuleExpression3.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            formatConditionRuleExpression3.Appearance.ForeColor = System.Drawing.Color.White;
            formatConditionRuleExpression3.Appearance.Options.UseBackColor = true;
            formatConditionRuleExpression3.Appearance.Options.UseForeColor = true;
            formatConditionRuleExpression3.Expression = "[Status] = \'Trong hàng đợi\' Or [Status] = \'In queue\'";
            gridFormatRule3.Rule = formatConditionRuleExpression3;
            gridFormatRule4.Column = this.Status;
            gridFormatRule4.ColumnApplyTo = this.Status;
            gridFormatRule4.Name = "Format3";
            formatConditionRuleExpression4.Appearance.BackColor = System.Drawing.Color.SkyBlue;
            formatConditionRuleExpression4.Appearance.Options.UseBackColor = true;
            formatConditionRuleExpression4.Expression = "[Status] = \'Đang xử lý\' Or [Status] = \'Processing\'";
            gridFormatRule4.Rule = formatConditionRuleExpression4;
            this.viewSMS.FormatRules.Add(gridFormatRule1);
            this.viewSMS.FormatRules.Add(gridFormatRule2);
            this.viewSMS.FormatRules.Add(gridFormatRule3);
            this.viewSMS.FormatRules.Add(gridFormatRule4);
            this.viewSMS.GridControl = this.gridSMS;
            this.viewSMS.Name = "viewSMS";
            // 
            // RealPort
            // 
            resources.ApplyResources(this.RealPort, "RealPort");
            this.RealPort.FieldName = "RealPort";
            this.RealPort.MinWidth = 25;
            this.RealPort.Name = "RealPort";
            this.RealPort.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
            // 
            // AppPort
            // 
            resources.ApplyResources(this.AppPort, "AppPort");
            this.AppPort.FieldName = "AppPort";
            this.AppPort.MinWidth = 25;
            this.AppPort.Name = "AppPort";
            this.AppPort.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
            // 
            // PhoneNumber
            // 
            resources.ApplyResources(this.PhoneNumber, "PhoneNumber");
            this.PhoneNumber.FieldName = "PhoneNumber";
            this.PhoneNumber.MinWidth = 25;
            this.PhoneNumber.Name = "PhoneNumber";
            this.PhoneNumber.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
            // 
            // PhoneTo
            // 
            resources.ApplyResources(this.PhoneTo, "PhoneTo");
            this.PhoneTo.FieldName = "PhoneTo";
            this.PhoneTo.MinWidth = 25;
            this.PhoneTo.Name = "PhoneTo";
            this.PhoneTo.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem()});
            // 
            // lblCompleReport
            // 
            resources.ApplyResources(this.lblCompleReport, "lblCompleReport");
            this.lblCompleReport.Name = "lblCompleReport";
            // 
            // labelControl7
            // 
            resources.ApplyResources(this.labelControl7, "labelControl7");
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Click += new System.EventHandler(this.labelControl7_Click);
            // 
            // simpleButton1
            // 
            resources.ApplyResources(this.simpleButton1, "simpleButton1");
            this.simpleButton1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton1.ImageOptions.SvgImage")));
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // btnImportExcel
            // 
            resources.ApplyResources(this.btnImportExcel, "btnImportExcel");
            this.btnImportExcel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnImportExcel.ImageOptions.SvgImage")));
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.ckPlayAudio);
            this.groupBox1.Controls.Add(this.btnExcelSample);
            this.groupBox1.Controls.Add(this.txtDuration);
            this.groupBox1.Controls.Add(this.labelControl4);
            this.groupBox1.Controls.Add(this.labelControl8);
            this.groupBox1.Controls.Add(this.txtDelaySecond);
            this.groupBox1.Controls.Add(this.svgImageBox1);
            this.groupBox1.Controls.Add(this.separatorControl1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnPrepareData);
            this.groupBox1.Controls.Add(this.btnRetry);
            this.groupBox1.Controls.Add(this.lblCompleReport);
            this.groupBox1.Controls.Add(this.labelControl7);
            this.groupBox1.Controls.Add(this.labelControl6);
            this.groupBox1.Controls.Add(this.labelControl5);
            this.groupBox1.Controls.Add(this.simpleButton1);
            this.groupBox1.Controls.Add(this.btnImportExcel);
            this.groupBox1.Controls.Add(this.btnStart);
            this.groupBox1.Controls.Add(this.pbSendProcess);
            this.groupBox1.Controls.Add(this.rgCallMode);
            this.groupBox1.Controls.Add(this.txtPhoneTo);
            this.groupBox1.Controls.Add(this.labelControl3);
            this.groupBox1.Controls.Add(this.labelControl1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // ckPlayAudio
            // 
            resources.ApplyResources(this.ckPlayAudio, "ckPlayAudio");
            this.ckPlayAudio.Name = "ckPlayAudio";
            this.ckPlayAudio.Properties.Caption = resources.GetString("ckPlayAudio.Properties.Caption");
            this.ckPlayAudio.Properties.DisplayValueChecked = resources.GetString("ckPlayAudio.Properties.DisplayValueChecked");
            this.ckPlayAudio.Properties.DisplayValueGrayed = resources.GetString("ckPlayAudio.Properties.DisplayValueGrayed");
            this.ckPlayAudio.Properties.DisplayValueUnchecked = resources.GetString("ckPlayAudio.Properties.DisplayValueUnchecked");
            this.ckPlayAudio.Properties.GlyphVerticalAlignment = ((DevExpress.Utils.VertAlignment)(resources.GetObject("ckPlayAudio.Properties.GlyphVerticalAlignment")));
            // 
            // btnExcelSample
            // 
            resources.ApplyResources(this.btnExcelSample, "btnExcelSample");
            this.btnExcelSample.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnExcelSample.ImageOptions.Image")));
            this.btnExcelSample.Name = "btnExcelSample";
            this.btnExcelSample.Click += new System.EventHandler(this.btnExcelSample_Click);
            // 
            // txtDuration
            // 
            resources.ApplyResources(this.txtDuration, "txtDuration");
            this.txtDuration.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // labelControl4
            // 
            resources.ApplyResources(this.labelControl4, "labelControl4");
            this.labelControl4.Name = "labelControl4";
            // 
            // labelControl8
            // 
            resources.ApplyResources(this.labelControl8, "labelControl8");
            this.labelControl8.Name = "labelControl8";
            // 
            // btnRetry
            // 
            resources.ApplyResources(this.btnRetry, "btnRetry");
            this.btnRetry.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnRetry.ImageOptions.SvgImage")));
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnStart
            // 
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnStart.ImageOptions.SvgImage")));
            this.btnStart.Name = "btnStart";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // WSCallOutUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.IconOptions.Image = global::C3TekClient.Properties.Resources.phone_3_icon_24;
            this.Name = "WSCallOutUI";
            this.Load += new System.EventHandler(this.WSCallOutUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtDelaySecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgCallMode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPhoneTo.Properties)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewSMS)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ckPlayAudio.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDuration)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown txtDelaySecond;
        private DevExpress.XtraEditors.SvgImageBox svgImageBox1;
        private DevExpress.XtraEditors.SeparatorControl separatorControl1;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SimpleButton btnPrepareData;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.ProgressBarControl pbSendProcess;
        private DevExpress.XtraEditors.RadioGroup rgCallMode;
        private DevExpress.XtraEditors.TextEdit txtPhoneTo;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraGrid.GridControl gridSMS;
        private DevExpress.XtraGrid.Views.Grid.GridView viewSMS;
        private DevExpress.XtraGrid.Columns.GridColumn RealPort;
        private DevExpress.XtraGrid.Columns.GridColumn AppPort;
        private DevExpress.XtraGrid.Columns.GridColumn PhoneNumber;
        private DevExpress.XtraGrid.Columns.GridColumn PhoneTo;
        private DevExpress.XtraGrid.Columns.GridColumn Status;
        private DevExpress.XtraEditors.LabelControl lblCompleReport;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SimpleButton btnImportExcel;
        private System.Windows.Forms.GroupBox groupBox1;
        private DevExpress.XtraEditors.SimpleButton btnRetry;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private System.Windows.Forms.NumericUpDown txtDuration;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.SimpleButton btnExcelSample;
        private DevExpress.XtraEditors.CheckEdit ckPlayAudio;
    }
}