using DevExpress.XtraEditors;
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
    public partial class WSActivation : DevExpress.XtraEditors.XtraForm
    {
        List<GSMCom> COMS = new List<GSMCom>();
        int countSuccess = 0;
        private object LockCountSuccess = new object();
        public WSActivation(List<GSMCom>coms)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            COMS = coms;
            this.Load += WSActivation_Load; ;
            this.FormClosed += WSActivation_FormClosed;
            waitScreen.Visible = false; 
        }

        private void WSActivation_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalVar.setAutoDashboardMode(true);
        }

        private void WSActivation_Load(object sender, EventArgs e)
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


        private void setConfigCode(string number, string dtmf1, string dtmf2)
        {
            this.txtNumber.Text = number;
            this.txtDtmf1.Text = dtmf1;
            this.txtDtmf2.Text = dtmf2;
        }

        private void cbNhaMang_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnumConverter ec = TypeDescriptor.GetConverter(typeof(FontStyle)) as EnumConverter;
            var cBox = sender as ComboBoxEdit;
            if (cBox.SelectedIndex != -1)
            {
                if (cBox.SelectedIndex == 0 || cBox.SelectedIndex == 1)
                {
                    //vina viettel
                    setConfigCode("900", "1", "");
                }
                else if(cBox.SelectedIndex == 2)
                {
                    //mobifone
                    setConfigCode("900", "3", "1");
                }
                else if (cBox.SelectedIndex ==3)
                {
                    //vnmobile
                    setConfigCode("188", "", "");
                }
                else if(cBox.SelectedIndex == 4)
                {   //metfone
                    setConfigCode("1529", "2", "");
                }
            }

        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            countSuccess = 0;
            waitScreen.Visible = true;
            try
            {
                if (string.IsNullOrEmpty(txtNumber.Text.Trim()))
                {
                    return;
                }
                string phone = txtNumber.Text.Trim();
                string dtmf1 = txtDtmf1.Text.Trim();
                string dtmf2 = txtDtmf2.Text.Trim();
                int time_activate = Convert.ToInt32(numTimeActivate.Value);
                time_activate = (time_activate < 15) ? 15 : time_activate;

                var tasks = new List<Task>();
                foreach (var com in COMS)
                {
                    tasks.Add(new Task(() =>
                    {
                        com.ActivateSIM(phone, dtmf1, dtmf2, time_activate);
                        lock (LockCountSuccess)
                        {
                            countSuccess += 1;
                        }
                        this.Invoke(new MethodInvoker(() =>
                        {
                            gridControl1.RefreshDataSource();
                            waitScreen.Description = $"{R.S("processed")} {countSuccess}/{COMS.Count}";
                        }));
                    }));
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
            catch { }
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {

        }
    }
}