using C3TekClient.C3Tek;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C3TekClient.Helper;
using DevExpress.Charts.Native;

namespace C3TekClient
{
    public partial class AppSettingForm : DevExpress.XtraEditors.XtraForm
    {
        List<string> languages = new List<string>() { "vi-VN", "en-US", "zh-CN","km-KH" };
        Regex regexPattern = new Regex(@"([http|https|sock4|sock5]+\:\/\/)?(.*?):(.*?):(.*?):(.*?)$");

        public AppSettingForm()
        {
            InitializeComponent();
            InitAutoMode(); 
        }
        private void InitAutoMode()
        {
            //check permission and init here

            AppModeSetting.AutoTrackingSMS = true;
            //AppModeSetting.AutoAnswerCall = true;
            //AppModeSetting.AutoPlayAnswerAudio = false; 

            if (AppModeSetting.Locale.Equals("vi-VN"))
            {
                cbLanguage.SelectedIndex = 0;
            }
            else
            if (AppModeSetting.Locale.Equals("en-US"))
            {
                cbLanguage.SelectedIndex = 1;
            }
            else
            if (AppModeSetting.Locale.Equals("zh-CN"))
            {
                cbLanguage.SelectedIndex = 2;
            }
            else if(AppModeSetting.Locale.Equals("km-KH"))
            {
                cbLanguage.SelectedIndex = 3;
            }
            else
            {
                cbLanguage.SelectedIndex = 0;
            }
            

            ////confuse reverse permission
            /*
            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_SMS_AUTO))
            {
                AppModeSetting.AutoTrackingSMS = false;
                ckListBoxAutoModeSetting.Items[0].Enabled = false;
            }
            */

            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT))
            {
                AppModeSetting.AutoAnswerCall = false;
                ckListBoxAutoModeSetting.Items[1].Enabled = false;

            }


            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT_RECORD))
            {
                AppModeSetting.AutoPlayAnswerAudio = false;
                ckListBoxAutoModeSetting.Items[2].Enabled = false;

                AppModeSetting.AutoRecordCall = false;
                ckListBoxAutoModeSetting.Items[3].Enabled = false;
            }

            //init UI setting

            ckListBoxAutoModeSetting.Items[0].CheckState = (AppModeSetting.AutoTrackingSMS) ? CheckState.Checked : CheckState.Unchecked;
            ckListBoxAutoModeSetting.Items[1].CheckState = (AppModeSetting.AutoAnswerCall) ? CheckState.Checked : CheckState.Unchecked;
            ckListBoxAutoModeSetting.Items[2].CheckState = (AppModeSetting.AutoPlayAnswerAudio) ? CheckState.Checked : CheckState.Unchecked;
            ckListBoxAutoModeSetting.Items[3].CheckState = (AppModeSetting.AutoRecordCall) ? CheckState.Checked : CheckState.Unchecked;
            ckListBoxAutoModeSetting.Items[4].CheckState = (AppModeSetting.OneTimeMode) ? CheckState.Checked : CheckState.Unchecked;

            //
            settingTimeEndAnswer.Value = AppModeSetting.TimeToEndAnswer;

            IEnumerable<string> alltexts = GlobalVar.UserSetting.ListProxyItems.Select(txt => txt.ToString());
            string str = string.Join(Environment.NewLine, alltexts);
            txtProxyList.Text = str;


        }
        
        private void SaveProxyList()
        {
            String line = String.Empty;
            GlobalVar.UserSetting.ListProxyItems.Clear();

            for (int i = 0; i < txtProxyList.Lines.Length; i++)
            {
                line = txtProxyList.Lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                ProxyItem parseProxy = ProxyItem.ParseProxyFromString(line);
                if (parseProxy != null)
                {
                    GlobalVar.UserSetting.ListProxyItems.Add(parseProxy);
                }

            }
            GlobalVar.UserSetting.Save();

        }
        
        private void btnSaveSetting_Click(object sender, EventArgs e)
        {
            AppModeSetting.AutoTrackingSMS = (ckListBoxAutoModeSetting.Items[0].CheckState == CheckState.Checked);
            AppModeSetting.AutoAnswerCall = (ckListBoxAutoModeSetting.Items[1].CheckState == CheckState.Checked);
            AppModeSetting.AutoPlayAnswerAudio = (ckListBoxAutoModeSetting.Items[2].CheckState == CheckState.Checked);
            AppModeSetting.AutoRecordCall = (ckListBoxAutoModeSetting.Items[3].CheckState == CheckState.Checked);
            AppModeSetting.OneTimeMode = (ckListBoxAutoModeSetting.Items[4].CheckState == CheckState.Checked);
            AppModeSetting.TimeToEndAnswer = Convert.ToInt32(settingTimeEndAnswer.Value);

            SaveProxyList();
            MessageBox.Show("Lưu cài đặt thành công");

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult confirmResult = new DialogResult();
            if (AppModeSetting.Locale.Equals("en-US"))
            {
                confirmResult = MessageBox.Show("Application will be restarted to apply new language changes",
                                     "Confirm",
                                     MessageBoxButtons.YesNo);
            }
            else if (AppModeSetting.Locale.Equals("zh-CN"))
            {
                confirmResult = MessageBox.Show("应用程序将重新启动以应用新的语言版本",
                                     "确定",
                                     MessageBoxButtons.YesNo);
            }
            else if (AppModeSetting.Locale.Equals("km-KH"))
            {
                confirmResult = MessageBox.Show("កម្មវិធីនឹងចាប់ផ្តើមឡើងវិញ ដើម្បីអនុវត្តកំណែភាសាថ្មី។",
                                    "បញ្ជាក់",
                                    MessageBoxButtons.YesNo);
            }
            else
            {
                confirmResult = MessageBox.Show("Ứng dụng sẽ khởi động lại để áp dụng phiên bản ngôn ngữ mới",
                                         "Xác nhận",
                                         MessageBoxButtons.YesNo);
            }
            if (confirmResult == DialogResult.Yes)
            {
                AppModeSetting.Locale = languages[cbLanguage.SelectedIndex];
                new ChangeLanguage().UpdateConfig("language", AppModeSetting.Locale);
                Application.Restart();
            }
            

        }

        private void btnCheckProxy_Click(object sender, EventArgs e)
        {
            
            if(Client.GetCurrentAccount().Is_buy_modem)
            {
                if (Client.GetCurrentAccount().SubscriptionPackage.Equals("")|| Client.GetCurrentAccount().SubscriptionPackage.Equals("BLANK"))
                {
                    MessageBox.Show("no_subscription_permission", "Message", MessageBoxButtons.OK);
                    btnGetProxy.Visible = false;
                }
                else
                {
                    C3TekPortal c3 = new C3TekPortal();
                    //string b = c3.stringProxy();
                    //MessageBox.Show(b);
                    
                    int a = c3.AddProxy();
                    MessageBox.Show("Got " + a.ToString() + " proxies","Message",MessageBoxButtons.OK);
                    int b = int.Parse(lblLiveProxyCount.Text);
                    b = b + a;
                    lblLiveProxyCount.Text = b.ToString();
                    btnGetProxy.Visible = false;
                }
            }
            else
            {
                MessageBox.Show("not_bought_modem", "Message", MessageBoxButtons.OK);
                btnGetProxy.Visible = false;
            }
            


        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnCheckProxy_Click_1(object sender, EventArgs e)
        {
            string listProxyStr = txtProxyList.Text;
            String line = String.Empty;

            string filterLiveProxy = "";

            new Task(() =>
            {
                int liveCount = 0;
                int dieCount = 0;

                for (int i = 0; i < txtProxyList.Lines.Length; i++)
                {
                    line = txtProxyList.Lines[i];
                    Match match = regexPattern.Match(line);
                    if (match.Success)
                    {
                        string protocol = match.Groups[1].Value ?? "";
                        string host = match.Groups[2].Value ?? "";
                        string port = match.Groups[3].Value ?? "80";
                        string username = match.Groups[4].Value ?? "";
                        string password = match.Groups[5].Value ?? "";
                        if (string.IsNullOrEmpty(protocol))
                        {
                            if (port == "80") protocol = "http://";
                            if (port == "443") protocol = "https://";
                        }

                        int portInt = Int32.Parse(port);
                        bool checkProxy = UtilHelper.CheckProxy(protocol + host, portInt, username, password);
                        if (checkProxy)
                        {
                            if (!string.IsNullOrEmpty(filterLiveProxy))
                            {
                                filterLiveProxy += Environment.NewLine;
                            }

                            filterLiveProxy += line;
                            liveCount++;
                        }
                        else
                        {
                            dieCount++;
                        }


                    }
                    else
                    {
                        dieCount++;

                    }
                    this.Invoke(new MethodInvoker(() =>
                    {

                        lblLiveProxyCount.Text = liveCount.ToString();
                        lblDieProxyCount.Text = dieCount.ToString();
                    }));
                }
                this.Invoke(new MethodInvoker(() =>
                {
                    txtProxyList.Text = filterLiveProxy;
                }));

                MessageBox.Show($"Hoàn thành Live: {liveCount} Die: {dieCount}");
            }).Start();

        }

        private void groupSettingAuto_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblDieProxyCount_Click(object sender, EventArgs e)
        {

        }

        private void AppSettingForm_Load(object sender, EventArgs e)
        {

        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupControl3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ckListBoxAutoModeSetting_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}