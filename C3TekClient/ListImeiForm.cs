using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace C3TekClient
{
    public partial class ListImeiForm : DevExpress.XtraEditors.XtraForm
    {
        public ListImeiForm()
        {
            InitializeComponent();
            
        }
        
        public static MainUI instance = MainUI.returnInstance();
        private void btnSaveImei_Click(object sender, EventArgs e)
        {
            int val = countValidImeiNumber();
            int num_available_port = C3TekClient.GSM.GSMControlCenter.GSMComs.Count;
            if(val<num_available_port)
            {
                MessageBox.Show("Số lượng imei ít hơn số lượng cổng cần đổi","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            instance.imeiManualList = validImeiNumber();
            MessageBox.Show("Đã lưu danh sách imei thành công", "Đã lưu danh sách", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public int countValidImeiNumber()
        {
            Regex reg = new Regex(@"^\d{15}$");
            int count_valid = 0;
            List<string> setValidSim = new List<string>();
            string[] lines = this.imeiList.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                bool is_valid = reg.IsMatch(line);
                if(is_valid && !setValidSim.Contains(line))
                {
                    count_valid++;
                    setValidSim.Add(line);
                }
            }
            return count_valid;
        }
        public void initListManualImei(List<string> listImei)
        {
            try
            {
                foreach(string imei in listImei)
                {
                    this.imeiList.Text += imei + "\n";
                }
                this.imeiList.Text = this.imeiList.Text.Trim();
            }
            catch(Exception er)
            {
                Console.WriteLine(er.Message);
            }
        }
        public List<string> validImeiNumber()
        {
            List<string> res = new List<string>();
            Regex reg = new Regex(@"^\d{15}$");
            string[] lines = this.imeiList.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                bool is_valid = reg.IsMatch(line);
                if (is_valid && !res.Contains(line))
                {
                    res.Add(line);
                }
            }
            return res;
        }

        private void ListImeiForm_Load(object sender, EventArgs e)
        {
            initListManualImei(instance.imeiManualList);
        }
        
    }
}