using AutoUpdaterDotNET;
using C3TekClient.C3Tek;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClient
{
    public static class VersionManager
    {
        public static string update_check_url = "http://localhost/update/version.json";
        public static bool force_update = false;
        public static VersionInfo CurrentVersion = new VersionInfo
        {
            version_code = "27",
            version_name = "v3.8.1",
            description = @"Quản lý modem GSM",
            force_upgrade = false,
            is_latest = true,
            product_code = "C3TEKCLIENT V3",
            version_date = "10/07/2021",
            version_type = "Released",
            url_download = "",
            product_name = "C3TEK CLIENT"
        };

        public static void InitUpdater()
        {
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            AutoUpdater.UpdateMode = Mode.Forced;
            AutoUpdater.Mandatory = true;
            AutoUpdater.ReportErrors = true;
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.DownloadPath = "update";

            AutoUpdater.Start(update_check_url);
           

        }

        private static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.IsUpdateAvailable)
            {
                if (force_update == true)
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
                else { 
                    DialogResult dialogResult;
                    dialogResult =
                            MessageBox.Show(
                                $@"Bạn ơi, phần mềm của bạn có phiên bản mới {args.CurrentVersion}. Phiên bản bạn đang sử dụng hiện tại  {args.InstalledVersion}. Bạn có muốn cập nhật phần mềm không?", @"Cập nhật phần mềm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

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
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            Application.Exit();
                            GlobalVar.ForceKillMyself();
                        }
                    }
                }
               
            }
            else
            {
                Console.WriteLine("Newest verion OK OK OK...........");
            }
        }

        private static void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
           
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
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

        public static void CheckForUpdate(bool forceUpdate = false)
        {
            AutoUpdater.Start(update_check_url);
            return;
            try
            {
                var versionInfo = new C3TekPortal().GetLastestVersion();
                string versionInfoJson = JsonConvert.SerializeObject(versionInfo);
                Console.WriteLine(versionInfoJson);
                if (versionInfo == null)
                {
                    MessageBox.Show(R.S("cannot_check_version"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GlobalVar.ForceKillMyself();
                    return;
                }
                else
                {
                    if (CurrentVersion.version_code != versionInfo.version_code)
                    {
                     
                        if (versionInfo.force_upgrade || forceUpdate)
                        {
                            MessageBox.Show(R.S("newer_version"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            GlobalVar.ForceKillMyself();
                            //ProcessStartInfo startInfo = new ProcessStartInfo();
                            //string updatePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\C3TekClientAutoUpdate.exe";
                            //startInfo.FileName = updatePath;
                            //startInfo.Arguments = "\"" + JsonConvert.SerializeObject(versionInfo).Replace("\"", "\\\"") + "\"";
                            //Process.Start(startInfo);
                            return;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(R.S("cannot_check_version"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                GlobalVar.ForceKillMyself();
                return;
            }
        }
    }
}
