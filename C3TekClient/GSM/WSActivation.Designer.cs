
namespace C3TekClient.GSM
{
    partial class WSActivation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WSActivation));
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.numTimeActivate = new System.Windows.Forms.NumericUpDown();
            this.waitScreen = new DevExpress.XtraWaitForm.ProgressPanel();
            this.btnRetry = new DevExpress.XtraEditors.SimpleButton();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            this.cbNhaMang = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtDtmf2 = new DevExpress.XtraEditors.TextEdit();
            this.txtDtmf1 = new DevExpress.XtraEditors.TextEdit();
            this.txtNumber = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.viewGSM = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn19 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeActivate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbNhaMang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDtmf2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDtmf1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNumber.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewGSM)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.labelControl5);
            this.panelControl1.Controls.Add(this.labelControl4);
            this.panelControl1.Controls.Add(this.numTimeActivate);
            this.panelControl1.Controls.Add(this.waitScreen);
            this.panelControl1.Controls.Add(this.btnRetry);
            this.panelControl1.Controls.Add(this.btnStart);
            this.panelControl1.Controls.Add(this.cbNhaMang);
            this.panelControl1.Controls.Add(this.txtDtmf2);
            this.panelControl1.Controls.Add(this.txtDtmf1);
            this.panelControl1.Controls.Add(this.txtNumber);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.labelControl2);
            this.panelControl1.Controls.Add(this.labelControl1);
            resources.ApplyResources(this.panelControl1, "panelControl1");
            this.panelControl1.Name = "panelControl1";
            // 
            // labelControl5
            // 
            resources.ApplyResources(this.labelControl5, "labelControl5");
            this.labelControl5.Name = "labelControl5";
            // 
            // labelControl4
            // 
            resources.ApplyResources(this.labelControl4, "labelControl4");
            this.labelControl4.Name = "labelControl4";
            // 
            // numTimeActivate
            // 
            resources.ApplyResources(this.numTimeActivate, "numTimeActivate");
            this.numTimeActivate.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numTimeActivate.Name = "numTimeActivate";
            this.numTimeActivate.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // waitScreen
            // 
            this.waitScreen.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.waitScreen.Appearance.Options.UseBackColor = true;
            resources.ApplyResources(this.waitScreen, "waitScreen");
            this.waitScreen.Name = "waitScreen";
            // 
            // btnRetry
            // 
            this.btnRetry.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnRetry.ImageOptions.Image")));
            resources.ApplyResources(this.btnRetry, "btnRetry");
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnStart
            // 
            this.btnStart.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnStart.ImageOptions.Image")));
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.Name = "btnStart";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cbNhaMang
            // 
            resources.ApplyResources(this.cbNhaMang, "cbNhaMang");
            this.cbNhaMang.Name = "cbNhaMang";
            this.cbNhaMang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(((DevExpress.XtraEditors.Controls.ButtonPredefines)(resources.GetObject("cbNhaMang.Properties.Buttons"))))});
            this.cbNhaMang.Properties.Items.AddRange(new object[] {
            resources.GetString("cbNhaMang.Properties.Items"),
            resources.GetString("cbNhaMang.Properties.Items1"),
            resources.GetString("cbNhaMang.Properties.Items2"),
            resources.GetString("cbNhaMang.Properties.Items3"),
            resources.GetString("cbNhaMang.Properties.Items4")});
            this.cbNhaMang.SelectedIndexChanged += new System.EventHandler(this.cbNhaMang_SelectedIndexChanged);
            // 
            // txtDtmf2
            // 
            resources.ApplyResources(this.txtDtmf2, "txtDtmf2");
            this.txtDtmf2.Name = "txtDtmf2";
            // 
            // txtDtmf1
            // 
            resources.ApplyResources(this.txtDtmf1, "txtDtmf1");
            this.txtDtmf1.Name = "txtDtmf1";
            // 
            // txtNumber
            // 
            resources.ApplyResources(this.txtNumber, "txtNumber");
            this.txtNumber.Name = "txtNumber";
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
            // labelControl1
            // 
            resources.ApplyResources(this.labelControl1, "labelControl1");
            this.labelControl1.Name = "labelControl1";
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.gridControl1);
            resources.ApplyResources(this.panelControl2, "panelControl2");
            this.panelControl2.Name = "panelControl2";
            // 
            // gridControl1
            // 
            resources.ApplyResources(this.gridControl1, "gridControl1");
            this.gridControl1.EmbeddedNavigator.Margin = ((System.Windows.Forms.Padding)(resources.GetObject("gridControl1.EmbeddedNavigator.Margin")));
            this.gridControl1.MainView = this.viewGSM;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewGSM});
            // 
            // viewGSM
            // 
            this.viewGSM.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.viewGSM.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.viewGSM.ColumnPanelRowHeight = 49;
            this.viewGSM.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn19,
            this.gridColumn4,
            this.gridColumn1,
            this.gridColumn2});
            this.viewGSM.DetailHeight = 431;
            this.viewGSM.GridControl = this.gridControl1;
            this.viewGSM.Name = "viewGSM";
            this.viewGSM.OptionsClipboard.CopyColumnHeaders = DevExpress.Utils.DefaultBoolean.False;
            this.viewGSM.OptionsSelection.MultiSelect = true;
            this.viewGSM.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
            this.viewGSM.OptionsView.ShowAutoFilterRow = true;
            this.viewGSM.OptionsView.ShowFooter = true;
            this.viewGSM.OptionsView.ShowGroupPanel = false;
            this.viewGSM.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            // 
            // gridColumn19
            // 
            resources.ApplyResources(this.gridColumn19, "gridColumn19");
            this.gridColumn19.FieldName = "DisplayName";
            this.gridColumn19.MaxWidth = 70;
            this.gridColumn19.MinWidth = 70;
            this.gridColumn19.Name = "gridColumn19";
            this.gridColumn19.OptionsColumn.AllowEdit = false;
            this.gridColumn19.OptionsColumn.ReadOnly = true;
            this.gridColumn19.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(((DevExpress.Data.SummaryItemType)(resources.GetObject("gridColumn19.Summary"))), resources.GetString("gridColumn19.Summary1"), resources.GetString("gridColumn19.Summary2"))});
            // 
            // gridColumn4
            // 
            resources.ApplyResources(this.gridColumn4, "gridColumn4");
            this.gridColumn4.FieldName = "PhoneNumber";
            this.gridColumn4.MaxWidth = 93;
            this.gridColumn4.MinWidth = 93;
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.OptionsColumn.AllowEdit = false;
            this.gridColumn4.OptionsColumn.ReadOnly = true;
            // 
            // gridColumn1
            // 
            resources.ApplyResources(this.gridColumn1, "gridColumn1");
            this.gridColumn1.FieldName = "LastUSSDCommand";
            this.gridColumn1.MinWidth = 70;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.OptionsColumn.AllowEdit = false;
            this.gridColumn1.OptionsColumn.ReadOnly = true;
            // 
            // gridColumn2
            // 
            resources.ApplyResources(this.gridColumn2, "gridColumn2");
            this.gridColumn2.FieldName = "LastUSSDResult";
            this.gridColumn2.MinWidth = 23;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.OptionsColumn.AllowEdit = false;
            this.gridColumn2.OptionsColumn.ReadOnly = true;
            // 
            // WSActivation
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.panelControl1);
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("WSActivation.IconOptions.SvgImage")));
            this.Name = "WSActivation";
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeActivate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbNhaMang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDtmf2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDtmf1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNumber.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewGSM)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView viewGSM;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn19;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txtNumber;
        private DevExpress.XtraEditors.TextEdit txtDtmf2;
        private DevExpress.XtraEditors.TextEdit txtDtmf1;
        private DevExpress.XtraEditors.ComboBoxEdit cbNhaMang;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private DevExpress.XtraEditors.SimpleButton btnRetry;
        private DevExpress.XtraWaitForm.ProgressPanel waitScreen;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private System.Windows.Forms.NumericUpDown numTimeActivate;
        private DevExpress.XtraEditors.LabelControl labelControl5;
    }
}