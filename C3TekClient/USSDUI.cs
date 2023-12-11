using C3TekClient.GSM;
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
    public partial class USSDUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        GSMCom COM = null;
        Thread hookThread = null;


        public USSDUI(GSMCom com)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.FormClosing += CallOutUI_FormClosing;
            COM = com;
            this.Load += USSDUI_Load;
        }

        private void USSDUI_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void CallOutUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalVar.setAutoDashboardMode(true);

            COM.USSDCancel();
        }
        private void Init()
        {
            COM.ResetUSSDEvent();
            hookThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    COM.USSDHook();
                }
                catch (Exception ex) {
                    COM.USSDCancel();

                }

            }));
            hookThread.Start();
            COM.USSDResponse = (response) => { 
                txtScreen.Text = response; 
                HideWait();
            };
            btnNum1.Click += BtnNum_Click;

            btnNum2.Click += BtnNum_Click; btnNum3.Click += BtnNum_Click; btnNum4.Click += BtnNum_Click; btnNum5.Click += BtnNum_Click; btnNum6.Click
            += BtnNum_Click; btnNum7.Click += BtnNum_Click; btnNum8.Click += BtnNum_Click; btnNum9.Click += BtnNum_Click; btnNum10.Click += BtnNum_Click; btnNum11.Click += BtnNum_Click;
            btnNum0.Click += BtnNum_Click;
        }
        private void BtnNum_Click(object sender, EventArgs e)
        {
            ShowWait();
            new Task(() =>
            {
                COM.USSDRequest(((SimpleButton)sender).Text.Trim());
            }).Start();
        }
        private void ShowWait()
        {
            waitScreen.Visible = true;
        }
        private void HideWait()
        {
            waitScreen.Visible = false;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                return;
            COM.LastUSSDResult= "";
            ShowWait();
            new Task(() =>
            {
                COM.USSDReset();
                COM.USSDRequest(txtUSSD.Text.Trim());
            }).Start();
        }
    }
}
