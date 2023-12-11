using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C3TekTest
{
    class Program
    {
        static List<string> GetMessage()
        {
            List<string> listMessage = new List<string>();
            string noidung =
               "\r\n+CMGL: 1,\"REC UNREAD\",\"+66816144755\",\"\",\"2022/03/18 21:29:06+28\"\r\nhttp://www.smscaster.com test gui tin\r\n\r\nOK";

            string noidung2 =
                "+CMGL: 1,\"REC UNREAD\",\"+84846889911\",\"\",\"2022/03/18 21:47:30+28\",145,7\r\nTest ne\r\n\r\nOK\r\n";
            string[] responsees = noidung2.Split(new char[] { '' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var res in responsees)
            {
                string response = res.Replace("\n", " ").Replace("\r", "");
                var match = Regex.Match(response, "(\\+CMGL: (.*)OK)");
                if (match.Success && !string.IsNullOrEmpty(match.Value))
                {
                    string[] msgs = match.Value.Split(new string[] { "+CMGL" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var msg in msgs)
                    {
                        string msgContent = msg.Substring(msg.LastIndexOf("\""), msg.Length - msg.LastIndexOf("\""))
                            .Replace("\"", string.Empty).Replace("\r", string.Empty);
                        if (msgContent.Contains(","))
                        {
                            var matchContent = Regex.Match(response, @"((\d+)?,(\d+) )(.*)(?!OK)");
                            if (matchContent.Success)
                            {
                                msgContent = matchContent.Groups[4].Value.Replace("OK", "") ?? "";
                                msgContent = msgContent.Trim();
                            }
                        }

                        string msgHeader = msg.Substring(0, msg.LastIndexOf("\""));
                        string[] headerAtts = msgHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string sender = headerAtts[2].Replace("\"", string.Empty);
                        string receiveDate = headerAtts[4].Replace("\"", string.Empty);
                        string otpDetector = string.Empty;


                        if (msgContent.StartsWith(" "))
                            msgContent = msgContent.Remove(0, 1);

                        if (msgContent.EndsWith(" OK"))
                            msgContent = msgContent.Remove(msgContent.Length - 3, 3);
                        msgContent = msgContent.Trim();

                        try
                        {
                            if (Regex.IsMatch(msgContent.Trim(), "(?:0[xX])?[0-9a-fA-F]+") && msgContent.Length > 3)
                            {
                                string hex = msgContent.Trim();
                                byte[] bytes = Enumerable.Range(0, hex.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                    .ToArray();
                                UTF8Encoding utf8 = new UTF8Encoding();
                                string content = Encoding.GetEncoding("utf-16BE").GetString(bytes);
                                if (!string.IsNullOrEmpty(content))
                                    msgContent = content;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(" Exception when parese Message", ex.Message);
                        }
                        listMessage.Add(msgContent);
                    }
                }
            }
            return listMessage;
        }
        static void Main(string[] args)
        {
            var listStr = GetMessage();
        }
    }
}

