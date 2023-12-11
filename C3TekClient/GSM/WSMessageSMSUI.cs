using C3TekClient.GSM.Model;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Spreadsheet;
using System.Threading;
using C3TekClient.Helper;
using C3TekClient.Properties;
using DevExpress.Utils.Svg;
using Newtonsoft.Json;

namespace C3TekClient.GSM
{
    public partial class WSMessageSMSUI : DevExpress.XtraEditors.XtraForm
    {
        bool Stop = true;
        bool scheduleSMS = false;
        bool runningSMS = false;

        Thread threadLogSession;

        List<GSMCom> _gsmComList;
        public BindingList<SendMessageSource> bindingListSMS = new BindingList<SendMessageSource>();

        private readonly object _lockerBindingList = new object();

        private readonly object _lockerCompleteCounter = new object();
        private readonly object _lockerSMS = new object();

        public int CompleteCounter { get; set; }

        public WSMessageSMSUI()
        {

            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.gridSMS.DataSource = bindingListSMS;

            CompleteCounter = 0;
            removeFeatureByPermission();
        }


        public void initThreadLogSession()
        {
            if (threadLogSession == null || !threadLogSession.IsAlive)
            {
                threadLogSession = new Thread(new ThreadStart(() =>
                {
                    while (!GlobalVar.IsApplicationExit && !Stop)
                    {
                        try
                        {
                            if (bindingListSMS == null)
                            {
                                continue;
                            }
                            List<SendMessageSource> listMessageSource = bindingListSMS.ToList();


                            SessionSendMessage sessionSendMessage = new SessionSendMessage()
                            {
                                ListMessageSources = listMessageSource,
                                ContentSms = txtMessageBody.Text ?? "",
                                ReceivePhone = txtPhoneTo.Text ?? ""
                            };
                            string jsonString = JsonConvert.SerializeObject(sessionSendMessage);
                            File.WriteAllText("sms.json", jsonString);
                        }
                        catch
                        {
                        }

                        Thread.Sleep(5000);
                    }
                }));
                try
                {
                    threadLogSession.Start();
                }
                catch (Exception exception)
                {
                }
            }
        }
        public WSMessageSMSUI(List<GSMCom> gsmComUse)
        {


            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this._gsmComList = gsmComUse;
            this.gridSMS.DataSource = bindingListSMS;
            this.FormClosing += WSMessageSMSUI_FormClosing;
            CompleteCounter = 0;
            removeFeatureByPermission();
        }
        public void removeFeatureByPermission()
        {
            btnImportExcel.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.SEND_SMS_EXCEL);
            btnExcelSample.Visible = AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.SEND_SMS_EXCEL);
        }
        private void WSMessageSMSUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
            GlobalVar.setAutoDashboardMode(true);
        }

        /// <summary>
        ///         prepare source data for text input
        /// </summary>
        public void prepareSourceData()
        {
            if (_gsmComList == null || _gsmComList.Count <= 0)
            {
                return;
            }
            bindingListSMS.Clear();


            foreach (GSMCom gsm in _gsmComList)
            {
                bindingListSMS.Add(new SendMessageSource()
                {
                    RealPort = gsm.PortName,
                    AppPort = gsm.DisplayName,
                    PhoneNumber = gsm.PhoneNumber,
                    PhoneTo = standardFormatString(txtPhoneTo.Text.Trim()),
                    State = ESendMessageSource.WAITING,
                    Body = txtMessageBody.Text.Trim(),
                    Status = R.S("in_queue")
                });
            }
            gridSMS.Refresh();

            //turn on schedule mode
            scheduleSMS = true;
            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM1",
            //    RealPort = "COM1",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",
            //    State = ESendMessageSource.WAITING
            //});
            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM2",
            //    RealPort = "COM2",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",
            //    State = ESendMessageSource.WAITING
            //});
            //bindingListSMS.Add(new SendMessageSource()
            //{
            //    AppPort = "COM3",
            //    RealPort = "COM3",
            //    PhoneNumber = "012345",
            //    PhoneTo = "0846889911",
            //    Status = "OK",
            //    Body = "ABC",

            //    State = ESendMessageSource.WAITING
            //});    
        }


        public bool HashQueueBindingCom(String com)
        {
            lock (_lockerSMS)
            {
                return bindingListSMS.Any(
                    queue => 
                        !queue.Resolved && queue.State == ESendMessageSource.WAITING && queue.RealPort == com
                                                   );
            }
        }
        private SendMessageSource GetQueueBindingCom(String com)
        {
            lock (_lockerSMS)
            {
                var _queue = bindingListSMS.FirstOrDefault(queue => !queue.Resolved && queue.State == ESendMessageSource.WAITING && queue.RealPort == com);

                if (_queue != null)
                {
                    _queue.State = ESendMessageSource.SENDING;
                }
                return _queue;
            }
        }
        public bool HashQueueBinding()
        {
            lock (_lockerSMS)
            {
                return bindingListSMS.Any(queue => !queue.Resolved
                && queue.State == ESendMessageSource.WAITING);
            }
        }
        private SendMessageSource GetQueueBinding()
        {
            lock (_lockerSMS)
            {
                var _queue = bindingListSMS.Where(queue => !queue.Resolved
                   && queue.State == ESendMessageSource.WAITING).FirstOrDefault();
                if (_queue != null)
                {
                    _queue.State = ESendMessageSource.SENDING;
                }
                return _queue;
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            startTask();
        }

        private string generateRandom(int nums)
        {
            string val = "";
            try
            {
                const string sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random();
                for(int i=0;i<nums;i++)
                {
                    val += sample[random.Next(sample.Length)];
                }
            }
            catch(Exception er)
            {
                Console.WriteLine(er.Message);
            }
            return val;
        }

        private string standardFormatString(string phone)
        {
            string standard_nums = "";
           try
            {  
                Regex reg = new Regex(@"[^\d]");
                standard_nums = reg.Replace(phone, "");
            }
            catch(Exception er)
            {
                Console.WriteLine(er.Message);
            }
            return standard_nums;
        }

        void updateCurrentCounter()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                lblCompleReport.Text = $"{CompleteCounter.ToString()}/{bindingListSMS.Count}";
            }));
        }
        void startTask()
        {
            //Click bat
            if (!Stop)
            {
               //Click de stop
                Stop = true; 
                btnStart.Text = "Bắt đầu";
                if (AppModeSetting.Locale.Equals("zh-CN"))
                {
                    btnStart.Text = "开始";

                }
                else if (AppModeSetting.Locale.Equals("en-US"))
                {
                    btnStart.Text = "Start";
                }
                try
                {
                    DevExpress.Utils.Svg.SvgImage playImage = Resources.gettingstarted;
                    btnStart.ImageOptions.SvgImage = playImage;
                    btnStart.Refresh();
                }
                catch { }
                return;
            }

            int fromSecond = Convert.ToInt32(txtDelaySecond.Value);
            int toSecond = Convert.ToInt32(toSecondTxt.Value);
            Random r = new Random();
            int delaySecond = r.Next(fromSecond, toSecond + 1);
            int numberWaitNetwork = Convert.ToInt32(txtNumberWaitNetwork.Text);
            int numsRandomWord = Convert.ToInt32(randomWord.Value);
            string randomWordGen = generateRandom(numsRandomWord);
            int nums_selected_grid = _gsmComList.Count;
            int limit_mess = Convert.ToInt32(limitMessTxt.Value);
           
            CompleteCounter = bindingListSMS.Count(e => e.Resolved);
            if (CompleteCounter == bindingListSMS.Count)
            {
                MessageBox.Show(
                    $"Đã xử lý {CompleteCounter}/{CompleteCounter}, vui lòng chuẩn bị dữ liệu trước khi bắt đầu");
                Stop = true;
                return;
            }
            updateCurrentCounter();

            pbSendProcess.Visible = true;
            pbSendProcess.Reset();
            pbSendProcess.Properties.Maximum = bindingListSMS.Count;
            pbSendProcess.Properties.Step = 1;
            pbSendProcess.EditValue = CompleteCounter;

            int epoch = 0;

            string selected = rgCallMode.Properties.Items[rgCallMode.SelectedIndex].Description;    
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            if (selected == "Đồng thời" || selected == "Concurrent" || selected=="同时")
            {
                if (scheduleSMS == true)
                {
                    foreach (SendMessageSource current_source in bindingListSMS)
                    {   
                        tasks.Add(new Task(() =>
                        {
                            try
                            {
                                current_source.Status = R.S("processing");
                                current_source.State = ESendMessageSource.SENDING;
                                current_source.Resolved = false;

                                if(!string.IsNullOrEmpty(current_source.Body) && numsRandomWord>0)
                                {
                                    current_source.Body += string.Concat(Enumerable.Repeat("\n", 3))+randomWordGen;
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));

                                GSMCom com = _gsmComList.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                if (com == null)
                                {
                                    com = GSMControlCenter.GSMComs.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                }

                                if (com == null)
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = "Khong ton tai COM "+ current_source.RealPort;
                                    return;
                                }
                                string response = com.SendMessage(current_source.PhoneTo, current_source.Body, numberWaitNetwork);

                                if (response.Equals("NOT_CONNECTED_OR_PHONE"))
                                {
                                    //fail
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = R.S("sms_error_connection");
                                }
                                else if (response.Equals("ERROR"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = R.S("sms_error_send");
                                    current_source.Resolved = true;
                                }
                                else if (response.Equals("ERROR_PHONE_NUMBER"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = "SĐT không đúng";
                                    current_source.Resolved = true;
                                }
                                else if (response.Equals("OK_NOT_STATUS"))
                                {
                                    current_source.State = ESendMessageSource.SENT_SUCESS;
                                    current_source.Status = R.S("sms_success_no_state");
                                    current_source.Resolved = true;
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                        updateCurrentCounter();
                                    }
                                }
                                else
                                {
                                    current_source.State = ESendMessageSource.SENT_SUCESS;
                                    current_source.Status = R.S("success");
                                    current_source.Resolved = true;
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                        updateCurrentCounter();
                                    }
                                }
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

                                //FAKE SIMULATE TASK
                                //Thread.Sleep(4000);
                                //current_source.Status = "Thành công";
                                //current_source.Resolved = true;
                                //current_source.State = ESendMessageSource.SENT_SUCESS; 

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));

                                if (Stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                            Thread.Sleep(delaySecond * 1000);
                        }));
                    }
                }
                else if (scheduleSMS == false) {
                    
                    foreach (SendMessageSource current_source in bindingListSMS)
                    {
                        tasks.Add(new Task(() =>
                        {
                            try
                            {
                                current_source.Status = R.S("processing");
                                current_source.State = ESendMessageSource.SENDING;
                                current_source.Resolved = false;

                                if (!string.IsNullOrEmpty(current_source.Body) && numsRandomWord > 0)
                                {
                                    current_source.Body += string.Concat(Enumerable.Repeat("\n", 3)) + randomWordGen;
                                }

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));

                                GSMCom com = _gsmComList.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                if (com == null)
                                {
                                    com = GSMControlCenter.GSMComs.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                }
                                
                                if (com == null)
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = "Khong ton tai COM " + current_source.RealPort;
                                    return;
                                }
                                if(com.LimitMess>limit_mess)
                                {
                                    
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = "Sim đã gửi quá giới hạn tin nhắn đã thiết lập.";
                                    if (AppModeSetting.Locale.Equals("zh-CN"))
                                    {
                                        current_source.Status = "超过寄信限制.";

                                    }
                                    else if (AppModeSetting.Locale.Equals("en-US"))
                                    {
                                        btnStart.Text = "Passed the mail sent limit";
                                    }
                                    return;
                                }
                              
                                string response = com.SendMessage(current_source.PhoneTo, current_source.Body, numberWaitNetwork);

                                if (response.Equals("NOT_CONNECTED_OR_PHONE"))
                                {
                                    //fail
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = R.S("sms_error_connection");
                                }
                                else if (response.Equals("ERROR"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = R.S("sms_error_send");
                                    current_source.Resolved = true;
                                }
                                else if (response.Equals("ERROR_PHONE_NUMBER"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = "SĐT không đúng";
                                    current_source.Resolved = true;
                                }
                                else if (response.Equals("OK_NOT_STATUS"))
                                {
                                    com.LimitMess += 1;
                                    current_source.State = ESendMessageSource.SENT_SUCESS;
                                    current_source.Status = R.S("sms_success_no_state");
                                    current_source.Resolved = true;
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                        updateCurrentCounter();
                                    }
                                }
                                else
                                {
                                    com.LimitMess += 1;
                                    current_source.State = ESendMessageSource.SENT_SUCESS;
                                    current_source.Status = R.S("success");
                                    current_source.Resolved = true;
                                    
                                    lock (_lockerCompleteCounter)
                                        CompleteCounter += 1;                                    {
                                        updateCurrentCounter();
                                    }
                                }
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

                                //FAKE SIMULATE TASK
                                //Thread.Sleep(4000);
                                //current_source.Status = "Thành công";
                                //current_source.Resolved = true;
                                //current_source.State = ESendMessageSource.SENT_SUCESS; 

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));

                                if (Stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }));
                    }
                    /* foreach (SendMessageSource current_source in bindingListSMS)
                     {
                         GSMCom com = GSMControlCenter.GSMComs.FirstOrDefault(s => s.PortName == current_source.RealPort);
                         if (!_gsmComList.Contains(com))
                         {
                             _gsmComList.Add(com);
                         }
                     }*/


                    /*  foreach (var comThread in _gsmComList)
                      {

                          tasks.Add(new Task(() =>
                          {
                              while (HashQueueBindingCom(comThread.PortName))
                              {
                                  try
                                  {
                                      if (Stop)
                                      {
                                          return;
                                      }

                                      SendMessageSource current_source = GetQueueBindingCom(comThread.PortName);
                                      if (!string.IsNullOrEmpty(current_source.Body) && numsRandomWord > 0)
                                      {
                                          current_source.Body += string.Concat(Enumerable.Repeat("\n", 3)) + randomWordGen;
                                      }
                                      GSMCom com = _gsmComList.FirstOrDefault(s => s.PortName == current_source.RealPort);
                                      if (com == null)
                                      {
                                          com = GSMControlCenter.GSMComs.FirstOrDefault(s => s.PortName == current_source.RealPort);
                                      }

                                      if (com == null)
                                      {
                                          current_source.State = ESendMessageSource.SENT_FAIL;
                                          current_source.Resolved = true;
                                          current_source.Status = "Khong ton tai COM "+ current_source.RealPort;
                                          return;
                                      }

                                      current_source.Status = R.S("processing");
                                      current_source.State = ESendMessageSource.SENDING;
                                      current_source.RealPort = com.PortName;
                                      current_source.AppPort = com.DisplayName;
                                      current_source.PhoneNumber = com.PhoneNumber;

                                      this.Invoke(new MethodInvoker(() =>
                                      {
                                          gridSMS.RefreshDataSource();
                                      }));

                                      // execute sms
                                      string response = com.SendMessage(current_source.PhoneTo, current_source.Body, numberWaitNetwork);
                                      if (response.Equals("NOT_CONNECTED_OR_PHONE"))
                                      {
                                          //fail
                                          current_source.State = ESendMessageSource.SENT_FAIL;
                                          current_source.Resolved = true;
                                          current_source.Status = R.S("sms_error_connection");
                                      }
                                      else if (response.Equals("ERROR"))
                                      {
                                          current_source.State = ESendMessageSource.SENT_FAIL;
                                          current_source.Status = R.S("sms_error_send");
                                          current_source.Resolved = true;
                                      }
                                      else if (response.Equals("ERROR_PHONE_NUMBER"))
                                      {
                                          current_source.State = ESendMessageSource.SENT_FAIL;
                                          current_source.Status = "SĐT không đúng";
                                          current_source.Resolved = true;
                                      }
                                      else
                                      {
                                          current_source.State = ESendMessageSource.SENT_SUCESS;
                                          current_source.Status = R.S("success");
                                          current_source.Resolved = true;
                                          lock (_lockerCompleteCounter)
                                          {
                                              CompleteCounter += 1;
                                              updateCurrentCounter();
                                          }
                                      }
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

                                      //FAKE SIMULATE TASK
                                      //Thread.Sleep(4000);
                                      //current_source.Status = "Thành công";
                                      //current_source.Resolved = true;
                                      //current_source.State = ESendMessageSource.SENT_SUCESS; 

                                      this.Invoke(new MethodInvoker(() =>
                                      {
                                          gridSMS.RefreshDataSource();
                                      }));

                                      if (Stop)
                                      {
                                          return;
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      Console.WriteLine("[WSMEssageSMSUI - SendSMSTask - Dong thoi]");
                                  }
                                  Thread.Sleep(delaySecond * 1000);
                              }
                          }));
                      }*/
                }
                else
                {
                    //foreach (var comCho in _gsmComList)
                    //{

                    tasks.Add(new Task(() =>
                    {
                        while (HashQueueBinding())
                        {
                            try
                            {
                                if (Stop)
                                {
                                    return;
                                }

                                SendMessageSource current_source = GetQueueBinding();
                               
                                if (!string.IsNullOrEmpty(current_source.Body) && numsRandomWord > 0)
                                {
                                    current_source.Body += string.Concat(Enumerable.Repeat("\n", 3)) + randomWordGen;
                                }

                                GSMCom com = _gsmComList.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                if (com == null)
                                {
                                    com = GSMControlCenter.GSMComs.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                                }

                                if (com == null)
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = "Khong ton tai COM "+ current_source.RealPort;
                                    return;
                                }

                                current_source.Status = R.S("processing");
                                current_source.State = ESendMessageSource.SENDING;
                                current_source.RealPort = com.PortName;
                                current_source.AppPort = com.DisplayName;
                                current_source.PhoneNumber = com.PhoneNumber;

                                this.Invoke(new MethodInvoker(() =>
                                {
                                    gridSMS.RefreshDataSource();
                                }));

                                    // execute sms
                                    string response = com.SendMessage(current_source.PhoneTo, current_source.Body, numberWaitNetwork);
                                if (response.Equals("NOT_CONNECTED_OR_PHONE"))
                                {
                                        //fail
                                        current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Resolved = true;
                                    current_source.Status = R.S("sms_error_connection");
                                }
                                else if (response.Equals("ERROR"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = R.S("sms_error_send");
                                    current_source.Resolved = true;
                                }
                                else if (response.Equals("ERROR_PHONE_NUMBER"))
                                {
                                    current_source.State = ESendMessageSource.SENT_FAIL;
                                    current_source.Status = "SĐT không đúng";
                                    current_source.Resolved = true;
                                }
                                else
                                {
                                    current_source.State = ESendMessageSource.SENT_SUCESS;
                                    current_source.Status = R.S("success");
                                    current_source.Resolved = true;
                                    lock (_lockerCompleteCounter)
                                    {
                                        CompleteCounter += 1;
                                        updateCurrentCounter();
                                    }
                                }
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

                                    //FAKE SIMULATE TASK
                                    //Thread.Sleep(4000);
                                    //current_source.Status = "Thành công";
                                    //current_source.Resolved = true;
                                    //current_source.State = ESendMessageSource.SENT_SUCESS; 

                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        gridSMS.RefreshDataSource();
                                    }));

                                if (Stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[WSMEssageSMSUI - SendSMSTask - Dong thoi]");
                            }
                            Thread.Sleep(delaySecond * 1000);
                        }
                    }));
                    //}

                }

                new Task(async() =>
                {
                    //epoch process
                    //step1. preparing the data
                    try
                    {
                        int batch_size = _gsmComList.Count;
                        for(int i=0;i<tasks.Count;i+=batch_size)
                        {
                            List<Task> currentBatch = tasks.GetRange(i, Math.Min(batch_size, tasks.Count - i));
                            currentBatch.ForEach(task => task.Start());
                            await Task.WhenAll(currentBatch);
                            await Task.Delay(delaySecond*1000);
                        }
                        /*tasks.ForEach(task => task.Start());
                        System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), 60);*/
                        Console.WriteLine("Log ok");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception on execute task" + ex.Message);
                    }

                }).Start();
            }
            else
            {
                //tuan tu
                new Task(() =>
                {
                    int gsmidx = 0;

                    //while (gsmidx < _gsmComList.Count)
                    //{ 
                    while (HashQueueBinding())
                    {
                        try
                        {

                            SendMessageSource current_source = GetQueueBinding();
                            
                            if (!string.IsNullOrEmpty(current_source.Body) && numsRandomWord > 0)
                            {
                                current_source.Body += string.Concat(Enumerable.Repeat("\n", 3)) + randomWordGen;
                            }
                            GSMCom com = _gsmComList.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                            if (com == null)
                            {
                                com = GSMControlCenter.GSMComs.Where(s => s.PortName == current_source.RealPort).FirstOrDefault();
                            }

                            if (com == null)
                            {
                                current_source.State = ESendMessageSource.SENT_FAIL;
                                current_source.Resolved = true;
                                current_source.Status = "Khong ton tai COM "+ current_source.RealPort;
                                return;
                            }

                            current_source.Status = R.S("processing");
                            current_source.State = ESendMessageSource.SENDING;
                            current_source.RealPort = com.PortName;
                            current_source.AppPort = com.DisplayName;
                            current_source.PhoneNumber = com.PhoneNumber;
                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridSMS.RefreshDataSource();
                            }));

                            string response = com.SendMessage(current_source.PhoneTo, current_source.Body);

                            // execute sms
                            //string response = _gsmComList[gsmidx].SendMessage(current_source.PhoneTo, current_source.Body);
                            if (response.Equals("NOT_CONNECTED_OR_PHONE"))
                            {
                                //fail
                                current_source.State = ESendMessageSource.SENT_FAIL;
                                current_source.Resolved = true;
                                current_source.Status = R.S("sms_error_connection");
                            }
                            else if (response.Equals("ERROR"))
                            {
                                current_source.State = ESendMessageSource.SENT_FAIL;
                                current_source.Status = R.S("sms_error_send");
                                current_source.Resolved = true;
                            }
                            else if (response.Equals("ERROR_PHONE_NUMBER"))
                            {
                                current_source.State = ESendMessageSource.SENT_FAIL;
                                current_source.Status = "SĐT không đúng";
                                current_source.Resolved = true;
                            }
                            else
                            {
                                current_source.State = ESendMessageSource.SENT_SUCESS;
                                current_source.Status = R.S("success");
                                current_source.Resolved = true;
                                lock (_lockerCompleteCounter)
                                {
                                    CompleteCounter += 1;
                                    updateCurrentCounter();
                                }
                            }

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

                            //execute task send message and check reponse here
                            //fake SIMULATE
                            /*
                            Thread.Sleep(4000);
                            current_source.Status = "Thành công";
                            current_source.Resolved = true;
                            current_source.State = ESendMessageSource.SENT_SUCESS;
                            */
                            this.Invoke(new MethodInvoker(() =>
                            {
                                gridSMS.RefreshDataSource();
                            }));

                            if (Stop)
                            {
                                return;
                            }
                            Thread.Sleep(delaySecond * 1000);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[WSMEssageSMSUI - SendSMSTask - Tuan tu]" + ex.Message);
                        }

                    }

                    if (!HashQueueBinding())
                    {



                        try
                        {
                            Stop = true;
                            btnStart.Text = "Bắt đầu";
                            DevExpress.Utils.Svg.SvgImage playImage = Resources.gettingstarted;
                            btnStart.ImageOptions.SvgImage = playImage;
                            btnStart.Refresh();
                        }
                        catch { }
                    }
                    //else
                    //{
                    //    break;
                    //}
                    ////check end of idx => reset to 0

                    //if (gsmidx >= _gsmComList.Count - 1)
                    //{
                    //    gsmidx = 0;
                    //}
                    //else { gsmidx++; }
                    //}
                }).Start();
            }

            btnStart.Text = "Tạm dừng";
            if (AppModeSetting.Locale.Equals("zh-CN"))
            {
                btnStart.Text = "停止";

            }
            else if (AppModeSetting.Locale.Equals("en-US"))
            {
                btnStart.Text = "Stop";
            }

            Stop = false;
            try
            {
                initThreadLogSession();
                DevExpress.Utils.Svg.SvgImage pauseImage = Resources.pause;
                btnStart.ImageOptions.SvgImage = pauseImage;
                btnStart.Refresh();
            }
            catch { }
        }




        #region LOAD_AND_HANDLE_EVENT
        private void WSMessageSMSUI_Load(object sender, EventArgs e)
        {
            viewSMS.PopupMenuShowing += GridView_PopupMenuShowing;
            viewSMS.Appearance.SelectedRow.BackColor = Color.FromArgb(0, 0, 0, 0);
            viewSMS.Appearance.FocusedRow.BackColor = Color.FromArgb(0, 0, 0, 0);

        }
        private void GridView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                DXMenuItem item = new DXMenuItem(R.S("delete_all"));
                item.Click += (o, args) =>
                {
                    this.bindingListSMS.Clear();
                    this.gridSMS.Refresh();
                };
                e.Menu.Items.Add(item);
            }
        }

        #endregion

        private void btnPrepareData_Click(object sender, EventArgs e)
        {
            prepareSourceData();
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {

            importExcel();
        }

        private bool checkSimRemain(int limit,List<GSMCom> gsm_list)
        {
            int remain_limit_sim = 0;
            for (int j = 0; j < gsm_list.Count; j++)
            {
                GSMCom new_com = _gsmComList[j];
                if (new_com == null)
                {
                    new_com = GSMControlCenter.GSMComs[j];
                }
                if (new_com.LimitMess < limit)
                {
                    remain_limit_sim++;
                }
            }
            if (remain_limit_sim == 0)
            {
                return false;
            }
            return true;
        }
        private void importExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose file";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Excel File (*.XLXS)|*.XLSX";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                for(int i=0;i<_gsmComList.Count;i++)
                {
                    GSMCom new_com = _gsmComList[i];
                    if (new_com == null)
                    {
                        new_com = GSMControlCenter.GSMComs[i];
                    }
                    new_com.LimitMess = 0;
                }

                new Task(() =>
                {
                    try
                    {
                        Workbook workbook = new Workbook();
                        // Load a workbook from the file.
                        workbook.LoadDocument(openFileDialog.FileName, DocumentFormat.Xlsx);



                        this.Invoke(new MethodInvoker(() =>
                        {
                            bindingListSMS.Clear();
                            gridSMS.Refresh();
                            gridSMS.RefreshDataSource();
                        }));

                        if (_gsmComList == null || _gsmComList.Count <0)
                        {
                            MessageBox.Show("Vui lòng chọn danh sách cổng ở dashboard");
                            return;
                        }

                        if (workbook.Worksheets.Count > 0)
                        {
                            
                            Worksheet worksheet = workbook.Worksheets[0];
                            RowCollection rows = worksheet.Rows;
                            CellRange usedRange = worksheet.GetUsedRange();
                            int count_error_record = 0;
                            int limit_mess = Convert.ToInt32(limitMessTxt.Value);
                            int currentGsmIdx = 0;
                            for (int i = 0; i < usedRange.RowCount; i++)
                            {
                                string phoneNumberTo = (worksheet[i, 0].Value != null) ? worksheet[i, 0].Value.ToString() : "";
                                phoneNumberTo = standardFormatString(phoneNumberTo);
                                string body = (worksheet[i, 1].Value != null) ? worksheet[i, 1].Value.ToString() : "";
                                string count_str = (worksheet[i, 2].Value != null) ? worksheet[i, 2].Value.ToString() : "";
                                int count = 0;
                                if (Int32.TryParse(count_str, out count))
                                {
                                    //ok giới hạn 100 tin 1 sim thôi
                                    if (!string.IsNullOrEmpty(phoneNumberTo) && !string.IsNullOrEmpty(body) && count >= 1 && count <= 100000)
                                    {
                                        for (int k = 0; k < count; k++)
                                        {
                                           

                                            //int idxGsm = k % _gsmComList.Count;
                                            GSMCom com = _gsmComList[currentGsmIdx];
                                            if (com == null)
                                            {
                                                com = GSMControlCenter.GSMComs[currentGsmIdx];
                                            }
                                            bindingListSMS.Add(new SendMessageSource()
                                            {
                                                RealPort = com.PortName,
                                                AppPort = com.DisplayName,
                                                PhoneNumber = com.PhoneNumber,
                                                Body = body,
                                                PhoneTo = phoneNumberTo,
                                                State = ESendMessageSource.WAITING,
                                                Status = R.S("in_queue")
                                            });
                                            currentGsmIdx += 1;
                                      
                                            if (currentGsmIdx > _gsmComList.Count - 1)
                                            {
                                                currentGsmIdx = 0;
                                            }
                                           
                                        }

                                    }
                                    else
                                    {
                                        count_error_record++;
                                    }
                                }

                                else
                                {
                                    //process not ok 
                                    count_error_record++;
                                }
                            
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    lblCompleReport.Text = $"0/{bindingListSMS.Count.ToString() ?? ""}";
                                }));

                            }
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show(this, R.S("check_excel_file"));
                            }));
                        }

                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, R.S("error_read_another_soft"));
                        }));
                    }
                    this.Invoke(new MethodInvoker(() =>
                    {
                        gridSMS.Refresh();
                        gridSMS.RefreshDataSource();
                    }));

                }).Start();
                scheduleSMS = false;
            }
        }

        private void gridSMS_Click(object sender, EventArgs e)
        {

        }

        private void btnRetry_click(object sender, EventArgs e)
        {
            IReadOnlyList<SendMessageSource> filterList = bindingListSMS.Where(s => s.State != ESendMessageSource.SENT_FAIL).ToList();
            foreach (SendMessageSource item in filterList)
            {
                bindingListSMS.Remove(item);
            }

            this.gridSMS.Refresh();

            foreach (SendMessageSource item in bindingListSMS)
            {
                item.Resolved = false;
                item.State = ESendMessageSource.WAITING;
                item.Status = R.S("in_queue");
            }
            if (bindingListSMS.Count <= 0)
            {
                MessageBox.Show(R.S("success"));
            }
            startTask();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridSMS.ExportToXlsx(svd.FileName);
            }
        }

        private void btnExcelSample_Click(object sender, EventArgs e)
        {
            FileHelper.openExcelAndReleaseFile(PathHelper.getPathExecute() + "/mau_sms.xlsx");
        }

        private void labelControl5_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void labelControl8_Click(object sender, EventArgs e)
        {

        }

        private void labelControl4_Click(object sender, EventArgs e)
        {

        }

        private void btnSaveSession_Click(object sender, EventArgs e)
        {
            List<SendMessageSource> listMessageSource = bindingListSMS.ToList();


            SessionSendMessage sessionSendMessage = new SessionSendMessage()
            {
                ListMessageSources = listMessageSource,
                ContentSms = txtMessageBody.Text ?? "",
                ReceivePhone = txtPhoneTo.Text ?? ""
            };
            string jsonString = JsonConvert.SerializeObject(sessionSendMessage);
            File.WriteAllText("sms.json", jsonString);
            MessageBox.Show("Lưu phiên thành công");
        }

        private void btnOpenLastSession_Click(object sender, EventArgs e)
        {
            SessionSendMessage sessionSendMessage = null;
            try
            {
                if (File.Exists("sms.json"))
                {
                    string jsonData = File.ReadAllText("sms.json");
                    sessionSendMessage = ConvertHelper.ParseJsonObject<SessionSendMessage>(jsonData);
                    if (sessionSendMessage ==null || sessionSendMessage.ListMessageSources ==null)
                    {
                        MessageBox.Show("Có lỗi xảy ra khi Load dữ liệu");
                        return;
                    }
                }

                else
                {
                    MessageBox.Show("Không tồn tại phiên dữ liệu cũ");
                    return;
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Có lỗi xảy ra khi Load dữ liệu");
                return;
            }
            bindingListSMS.Clear();

            foreach (SendMessageSource item in sessionSendMessage.ListMessageSources)
            {
                bindingListSMS.Add(item);
            }

            gridSMS.Refresh();
            txtMessageBody.Text = sessionSendMessage.ContentSms;
            txtPhoneTo.Text = sessionSendMessage.ReceivePhone;
            //turn on schedule mode
            scheduleSMS = true;
            MessageBox.Show("Đã load dữ liệu SMS thành công " + bindingListSMS.Count.ToString());
        }

        private void txtDelaySecond_ValueChanged(object sender, EventArgs e)
        {

        }

        private void xóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
             
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}