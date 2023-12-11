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
    public partial class SendSMSUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
    "đ",
    "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
    "í","ì","ỉ","ĩ","ị",
    "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
    "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
    "ý","ỳ","ỷ","ỹ","ỵ",};
        string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
    "d",
    "e","e","e","e","e","e","e","e","e","e","e",
    "i","i","i","i","i",
    "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
    "u","u","u","u","u","u","u","u","u","u","u",
    "y","y","y","y","y",};


        private List<string> Senders = new List<string>();
        public string to { get; set; }
        public string content { get; set; }
        public string content_ascii
        {
            get
            {
                string result = content;
                for (int i = 0; i < arr1.Length; i++)
                {
                    result = result.Replace(arr1[i], arr2[i]);
                    result = result.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
                }
                return result;

            }
        }
        public bool Stop = false;
        public SendSMSUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }
        public SendSMSUI(List<string> sender)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Senders = sender;
            this.FormClosing += SendSMSUI_FormClosing;
        }

        private void SendSMSUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
        }

        private void txtTo_EditValueChanged(object sender, EventArgs e)
        {
            to = txtTo.Text;
        }

        private void txtContent_TextChanged(object sender, EventArgs e)
        {
            content = txtContent.Text;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (Stop)
                    return;
            }
            catch { }
            if (string.IsNullOrEmpty(to))
            {
                MessageBox.Show("Please input receiver phone number", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please input content to send", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var serialPorts = GSMControlCenter.GSMComs.Where(com => Senders.Contains(com.PhoneNumber));
            if (serialPorts.Any())
            {
                pbSendProcess.Visible = true;
                pbSendProcess.Reset();
                pbSendProcess.Properties.Maximum = serialPorts.Count();
                pbSendProcess.Properties.Step = 1;
                pbSendProcess.EditValue = 0;
                string selected = rgSendMode.Properties.Items[rgSendMode.SelectedIndex].Description;
                switch (selected)
                {
                    case "Đồng thời":
                        {
                            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                            foreach (var com in serialPorts)
                            {
                                tasks.Add(new System.Threading.Tasks.Task(() =>
                                {
                                    try
                                    {
                                        com.SendMessage(to, content_ascii);
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
                            break;
                        }
                    case "Tuần tự":
                        {
                            new System.Threading.Tasks.Task(() =>
                            {
                                foreach (var com in serialPorts)
                                {
                                    try
                                    {
                                        if (Stop)
                                            return;
                                        com.SendMessage(to, content_ascii);
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
                                    if (Stop)
                                        return;
                                }
                            }).Start();
                            break;
                        }
                }
            }
        }

        private void pbSendProcess_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}
