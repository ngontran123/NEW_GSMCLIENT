using Chilkat;
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
    public partial class CallOutUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private List<string> Callers = new List<string>();
        public string DialNo { get; set; }
        public bool Loop { get { return ckLoop.Checked; } }
        bool Stop = false;


        public CallOutUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }
        public CallOutUI(List<string> sender)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Callers = sender;
            this.FormClosing += CallOutUI_FormClosing;
        }
        private void CallOutUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
        }

        private void btnCall_Click(object sender, EventArgs e)
        {
            try
            {
                if (Stop)
                    return;
            }
            catch { }
            if (string.IsNullOrEmpty(DialNo))
            {
                MessageBox.Show("Vui lòng nhập sdt cần gọi", "Cảnh báo", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;

            }

            int duration = Convert.ToInt32(txtDuration.Value);


            var serialPorts = GSMControlCenter.GSMComs.Where(com => Callers.Contains(com.PhoneNumber));
            if (serialPorts.Any())
            {
                pbSendProcess.Visible = true;
                pbSendProcess.Reset();
                pbSendProcess.Properties.Maximum = serialPorts.Count();
                pbSendProcess.Properties.Step = 1;
                pbSendProcess.EditValue = 0;
                string selected = rgCallMode.Properties.Items[rgCallMode.SelectedIndex].Description;
                if (selected  == "Đồng thời" || selected == "Concurrent")
                {
                    List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                    foreach (var com in serialPorts)
                    {
                        tasks.Add(new System.Threading.Tasks.Task(() =>
                        {
                            try
                            {
                                com.Call(DialNo, duration);
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
                    new System.Threading.Tasks.Task(() =>
                    {
                        try
                        {
                            tasks.ForEach(task => task.Start());
                            System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), 60);
                        }
                        catch { }
                    }).Start();
                }
                else if(selected =="Tuần tự" || selected == "Sequential")
                {
                    new System.Threading.Tasks.Task(() =>
                    {
                        foreach (var com in serialPorts)
                        {
                            try
                            {
                                if (Stop)
                                    return;
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    try
                                    {

                                        lblCallInfo.Text = $"{com.PhoneNumber} -> Đang gọi...";
                                    }
                                    catch { }
                                }));
                                com.Call(DialNo, duration);
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
                        lblCallInfo.Text = string.Empty;

                        if (Loop)
                        {
                            try
                            {
                                btnCall_Click(null, null);
                            }
                            catch { }
                        }
                    }).Start();
                }
               
            }
        }

        private void txtTo_EditValueChanged(object sender, EventArgs e)
        {
            DialNo = txtTo.Text;
        }

        private void rgCallMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = rgCallMode.Properties.Items[rgCallMode.SelectedIndex].Description;
                
            switch (selected)
            {
                case "Đồng thời":
                    {
                        ckLoop.Enabled = false;
                        ckLoop.Checked = false;
                        break;
                    }
                case "Tuần tự":
                    {
                        ckLoop.Enabled = true;
                        break;
                    }
            }
        }
    }
}
