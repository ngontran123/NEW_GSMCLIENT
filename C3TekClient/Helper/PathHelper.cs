using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClient.Helper
{
    public static class PathHelper
    {
        public static string getPathExecuteFile()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
        public static string getPathExecute()
        {
            return System.IO.Path.GetDirectoryName(Application.ExecutablePath);
        }
        
    }
}
