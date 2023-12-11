using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClient.GSM
{

    public partial class GSMDataConsumeUI : DevExpress.XtraEditors.XtraForm
    {
        bool Stop = false;
        private List<GSMTransferModel> _gsm = new List<GSMTransferModel>();
        private List<string> _gsmString = new List<string>();

        public string DialNo { get; set; }



        private readonly object _lockerCompleteCounter = new object();

        public int CompleteCounter { get; set; } 

        public class DataConsumeSource
        {
            public string comRealPort { get; set; }
            public string comAppPort { get; set; }
            public string phoneNumber { get; set; }
            public string dataStatus { get; set; }
        }
        public BindingList<DataConsumeSource> dataConsumingStatusBinding = new BindingList<DataConsumeSource>();


        public GSMDataConsumeUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            CompleteCounter = 0;

        }

        public GSMDataConsumeUI(List<GSMTransferModel> sender)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            _gsm = sender;
            this.FormClosing += GSMDataConsumeUI_FormClosing;
            CompleteCounter = 0;


            //dataConsumingStatusBinding.Add(new DataConsumeSource() { phoneNumber = "1", dataStatus = "Thất bại" } );
            //dataConsumingStatusBinding.Add(new DataConsumeSource() { phoneNumber = "2", dataStatus = "Thành công" });
            //dataConsumingStatusBinding.Add(new DataConsumeSource() { phoneNumber = "3", dataStatus = "Thành công" });
            //dataConsumingStatusBinding.Add(new DataConsumeSource() { phoneNumber = "4", dataStatus = "Đang xử lý" });
            //dataConsumingStatusBinding.Add(new DataConsumeSource() { phoneNumber = "4", dataStatus = "Đang xử lý" });



            prepareSource();
            this.gridDataStatus.DataSource = dataConsumingStatusBinding;


        }
        private void prepareSource()
        {
            dataConsumingStatusBinding = new BindingList<DataConsumeSource>();
            foreach (GSMTransferModel phone in _gsm)
            {
                DataConsumeSource item = new DataConsumeSource()
                {
                    phoneNumber = phone.PhoneNumber,
                    comAppPort = phone.AppPortName,
                    comRealPort = phone.PortName,
                    dataStatus = ""
                };
                dataConsumingStatusBinding.Add(item);
            }
        }
        private void GSMDataConsumeUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);

        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (Stop)
                    return;
            }
            catch { }
            this.CompleteCounter = 0;
            lblCompleteNum.Text = $"0";

            //check change imei
            bool stateChangeIMEI = toggleChangeImei.IsOn;



            int spaceTime = Convert.ToInt32(txtSpaceTime.Text);
            _gsmString = (List<string>)_gsm.Select(com => com.PhoneNumber).ToList();
            List<GSMCom> serialPorts = (List<GSMCom>)GSMControlCenter.GSMComs.Where(com => _gsmString.Contains(com.PhoneNumber)).ToList();
            CompleteCounter = 0;
            string package_size_value = comboMB.SelectedItem.ToString();
            if (string.IsNullOrEmpty(package_size_value))
            {
                package_size_value = "5";
            }
            if (serialPorts.Any())
            {
                pbSendProcess.Visible = true;
                pbSendProcess.Reset();
                pbSendProcess.Properties.Maximum = serialPorts.Count();
                pbSendProcess.Properties.Step = 1;
                pbSendProcess.EditValue = 0;
                string selected = rgDataMode.Properties.Items[rgDataMode.SelectedIndex].Description;
                if (selected == "Đồng thời" || selected == "Concurrent")
                {
                    List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                    int[] tempseeder = GSMHelper.InitializeArrayWithNoDuplicates(dataConsumingStatusBinding.Count());

                    for (int i = 0; i < serialPorts.Count(); i++)
                    {
                        GSMCom com = serialPorts[i];
                        int idx = i;
                        dataConsumingStatusBinding[idx].dataStatus = "Đang xử lý";
                        this.Invoke(new MethodInvoker(() =>
                        {
                            gridDataStatus.RefreshDataSource();
                        }));
                        if (stateChangeIMEI == true)
                        {
                            com.ChangeRandomIMEI();
                        }
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                //execute data
                                /// com.Call(DialNo, duration
                                ///

                                //TaskConsumeDataResult _responseConsumeResult = com.ExecuteConsumeData();
                                var task2watch = new Stopwatch();
                                task2watch.Start();
                                string responseConsumeData = com.ExecuteConsumeData(package_size_value);
                                task2watch.Stop();
                                // Console.WriteLine($"Response length: {response.Length} with time : {task2watch.ElapsedMilliseconds / 1000.00 }s");
                                if (!string.IsNullOrEmpty(responseConsumeData))
                                {
                                    string size_success_mb = String.Format("{0:0.00}", responseConsumeData.Length / 1024.00 / 1024.00);      // "123.46"
                                    dataConsumingStatusBinding[idx].dataStatus = $"Thành công {package_size_value} MB - {task2watch.ElapsedMilliseconds / 1000.00}s";
                                    Console.WriteLine($"{idx} - Thanh cong");
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                    }
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        lblCompleteNum.Text = $"{CompleteCounter}/{_gsm.Count()}";
                                    }));
                                }
                                else
                                {
                                    Console.WriteLine($"{idx} - That bai");

                                    dataConsumingStatusBinding[idx].dataStatus = $"Thất bại {task2watch.ElapsedMilliseconds / 1000.00}s";
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridDataStatus.RefreshDataSource();
                                }));

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
                                if (Stop)
                                    return;
                            }
                            catch { }
                        }));

                    }
                    await Task.WhenAll(tasks);
                }
                else if (selected == "Tuần tự" || selected =="Sequential")
                {
                    foreach (var item in dataConsumingStatusBinding)
                    {
                        item.dataStatus = "Trong hàng đợi";
                    }

                    this.Invoke(new MethodInvoker(() =>
                    {
                        gridDataStatus.RefreshDataSource();
                    }));
                    int[] tempseeder = GSMHelper.InitializeArrayWithNoDuplicates(dataConsumingStatusBinding.Count());

                    new System.Threading.Tasks.Task(() =>
                    {
                        for (int i = 0; i < serialPorts.Count(); i++)
                        {
                            int idx = i;
                            GSMCom com = serialPorts[i];
                            if (stateChangeIMEI == true)
                            {

                                com.ChangeRandomIMEI();
                            }
                            try
                            {
                                if (Stop)
                                    return;

                                dataConsumingStatusBinding[idx].dataStatus = "Đang xử lý";
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridDataStatus.RefreshDataSource();
                                }));

                                var task2watch = new Stopwatch();
                                task2watch.Start();
                                string responseConsumeData = com.ExecuteConsumeData(package_size_value);
                                task2watch.Stop();


                                if (!string.IsNullOrEmpty(responseConsumeData))
                                {
                                    string size_success_mb = String.Format("{0:0.00}", responseConsumeData.Length / 1024.00 / 1024.00);      // "123.46"

                                    dataConsumingStatusBinding[idx].dataStatus = $"Thành công {size_success_mb} MB - {task2watch.ElapsedMilliseconds / 1000.00}s";

                                    Console.WriteLine($"{idx} - Thanh cong");

                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                    }


                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        lblCompleteNum.Text = $"{CompleteCounter.ToString()}/{_gsm.Count()}";
                                    }));
                                }
                                else
                                {
                                    Console.WriteLine($"{idx} - That bai");

                                    dataConsumingStatusBinding[idx].dataStatus = $"Thất bại {task2watch.ElapsedMilliseconds / 1000.00}s";
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridDataStatus.RefreshDataSource();
                                }));

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    try
                                    {
                                        pbSendProcess.PerformStep();
                                    }
                                    catch { }
                                }));
                                if (Stop)
                                    return;
                            }
                            catch { }
                        }
                        Thread.Sleep(spaceTime);

                    }).Start();
                }
            }

        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridViewDataStatus.ExportToXlsx(svd.FileName);
            }

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}