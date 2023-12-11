using C3TekClient.C3Tek;
using C3TekClient.GSM;
using C3TekClient.Helper;
using C3TekClient.MyMobifone;
using C3TekClient.MyViettel;
using C3TekClient.MyVNMB;
using C3TekClient.MyVNPT;
using DevExpress.DirectX.NativeInterop.DXGI;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using System.IO.Ports;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Views.Layout.Modes;

namespace C3TekClient
{
    public partial class MainUI : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        Thread paintThread;
        Thread balanceHandler;
        Thread autoModeHandler;
        Thread socketCheckHandler;
        public List<string> imeiList = new List<string>();
        public RealPhoneImei phoneImei = new RealPhoneImei();
        public List<string> imeiManualList = new List<string>();
        public static MainUI instance;
        public static MainUI returnInstance()
        {
            return instance;
        }
        public MainUI()
        {
            instance = this;
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.CenterToScreen();
            //Console.WriteLine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));   
        }

        private void removePermissionControl()
        {
            
            this.btnThanhToan.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_MULTI_USSD);
            this.btnCallAndPlay.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT_RECORD);
            this.btnAdvanceUSSD.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_MULTI_USSD);
            this.btnActivate.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_MULTI_USSD);
        }
        private List<string> portComboBox()
        {
            List<string> values = new List<string>();
            for(int i=1;i<=32;i++)
            {
                values.Add(i.ToString());
            }
            return values;
        }

        private List<string> portInformation()
        {
            List<string> port_info = new List<string>();
            try
            {
                using(var searcher=new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
               {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString()).ToList();
                    port_info = ports;                    
                }
            }
            catch(Exception er)
            {

            }
            return port_info;
        }

        private List<string> getSimbankPort(List<string> ports)
        {
            List<string> simbankPorts = new List<string>();
            try
            {
                int num_port_serial = ports.Count(c => c.Contains("Serial Port"));
                if(num_port_serial==0)
                {
                    return simbankPorts;
                }
                var list_serial_port = ports.Where(s =>s.Contains("Serial Port")).Select(s=>s).ToList();
                simbankPorts = list_serial_port;

            }
            catch(Exception er)
            {

            }
            return simbankPorts;
        }

        private string realSimBankPort(List<string> sim_bank_ports)
        {
            string real_sim_bank_port = "";
            if(sim_bank_ports.Count==0)
            {
                return "";
            }
            try
            {
                int max_port_val = 0;
                Regex digits = new Regex(@"[^\d]");
                List<string> ports_val = sim_bank_ports.Select(x => digits.Replace(x, "")).ToList();
                foreach(string port in ports_val)
                {
                    int port_val = int.Parse(port);
                    if(port_val>max_port_val)
                    {
                        max_port_val = port_val;
                    }
                }
                real_sim_bank_port = "COM" + max_port_val;
            }
            catch(Exception er)
            {

            }
            return real_sim_bank_port;
        }
        private void MainUI_Load(object sender, EventArgs e)
        {           
            viewGSM.PopupMenuShowing +=ViewGSM_PopupMenuShowing;
            lblClientName.Caption = $"{Client.GetCurrentAccount().Name}";
            //lblClientBalance.Caption = $"Số dư: {Client.GetCurrentAccount().Balance.ToString("N0")}đ";
            lblClientBalance.Caption = R.S("balance")+ Client.GetCurrentAccount().Balance.ToString("N0") + "đ";
            string sub_package = GlobalVar.MapSubscriptionPackage[Client.SubcriptionPackage];

            lblBuyModemStatus.Caption = Client.GetCurrentAccount().Is_buy_modem ? Client.GetCurrentAccount().Name + " - "+ R.S("is_bought_modem") + " - " + sub_package : Client.GetCurrentAccount().Name + " - " + R.S("not_bought_modem") +  " - " + sub_package;

            imeiList = imeiList.Concat(phoneImei.getAllTypeTalco("Apple")).Concat(phoneImei.getAllTypeTalco("Samsung")).Concat(phoneImei.getAllTypeTalco("HTC")).Concat(phoneImei.getAllTypeTalco("Motorola")).ToList();
             
            GSMPayloadSchemasInstance.loadLocalSchemas();
            removePermissionControl();
            Application.ApplicationExit += (_sender, @event) =>
            {
                GlobalVar.IsApplicationExit = true;
                GSMControlCenter.Dispose();
                GlobalVar.ForceKillMyself();
            };


            List<string> simbank_ports = portComboBox();
            List<string> ports_info = portInformation();
            List<string> simbank_real_ports = getSimbankPort(ports_info);
            comboBoxEdit1.Properties.Items.AddRange(simbank_ports);
            comboBoxEdit1.SelectedIndex = 0;
            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.SEND_SMS_MANUAL))
            {
                this.btnSendSMS.Visible = false;
            } 
            
            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.SEND_SMS_MANUAL))
            {
                this.btnCall.Visible = false;
            }

            


            if (Client.GetCurrentAccount().IsOtherUser  == true)
            {
                this.btnActivate.Visible = false;
                this.btnUSSD.Visible = false;
                //this.btnSettingForm.Visible = false;
                this.btnChangeIMEI.Visible = false;
            }

            if (GlobalVar.CheckRegManualImei)
            {
                this.checkImeiManual.Checked = true;
            }
            else
            {
                this.checkImeiManual.Checked = false;
            }
            this.btnCallAndPlay.Visible = false;

            if (Client.GetCurrentAccount().Username == "0846889911")
            {
                tabNavigationPage5.PageVisible = true;
            }

            if (AppModeSetting.Locale == "en-US")
            {
                tabNavigationPage4.PageVisible = false;
            }
            else
            {
                tabNavigationPage4.PageVisible = true;
            }

            ckCheckBalanceMode.Checked = GlobalVar.CheckBalanceAndPhone;
            if (ckCheckBalanceMode.Checked)
            {
                btnStartRegister.Enabled = true;
            }
            else
            {
                btnStartRegister.Enabled = false;
            }
            gridGSM.DataSource = GSMControlCenter.GSMComs;
            gridSMS.DataSource = GSMControlCenter.GSMMessages;
            gridMy.DataSource = MyCenter.Mys;
            gridRequestLogger.DataSource = RequestLoggerCenter.RequestLoggers;
            viewGSM.CustomColumnSort += ViewGSM_CustomColumnSort;
            paintThread = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        gridGSM.RefreshDataSource();
                    }
                    catch 
                    {
                    }
                    Thread.Sleep(1500);
                }
            }));
            socketCheckHandler = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        if (GSMControlCenter.webSocketClient != null && GSMControlCenter.webSocketClient.isAlive())
                        {
                            this.Invoke(new MethodInvoker(() =>
                            {
                                lblServerSocketStatus.Text = R.S("connected");
                            }));
                        }
                        else
                        {
                            this.Invoke(new MethodInvoker(() =>
                            {
                                lblServerSocketStatus.Text = R.S("disconnected");
                            }));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    Thread.Sleep(3000);
                }
            }));



            balanceHandler = new Thread(new ThreadStart(() =>
            {
                var portal = new C3TekPortal();
                BalanceInfo balanceInfo = null;
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        balanceInfo = portal.BalanceTracking();
                        if (balanceInfo != null)
                        {
                            this.Invoke(new MethodInvoker(() => 
                            {
                                lblClientBalance.Caption = R.S("balance") + balanceInfo.amount.ToString("N0") + "đ";
                            }));
                        }
                    }
                    catch { }
                    Thread.Sleep(3000);
                    
                }
            }));


            autoModeHandler = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                { 
                    try
                    {

                        this.Invoke(new MethodInvoker(() =>
                        {
                            lblAutoMode.Caption = GlobalVar.AutoDashboardMode ? R.S("auto_on") : R.S("auto_off");
                        }));
                    }
                    catch { }
                    Thread.Sleep(1500);
                }
            }));
            if (AppModeSetting.Locale.Equals("zh-CN"))
            {
                lblVersion.Caption = $@"版本 {VersionHelper.GetAssemblyVersion()}";
            }
            else if (AppModeSetting.Locale.Equals("en-US"))
            {
                lblVersion.Caption = $@"Version {VersionHelper.GetAssemblyVersion()}";
            }
            else if (AppModeSetting.Locale.Equals("km-KH"))
            {
                lblVersion.Caption = $@"កំណែ {VersionHelper.GetAssemblyVersion()}";
            }
            else
            {
                lblVersion.Caption = $@"Phiên bản {VersionHelper.GetAssemblyVersion()}";
            }

            //this.lblVersion.Caption = VersionManager.CurrentVersion.version_name;

            viewGSM.OptionsBehavior.KeepFocusedRowOnUpdate = true;
            //prevent not update when focus
            viewGSM.OptionsBehavior.ImmediateUpdateRowPosition = true;

            ckRegMyUseProxy.Checked = GlobalVar.CheckRegMyUseProxy;

            paintThread.Start();
            balanceHandler.Start();
            autoModeHandler.Start();
            socketCheckHandler.Start();

            GlobalEvent.ONATCommandResponse += ONATCommandResponse;
            GlobalEvent.OnGlobalMessaging += OnGlobalMessaging;
            GlobalEvent.OnJobLog += OnJobLog;



            GSMControlCenter.Start();
            TTTBHelper.Start();

            


        }

        private void ViewGSM_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;

            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                DXMenuItem item = new DXMenuItem("Delete");
                int hitRow = e.HitInfo.RowHandle;
                e.Menu.Items.Clear();
                e.Menu.Items.Add(CreateSubMenuRows(view, hitRow));
            }
        }


        DXMenuItem CreateSubMenuRows(GridView view, int rowHandle)
        {
            DXSubMenuItem subMenu = new DXSubMenuItem("Gán sóng nhà mạng");
          
            DXMenuItem assignCustomerCarrierViettel = new DXMenuItem("Viettel", new EventHandler(AssignCustomCarrierMenuClick));
            assignCustomerCarrierViettel.Tag = new RowInfo(view, rowHandle);
            DXMenuItem assignCustomerCarrierMobifone = new DXMenuItem("Mobifone", new EventHandler(AssignCustomCarrierMenuClick));
            assignCustomerCarrierMobifone.Tag = new RowInfo(view, rowHandle);
            DXMenuItem assignCustomerCarrierVietnamMobile = new DXMenuItem("VietnamMobile", new EventHandler(AssignCustomCarrierMenuClick));
            assignCustomerCarrierVietnamMobile.Tag = new RowInfo(view, rowHandle);
            DXMenuItem assignCustomerCarrierVinaphone = new DXMenuItem("Vinaphone", new EventHandler(AssignCustomCarrierMenuClick));
            assignCustomerCarrierVinaphone.Tag = new RowInfo(view, rowHandle);
            DXMenuItem assignCustomerCarrierAutoDetect = new DXMenuItem("Tự động", new EventHandler(AssignCustomCarrierMenuClick));
            assignCustomerCarrierAutoDetect.Tag = new RowInfo(view, rowHandle);



            subMenu.Items.Add(assignCustomerCarrierViettel);
            subMenu.Items.Add(assignCustomerCarrierMobifone);
            subMenu.Items.Add(assignCustomerCarrierVietnamMobile);
            subMenu.Items.Add(assignCustomerCarrierVinaphone);
            subMenu.Items.Add(assignCustomerCarrierAutoDetect);


            return subMenu;
        }
        private void AssignCustomCarrierMenuClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            RowInfo ri = menuItem.Tag as RowInfo;
            MessageBox.Show(menuItem.Caption);
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            
            List<GSMCom> senders = new List<GSMCom>();

            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    switch (menuItem.Caption)
                    {
                        case "Viettel":
                            com.CarrierDefault = SIMCarrier.Viettel;
                            break;
                        case "Mobifone":
                            com.CarrierDefault = SIMCarrier.Mobifone;
                            break;
                        case "VietnamMobile":
                            com.CarrierDefault = SIMCarrier.VietnamMobile;
                            break;
                        case "Vinaphone":
                            com.CarrierDefault = SIMCarrier.Vinaphone; 
                            break;
                        case "Tự động":
                            com.CarrierDefault = SIMCarrier.NO_SIM_CARD;
                            com.AutoAssignCarrier = true;
                            break;
                    } 
                    com.AutoAssignCarrier = false;
                    com.MyProcessMessage = $"Đã thiết lập mạng: {menuItem.Caption}";
                }
            }

            if (!senders.Any())
            {

                return;
            }
        }

        class RowInfo
        {
            public RowInfo(GridView view, int rowHandle)
            {
                this.RowHandle = rowHandle;
                this.View = view;
            }
            public GridView View;
            public int RowHandle;
        }


        //Custom column sort by number
        private void ViewGSM_CustomColumnSort(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == "DisplayName" || e.Column.FieldName == "PortName")
            {
                string val1 = e.Value1.ToString();
                string val2 = e.Value2.ToString();
                if (e.Value1.ToString().Length == e.Value2.ToString().Length)
                {
                    e.Result = val1.CompareTo(val2);

                }
                else
                {
                    e.Result = val1.Length - val2.Length;

                }
                e.Handled = true;
            }
        }

        public void ONATCommandResponse(string response)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    if (chkRecord.Checked)
                    {
                        txtAtcommandLog.AppendText("\n" + response);
                        txtAtcommandLog.SelectionStart = txtAtcommandLog.Text.Length;
                        txtAtcommandLog.ScrollToCaret();
                    }
                }
                catch { Console.WriteLine("Error"); }
            }));
        }
        public void OnGlobalMessaging(string message)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    if (chkRecord.Checked)
                    {
                        txtGlobalMessage.AppendText("\n" + message);
                        txtGlobalMessage.SelectionStart = txtGlobalMessage.Text.Length;
                        txtGlobalMessage.ScrollToCaret();
                    }
                }
                catch {
                      }
            }));
        }
        public void OnJobLog(string message)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    txtJobLog.AppendText("\n" + message);
                    txtJobLog.SelectionStart = txtJobLog.Text.Length;
                    txtJobLog.ScrollToCaret();
                }
                catch(Exception e) { }
            }));
        }
        private void btnReconnect_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<string> ports_information = portInformation();
            List<string> simbank_ports = getSimbankPort(ports_information);
            string real_simbank_port = realSimBankPort(simbank_ports);
            string selected_port = comboBoxEdit1.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(real_simbank_port))
            {
                int val = -1;
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var gsm_com = (GSMCom)row;
                        var cur_port = gsm_com.PortName;
                       
                        if (cur_port == real_simbank_port)
                        {
                            val = 1;
                            bool handshake_port = gsm_com.checkSimBank();
                            if (handshake_port)
                            {
                                bool reset_sim_bank = gsm_com.resetAllChannel();
                                if (reset_sim_bank)
                                {
                                    bool switch_simbank_port = gsm_com.switchSimChannel(selected_port);
                                    if (switch_simbank_port)
                                    {
                                        resetAllPorts(selectedRowsHandle, real_simbank_port);
                                        break;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Có lỗi xảy ra khi đổi cổng simbank.Vui lòng thử lại","Đổi cổng simbank",MessageBoxButtons.OK,MessageBoxIcon.Error);
                                    }
                                }
                            }
                            else
                            {
                                resetAllPorts(selectedRowsHandle, "");
                                break;
                            }
                        }
                }
                }
                if(val==-1)
                {
                    resetAllPorts(selectedRowsHandle, "");
                }
            }
            else
            {
                resetAllPorts(selectedRowsHandle, "");
            }
            /*List<Task> tasks = new List<Task>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    tasks.Add(new Task(
                        () => { ((GSMCom)row).Reconnect(); }
                        ));
                }
            }
            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                Task.WaitAll(tasks.ToArray());
            }).Start();*/
        }


        private void resetAllPorts(int[] selectedRows,string device_port)
        {
            List<Task> tasks = new List<Task>();
            foreach(int rowHandle in selectedRows)
            {
                var row = viewGSM.GetRow(rowHandle);
                if(row!=null)
                {
                    var gsm_com = (GSMCom)row;
                    var port_name = gsm_com.PortName;
                    if (port_name != device_port)
                    {                       
                        tasks.Add(new Task(() => { gsm_com.reConnect(); }));
                    }
                 }
                  
            }
            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                Task.WaitAll(tasks.ToArray());
            }).Start();
        }
        private void btnSendSMS_Click(object sender, EventArgs e)
        {
            //Old version
            //int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            //List<string> senders = new List<string>();
            //foreach (int rowHandle in selectedRowsHandle)
            //{
            //    var row = viewGSM.GetRow(rowHandle);
            //    if (row != null)
            //    {
            //        var com = ((GSMCom)row);
            //        if (com.IsPortConnected && com.IsSIMConnected
            //            && !string.IsNullOrEmpty(com.PhoneNumber))
            //        {
            //            senders.Add(com.PhoneNumber);
            //        }
            //    }
            //}
            //if (!senders.Any())
            //{
            //    MessageBox.Show("Vui lòng chọn sim cần gửi tin nhắn (Giữ ctrl hoặc shift để chọn nhiều)", "Cảnh báo", MessageBoxButtons.OK,
            //        MessageBoxIcon.Warning);
            //    return;
            //}
            //new SendSMSUI(senders).ShowDialog(this);

            int flagCheckBalanceWarningBlock = 0;

            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            GlobalEvent.OnGlobalMessaging(selectedRowsHandle.Length.ToString());
            List<GSMCom> senders = new List<GSMCom>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected)
                    {
                        if (ckCheckBalanceMode.Checked && string.IsNullOrEmpty(com.PhoneNumber))
                        {
                            //Port có kết nối, sim có kết nối nhưng Không nhận được sđt thì không thêm
                            flagCheckBalanceWarningBlock++; 

                        }
                        else
                        {
                            senders.Add(com);
                        }
                    }
                }
            }

            GlobalEvent.OnGlobalMessaging(senders.ToString());

            if (flagCheckBalanceWarningBlock > 0)
            {
                MessageBox.Show(
                    $"Không nhận dạng được {flagCheckBalanceWarningBlock} SĐT, vui lòng kiểm tra lại thuê bao có khoá 1 chiều không?", "Cảnh báo");
                return;
            }

            if (!senders.Any())
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
                return;
            }

            GlobalVar.setAutoDashboardMode(false);
            new WSMessageSMSUI(senders).ShowDialog(this);
        }

        private void btnCall_Click(object sender, EventArgs e)
        {
            /*
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<string> senders = new List<string>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected
                        && !string.IsNullOrEmpty(com.PhoneNumber))
                    {
                        senders.Add(com.PhoneNumber);
                    }
                }
            }

            if (!senders.Any())
            {
                MessageBox.Show("Vui lòng chọn sim thực hiện gọi đi (Giữ ctrl hoặc shift để chọn nhiều)", "Cảnh báo", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            new CallOutUI(senders).ShowDialog(this);
            */


            int[] selectedRowsHandle = viewGSM.GetSelectedRows();

            List<GSMCom> senders = new List<GSMCom>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected)
                    {
                        senders.Add(com);
                    }
                }
            }
            if (!senders.Any())
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
                return;
            }
            GlobalVar.setAutoDashboardMode(false);
            new WSCallOutUI(senders).ShowDialog(this);
        }


        private void btnGSMExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewGSM.ExportToXlsx(svd.FileName);
            }
        }

        private void btnDeleteSMS_Click(object sender, EventArgs e)
        {
            lock (GSMControlCenter.LockGSMMessages)
            {
                GSMControlCenter.GSMMessages.Clear();
            }
        }

        private void btnExportSMS_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewSMS.ExportToXlsx(svd.FileName);
            }
        }


        private void SwitchRunning()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        btnStartRegister.Text = "Dừng tạo My***";
                        txtPassword.Enabled = false;
                        btnStartRegister.ImageOptions.Image = Properties.Resources.stop_icon_16;
                    }
                    catch { }
                }));
            }
            else
            {
                try
                {
                    btnStartRegister.Text = "Dừng tạo My***";
                    txtPassword.Enabled = false;
                    btnStartRegister.ImageOptions.Image = Properties.Resources.stop_icon_16;
                }
                catch { }
            }




        }

        private void SwitchStop()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        btnStartRegister.Text = "Bắt đầu tạo My***";
                        txtPassword.Enabled = true;
                        btnStartRegister.ImageOptions.Image = Properties.Resources.play_icon_16;
                    }
                    catch { }
                }));
            }
            else
            {
                try
                {
                    btnStartRegister.Text = "Bắt đầu tạo My***";
                    txtPassword.Enabled = true;
                    btnStartRegister.ImageOptions.Image = Properties.Resources.play_icon_16;
                }
                catch { }
            }

        }

        private void btnStartRegister_Click(object sender, EventArgs e)
        {
            if (btnStartRegister.Text == "Dừng tạo My***")
            {
                MVTGlobalVar.RegisterVar.Stop = MVNPTGlobalVar.RegisterVar.Stop = MMFGlobalVar.RegisterVar.Stop = MVNMBGlobalVar.RegisterVar.Stop = true;
                return;
            }
            else
            {
                MVTGlobalVar.RegisterVar.Reset();
                MVNPTGlobalVar.RegisterVar.Reset();
                MMFGlobalVar.RegisterVar.Reset();
                MVNMBGlobalVar.RegisterVar.Reset();
                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu để [đăng ký/đặt lại mật khẩu]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //if (!Regex.IsMatch(txtPassword.Text, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$"))
                //{
                //    MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự, bao gồm [chữ in hoa, chữ thường, số, ký tự đặc biệt]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (txtPassword.Text.Length < 6 || txtPassword.Text.Length > 20)
                {
                    MessageBox.Show("Mật khẩu phải chứa ít nhất 6 đến 20 ký tự", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                SwitchRunning();
                MVTGlobalVar.RegisterVar.Password =
                MMFGlobalVar.RegisterVar.Password =
                MVNPTGlobalVar.RegisterVar.Password =
                MVNMBGlobalVar.RegisterVar.Password = txtPassword.Text;

                MVTRegisterHandler();
                MMFRegisterHandler();
                MVNPTRegisterHandler();
                MVNMBRegisterHandler();
            }
        }

        private void MVTRegisterHandler()
        {
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            SwitchStop();
                        }
                        catch { }
                    }));
                    return;
                }

                while (MVTGlobalVar.RegisterVar.RunningThread
                < MVTGlobalVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                SwitchStop();
                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MVTGlobalVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }



                    lock (MVTGlobalVar.RegisterVar.LockRunningThread)
                    {
                        MVTGlobalVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MVTAccount().Register();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }
        private void MMFRegisterHandler()
        {
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            SwitchStop();
                        }
                        catch { }
                    }));
                    return;
                }

                while (MMFGlobalVar.RegisterVar.RunningThread
                < MMFGlobalVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                SwitchStop();
                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MMFGlobalVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    lock (MMFGlobalVar.RegisterVar.LockRunningThread)
                    {
                        MMFGlobalVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MMFAccount().Register();
                                Thread.CurrentThread.Abort();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }
        private void MVNPTRegisterHandler()
        {
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            SwitchStop();
                        }
                        catch { }
                    }));
                    return;
                }

                while (MVNPTGlobalVar.RegisterVar.RunningThread
                < MVNPTGlobalVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                SwitchStop();
                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MVNPTGlobalVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }


                    lock (MVNPTGlobalVar.RegisterVar.LockRunningThread)
                    {
                        MVNPTGlobalVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MVNPTAccount().Register();
                                Thread.CurrentThread.Abort();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }

        private void MVNMBRegisterHandler()
        {
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MMFGlobalVar.RegisterVar.Stop && MVTGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            SwitchStop();
                        }
                        catch { }
                    }));
                    return;
                }

                while (MVNMBGlobalVar.RegisterVar.RunningThread
                < MVNMBGlobalVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MVTGlobalVar.RegisterVar.Stop && MMFGlobalVar.RegisterVar.Stop && MVNPTGlobalVar.RegisterVar.Stop && MVNMBGlobalVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                SwitchStop();
                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MVNMBGlobalVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }


                    lock (MVNMBGlobalVar.RegisterVar.LockRunningThread)
                    {
                        MVNMBGlobalVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MVNMBAccount().Register();
                                Thread.CurrentThread.Abort();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }


        private void btnExportMy_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewMy.ExportToXlsx(svd.FileName);
            }
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            txtAtcommandLog.Clear();
            txtGlobalMessage.Clear();
            RequestLoggerCenter.ClearLog();
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControlLog.SelectedTabPage == tabPageGlobalLog)
                {
                    Clipboard.SetText(txtGlobalMessage.Text);
                }
                if (tabControlLog.SelectedTabPage == tabPageATCommandLog)
                {
                    Clipboard.SetText(txtAtcommandLog.Text);
                }
            }
            catch { }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    ((GSMCom)row).DoNotConnect = true;

                }
            }
        }

        private void btnUSSD_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            if (selectedRowsHandle.Length == 0)
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                   MessageBoxIcon.Warning);
                return;
            }

            if (selectedRowsHandle.Length > 1)
            {
               
                List<GSMCom> coms = new List<GSMCom>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var com = ((GSMCom)row);
                        if (com.IsPortConnected && com.IsSIMConnected)
                        {
                            coms.Add(com);
                        }
                    }
                }
                GlobalVar.setAutoDashboardMode(false);
                new USSDMulUI(coms).ShowDialog(this);
            }
            else
            {
                var row = viewGSM.GetRow(selectedRowsHandle[0]);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected)
                    {
                        GlobalVar.setAutoDashboardMode(false);
                        new USSDUI(com).ShowDialog(this);
                    }
                }
            }
        }
        private void btnDataConsume_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<string> senders = new List<string>();
            List<GSMTransferModel> senderTranser = new List<GSMTransferModel>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);

                    if (com.IsPortConnected && com.IsSIMConnected && ((com.ModemName == "EC20F") || com.ModemName == "SIMCOM_SIM5320E" || com.ModemName == "UC20" || com.ModemName =="M26"))
                    {
                        senderTranser.Add(new GSMTransferModel()
                        {
                            PhoneNumber = com.PhoneNumber,
                            PortName = com.PortName,
                            AppPortName = com.DisplayName
                        });
                        //senders.Add(com.PhoneNumber);
                    }
                }
            }
            if (!senderTranser.Any())
            {
                MessageBox.Show("0 module 3G/4G, " + R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                 MessageBoxIcon.Warning);
                return;
            }
            initUrlDataConsume(senderTranser);
        }
        private async void initUrlDataConsume(List<GSMTransferModel> senders)
        {
            GSMPayloadSchemasInstance._payloadInstance = new GSMPayloadSchemas();
            //await GSMPayloadSchemasInstance._payloadInstance.getPayloadFromURLAsync("https://lamsim.biz/apis/gms/urlPsd");
            await GSMPayloadSchemasInstance._payloadInstance.getPayloadFromLocal();

            if (GSMPayloadSchemasInstance._payloadInstance.payloads == null || GSMPayloadSchemasInstance._payloadInstance.payloads.Count() <= 0)
            {
                MessageBox.Show(R.S("warning_consume_condition"), R.S("warning"), MessageBoxButtons.OK,
                 MessageBoxIcon.Warning);
            }
            else
            {
                GlobalVar.setAutoDashboardMode(false);

                new GSMDataConsumeUI(senders).Show(this);
            }
        }


        private void btnFeedback_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txtFeedback.EditValue != null)
            {
                string content = txtFeedback.EditValue.ToString();
                if (content.StartsWith("dev_private_feature"))
                {
                    if (Client.GetCurrentAccount().Username == "0846889911")
                    {
                        viewGSM.Columns["SignalStr"].Visible = true;
                        tabNavigationPage5.PageVisible = true;
                    }
                    return;
                }
                GlobalVar.CheckBalanceAndPhone = true;
                if (!string.IsNullOrEmpty(content))
                {
                    new C3TekPortal().Feedback(content);
                    txtFeedback.EditValue = string.Empty;
                    MessageBox.Show(R.S("thanks_feedback"), "Thanks!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(R.S("empty_feedback"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(R.S("empty_feedback"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void viewGSM_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
            {
                GridView view = sender as GridView;
                GridHitInfo hi = view.CalcHitInfo(e.Location);
                if (hi.InRowCell)
                {
                    view.FocusedRowHandle = hi.RowHandle;
                    view.FocusedColumn = hi.Column;
                    if (hi.Column == colManualPortName)
                    {
                        view.ShowEditor();
                    }
                    if (hi.Column == colConsole)
                    {
                        view.ShowEditor();
                        CheckEdit edit = (view.ActiveEditor as CheckEdit);
                        if (edit != null)
                        {
                            edit.Toggle();
                            (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
                        }
                    }


                }
            }
        }

        private void btnSaveConfiguration_Click(object sender, EventArgs e)
        {
            GlobalVar.UserSetting.Save();
            MessageBox.Show(R.S("saved"), R.S("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        {  if (GlobalVar.CheckRegManualImei)
            {
                if (this.imeiManualList.Count == 0)
                {
                    MessageBox.Show("Cần nạp danh sách imei đổi trước khi thực hiện thao tác này", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                List<string> allListImei = listCurrentImeiAvailable();
                int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                List<string> senders = new List<string>();
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                int[] tempseeder = GSMHelper.InitializeArrayWithNoDuplicates(selectedRowsHandle.Count());
                if (!selectedRowsHandle.Any())
                {
                    MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    MessageBox.Show(R.S("wait_update_imei"), R.S("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                for (int i = 0; i < selectedRowsHandle.Length; i++)
                {
                    var rowHandle = selectedRowsHandle[i];
                    var row = viewGSM.GetRow(rowHandle);
                    int idx = i;
                    if (row != null)
                    {
                        tasks.Add(Task.Factory.StartNew(async () =>
                        {
                            var com = ((GSMCom)row);
                            if (com.IsPortConnected)
                            {  random_tag:
                                Random rand = new Random();
                                int imei_index = rand.Next(1, imeiManualList.Count);
                                string imei = imeiManualList[imei_index];
                                if(allListImei.Contains(imei))
                                {
                                    goto random_tag;
                                }
                                com.ChangeImeiManual(imei);
                                await Task.Delay(100);
                            }
                        }));
                        await Task.Delay(1000);
                    }
                }
                await Task.WhenAll(tasks);
                await Task.Delay(10000);
                MessageBox.Show("Đã hoàn thành thay đổi imei thủ công cho thiết bị", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                List<string> senders = new List<string>();
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                int[] tempseeder = GSMHelper.InitializeArrayWithNoDuplicates(selectedRowsHandle.Count());
                if (!selectedRowsHandle.Any())
                {
                    MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    MessageBox.Show(R.S("wait_update_imei"), R.S("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                for (int i = 0; i < selectedRowsHandle.Length; i++)
                {
                    var rowHandle = selectedRowsHandle[i];
                    var row = viewGSM.GetRow(rowHandle);
                    int idx = i;
                    if (row != null)
                    {
                        tasks.Add(Task.Factory.StartNew(async () =>
                     {
                         var com = ((GSMCom)row);
                         if (com.IsPortConnected)
                         {
                             com.ChangeRandomIMEI();
                             await Task.Delay(100);
                         }
                     }));
                        await Task.Delay(2000);
                    }
                }
                await Task.WhenAll(tasks);
                await Task.Delay(10000);
                MessageBox.Show("Đã hoàn thành thay đổi imei thiết bị", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                /*   try
                   {
                       tasks.ForEach(task => task.Start());
                   }
                   catch { }*/

                ///new GSMDataConsumeUI(senders).ShowDialog(this);
            }
        }

        private void barStaticItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }

        private void ribbonStatusBar1_Click(object sender, EventArgs e)
        {

        }

        private void gridGSM_Click(object sender, EventArgs e)
        {

        }

        private void btnDownloadWav_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();

            var rowHandle = selectedRowsHandle[0];
            var row = viewGSM.GetRow(rowHandle);

            if (row != null)
            {
                var com = ((GSMCom)row);
                new Task(() =>
                {
                    try
                    {
                        com.testReadWav();
                        MessageBox.Show("OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }).Start();
            }
        }
        public List<string> listCurrentImeiAvailable()
        {
            List<string> allListImei = new List<string>();
            for(int i=0;i<viewGSM.RowCount;i++)
            {
                var row = viewGSM.GetRow(i);
                if (row != null)
                {
                    var com = (GSMCom)row;
                    if(!string.IsNullOrEmpty(com.ImeiDevice))
                    {
                        allListImei.Add(com.ImeiDevice);
                    }
                }
            }
            return allListImei;
        }
        private void barStaticItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }




        private void btnAdvanceUSSD_Click(object sender, EventArgs e)
        {
           // string userInput = Properties.Settings.Default.UserKey;
            if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_WEB_MULTI_USSD))
            {
                int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                if (selectedRowsHandle.Length == 0)
                {
                    MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (selectedRowsHandle.Length > 0)
                {
                    List<GSMCom> coms = new List<GSMCom>();
                    foreach (int rowHandle in selectedRowsHandle)
                    {
                        var row = viewGSM.GetRow(rowHandle);
                        if (row != null)
                        {
                            var com = ((GSMCom)row);
                            if (com.IsPortConnected && com.IsSIMConnected)
                            {
                                coms.Add(com);
                            }
                        }
                    }
                    GlobalVar.setAutoDashboardMode(false);
                    new WSAdvancedUSSD(coms).ShowDialog(this);
                }
            }
           /* else
            {  
                if (Properties.Settings.Default.UserKey == null || string.IsNullOrEmpty(Properties.Settings.Default.UserKey))
                {
                    if (AppModeSetting.Locale.Equals("km-KH"))
                    {
                        userInput = XtraInputBox.Show("បញ្ចូលលេខកូដអាជ្ញាប័ណ្ណរបស់អ្នក។", "គន្លឹះរក្សាសិទ្ធិ", "");
                    }
                    else if (AppModeSetting.Locale.Equals("zh-CN"))
                    {
                        userInput = XtraInputBox.Show("输入您的许可证密钥", "版权密钥", "");
                    }
                    else if (AppModeSetting.Locale.Equals("en-US"))
                    {
                        userInput = XtraInputBox.Show("Input your activation key", "Activation Key", "");
                    }
                    else
                    {
                        userInput = XtraInputBox.Show("Nhập key bản quyền cùa bạn", "Key bản quyền", "");
                    }
                    Properties.Settings.Default.UserKey = userInput;
                    Properties.Settings.Default.Save();
                }
                if (!string.IsNullOrEmpty(userInput))
                {  
                    int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                    if (selectedRowsHandle.Length == 0)
                    {
                        MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (selectedRowsHandle.Length > 0)
                    {

                        List<GSMCom> coms = new List<GSMCom>();
                        foreach (int rowHandle in selectedRowsHandle)
                        {
                            var row = viewGSM.GetRow(rowHandle);
                            if (row != null)
                            {
                                var com = ((GSMCom)row);
                                if (com.IsPortConnected && com.IsSIMConnected)
                                {
                                    coms.Add(com);
                                }
                            }
                        }
                        GlobalVar.setAutoDashboardMode(false);
                        new WSAdvancedUSSD(coms).ShowDialog(this);
                    }
                }
                else
                {
                    if (AppModeSetting.Locale.Equals("zh-CN"))
                    {
                        MessageBox.Show("您的密钥无效", "版权密钥", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (AppModeSetting.Locale.Equals("en-US"))
                    {
                        MessageBox.Show("Your activation key is invalid", "Activation Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (AppModeSetting.Locale.Equals("km-KH"))
                    {
                        MessageBox.Show("សោរបស់អ្នកមិនត្រឹមត្រូវទេ។", "គន្លឹះរក្សាសិទ្ធិ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Key của bạn không hợp lệ", "Key bản quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }*/
        }
        #region SOCKET_API
        private void btnSocketGetTaskList_Click(object sender, EventArgs e)
        {
            GSMControlCenter.webSocketClient.SendGetListTask();
        }

        #endregion

        private void btnSocketSyncGSMCom_Click(object sender, EventArgs e)
        {
            BindingList<GSMCom> coms = GSMControlCenter.GSMComs;
            List<GSMCom> listComsReady = new List<GSMCom>();
            foreach (GSMCom c in coms)
            {
                if (!string.IsNullOrEmpty(c.Serial))
                {
                    listComsReady.Add(c);
                }
            }
            GSMControlCenter.webSocketClient.SendUpdateGSM(listComsReady);
        }

        private void btnSocketConnect_Click(object sender, EventArgs e)
        {
            GSMControlCenter.webSocketClient.Open();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
           // string userInput = "";
            if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_WEB_MULTI_USSD))
            {
                int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                List<GSMCom> senders = new List<GSMCom>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var com = ((GSMCom)row);
                        if (com.IsPortConnected)
                        {
                            senders.Add(com);
                        }
                    }
                }
                if (!senders.Any())
                {
                    MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                    return;
                }
                GlobalVar.setAutoDashboardMode(false);
                new WSActivation(senders).ShowDialog(this);
            }
            /*else
            {
                if (AppModeSetting.Locale.Equals("km-KH"))
                {
                    userInput = XtraInputBox.Show("បញ្ចូលលេខកូដអាជ្ញាប័ណ្ណរបស់អ្នក។", "គន្លឹះរក្សាសិទ្ធិ", "");
                }
                else if (AppModeSetting.Locale.Equals("zh-CN"))
                {
                    userInput = XtraInputBox.Show("输入您的许可证密钥", "版权密钥", "");
                }
                else if (AppModeSetting.Locale.Equals("en-US"))
                {
                    userInput = XtraInputBox.Show("Input your activation key", "Activation Key", "");
                }
                else
                {
                    userInput = XtraInputBox.Show("Nhập key bản quyền cùa bạn", "Key bản quyền", "");
                }
                if(!string.IsNullOrEmpty(userInput))
                {

                    int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                    List<GSMCom> senders = new List<GSMCom>();
                    foreach (int rowHandle in selectedRowsHandle)
                    {
                        var row = viewGSM.GetRow(rowHandle);
                        if (row != null)
                        {
                            var com = ((GSMCom)row);
                            if (com.IsPortConnected)
                            {
                                senders.Add(com);
                            }
                        }
                    }
                    if (!senders.Any())
                    {
                        MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                        return;
                    }
                    GlobalVar.setAutoDashboardMode(false);
                    new WSActivation(senders).ShowDialog(this);
                }
                else
                {

                    if (AppModeSetting.Locale.Equals("zh-CN"))
                    {
                        MessageBox.Show("您的密钥无效", "版权密钥", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (AppModeSetting.Locale.Equals("en-US"))
                    {
                        MessageBox.Show("Your activation key is invalid", "Activation Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (AppModeSetting.Locale.Equals("km-KH"))
                    {
                        MessageBox.Show("សោរបស់អ្នកមិនត្រឹមត្រូវទេ។", "គន្លឹះរក្សាសិទ្ធិ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Key của bạn không hợp lệ", "Key bản quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }*/

        }

        private void btnWriteFile_Click(object sender, EventArgs e)
        {

        }

        private void btnCallAndPlay_Click(object sender, EventArgs e)
        {

        }

        private void btnCallAndPlay_Click_1(object sender, EventArgs e)
        {
            string phone_number = DialogHelper.ShowDialog("Phone Number", R.S("input_phone_number"));
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<GSMCom> senders = new List<GSMCom>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.ModemName == "M26")
                    {
                        senders.Add(com);
                    }
                }
            }

            //process
            var tasks = new List<Task>();
            foreach (var com in senders)
            {
                tasks.Add(new Task(() =>
                {
                    com.MakeCallAndPlay(phone_number, 30, true, "send.amr", 3);
                    Console.WriteLine("Task make call");
                }));
            }

            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("End task make call");

            }).Start();

            if (!senders.Any())
            {
                  MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                  MessageBoxIcon.Warning);
                  return;
            }

        }

        private void simpleButton1_Click_1(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse AMR Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "amr",
                Filter = "amr files (*.amr)|*.amr",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };
            MessageBox.Show(R.S("wait_upload_file"), R.S("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                string fn = openFileDialog1.FileName;
                int[] selectedRowsHandle = viewGSM.GetSelectedRows();
                List<GSMCom> senders = new List<GSMCom>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var com = ((GSMCom)row);
                        if (com.IsPortConnected && com.ModemName == "M26")
                        {
                            senders.Add(com);
                        }

                    }
                }
                //process
                var tasks = new List<Task>();
                foreach (var com in senders)
                {
                    tasks.Add(new Task(() =>
                    {
                        com.safeDownloadAudioToDevice("send.amr", fn);
                        //com.MakeCallAndPlay("0846889911", 30, true, "send.amr", 3);
                        Console.WriteLine("Task make call");
                    }));
                }

                new Task(() =>
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());
                    Console.WriteLine("End task make call");

                }).Start();

                if (!senders.Any())
                {
                    MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                  MessageBoxIcon.Warning);
                    return;
                }
            }

        }

        private void btnSocketRemoveHistory_Click(object sender, EventArgs e)
        {
            this.txtJobLog.Text = "";
        }

        private void simpleButton1_Click_2(object sender, EventArgs e)
        {
            AppSettingForm a = new AppSettingForm();
            a.btnGetProxy.Visible = false;
            if (Client.GetCurrentAccount().SubscriptionPackage.Equals("DELUX") || Client.GetCurrentAccount().SubscriptionPackage.Equals("BASIC") || Client.GetCurrentAccount().SubscriptionPackage.Equals("ADMIN"))
            {
                a.btnGetProxy.Visible = true;
            }
            a.ShowDialog(this);
        }

        private void ckCheckBalanceMode_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBox ckb = (CheckBox)sender;
            if (ckb.Checked)
            {   
                GlobalVar.CheckBalanceAndPhone = true;
                btnStartRegister.Enabled = true;
                List<Task> tasks = new List<Task>();
                foreach (GSMCom rowHandle in GSMControlCenter.GSMComs)
                {
                    tasks.Add(new Task(
                        () => { ((GSMCom)rowHandle).Reconnect(); }
                        ));
                }
                new Task(() =>
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());
                }).Start();
            }
            else
            {
                btnStartRegister.Enabled = false;
                GlobalVar.CheckBalanceAndPhone = false;

                //enable
            }
        }

        private void btnThanhToan_Click_1(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            if (selectedRowsHandle.Length == 0)
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                  MessageBoxIcon.Warning);
                return;
            }

            if (selectedRowsHandle.Length > 0)
            { 
                List<GSMCom> coms = new List<GSMCom>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var com = ((GSMCom)row);
                        if (com.IsPortConnected && com.IsSIMConnected)
                        {
                            coms.Add(com);
                        }
                    }
                }
                GlobalVar.setAutoDashboardMode(false);
                
                new USSDTopup(coms).ShowDialog(this);
            }
        }

        private void btnRequestOTP_Click(object sender, EventArgs e)
        {
           var telecomService =  new MyTelecom.MyTelecomService();
           
            //var responseGetOTPLogin = telecomService.PostApiMyVNPT<ResponseMyVNPT>("otp_send",
            //    new {msisdn = "0846889911", otp_service = "authen_register,payment_wallet_register"},
            //    (WebProxy)telecomService.GetProxy("http://p.webshare.io", 80, "oimvrhkb-rotate", "qa9f5qt1xgr3")
            //); 
            //var responseGetOTPLogin = telecomService.PostApiMyVNPT<ResponseMyVNPT>("otp_send",
            //    new { msisdn = "0846889911", otp_service = "authen_register,payment_wallet_register" },
            //    (WebProxy)telecomService.GetProxy("http://u1.p.webshare.io", 80, "yabyyibo-1", "r26brbb4m14f")
            //);

            var responseGetOtp = telecomService.PostApiMyViettel<ResponseMyViettelOtp>("getOtp", new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string> ("msisdn", "0982259245"),
                new KeyValuePair<string, string> ("device_name", "Nexus%205X"),
                new KeyValuePair<string, string> ("device_id",  StringHelper.RandomDeviceID()),
                new KeyValuePair<string, string> ("os_type", "0"),
                new KeyValuePair<string, string> ("os_version", "27"),
                new KeyValuePair<string, string> ("app_version", ""),
                new KeyValuePair<string, string> ("user_type", "1")
            })
            );
        }

        class ResponseMyViettelOtp
        {
            public int error_code { get; set; }
            public string message { get; set; }
        }
        class ResponseMyVNPT
        {
            public string error_code { get; set; }
            public string message { get; set; }
        }

        private void btnLoginOTP_Click(object sender, EventArgs e)
        {
            string otp = txtOTPLoginVNPT.Text;
            var telecomService = new MyTelecom.MyTelecomService();
            var responseGetOTPLogin = telecomService
                .SetWebProxy((WebProxy)telecomService.GetProxy("http://p.webshare.io", 80, "oimvrhkb-rotate", "qa9f5qt1xgr3"))
                .PostApiMyVNPT<ResponseMyVNPT>("authen_register",
                new { msisdn = "0846889911", pin = otp, password = SecurityHelper.CreateMD5("Matkhau123@!") }
         );
        }

        private void ckRegMyUseProxy_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ckb = (CheckBox)sender;
            if (ckb.Checked)
            {
                GlobalVar.CheckRegMyUseProxy = true;
            }
            else
            {
                GlobalVar.CheckRegMyUseProxy = false;
                //enable
            }

        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            if (selectedRowsHandle.Length == 0)
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (selectedRowsHandle.Length > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        tasks.Add(new Task(
                            () => { ((GSMCom)row).GetDeviceFirmware(); }
                        ));
                    }
                }
                new Task(() =>
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());
                }).Start();
            }
        }

        private void btnDeleteSMSModem_Click(object sender, EventArgs e)
        {
        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupControl2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkImeiManual_CheckedChanged(object sender, EventArgs e)
        {   
            CheckBox manual_imei_ckb = (CheckBox)sender;
            GlobalVar.CheckRegManualImei = false;
            if (manual_imei_ckb.Checked)
            {
                GlobalVar.CheckRegManualImei = true;
            }
        }

        private void simpleButton1_Click_3(object sender, EventArgs e)
        {
            ListImeiForm list_imei_form = new ListImeiForm();
            if (!GlobalVar.CheckRegManualImei)
            {
                GlobalVar.CheckRegManualImei = true;
                this.checkImeiManual.Checked = true;
            }
            list_imei_form.ShowDialog();
        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            List<string> ports_information = portInformation();
            List<string> simbank_ports = getSimbankPort(ports_information);
            string real_simbank_port = realSimBankPort(simbank_ports);
           
            List<GSMCom> senders = new List<GSMCom>();
            int rowHandle = 0;
            while(viewGSM.IsValidRowHandle(rowHandle))
            {
                var row = viewGSM.GetRow(rowHandle);
                if(row!=null)
                {
                    var gsmCom = (GSMCom)row;
                    senders.Add(gsmCom);
                }
                rowHandle++;
            }
            if (!senders.Any())
            {
                MessageBox.Show(R.S("choose_sim_dashboard_to_use"), R.S("warning"), MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
                return;
            }
            ChannelSimbank channelSimbank = new ChannelSimbank(senders, real_simbank_port);
            channelSimbank.ShowDialog();
        }

        private void viewGSM_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {

        }
    }
}
