using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClientAutoUpdate
{
    public partial class MainUI : Form
    {
        private VersionInfo VersionInfo { get; set; }
        string fileLocation = string.Empty;
        public MainUI()
        {
            InitializeComponent();
        }

        public MainUI(VersionInfo _versionInfo)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            VersionInfo = _versionInfo;
            //VersionInfo.url_download = "https://file-examples-com.github.io/uploads/2017/02/zip_2MB.zip";
            this.Load += MainUI_Load;
        }
        private void MainUI_Load(object sender, EventArgs e)
        {
            txtDescription.Text = VersionInfo.description;
            txtVersionName.Text = VersionInfo.version_name;

            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

                //Uri URL = VersionInfo.url_download.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ?
                //    new Uri(VersionInfo.url_download) : new Uri("http://" + VersionInfo.url_download);
                Uri URL = new Uri(VersionInfo.url_download);

                try
                {
                    fileLocation = Path.GetTempFileName();
                    Console.WriteLine("[File Location ] :  " + fileLocation);
                    webClient.DownloadFileAsync(URL, fileLocation);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra cập nhật phiên bản, vui lòng liên hệ Admin");
                }
            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Cập nhật thất bại");
            }
            else
            {
                new Task(() =>
                {
                    lblDownloadInfo.Text = "Đang cài đặt bản cập nhật mới";
                    Thread.Sleep(1000);
                    try { 
                    using (ZipFile zip = ZipFile.Read(fileLocation))
                    {
                        foreach (ZipEntry entry in zip)
                        {
                            lblDownloadInfo.Text = $"Module {entry.FileName}";
                            entry.Extract(Application.StartupPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    }catch(Exception ex)
                    {
                        Console.WriteLine("[Exception] " + ex.Message);
                    }

                    lblDownloadInfo.Text = "Cập nhật hoàn tất";
                    File.Delete(fileLocation);
                    Thread.Sleep(1000);
                    try { 
                    Process.Start(Application.StartupPath + "\\C3Tek.exe");
                    }catch(Exception ex)
                    {
                        Console.WriteLine("[Open client exception] Cannot open client ");
                    }
                    Application.Exit();
                }).Start();
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
            lblDownloadInfo.Text = e.ProgressPercentage.ToString() + "% | " + string.Format("{0} MB's / {1} MB's",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
        }


    }
}
