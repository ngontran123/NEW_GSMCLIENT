using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace C3TekClient
{
    static class R
    {
        public static string S(string key){
            return AppModeSetting.MyResource.GetString(key, AppModeSetting.CurrentCultureInfo);
        }
    }
}
