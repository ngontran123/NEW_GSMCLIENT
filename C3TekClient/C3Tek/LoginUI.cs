using AutoUpdaterDotNET;
using Chilkat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C3TekClient.Core;
using C3TekClient.Helper;
using System.Threading;
using System.Globalization;

namespace C3TekClient.C3Tek
{
    public partial class LoginUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public Action<string, string, AccountSubmit, bool> Submit = (phone, password, submitType, remember) => { };
        public Action<string> Failure = (message) => { };

        public LoginUI()
        {
            SoftwareUpdater.InitUpdate();
            InitializeComponent();

            this.CenterToScreen();

            Failure += (message) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    lblFalure.Text = message;
                }));
            };
            this.Load += LoginUI_Load;

            //  testChilkat();
        }

        private void LoginUI_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            if(AppModeSetting.Locale.Equals("km-KH"))
            {
                lblVersion.Caption = $@"កំណែ {VersionHelper.GetAssemblyVersion()}";
            }
            else if(AppModeSetting.Locale.Equals("en-US"))
            {
                lblVersion.Caption = $@"Version {VersionHelper.GetAssemblyVersion()}";

            }
            else if(AppModeSetting.Locale.Equals("zh-CN"))
            {
                lblVersion.Caption = $@"版本 {VersionHelper.GetAssemblyVersion()}";
            }
            else
            {
                lblVersion.Caption = $@"Phiên bản {VersionHelper.GetAssemblyVersion()}";

            }

            if (GlobalVar.UserSetting.RememberLogin && !string.IsNullOrEmpty(GlobalVar.UserSetting.PasswordHash))
            {
                ckRemember.Checked = GlobalVar.UserSetting.RememberLogin;
                txtPassword.Text = new C3TekPortal().Decrypt(GlobalVar.UserSetting.PasswordHash);
                txtPhone.Text = GlobalVar.UserSetting.Username;
                if (GlobalVar.__DEBUG_AUTOLOGIN)
                {
                    login();
                }
            }
        }

        

        private void testChilkat()
        {
            string encode_str = "H4sIAAAAAAAACwHAAj/9evgnHd41HNAqsRsBRj4+ktWodv/3XmpJ3DBsCDVg3TMWDIx3hsAP+uUqtn8qemh/qbZEsbYIX+VIa/yf1RAWbg8jpC4uZ52HpPOhYAV4qgTfnAyNEy1KQsMshrEdbID9dHA/bsS73gKuXxnDxGRCgndAYYD+Rasskv+2xkeLyR0U7Uz63/WRYjweYTyjd/RGIzESkMUNj3SDcJnniCG2BGCDzzebEhWMHysNMVNeCCusEjTMJT7mGg5cCk6hehJ5dl4aCqbQBWFKx4X1yxL5fB5En/O0J+ZCaEfKv/9pvWSRydo5pEwjL4EAXd8f9nkfH4AnHO6hFd+x6oyoNOKOgSFsEwmjX+5rPC4UFLXMVUcJ40iC6topEfh75Lix3mHFiOSTEw1cF9kQIdu+yyZwcxhdWmEGMJTUODH7VAt5pGDE3y+IXjSYtgYbaFzUhaKMwzjHy8RLxarRNvWMe/VtQEQtZIUXC1wTGO4TXyiqL0IBqxijL5KOIsc/NBDeTnPr+EzuI86stFcx7cvSA7ETS+ahktsmCV0OB8KYeiTb4f6kvRzlM3nAdcbWMD39BwvMPLMuGhUSI6aeoAyi07AUiRwetFMgmL5MSnMEJNRyvX11nRFHVnfWjSkMWn8Y9rmEh8UQ63N99++ml6bMNHZtoyh7Joj3UVJJyxBEHWAroxLUcOnICsaLI2TPaEEMDw6OT+tuKPVJKUUO+Hx3eelkfRL7fXJsfFyiweLpIMkbVlfJskHMrV0HlqkQNoRZtMmpHywaEDZ4yjYUbsy3bSAIwpoFtErmajlUrIgnDJ9SJmxD7XjbSmGS8gAGytQA0bRR1k4eZbVUcOLw+bYPhJMR5QwiTe/WSqjNvI4CuMmLCSxHyzFFkdWLo9kj3E/7qBmfEAKTn9A7Au0pRWYzXpe8BOL9cLNYC11E+8H9yHihfRxlcWXIwAIAAA==:kS3qISAlIJssJmsJla7Y9g==";
            string reslt = new C3TekPortal().Decrypt(encode_str);
            Console.WriteLine(reslt);
        }

        public void login()
        {
            new System.Threading.Tasks.Task(() =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    txtPhone.Enabled = false;
                    txtPassword.Enabled = false;
                    if (AppModeSetting.Locale.Equals("en-US"))
                    {
                        btnLogin.Text = "Is signing in";
                    }
                    else if (AppModeSetting.Locale.Equals("zh-CN"))
                    {
                        btnLogin.Text = "登录中";
                    }
                    else if (AppModeSetting.Locale.Equals("km-KH"))
                    {
                        btnLogin.Text = "បាន​ចូល​ក្នុង​ពេល​បច្ចុប្បន្ន";
                    }
                    else
                    {
                        btnLogin.Text = "Đang đăng nhập";
                    }
                    btnLogin.Enabled = false;
                    wait.Visible = true;
                }));
                lblFalure.Text = string.Empty;
                Submit(txtPhone.Text, txtPassword.Text, AccountSubmit.Login, ckRemember.Checked);

                try
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            if (!this.IsDisposed)
                            {
                                wait.Visible = false;
                                btnLogin.Enabled = true;
                                btnLogin.Text = "Đăng nhập";
                                txtPhone.Enabled = true;
                                txtPassword.Enabled = true;
                            }
                        }
                        catch { }
                    }));
                }
                catch (Exception ex)
                {

                }

            }).Start();
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            login();
        }
        private void txtPhone_EditValueChanged(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
        }

        private void txtPassword_EditValueChanged(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
        }

        private void LoginUI_Load_1(object sender, EventArgs e)
        {

        }
    }
}
