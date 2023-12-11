using C3TekClient.GSM;
using C3TekClient.GSM.Model;
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

namespace C3TekClient
{
    public partial class USSDTopup : DevExpress.XtraEditors.XtraForm
    {
        bool Stop = false;

        List<GSMCom> COMS = new List<GSMCom>();
        BindingList<TopupUSSDBinding> listBinding = new BindingList<TopupUSSDBinding>();
        private readonly object _lockerCompleteCounter = new object();

        public USSDTopup()
        {
            InitializeComponent();
        }

        public USSDTopup(List<GSMCom> gsm)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            COMS = gsm;
            listBinding = new BindingList<TopupUSSDBinding>();
            this.Load += USSDTopup_Load;

            InitializeComponent();
            viewGSM.ClipboardRowPasting += GridView1_ClipboardRowPasting;

            lblNumberCOM.Text = COMS.Count.ToString();
            this.FormClosing += USSDTopup_FormClosing1; ;


        }

        private void USSDTopup_FormClosing1(object sender, FormClosingEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);

        }


        private void GridView1_ClipboardRowPasting(object sender, DevExpress.XtraGrid.Views.Grid.ClipboardRowPastingEventArgs e)
        { 
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            var cells = view.GetSelectedCells() as DevExpress.XtraGrid.Views.Base.GridCell[];

            if (cells.Length <= 1 || e.Values.Count > 1 || System.Windows.Forms.Clipboard.GetText().Contains(System.Environment.NewLine))
                return;

            e.Cancel = true;
            for (int i = 0; i < cells.Length; i++)
                view.SetRowCellValue(cells[i].RowHandle, cells[i].Column, e.OriginalValues[0]);
        }


        private void USSDTopup_Load(object sender, EventArgs e)
        {
            listBinding.Clear();
            foreach (var com in COMS)
            {
                com.LastUSSDCommand = string.Empty;
                com.LastUSSDResult = string.Empty;
                listBinding.Add(new TopupUSSDBinding()
                {
                    AppPort = com.DisplayName,
                    PhoneNumber = com.PhoneNumber,
                    Status = "",
                    DelayTime = 0,
                    USSD = "",
                    MaTT = "",
                    USSDResult = ""
                });
            }
            gridControl1.DataSource = listBinding;
        }
        void run()
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            pbSendProcess.Visible = true;
            pbSendProcess.Reset();
            pbSendProcess.Properties.Maximum = listBinding.Count;
            pbSendProcess.Properties.Step = 1;
            pbSendProcess.EditValue = 0;

            foreach (TopupUSSDBinding current_source in listBinding)
            {
                tasks.Add(new Task(() =>
                {
                    try
                    {
                        current_source.Status = R.S("processing");


                        this.Invoke(new MethodInvoker(() =>
                        {
                            gridControl1.RefreshDataSource();
                        }));

                        GSMCom com = COMS.Where(s => s.DisplayName == current_source.AppPort).FirstOrDefault();

                        StringBuilder builder = new StringBuilder(current_source.USSD);
                        builder.Replace("{MTT}", current_source.MaTT.Trim());

                        string processedMultiUSSD = builder.ToString(); // Value of y is "Hello 1st 2nd world"
                        string[] ussdTokens = processedMultiUSSD.Split(new[] { "|" }, StringSplitOptions.None);

                        string response = "";

                        for(int i= 0; i<ussdTokens.Length; i++ )
                        {
                            string token = ussdTokens[i];
                            response = com.ExecuteSingleUSSD(token);
                            if (string.IsNullOrEmpty(response))
                            {
                                current_source.USSDResult = response;
                                current_source.Status = "Thất bại";
                                break; 
                            }
                            else
                            {
                                current_source.USSDResult = response;
                                current_source.Status = $"Đang xử lý {i+1}/{ussdTokens.Length} ";
                            }
                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridControl1.RefreshDataSource();
                            }));
                        }

                        if (string.IsNullOrEmpty(response))
                        {
                            current_source.USSDResult = response;
                            current_source.Status = "Thất bại";
                           
                        }
                        else
                        {
                            current_source.USSDResult = response;
                            current_source.Status = "Thực thi thành công";
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



                        this.Invoke(new MethodInvoker(() =>
                        {
                            gridControl1.RefreshDataSource();
                        }));

                        if (Stop)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }));
            }
            try
            {
                new Task(() =>
                {
                    try
                    {
                        tasks.ForEach(task => task.Start());
                        Task.WaitAll(tasks.ToArray());

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Co loi xay ra USSD Topup" + ex.Message);
            }
        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupControl3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {

        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            run();
        }

        private void btnPrepareData_Click(object sender, EventArgs e)
        {
            foreach (var com in listBinding)
            {
                com.USSD = txtCommandUSSD.Text.Trim();
            }
            gridControl1.Refresh();
            gridControl1.RefreshDataSource();
        }
    }
}