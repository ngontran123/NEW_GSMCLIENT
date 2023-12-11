using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Helper
{
    public static class StringHelper
    {
        public static string FormatPhoneZero(string phone)
        {
            string result = phone;
            result = result.Replace("+", string.Empty).Replace(" ", string.Empty);

            if (result.StartsWith("84"))
            {
                //8498xxx -> 098xx
                result = result.Remove(0, 2).Insert(0, "0");
            }
            else if (!result.StartsWith("0"))
            {
                result = result.Insert(0, "0");
            }
            return result;
        }

        public static string FormatPhone84(string phone)
        {
            return FormatPhoneZero(phone).Remove(0, 1).Insert(0, "84");
        }

        public static string RandomDeviceID()
        {
            Random r = new Random();
            int rInt = r.Next(0, 100); //for ints
            string result = "";
            result = r.Next(0, 100).ToString() + "0" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() +
                     r.Next(0, 100).ToString();
            return result;
        }
        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace(lineSeparator, string.Empty)
                        .Replace(paragraphSeparator, string.Empty);
        }
        public static string ParseDigitString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            return new String(str.Where(Char.IsDigit).ToArray());

        }
        public static T ParseJsonObject<T>(string json) where T : class, new()
        {
            try
            {
                JObject jobject = JObject.Parse(json);

                return JsonConvert.DeserializeObject<T>(jobject.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ParseJsonObject] : " + json);
                return null;
            }
        }
    }
}
