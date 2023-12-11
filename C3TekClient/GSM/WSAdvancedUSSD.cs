using C3TekClient.Core.JobModel;
using C3TekClient.GSM.Model;
using C3TekClient.Helper;
using DevExpress.Spreadsheet;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClient.GSM
{
    public partial class WSAdvancedUSSD : DevExpress.XtraEditors.XtraForm
    {
        bool Stop = false;

        List<GSMCom> COMS = new List<GSMCom>();
        IDictionary<string, GSMCom> _dictCom = new Dictionary<string, GSMCom>();

        AutoCompleteStringCollection comCollection = new AutoCompleteStringCollection();

        BindingList<CustomAdvanceUSSDBinding> listBinding = new BindingList<CustomAdvanceUSSDBinding>();
        public WSAdvancedUSSD()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }
        public WSAdvancedUSSD(List<GSMCom> coms)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            COMS = coms;

            buildSuggestSource();

            this.Load += WSAdvancedUSSD_Load;
            listBinding = new BindingList<CustomAdvanceUSSDBinding>();
            this.FormClosed += WSAdvancedUSSD_FormClosed;
            lblNumberCOM.Text = coms.Count.ToString();
            viewGSM.PopupMenuShowing += gridView1_PopupMenuShowing;
        }

        private void buildSuggestSource()
        {
            comCollection.Clear();
            _dictCom = new Dictionary<string, GSMCom>();
            foreach (var com in COMS)
            {
                _dictCom.Add(new KeyValuePair<string, GSMCom>(com.DisplayName, com));
                comCollection.Add(com.DisplayName);
            }
        }

        #region ContextMenuGridView


        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                // Delete existing menu items, if any.
                e.Menu.Items.Clear();
                // Add the Rows submenu with the 'Delete Row' command
                e.Menu.Items.Add(CreateSubMenuRows(view, rowHandle));
                // Add the 'Cell Merging' check menu item.
                DXMenuItem item = CreateMenuItemCellMerging(view, rowHandle);
                item.BeginGroup = true;
                e.Menu.Items.Add(item);
            }
        }

        DXMenuItem CreateSubMenuRows(GridView view, int rowHandle)
        {
            DXSubMenuItem subMenu = new DXSubMenuItem("Rows");
            string deleteRowsCommandCaption;
            if (view.IsGroupRow(rowHandle))
                deleteRowsCommandCaption = "&Delete rows in this group";
            else
                deleteRowsCommandCaption = "&Delete this row";
            DXMenuItem menuItemDeleteRow = new DXMenuItem(deleteRowsCommandCaption, new EventHandler(OnDeleteRowClick));
            menuItemDeleteRow.Tag = new RowInfo(view, rowHandle);
            menuItemDeleteRow.Enabled = view.IsDataRow(rowHandle) || view.IsGroupRow(rowHandle);
            subMenu.Items.Add(menuItemDeleteRow);
            return subMenu;
        }

        DXMenuCheckItem CreateMenuItemCellMerging(GridView view, int rowHandle)
        {
            DXMenuCheckItem checkItem = new DXMenuCheckItem("Cell &Merging",
              view.OptionsView.AllowCellMerge, null, new EventHandler(OnCellMergingClick));
            checkItem.Tag = new RowInfo(view, rowHandle);
            //checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        void OnDeleteRowClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            RowInfo ri = menuItem.Tag as RowInfo;
            if (ri != null)
            {
                string message = menuItem.Caption.Replace("&", "");
                if (XtraMessageBox.Show(message + " ?", "Confirm operation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                ri.View.DeleteRow(ri.RowHandle);
            }
        }

        void OnCellMergingClick(object sender, EventArgs e)
        {
            DXMenuCheckItem item = sender as DXMenuCheckItem;
            RowInfo info = item.Tag as RowInfo;
            info.View.OptionsView.AllowCellMerge = item.Checked;
        }

        class RowInfo
        {
            public RowInfo(GridView view, int rowHandle)
            {
                this.RowHandle = rowHandle;
                this.View = view;
            }
            public GridView View;
            public int RowHandle;
        }

        void setupViewContext()
        {
            viewGSM.PopupMenuShowing += GridView_PopupMenuShowing;
            viewGSM.Appearance.SelectedRow.BackColor = Color.FromArgb(0, 0, 0, 0);
            viewGSM.Appearance.FocusedRow.BackColor = Color.FromArgb(0, 0, 0, 0);
        }
        private void clearAllGridView()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                this.listBinding.Clear();
                this.gridControl1.Refresh();
            }));
            foreach (var com in COMS)
            {
                com.GetUSSDJobWorker().ClearAllJob();
            }

        }

        private void GridView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                DXMenuItem item = new DXMenuItem(R.S("delete_all"));
                item.Click += (o, args) =>
                {
                    if(comfirmYNDialog(R.S("comfirm"), R.S("question_comfirm_delete_all")) == true)
                    {
                        clearAllGridView();
                    }
                };

                DXMenuItem insertMenuItem = new DXMenuItem(R.S("add_more_ussd_after"));
                insertMenuItem.Click += MenuItemClickEvent;
                insertMenuItem.Tag = "DXMI_ADD_ITEM";

                e.Menu.Items.Add(insertMenuItem);
                e.Menu.Items.Add(item);
            }
        }

        private void MenuItemClickEvent(object sender, EventArgs e)
        {
            MessageBox.Show(((DXMenuItem)sender).Tag.ToString());
        }
        #endregion
        private void WSAdvancedUSSD_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);
        }

        private void WSAdvancedUSSD_Load(object sender, EventArgs e)
        {
            foreach (var com in COMS)
            {
                com.LastUSSDCommand = string.Empty;
                com.LastUSSDResult = string.Empty;
            }
            gridControl1.DataSource = listBinding;
            initSuggestEdit();
           // setupViewContext();
        }
       
        private void initSuggestEdit()
        {    
            viewGSM.ShownEditor += ViewGSM_ShownEditor;
        }
        private void ViewGSM_ShownEditor(object sender, EventArgs e)
        {

            TextEdit currentEditor = (sender as GridView).ActiveEditor as TextEdit;
            if (currentEditor != null)
            {
                currentEditor.MaskBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                currentEditor.MaskBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                currentEditor.MaskBox.AutoCompleteCustomSource = comCollection;
            }
        }

       
        private void btnSinglePrepareData_Click_1(object sender, EventArgs e)
        {
            prepareSourceData();
        }
        /// <summary>
        /// Clean and set status not resolved 
        /// </summary>
        private void prepareJobAndPushJobWorker()
        {
            foreach(GSMCom com in COMS)
            {
                com.GetUSSDJobWorker().ClearAllJob();
            }
            int config_retry = Convert.ToInt32(Math.Round(txtNumberRetry.Value, 0));
            foreach (CustomAdvanceUSSDBinding record in listBinding)
            {
                record.Status = R.S("processing");
                record.USSDResult = "";
                if (_dictCom.ContainsKey(record.AppPort)){
                    USSDJobInfo ussdJOB = new USSDJobInfo(-1, record, "USSD-NAME", "USSD", new USSDJobBody(record.USSD), config_retry);
                    ussdJOB.LogHook = logUSSD;
                    GSMCom _com = _dictCom[record.AppPort];
                    _com.GetUSSDJobWorker().PushJobInfo(ussdJOB);
                }
                else
                {
                    record.Status = R.S("no_found_port");
                }
                //USSDJobInfo ussdJob = new USSDJobInfo(bitem, "USSD-TASK", "USSD", new USSDJobBody(bitem.USSD));
                //ussdJob.LogHook = logUSSD;
            }
            gridControl1.Refresh();
        }
        private void prepareSourceData(bool removeAllJobWorker = true)
        {

            listBinding.Clear();
            foreach (var com in COMS)
            {
                CustomAdvanceUSSDBinding bitem = new CustomAdvanceUSSDBinding()
                {
                    AppPort = com.DisplayName,
                    //RealPort = com.PortName,
                    PhoneNumber = com.PhoneNumber,
                    DelayTime = Convert.ToInt32(txtSingleDelay.Text),
                    Status = R.S("processing"),
                    USSD = txtSingleUSSD.Text,
                    USSDResult = ""
                };
                listBinding.Add(bitem);
                if (removeAllJobWorker)
                {
                    com.GetUSSDJobWorker().ClearAllJob();
                }
                //USSDJobInfo ussdJob = new USSDJobInfo(bitem, "USSD-TASK", "USSD", new USSDJobBody(bitem.USSD));
                //ussdJob.LogHook = logUSSD;
                //com.GetUSSDJobWorker().PushJobInfo(ussdJob);
            }
            gridControl1.Refresh();
        }
        private void logUSSD(string msg)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                   
                    txtLogUSSD.AppendText("\n" + msg);
                    txtLogUSSD.SelectionStart = txtLogUSSD.Text.Length;
                    txtLogUSSD.ScrollToCaret();

                }
                catch { Console.WriteLine("Error"); }
            }));
        }
        private bool comfirmYNDialog(string title, string msg)
        {
            if (XtraMessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return false;
            }
            else
            {
                return true; 
            }

        }
        
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (comfirmYNDialog(R.S("comfirm"), R.S("comfirm_reset_all_result")) == false) {
                return;
            }
            prepareJobAndPushJobWorker();

            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            pbSendProcess.Visible = true;
            pbSendProcess.Reset();
            pbSendProcess.Properties.Maximum = listBinding.Count;
            pbSendProcess.Properties.Step = 1;
            pbSendProcess.EditValue = 0;

            foreach (var com in COMS)
            {
                tasks.Add(new Task(() =>
                {
                    try
                    {
                        while (com.GetUSSDJobWorker().HasJobQueue())
                        {
                            try
                            {
                                com.GetUSSDJobWorker().ConsumeNextJob();
                                lock (pbSendProcess)
                                {
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        try
                                        {
                                            pbSendProcess.PerformStep();
                                        }
                                        catch { }
                                    }));
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridControl1.RefreshDataSource();
                                }));
                                //sleep here
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[WsAdvancedUSSD Exception] : inner task  {ex.Message}");
                            }

                            if (Stop)
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WSAdvancedUSSD Exception] : {com.PortName}  - {ex.Message}");
                    }
                }
                ));
            }

            new Task(() =>
            {
                //epoch process
                //step1. preparing the data
                try
                {
                    tasks.ForEach(task => task.Start());
                    System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), 60);
                    Console.WriteLine("Log ok");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception on execute task" + ex.Message);
                }

            }).Start();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridControl1.ExportToXlsx(svd.FileName);
            }
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose file";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.InitialDirectory = PathHelper.getPathExecute() ;
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Excel File (*.XLXS)|*.XLSX";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                new Task(() =>
                {
                    try
                    {
                        Workbook workbook = new Workbook();
                        // Load a workbook from the file.
                        workbook.LoadDocument(openFileDialog.FileName, DocumentFormat.Xlsx);

                        //no clear
                        if (workbook.Worksheets.Count > 0)
                        {

                            Worksheet worksheet = workbook.Worksheets[0];
                            RowCollection rows = worksheet.Rows;
                            CellRange usedRange = worksheet.GetUsedRange();
                            int count_error_record = 0;
                            for (int i = 0; i < usedRange.RowCount; i++)
                            {
                                //

                                string DisplayName = (worksheet[i, 0].Value != null) ? worksheet[i, 0].Value.ToString() : "";
                                string USSD = (worksheet[i, 1].Value != null) ? worksheet[i, 1].Value.ToString() : "";
                                string TimeDelay = (worksheet[i, 2].Value != null) ? worksheet[i, 2].Value.ToString() : "0";
                                int delay = 0;
                                MessageBox.Show(DisplayName);
                                MessageBox.Show(USSD);
                                MessageBox.Show(TimeDelay);
                                if (Int32.TryParse(TimeDelay, out delay))
                                {
                                    MessageBox.Show("it used to be here");
                                    //ok giới hạn 100 tin 1 sim thôi
                                    if (DisplayName == "")
                                    {
                                        DisplayName = this.COMS[i % this.COMS.Count].DisplayName;
                                    }
                                    else
                                    {
                                        if (!DisplayName.StartsWith(R.S("custom_prefix_port")))
                                        {
                                            string NumbericVal = new String(DisplayName.Where(Char.IsDigit).ToArray());
                                            DisplayName = $"{R.S("custom_prefix_port")} {NumbericVal}";
                                        }
                                        if (!_dictCom.ContainsKey(DisplayName))
                                        {
                                            count_error_record++;
                                            MessageBox.Show("damn,it is this place");
                                            continue;
                                        }
                                        if (delay < 0)
                                        {
                                            delay = 0;
                                        }

                                        if (string.IsNullOrEmpty(USSD))
                                        {
                                            count_error_record++;
                                            MessageBox.Show("there is error in this place");
                                            continue;
                                        }
                                        else
                                        {
                                            MessageBox.Show("did stay here");
                                            CustomAdvanceUSSDBinding bitem = new CustomAdvanceUSSDBinding()
                                            {
                                                AppPort = DisplayName,
                                                //RealPort = com.PortName,
                                                PhoneNumber = _dictCom[DisplayName].PhoneNumber,
                                                DelayTime = delay,
                                                Status = R.S("processing"),
                                                USSD = USSD,
                                                USSDResult = ""
                                            };
                                            listBinding.Add(bitem);
                                        }
                                    }

                                }
                                else
                                {
                                    //process not ok 
                                    MessageBox.Show("error here");
                                    count_error_record++;
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridControl1.RefreshDataSource();
                                    gridControl1.Refresh();
                                }));
                            }
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show("fuckk");
                                MessageBox.Show(this, string.Format(R.S("import_success"), count_error_record.ToString()));
                            }));
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show(this, R.S("check_excel_file"));
                            }));
                        }

                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, R.S("error_read_another_soft"));
                        }));
                    }
                    this.Invoke(new MethodInvoker(() =>
                    {
                        gridControl1.Refresh();
                        gridControl1.RefreshDataSource();
                    }));
                }).Start();
            }
        }

        private void btnExcelSample_Click(object sender, EventArgs e)
        {
            FileHelper.openExcelAndReleaseFile(PathHelper.getPathExecute() + "/mau_ussd.xlsx");

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                this.gridControl1.ExportToXlsx(svd.FileName);
            }
        }

        private void btnImportExcel_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose file";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.InitialDirectory = PathHelper.getPathExecute();
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Excel File (*.XLXS)|*.XLSX";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                new Task(() =>
                {
                    try
                    {
                        Workbook workbook = new Workbook();
                        // Load a workbook from the file.
                        workbook.LoadDocument(openFileDialog.FileName, DocumentFormat.Xlsx);

                        //no clear
                        if (workbook.Worksheets.Count > 0)
                        {

                            Worksheet worksheet = workbook.Worksheets[0];
                            RowCollection rows = worksheet.Rows;
                            CellRange usedRange = worksheet.GetUsedRange();
                            int count_error_record = 0;
                            for (int i = 1; i < usedRange.RowCount; i++)
                            {
                                //
                                string DisplayName = (worksheet[i, 0].Value != null) ? worksheet[i, 0].Value.ToString() : "";
                                string USSD = (worksheet[i, 1].Value != null) ? worksheet[i, 1].Value.ToString() : "";
                                string TimeDelay = "0";
                                int delay = 0;
                                if (Int32.TryParse(TimeDelay, out delay))
                                {
                                    //ok giới hạn 100 tin 1 sim thôi
                                    if (DisplayName == "")
                                    {
                                        DisplayName = this.COMS[i % this.COMS.Count].DisplayName;
                                    }
                                    else
                                    {
                                        /*if (!DisplayName.StartsWith(R.S("custom_prefix_port")))
                                        {
                                            string NumbericVal = new String(DisplayName.Where(Char.IsDigit).ToArray());
                                            MessageBox.Show("numerical:" + NumbericVal);
                                            DisplayName = $"{R.S("custom_prefix_port")} {NumbericVal}";
                                            MessageBox.Show("after change value:" + DisplayName);
                                        }*/
                                        if (!_dictCom.ContainsKey(DisplayName))
                                        {
                                            /*  MessageBox.Show("key in excel file:" + DisplayName);
                                              MessageBox.Show("ussd in excel file:" + USSD);
                                             foreach(string key in _dictCom.Keys)
                                              {
                                                  MessageBox.Show(key);
                                              }*/
                                            count_error_record++;
                                            continue;
                                        }
                                        if (delay < 0)
                                        {
                                            delay = 0;
                                        }

                                        if (string.IsNullOrEmpty(USSD))
                                        {
                                            count_error_record++;
                                            continue;
                                        }
                                        else
                                        {
                                            CustomAdvanceUSSDBinding bitem = new CustomAdvanceUSSDBinding()
                                            {   
                                                AppPort = DisplayName,
                                                //RealPort = com.PortName,
                                                PhoneNumber = _dictCom[DisplayName].PhoneNumber,
                                                DelayTime = delay,
                                                Status = R.S("processing"),
                                                USSD = USSD,
                                                USSDResult = ""
                                            };
                                            listBinding.Add(bitem);
                                        }
                                    }

                                }
                                else
                                {
                                    //process not ok 
                                    count_error_record++;
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridControl1.RefreshDataSource();
                                    gridControl1.Refresh();
                                }));
                            }
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show(this, string.Format(R.S("import_success"), count_error_record.ToString()));
                            }));
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show(this, R.S("check_excel_file"));
                            }));
                        }

                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, R.S("error_read_another_soft"));
                        }));
                    }
                    this.Invoke(new MethodInvoker(() =>
                    {
                        gridControl1.Refresh();
                        gridControl1.RefreshDataSource();
                    }));
                }).Start();
            }
        }

        private void btnExcelSample_Click_1(object sender, EventArgs e)
        {
            FileHelper.openExcelAndReleaseFile(PathHelper.getPathExecute() + "/mau_ussd.xlsx");
        }
        private void simpleButton1_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                this.gridControl1.ExportToXlsx(svd.FileName);
            }
        }

        private void groupControl6_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}