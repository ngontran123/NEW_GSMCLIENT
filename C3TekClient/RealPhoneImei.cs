using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace C3TekClient
{
    public class RealPhoneImei
    {

        public object lockChangeImei = new object();
        public RealPhoneImei()
        {
        }
        public List<string> getAllTypeTalco(string brand_name)
        {
            List<string> talcos = new List<string>();
            try
            {
                string file = "imeidb.csv";
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string row = sr.ReadLine();
                        string[] column = row.Split(',');
                        string talco = column[0];
                        string brand = column[1];
                        if (brand.ToUpper().Contains(brand_name.ToUpper()))
                        {
                            talcos.Add(talco);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("file" + ex.Message);
            }
            catch (Exception er)
            {
                Console.WriteLine("er" + er.Message);
            }
            return talcos;
        }
        public int sumOfDigit(int n)
        {
            int res = 0;
            while (n != 0)
            {
                res += n % 10;
                n /= 10;
            }
            return res;
        }
        public int calculateDoubleDigitTalco(string talco)
        {
            int res = 0;
            for (int i = 1; i < 8; i += 2)
            {
                res += sumOfDigit(int.Parse(talco[i].ToString()) * 2);
            }
            return res;
        }
        public int calculateSingleDigitTalco(string talco)
        {
            int res = 0;
            for (int i = 0; i < 8; i += 2)
            {
                res += int.Parse(talco[i].ToString());
            }
            return res;
        }
        public int calculateSumFromCheckDigit(int checkDigit)
        {   
            if (checkDigit > 9 || checkDigit < 0)
            {
                return -1;
            }
            int val = 10 - checkDigit;

            int res = val + 50;

            return res;

           
        }

        public int calculateRemainingSum(string talco, int checkDigit)
        {
            int double_digit = calculateDoubleDigitTalco(talco);
            int single_digit = calculateSingleDigitTalco(talco);
            int sum_both = double_digit + single_digit;
            int origin_sum = calculateSumFromCheckDigit(checkDigit);
            int res = 0;
            if (origin_sum != -1)
            {
                res = origin_sum - sum_both;
            }
            return res;
        }

        public string generateSequenceNumber(int remaining)
        {
            string sqn = "";
            try
            {
                for (int x1 = 0; x1 < 10; x1++)
                {
                    for (int x2 = 0; x2 < 10; x2++)
                    {
                        for (int x3 = 0; x3 < 10; x3++)
                        {
                            for (int x4 = 0; x4 < 10; x4++)
                            {
                                for (int x5 = 0; x5 < 10; x5++)
                                {
                                    for (int x6 = 0; x6 < 10; x6++)
                                    {
                                        int sum = x1 + sumOfDigit(2 * x2) + x3 + sumOfDigit(2 * x4) + x5 + sumOfDigit(2 * x6);
                                        if (sum == remaining)
                                        {
                                            sqn = "" + x1 + x2 + x3 + x4 + x5 + x6;
                                            return sqn;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return sqn;
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }
            return sqn;
        }
        public string generatePhoneImeiNumber(string talco, int check_digit)
        {
            string imei = "";
            try
            {
                    int remaining = calculateRemainingSum(talco, check_digit);
                    string sqn = generateSequenceNumber(remaining);
                    imei = talco + sqn + check_digit.ToString();
                    return imei;
                }
                catch (Exception er)
                {
                    Console.WriteLine(er.Message);
                }
                return imei;
        }
    }
}

