using C3TekClient.GSM;
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
    public partial class USSDMulUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        List<GSMCom> COMS = new List<GSMCom>();
        int countSuccess = 0;
        private object LockCountSuccess = new object();

        private Thread paintThread;
        private bool Stop = false;
        public USSDMulUI(List<GSMCom> coms)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            COMS = coms;
            this.Load += USSDUI_Load;
            waitScreen.Visible = false;
            this.FormClosed += USSDMulUI_FormClosed;

            paintThread = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit && !Stop)
                {
                    try
                    {

                        gridControl1.RefreshDataSource();
                    }
                    catch
                    {
                    }

                    Thread.Sleep(1500);
                }
            }));
            paintThread.Start();
        }


        private void USSDMulUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);
            
        }

        private void USSDUI_Load(object sender, EventArgs e)
        {
            Init();
        }
        private void Init()
        {
            
            foreach (var com in COMS)
            {
                com.LastUSSDCommand = string.Empty;
                com.LastUSSDResult = string.Empty;
            }
            gridControl1.DataSource = COMS;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            bool reset_ussd_session = txtUSSD.Text.StartsWith("*") ? true : false; // *101# =>reset
            countSuccess = 0;
            waitScreen.Visible = true;
            try
            {
                if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                    return;
                string ussd = txtUSSD.Text.Trim();
                var tasks = new List<Task>();
                foreach (var com in COMS)
                {
                    tasks.Add(new Task(() =>
                    {
                        try
                        {
                            com.ExecuteSingleUSSD(ussd, reset_ussd_session);
                            lock (LockCountSuccess)
                            {
                                countSuccess += 1;
                            }
                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridControl1.RefreshDataSource();
                                waitScreen.Description = $"{R.S("processed")} {countSuccess}/{COMS.Count}";
                            }));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }));
                }
                //waitScreen.Visible = true;
                new Task(() =>
                {
                    try
                    {
                        tasks.ForEach(task => task.Start());
                        Task.WaitAll(tasks.ToArray());
                        waitScreen.Visible = false;
                        waitScreen.Description = "";
                    }
                    catch(Exception ex){
                        Console.WriteLine(ex.Message);
                    }
                }).Start();
            }
            catch { }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //retry butotn

            countSuccess = 0;
            waitScreen.Visible = true;
            try
            {
                if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                    return;
                string ussd = txtUSSD.Text.Trim();
                var tasks = new List<Task>();
                foreach (var com in COMS)
                {
                    if (string.IsNullOrEmpty(com.LastUSSDResult) || com.LastUSSDResult.Contains("ERROR"))
                    {
                        tasks.Add(new Task(() =>
                        {
                            com.ExecuteSingleUSSD(ussd);
                            lock (LockCountSuccess)
                            {
                                countSuccess += 1;
                            }

                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridControl1.RefreshDataSource();
                                waitScreen.Description = $"Đã thử lại {countSuccess}";
                            }));
                        }));
                    }
                }

                //waitScreen.Visible = true;
                new Task(() =>
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());
                    waitScreen.Visible = false;
                    waitScreen.Description = "";
                }).Start();
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging("Có lỗi khi retry USSD" + ex.Message);
            }
        }

        private void simpleButton1_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridControl1.ExportToXlsx(svd.FileName);
            }
        }

        private void waitScreen_Click(object sender, EventArgs e)
        {

        }
    }
}
