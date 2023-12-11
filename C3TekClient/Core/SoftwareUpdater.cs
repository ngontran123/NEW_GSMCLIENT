using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using C3TekClient.C3Tek;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace C3TekClient.Core
{
    public static class SoftwareUpdater
    {
        public static bool force_update = false;
        public readonly static string basicAuth = "YWRtaW46OGFjQjZTczdqUUM2cG5oQg==";

        public static void InitUpdate()
        {

            AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            if(GlobalVar.__ENABLE_AUTHEN_CHECK_VERSION) {
                AutoUpdater.AuthenticationHeader = $"Basic {basicAuth},Bearer {Client.AccessToken}";
            }
            AutoUpdater.UpdateMode = Mode.Forced;
            AutoUpdater.Mandatory = true;
            AutoUpdater.ReportErrors = true;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.DownloadPath = "update";

            AutoUpdater.Start(GlobalVar.UD_CHECKVERSION_URL);
        }

        public static bool checkConditionLowerVersion(string curentVersion, string minUpdateVersion)
        {
            var version1 = new Version(curentVersion);
            var version2 = new Version(minUpdateVersion);
            return version1.CompareTo(version2) < 0;
        }
        public static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string currentAssemblyVersion = fvi.FileVersion;
            try
            {
                if (args.Error == null)
                {
                    if (args.IsUpdateAvailable)
                    {

                        if (force_update || (args.Mandatory.Value == true &&
                                                     checkConditionLowerVersion(args.InstalledVersion.ToString(),
                                                         args.Mandatory.MinimumVersion)))
                        {
                            //Bat buoc update
                            DialogResult dialogForceResult;
                            dialogForceResult =
                                MessageBox.Show(
                                    $@"Phiên bản đã quá cũ, bạn cần cập nhật lên phiên bản mới {args.CurrentVersion}. Phiên bản bạn đang sử dụng hiện tại  {args.InstalledVersion}. Nếu quá trình có lỗi vui lòng liên hệ bộ phận kỹ thuật để được hỗ trợ hoặc tải bản .zip từ website",
                                    @"Cập nhật phần mềm",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                    (MessageBoxOptions)0x40000);

                            if (dialogForceResult.Equals(DialogResult.Yes) || dialogForceResult.Equals(DialogResult.OK))
                            {
                                //
                            }
                            else
                            {
                                Application.Exit();
                                GlobalVar.ForceKillMyself();
                            }

                            try
                            {
                                if (AutoUpdater.DownloadUpdate(args))
                                {
                                    Application.Exit();
                                    GlobalVar.ForceKillMyself();
                                }
                            }
                            catch (Exception exception)
                            {
                                GlobalVar.IsApplicationExit = true;
                                //MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                //    MessageBoxIcon.Error);
                                Application.Exit();
                                GlobalVar.ForceKillMyself();
                            }
                            finally
                            {
                                Application.Exit();
                                GlobalVar.ForceKillMyself();
                            }
                        }
                        else
                        {
                            //MessageBox.Show(
                            //    $@"{(force_update ? "Force update" : "Not force")} - {(args.Mandatory.Value ? "Manadtory" : "Not Manadtory")} - {(checkConditionLowerVersion(args.InstalledVersion.ToString(), args.Mandatory.MinimumVersion) ? "Check ok" : "Not ok")} ");
                            DialogResult dialogResult;
                            dialogResult =
                                MessageBox.Show(
                                    $@"Bạn ơi, phần mềm của bạn có phiên bản mới {args.CurrentVersion}. Phiên bản bạn đang sử dụng hiện tại  {args.InstalledVersion}. Bạn có muốn cập nhật phần mềm không?",
                                    @"Cập nhật phần mềm",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                    (MessageBoxOptions)0x40000);

                            if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                            {
                                try
                                {
                                    if (AutoUpdater.DownloadUpdate(args))
                                    {
                                        Application.Exit();
                                        GlobalVar.ForceKillMyself();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    //MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                    //    MessageBoxIcon.Error);
                                    Application.Exit();
                                    GlobalVar.ForceKillMyself();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Newest verion OK OK OK...........");
                                // InitializeComponent();
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Newest verion OK OK OK...........");
                        //InitializeComponent();

                    }
                }
                else
                {
                    if (args.Error is WebException)
                    {
                        MessageBox.Show("Có lỗi xảy ra khi cập nhật, vui lòng kiểm tra lại internet hoặc liên hệ Admin 0917111666. " + args.Error.ToString(), "Cập nhật");
                        Application.Exit();
                        GlobalVar.ForceKillMyself();
                        //MessageBox.Show(
                        //    @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        //    @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        //MessageBox.Show(args.Error.Message,
                        //    args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        //    MessageBoxIcon.Error);
                        MessageBox.Show("Có lỗi xảy ra khi cập nhật, vui lòng kiểm tra lại internet hoặc liên hệ Admin 0917111666 " + args.Error.ToString(), "Cập nhật");
                        Application.Exit();
                        GlobalVar.ForceKillMyself();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when update", ex.Message);
                Application.Exit();
                GlobalVar.ForceKillMyself();
            }

        }

        public static void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            string response = args.RemoteData;
            string dataEncrypted = JObject.Parse(response).SelectToken("data").Value<string>();
            var c3Tekportal = new C3TekPortal();
            
            string dataJson = GlobalVar.__ENABLE_CRYPT ?  c3Tekportal.Decrypt(dataEncrypted) : dataEncrypted;
            
            var dataObject = JObject.Parse(dataJson).SelectToken("data");
            if (dataObject != null)
            {
                dynamic json = JsonConvert.DeserializeObject(dataObject.ToString());
                force_update = json.force_update ?? false;
                args.UpdateInfo = new UpdateInfoEventArgs
                {
                    CurrentVersion = json.version,
                    ChangelogURL = json.changelog,
                    DownloadURL = json.url,
                    Mandatory = new Mandatory
                    {
                        Value = json.mandatory.value,
                        UpdateMode = json.mandatory.mode,
                        MinimumVersion = json.mandatory.minVersion
                    },

                    //CheckSum = new CheckSum
                    //{
                    //    Value = json.checksum.value,
                    //    HashingAlgorithm = json.checksum.hashingAlgorithm
                    //}
                };
            }
            else
            {
                MessageBox.Show(@"Có lỗi xảy ra khi cập nhật, vui lòng kiểm tra lại internet hoặc liên hệ Admin 0917111666", "Lỗi cập nhật");
                Application.Exit();
                GlobalVar.ForceKillMyself();

            }
        }

    }
}
