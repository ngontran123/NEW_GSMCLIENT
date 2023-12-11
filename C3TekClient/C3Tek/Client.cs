using DevExpress.Internal;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C3TekClient.C3Tek
{
    public static class Client
    {
        
        public static bool __DEVELOPING__ = false;
        public static string AccessToken { get; set; }
        
        public static  bool isBuyModem { get; set; }
        public static ESubscriptionPackage SubcriptionPackage { get; set; }
            
        
        
        public static C3TekAccount _CurrentAccount { get; set; }

        public static C3TekAccount GetCurrentAccount()
        {
            if (_CurrentAccount == null)
            {
                var loginForm = new LoginUI() { TopMost = true };
                            

                loginForm.Submit += (phone, password, submitType, remember) =>
                {
                    try
                    {
                        var portal = new C3TekPortal();
                        var loginInfo = portal.Login(phone, password);
                        if (loginInfo != null)
                        {
                            Console.WriteLine(loginInfo.token);
                            if (!string.IsNullOrEmpty(loginInfo.token))
                            {
                                if (loginInfo.data.is_stk == true && loginInfo.data.phone != "0846889911")
                                {
                                    loginForm.Failure("Tài khoản bạn không có gói C3TekClient");
                                }
                                else
                                {
                                    _CurrentAccount = new C3TekAccount()
                                    {
                                        Username = loginInfo.data.phone,
                                        Password = password,
                                        Balance = loginInfo.data.amount,
                                        Name = loginInfo.data.name,
                                        Is_buy_modem = loginInfo.data.is_buy_modem,
                                        Message_Qc = "Quảng cáo từ server",
                                        Is_Subscription = loginInfo.data.is_subscription,
                                        //SubscriptionName = "SUBSCRIPTION_BASIC"
                                        SubscriptionPackage = loginInfo.data.subscription_package,
                                        IsOtherUser = loginInfo.data.is_other_user,
                                        IsSTK = loginInfo.data.is_stk,
                                        ListBuyModem = loginInfo.data.list_buy_modem
                                    };


                                    //if (string.IsNullOrEmpty(_CurrentAccount.SubscriptionName) ||     _CurrentAccount.SubscriptionName.Equals("BLANK")) 
                                    ////TODO: FAKE
                                    //{

                                    //}
                                    if (!GetCurrentAccount().IsOtherUser) //dang ki goi
                                    {
                                        if (_CurrentAccount.Is_Subscription)
                                        {
                                            if (_CurrentAccount.SubscriptionPackage.Equals("") ||
                                                _CurrentAccount.SubscriptionPackage.Equals("BLANK"))
                                            {
                                                Client.SubcriptionPackage = ESubscriptionPackage.NONE;
                                            }
                                            else if (_CurrentAccount.SubscriptionPackage.Equals("BASIC"))
                                            {
                                                Client.SubcriptionPackage = ESubscriptionPackage.SUB_BASIC;
                                            }
                                            else if (_CurrentAccount.SubscriptionPackage.Equals("DELUX"))
                                            {
                                                Client.SubcriptionPackage = ESubscriptionPackage.SUB_DELUX;
                                            }

                                            Client.AccessToken = loginInfo.token;
                                            Client.isBuyModem = _CurrentAccount.Is_buy_modem;                                            if (remember)

                                            {
                                                GlobalVar.UserSetting.RememberLogin = remember;
                                                GlobalVar.UserSetting.Username = phone;
                                                GlobalVar.UserSetting.PasswordHash =
                                                    new C3TekPortal().Encrypt(password);
                                                GlobalVar.UserSetting.Save();
                                            }
                                            else
                                            {
                                                GlobalVar.UserSetting.RememberLogin = remember;
                                                GlobalVar.UserSetting.Username = string.Empty;
                                                GlobalVar.UserSetting.PasswordHash = string.Empty;

                                                GlobalVar.UserSetting.Save();
                                            }

                                            loginForm.Close();
                                            if (loginInfo.data.phone == "0846889911")
                                            {
                                                Client.SubcriptionPackage = ESubscriptionPackage.ADMIN;

                                            }
                                        }

                                        else
                                        {
                                            Client.SubcriptionPackage = ESubscriptionPackage.NONE;
                                            loginForm.Failure(R.S("login_subscription"));   
                                        }
                                    }
                                    else
                                    {
                                        Client.SubcriptionPackage = ESubscriptionPackage.NONE;
                                        Client.AccessToken = loginInfo.token;
                                        Client.isBuyModem = _CurrentAccount.Is_buy_modem;
                                        if (remember)
                                        {
                                            GlobalVar.UserSetting.RememberLogin = remember;
                                            GlobalVar.UserSetting.Username = phone;
                                            GlobalVar.UserSetting.PasswordHash = new C3TekPortal().Encrypt(password);
                                            GlobalVar.UserSetting.Save();
                                        }
                                        else
                                        {
                                            GlobalVar.UserSetting.RememberLogin = remember;
                                            GlobalVar.UserSetting.Username = string.Empty;
                                            GlobalVar.UserSetting.PasswordHash = string.Empty;
                                            GlobalVar.UserSetting.Save();
                                        }

                                        loginForm.Close();
                                    }
                                }


                                //FAKE

                                //

                            }
                            else
                            {
                                loginForm.Failure(R.S("invalid_login"));
                            }
                        }
                        else
                        {
                            loginForm.Failure(portal.LastMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        loginForm.Failure($"Kết nối đến máy chủ thất bại {ex.Message}");
                    }
                };
                loginForm.FormClosed += (sender, @event) => { if (string.IsNullOrEmpty(AccessToken)) { GlobalVar.ForceKillMyself(); return; }; };
                
                loginForm.ShowDialog();
              
            }
            return _CurrentAccount;
        }
    }
}
