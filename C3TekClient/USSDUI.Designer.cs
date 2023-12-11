namespace C3TekClient
{
    partial class USSDUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(USSDUI));
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.repositoryItemButtonEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.waitScreen = new DevExpress.XtraWaitForm.ProgressPanel();
            this.txtScreen = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtUSSD = new DevExpress.XtraEditors.TextEdit();
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnNum11 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum0 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum10 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum9 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum8 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum7 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum6 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum5 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum4 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum3 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum2 = new DevExpress.XtraEditors.SimpleButton();
            this.btnNum1 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUSSD.Properties)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // repositoryItemTextEdit1
            // 
            resources.ApplyResources(this.repositoryItemTextEdit1, "repositoryItemTextEdit1");
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // repositoryItemButtonEdit1
            // 
            resources.ApplyResources(this.repositoryItemButtonEdit1, "repositoryItemButtonEdit1");
            this.repositoryItemButtonEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repositoryItemButtonEdit1.Name = "repositoryItemButtonEdit1";
            // 
            // barStaticItem1
            // 
            resources.ApplyResources(this.barStaticItem1, "barStaticItem1");
            this.barStaticItem1.Id = 3;
            this.barStaticItem1.ImageOptions.ImageIndex = ((int)(resources.GetObject("barStaticItem1.ImageOptions.ImageIndex")));
            this.barStaticItem1.ImageOptions.LargeImageIndex = ((int)(resources.GetObject("barStaticItem1.ImageOptions.LargeImageIndex")));
            this.barStaticItem1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barStaticItem1.ImageOptions.SvgImage")));
            this.barStaticItem1.Name = "barStaticItem1";
            // 
            // ribbonControl1
            // 
            resources.ApplyResources(this.ribbonControl1, "ribbonControl1");
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.ExpandCollapseItem.ImageOptions.ImageIndex = ((int)(resources.GetObject("ribbonControl1.ExpandCollapseItem.ImageOptions.ImageIndex")));
            this.ribbonControl1.ExpandCollapseItem.ImageOptions.LargeImageIndex = ((int)(resources.GetObject("ribbonControl1.ExpandCollapseItem.ImageOptions.LargeImageIndex")));
            this.ribbonControl1.ExpandCollapseItem.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("ribbonControl1.ExpandCollapseItem.ImageOptions.SvgImage")));
            this.ribbonControl1.ExpandCollapseItem.SearchTags = resources.GetString("ribbonControl1.ExpandCollapseItem.SearchTags");
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.ribbonControl1.SearchEditItem,
            this.barStaticItem1});
            this.ribbonControl1.MaxItemId = 19;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.OptionsMenuMinWidth = 385;
            this.ribbonControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1,
            this.repositoryItemButtonEdit1});
            // 
            // panelControl1
            // 
            resources.ApplyResources(this.panelControl1, "panelControl1");
            this.panelControl1.Controls.Add(this.waitScreen);
            this.panelControl1.Controls.Add(this.txtScreen);
            this.panelControl1.Controls.Add(this.panel1);
            this.panelControl1.Controls.Add(this.tableLayoutPanel1);
            this.panelControl1.Name = "panelControl1";
            // 
            // waitScreen
            // 
            resources.ApplyResources(this.waitScreen, "waitScreen");
            this.waitScreen.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.waitScreen.Appearance.Options.UseBackColor = true;
            this.waitScreen.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.waitScreen.ContentAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.waitScreen.FrameInterval = 400;
            this.waitScreen.Name = "waitScreen";
            this.waitScreen.WaitAnimationType = DevExpress.Utils.Animation.WaitingAnimatorType.Ring;
            // 
            // txtScreen
            // 
            resources.ApplyResources(this.txtScreen, "txtScreen");
            this.txtScreen.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScreen.Name = "txtScreen";
            this.txtScreen.ReadOnly = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.txtUSSD);
            this.panel1.Controls.Add(this.btnSubmit);
            this.panel1.Name = "panel1";
            // 
            // txtUSSD
            // 
            resources.ApplyResources(this.txtUSSD, "txtUSSD");
            this.txtUSSD.Name = "txtUSSD";
            this.txtUSSD.Properties.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("txtUSSD.Properties.Appearance.Font")));
            this.txtUSSD.Properties.Appearance.Options.UseFont = true;
            // 
            // btnSubmit
            // 
            resources.ApplyResources(this.btnSubmit, "btnSubmit");
            this.btnSubmit.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSubmit.ImageOptions.Image")));
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.btnNum11, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum0, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum10, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum9, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum8, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum7, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum6, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnNum5, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnNum4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnNum3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnNum2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnNum1, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // btnNum11
            // 
            resources.ApplyResources(this.btnNum11, "btnNum11");
            this.btnNum11.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum11.Appearance.Font")));
            this.btnNum11.Appearance.Options.UseFont = true;
            this.btnNum11.Name = "btnNum11";
            this.btnNum11.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum0
            // 
            resources.ApplyResources(this.btnNum0, "btnNum0");
            this.btnNum0.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum0.Appearance.Font")));
            this.btnNum0.Appearance.Options.UseFont = true;
            this.btnNum0.Name = "btnNum0";
            this.btnNum0.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum10
            // 
            resources.ApplyResources(this.btnNum10, "btnNum10");
            this.btnNum10.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum10.Appearance.Font")));
            this.btnNum10.Appearance.Options.UseFont = true;
            this.btnNum10.Name = "btnNum10";
            this.btnNum10.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum9
            // 
            resources.ApplyResources(this.btnNum9, "btnNum9");
            this.btnNum9.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum9.Appearance.Font")));
            this.btnNum9.Appearance.Options.UseFont = true;
            this.btnNum9.Name = "btnNum9";
            this.btnNum9.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum8
            // 
            resources.ApplyResources(this.btnNum8, "btnNum8");
            this.btnNum8.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum8.Appearance.Font")));
            this.btnNum8.Appearance.Options.UseFont = true;
            this.btnNum8.Name = "btnNum8";
            this.btnNum8.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum7
            // 
            resources.ApplyResources(this.btnNum7, "btnNum7");
            this.btnNum7.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum7.Appearance.Font")));
            this.btnNum7.Appearance.Options.UseFont = true;
            this.btnNum7.Name = "btnNum7";
            this.btnNum7.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum6
            // 
            resources.ApplyResources(this.btnNum6, "btnNum6");
            this.btnNum6.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum6.Appearance.Font")));
            this.btnNum6.Appearance.Options.UseFont = true;
            this.btnNum6.Name = "btnNum6";
            this.btnNum6.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum5
            // 
            resources.ApplyResources(this.btnNum5, "btnNum5");
            this.btnNum5.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum5.Appearance.Font")));
            this.btnNum5.Appearance.Options.UseFont = true;
            this.btnNum5.Name = "btnNum5";
            this.btnNum5.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum4
            // 
            resources.ApplyResources(this.btnNum4, "btnNum4");
            this.btnNum4.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum4.Appearance.Font")));
            this.btnNum4.Appearance.Options.UseFont = true;
            this.btnNum4.Name = "btnNum4";
            this.btnNum4.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum3
            // 
            resources.ApplyResources(this.btnNum3, "btnNum3");
            this.btnNum3.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum3.Appearance.Font")));
            this.btnNum3.Appearance.Options.UseFont = true;
            this.btnNum3.Name = "btnNum3";
            this.btnNum3.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum2
            // 
            resources.ApplyResources(this.btnNum2, "btnNum2");
            this.btnNum2.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum2.Appearance.Font")));
            this.btnNum2.Appearance.Options.UseFont = true;
            this.btnNum2.Name = "btnNum2";
            this.btnNum2.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // btnNum1
            // 
            resources.ApplyResources(this.btnNum1, "btnNum1");
            this.btnNum1.Appearance.Font = ((System.Drawing.Font)(resources.GetObject("btnNum1.Appearance.Font")));
            this.btnNum1.Appearance.Options.UseFont = true;
            this.btnNum1.Name = "btnNum1";
            this.btnNum1.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            // 
            // USSDUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.ribbonControl1);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("USSDUI.IconOptions.Icon")));
            this.Name = "USSDUI";
            this.Ribbon = this.ribbonControl1;
            this.RibbonVisibility = DevExpress.XtraBars.Ribbon.RibbonVisibility.Hidden;
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtUSSD.Properties)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEdit1;
        private DevExpress.XtraBars.BarStaticItem barStaticItem1;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraWaitForm.ProgressPanel waitScreen;
        private System.Windows.Forms.RichTextBox txtScreen;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraEditors.TextEdit txtUSSD;
        private DevExpress.XtraEditors.SimpleButton btnSubmit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DevExpress.XtraEditors.SimpleButton btnNum11;
        private DevExpress.XtraEditors.SimpleButton btnNum0;
        private DevExpress.XtraEditors.SimpleButton btnNum10;
        private DevExpress.XtraEditors.SimpleButton btnNum9;
        private DevExpress.XtraEditors.SimpleButton btnNum8;
        private DevExpress.XtraEditors.SimpleButton btnNum7;
        private DevExpress.XtraEditors.SimpleButton btnNum6;
        private DevExpress.XtraEditors.SimpleButton btnNum5;
        private DevExpress.XtraEditors.SimpleButton btnNum4;
        private DevExpress.XtraEditors.SimpleButton btnNum3;
        private DevExpress.XtraEditors.SimpleButton btnNum2;
        private DevExpress.XtraEditors.SimpleButton btnNum1;
    }
}

