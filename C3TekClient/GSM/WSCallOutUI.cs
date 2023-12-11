using C3TekClient.GSM.Model;
using C3TekClient.Helper;
using DevExpress.Spreadsheet;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClient.GSM
{
    public partial class WSCallOutUI : DevExpress.XtraEditors.XtraForm
    {

        bool Stop = false;
        bool StopTask = false;
        bool scheduleSMS = false;


        List<GSMCom> _gsmComList;
        public BindingList<SendCallSource> bindingListSMS = new BindingList<SendCallSource>();

        private readonly object _lockerCompleteCounter = new object();
        private readonly object _lockerSMS = new object();

        public int CompleteCounter { get; set; }

        public WSCallOutUI()
        {
            InitializeComponent();

            this.gridSMS.DataSource = bindingListSMS;
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            CompleteCounter = 0;
            removeFeatureByPermission(); 
        }
        public WSCallOutUI(List<GSMCom> gsmComUse)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this._gsmComList = gsmComUse;
            this.gridSMS.DataSource = bindingListSMS;
            this.FormClosing += WSCallOutUI_FormClosing;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            CompleteCounter = 0;
            removeFeatureByPermission(); 


        }
        public void removeFeatureByPermission()
        {
            btnImportExcel.Visible =  AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.CALL_OUT_EXCEL);
            btnExcelSample.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.CALL_OUT_EXCEL);
            ckPlayAudio.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.CALL_OUT_PLAY_AUDIO);
        }



        #region LOAD_AND_HANDLE_EVENT
        private void WSCallOutUI_Load(object sender, EventArgs e)
        {
            viewSMS.PopupMenuShowing += GridView_PopupMenuShowing;

            viewSMS.OptionsBehavior.Editable = true;
            viewSMS.OptionsSelection.EnableAppearanceFocusedCell = true;
            //viewSMS.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            viewSMS.Appearance.SelectedRow.BackColor = Color.FromArgb(0, 0, 0, 0);
            viewSMS.Appearance.FocusedRow.BackColor = Color.FromArgb(0, 0, 0, 0);


        }

        private void GridView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                DXMenuItem item = new DXMenuItem(R.S("delete_all"));
                item.Click += (o, args) =>
                {
                    this.bindingListSMS.Clear();
                    this.gridSMS.Refresh();
                };
                e.Menu.Items.Add(item);
            }
        }

        #endregion
        /// <summary>
        ///         prepare source data for text input
        /// </summary>
        public void prepareSourceData()
        {
            if (_gsmComList == null || _gsmComList.Count <= 0)
            {
                return;
            }
            bindingListSMS.Clear();


            foreach (GSMCom gsm in _gsmComList)
            {
                bindingListSMS.Add(new SendCallSource()
                {
                    RealPort = gsm.PortName,
                    AppPort = gsm.DisplayName,
                    PhoneNumber = gsm.PhoneNumber,
                    PhoneTo = txtPhoneTo.Text.Trim(),
                    State = ESendCallSource.WAITING,
                    Status = R.S("in queue")
                });
            }
            gridSMS.Refresh();
            scheduleSMS = true;

            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM1",
            //    RealPort = "COM1",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",
            //    State = ESendCallSource.WAITING
            //});
            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM2",
            //    RealPort = "COM2",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",
            //    State = ESendCallSource.WAITING
            //});
            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM3",
            //    RealPort = "COM3",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",

            //    State = ESendCallSource.WAITING
            //});    
        }
        private void WSCallOutUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            startTask();
        }

        void updateCurrentCounter()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                lblCompleReport.Text = $"{CompleteCounter.ToString()}/{bindingListSMS.Count}";
            }));
        }
        public bool HashQueueBinding()
        {
            lock (_lockerSMS)
            {
                return bindingListSMS.Any(queue => !queue.Resolved
                && queue.State == ESendCallSource.WAITING);
            }
        }
        private SendCallSource GetQueueBinding()
        {
            lock (_lockerSMS)
            {
                var _queue = bindingListSMS.Where(queue => !queue.Resolved
                   && queue.State == ESendCallSource.WAITING).FirstOrDefault();
                if (_queue != null)
                {
                    _queue.State = ESendCallSource.CALLING;
                }
                return _queue;
            }
        }
        void startTask()
        {

            int durationSecond = Convert.ToInt32(txtDuration.Value);

            int delaySecond = Convert.ToInt32(txtDelaySecond.Value);

            CompleteCounter = 0;
            updateCurrentCounter();

            pbSendProcess.Visible = true;
            pbSendProcess.Reset();
            pbSendProcess.Properties.Maximum = bindingListSMS.Count;
            pbSendProcess.Properties.Step = 1;
            pbSendProcess.EditValue = 0;

            StopTask = false;
            int epoch = 0;

            string selected = rgCallMode.Properties.Items[rgCallMode.SelectedIndex].Description;
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            if (selected == "Đồng thời" || selected =="Concurrent")
            {

                if (scheduleSMS == true)
                {
                    //neu schedule
                    foreach (SendCallSource current_source in bindingListSMS)
                    {
                        tasks.Add(new Task(() =>
                    {

                        try
                        {
                            current_source.Status = R.S("processing");
                            current_source.State = ESendCallSource.CALLING;
                            current_source.Resolved = false;
                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridSMS.RefreshDataSource();

                            }));

                            // execute sms
                            GSMCom com = _gsmComList.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                            string response = "";
                            if (ckPlayAudio.Checked  && com.ModemName == "M26")
                            {
                                response = com.MakeCallAndPlay(current_source.PhoneTo, durationSecond, true, "send.amr", 2);
                            }
                            else
                            {
                                response = com.Call(current_source.PhoneTo, durationSecond);
                            }
                            if (response.Equals("CALL_OK"))
                            {

                                current_source.State = ESendCallSource.CALLED_SUCCESS;
                                current_source.Status = R.S("success");
                                current_source.Resolved = true;
                                lock (_lockerCompleteCounter)
                                {
                                    CompleteCounter += 1;
                                    updateCurrentCounter();
                                }
                            }
                            else
                            {
                                current_source.State = ESendCallSource.CALLED_FAIL;
                                current_source.Status = R.S("busy_or_no_carrier");
                                current_source.Resolved = true;
                            }
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

                            //FAKE SIMULATE TASK
                            //Thread.Sleep(4000);
                            //current_source.Status = "Thành công";
                            //current_source.Resolved = true;
                            //current_source.State = ESendCallSource.SENT_SUCESS; 

                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridSMS.RefreshDataSource();
                            }));

                            if (Stop)
                            {
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[WSCallOutUI - CallTask - Dong thoi]");
                        }
                        Thread.Sleep(delaySecond * 1000);
                    }));
                    }
                }
                else
                {
                    foreach (var com in _gsmComList)
                    {
                        tasks.Add(new Task(() =>
                        {
                            while (HashQueueBinding())
                            {
                                try
                                {
                                    SendCallSource current_source = GetQueueBinding();
                                    current_source.Status = R.S("processing");
                                    current_source.State = ESendCallSource.CALLING;
                                    current_source.RealPort = com.PortName;
                                    current_source.AppPort = com.DisplayName;
                                    current_source.PhoneNumber = com.PhoneNumber;

                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        gridSMS.RefreshDataSource();

                                    }));
                                    string response = "";
                                    // execute sms
                                    if (ckPlayAudio.Checked && com.ModemName == "M26")
                                    {
                                        response = com.MakeCallAndPlay(current_source.PhoneTo, durationSecond, true, "send.amr", 2);
                                    }
                                    else
                                    {
                                        response = com.Call(current_source.PhoneTo, durationSecond);
                                    }

                                    if (response.Equals("CALL_OK"))
                                    {

                                        current_source.State = ESendCallSource.CALLED_SUCCESS;
                                        current_source.Status = R.S("success");
                                        current_source.Resolved = true;
                                        lock (_lockerCompleteCounter)
                                        {
                                            CompleteCounter += 1;
                                            updateCurrentCounter();
                                        }
                                    }
                                    else
                                    {
                                        current_source.State = ESendCallSource.CALLED_FAIL;
                                        current_source.Status = R.S("busy_or_no_carrier");
                                        current_source.Resolved = true;
                                    }
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

                                    //FAKE SIMULATE TASK
                                    //Thread.Sleep(4000);
                                    //current_source.Status = "Thành công";
                                    //current_source.Resolved = true;
                                    //current_source.State = ESendCallSource.SENT_SUCESS; 

                                    this.Invoke(new MethodInvoker(() =>
                                        {
                                            gridSMS.RefreshDataSource();
                                        }));

                                    if (Stop)
                                    {
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("[WSCallOutUI - CallTask - Dong thoi]");
                                }
                                Thread.Sleep(delaySecond * 1000);
                            }
                        }));
                    }
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
            else
            {
                //tuan tu
                new Task(() =>
                {
                    int gsmidx = 0;

                    while (gsmidx < _gsmComList.Count)
                    {
                        if (HashQueueBinding())
                        {
                            try
                            {
                                SendCallSource current_source = GetQueueBinding();
                                current_source.Status = R.S("processing");
                                current_source.State = ESendCallSource.CALLING;
                                current_source.RealPort = _gsmComList[gsmidx].PortName;
                                current_source.AppPort = _gsmComList[gsmidx].DisplayName;
                                current_source.PhoneNumber = _gsmComList[gsmidx].PhoneNumber;
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));


                                string response = "";
                                if (ckPlayAudio.Checked && _gsmComList[gsmidx].ModemName == "M26")
                                {
                                    response = _gsmComList[gsmidx].MakeCallAndPlay(current_source.PhoneTo, durationSecond, true, "send.amr", 2);
                                }
                                else
                                {
                                    response = _gsmComList[gsmidx].Call(current_source.PhoneTo, durationSecond);
                                }
                                // execute sms
                                // string response  = _gsmComList[gsmidx].Call(current_source.PhoneTo, durationSecond);

                                if (response.Equals("CALL_OK"))
                                {

                                    current_source.State = ESendCallSource.CALLED_SUCCESS;
                                    current_source.Status = R.S("success");
                                    current_source.Resolved = true;
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                        updateCurrentCounter();
                                    }
                                }
                                else
                                {
                                    current_source.State = ESendCallSource.CALLED_FAIL;
                                    current_source.Status = R.S("busy_or_no_carrier") ;
                                    current_source.Resolved = true;
                                }


                                //execute task send message and check reponse here
                                //fake SIMULATE
                                /*
                                Thread.Sleep(4000);
                                current_source.Status = "Thành công";
                                current_source.Resolved = true;
                                current_source.State = ESendCallSource.SENT_SUCESS;
                                */

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
                                    gridSMS.RefreshDataSource();
                                }));

                                if (Stop)
                                {
                                    return;
                                }
                                Thread.Sleep(delaySecond * 1000);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[WSMEssageSMSUI - SendSMSTask - Tuan tu]" + ex.Message);
                            }

                        }
                        else
                        {
                            break;
                        }
                        //check end of idx => reset to 0

                        if (gsmidx >= _gsmComList.Count - 1)
                        {
                            gsmidx = 0;
                        }
                        else { gsmidx++; }
                    }
                }).Start();
            }

        }




        private void btnPrepareData_Click(object sender, EventArgs e)
        {
            prepareSourceData();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            IReadOnlyList<SendCallSource> filterList = bindingListSMS.Where(s => s.State != ESendCallSource.CALLED_FAIL).ToList();
            foreach (SendCallSource item in filterList)
            {
                bindingListSMS.Remove(item);
            }
            foreach (SendCallSource item in bindingListSMS)
            {
                item.Resolved = false;
                item.State = ESendCallSource.WAITING;
                item.Status = R.S("in_queue");
            }

            this.gridSMS.Refresh();
            if (bindingListSMS.Count <= 0)
            {
                MessageBox.Show(R.S("success"));
            }
            startTask();

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            importExcel();
        }
        private void importExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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

                        this.Invoke(new MethodInvoker(() =>
                        {
                            bindingListSMS.Clear();
                            gridSMS.Refresh();
                            gridSMS.RefreshDataSource();
                        }));

                        if (workbook.Worksheets.Count > 0)
                        {

                            Worksheet worksheet = workbook.Worksheets[0];
                            RowCollection rows = worksheet.Rows;
                            CellRange usedRange = worksheet.GetUsedRange();
                            int count_error_record = 0;
                            for (int i = 0; i < usedRange.RowCount; i++)
                            {
                                string phoneNumberTo = (worksheet[i, 0].Value != null) ? worksheet[i, 0].Value.ToString() : "";
                                string count_str = (worksheet[i, 1].Value != null) ? worksheet[i, 1].Value.ToString() : "";
                                int count = 0;
                                if (Int32.TryParse(count_str, out count))
                                {
                                    //ok giới hạn 100 tin 1 sim thôi
                                    if (!string.IsNullOrEmpty(phoneNumberTo) && count >= 1 && count <= 100)
                                    {
                                        for (int k = 0; k < count; k++)
                                        {
                                            bindingListSMS.Add(new SendCallSource()
                                            {
                                                PhoneTo = phoneNumberTo,
                                                State = ESendCallSource.WAITING,
                                                Status = R.S("in_queue")
                                            });
                                        }
                                    }
                                    else
                                    {
                                        count_error_record++;
                                    }
                                }
                                else
                                {
                                    //process not ok 
                                    count_error_record++;
                                }


                                this.Invoke(new MethodInvoker(() =>
                                {
                                    lblCompleReport.Text = $"0/{bindingListSMS.Count.ToString() ?? ""}";
                                }));

                            }
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
                        gridSMS.Refresh();
                        gridSMS.RefreshDataSource();
                    }));
                    scheduleSMS = false;

                }).Start();

            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridSMS.ExportToXlsx(svd.FileName);
            }
        }

        private void btnExcelSample_Click(object sender, EventArgs e)
        {
            FileHelper.openExcelAndReleaseFile(PathHelper.getPathExecute() + "/mau_call.xlsx");

        }

        private void labelControl7_Click(object sender, EventArgs e)
        {

        }

        private void txtDelaySecond_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}