using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM
{
    public class GSMHelper
    {
        public readonly object _lockRandom = new object();

        private static Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden
        public static string RandomString(int length)
        {
            const string pool = "0123456789";
            
            var builder = new StringBuilder();
            
            for (var i = 0; i < length; i++)
            {
                var c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
        public static string RandomDigits(int seed, int length)
        {
            var random = new Random(seed);
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }
        public static int[] InitializeArrayWithNoDuplicates(int size)
        {
            Random rand = new Random();
            return Enumerable.Repeat<int>(0, size).Select((x, i) => new { i = i, rand = rand.Next() }).OrderBy(x => x.rand).Select(x => x.i).ToArray();
        }
    }
}
