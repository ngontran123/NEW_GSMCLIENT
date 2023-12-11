using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MSUnitTestC3Tek
{
    [TestClass]
    public class UnitTest1
    {
        public string ParseResponseBalanceExpireByPattern(string response, string pattern)
        {
            var matchExpire = Regex.Match(response, pattern);

            if (!string.IsNullOrEmpty(matchExpire.Value))
            {
                string expire_date = matchExpire.Value;
                return expire_date;
            }

            return "";
        }

        public string GetExpireMobiStr(string response)
        {
            List<string> listExpirePattern = new List<string>()
            {
                @"([0-9]{2}[-|\/]{1}[0-9]{2}[-|\/]{1}[0-9]{4})",
                @"(\\d{2}\\/\\d{2}\\/\\d{4})"
            };

            string expiredResult = "";
            foreach (string pattern in listExpirePattern)
            {
                expiredResult = ParseResponseBalanceExpireByPattern(response, pattern);
                if (!string.IsNullOrEmpty(expiredResult)) break;
            }

            return expiredResult;
        }


        [TestMethod]
        public void TestMobifoneBalanceExpire()
        {
            string ckBalanceResponse = "+CUSD: 0,\"+84785165730. MobiQ . TKC 10425 d, TK no 0VND, HSD: 00:00 28-06-2022. SMS, 100, 30-05-2022 SMS LM, 40, 30-05-2022\",15";

            string expectResponse = GetExpireMobiStr(ckBalanceResponse);
            Assert.AreEqual("28-06-2022", expectResponse);

            string ckBalanceResponse2 = "OK+CUSD: 0,\"MobiQ. TKC:9400 d, 17/12/2022. SMS: 100 SMS, 30/05/2022. SMS LM:40,30/05/2022. KM2V 1000 d. KM3V 1000 d.\"";

            string expectResponse2 = GetExpireMobiStr(ckBalanceResponse2);
            Assert.AreEqual("17/12/2022", expectResponse2);


        }

        public string GetBalanceMobiStr(string response)
        {
            List<string> listExpirePattern = new List<string>()
            {
                @"(TKC (.*?) d)",
                @"(TKC:(.*?) d)"
            };

            string expiredResult = "";
            foreach (string pattern in listExpirePattern)
            {
                expiredResult = ParseResponseBalanceExpireByPattern(response, pattern);
                if (!string.IsNullOrEmpty(expiredResult)) break;
            }

            if (expiredResult != null)
                return expiredResult.Replace("TKC", "")
                    .Replace(" ", "")
                    .Replace(":", "")
                    .Replace(",", "")
                    .Replace("." , "")
                    .Replace("d", "").Trim();
            return "";
        }

        [TestMethod]
        public void TestMobifoneBalance()
        {
            string responseUSSDBalance1 =
                "OK+CUSD: 0,\"+84785163402. MobiQ . TKC 3300 d, TK no 0VND, HSD: 00:00 28-06-2022. SMS, 100, 30-05-2022 SMS LM, 40, 30-05-2022\"";
            string expectResponse1 = GetBalanceMobiStr(responseUSSDBalance1);
            Assert.AreEqual("3300", expectResponse1);

            string responseUSSDBalance2 =
                "OK+CUSD: 0,\"MobiQ. TKC:9400 d, 17/12/2022. SMS: 100 SMS, 30/05/2022. SMS LM:40,30/05/2022. KM2V 1000 d. KM3V 1000 d.\"";
            string expectResponse2 = GetBalanceMobiStr(responseUSSDBalance2);
            Assert.AreEqual("9400", expectResponse2);

        }

        [TestMethod]
        public void TestViettelBalance()
        {
            string balance1 =
                "+CUSD: 1,\"84962101417. TKG: 47.000d, dung den 0h ngay 29/11/2022. Bam chon dang ky:1. 15K=3GB/3ngay2. 30K=7GB/7ngayHoac bam goi *098#\"";
            string balance2 =
                "+CUSD: 1,\"84986171813. TKG: 27000d, dung den 0h ngay 29/11/2022. Bam chon dang ky:1. 15K=3GB/3ngay2. 30K=7GB/7ngayHoac bam goi *098#\"";

            string response = balance2;
            int MainBalance;
            string Expire;
            var matchBalance = Regex.Match(response, "(TKG:(.*?)d)");
            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
            {
                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKG:", "").Replace("d", "").Replace(".", ""));
                //Assert.AreEqual(MainBalance, 47000);
                Assert.AreEqual(MainBalance, 27000);

            }
            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
            {
                Expire = matchExpire.Value;
            }
        }
    }
}
