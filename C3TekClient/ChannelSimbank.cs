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
using C3TekClient.GSM;
using C3TekClient.GSM;
namespace C3TekClient
{
    public partial class ChannelSimbank : DevExpress.XtraEditors.XtraForm
    {
        public BindingList<ChannelSimBankObject> channelList = new BindingList<ChannelSimBankObject>();

        public List<GSMCom> gsmList;
        public string real_sim_port = "";
        public List<string> initChannel()
        {
            List<string> channels = new List<string>();
            for(int i=1;i<=32;i++)
            {
                channels.Add(i.ToString());
            }
            return channels;
        }

        public List<string> initPorts()
        {
            List<string> ports = new List<string>();
            for(int i=1;i<=32;i++)
            {
                ports.Add(i.ToString());
            }
            return ports;
        }
        public ChannelSimbank()
        {
           
        }
        public ChannelSimbank(List<GSMCom> senders,string real_sim_port)
        {
            InitializeComponent();
            List<string> channels = initChannel();
            List<string> ports = initPorts();
            this.channelCombobox.Properties.Items.AddRange(channels);
            this.channelCombobox.SelectedIndex = 0;
            this.portCombobox.Properties.Items.AddRange(ports);
            this.portCombobox.SelectedIndex = 0;
            this.gsmList = senders;
            this.real_sim_port = real_sim_port;
        }
      
        private void channelButton_Click(object sender, EventArgs e)
        {
            string channel = this.channelCombobox.SelectedItem.ToString();
            string port = this.portCombobox.SelectedItem.ToString();
            ChannelSimBankObject channel_ob=new ChannelSimBankObject(){ No=(channelList.Count+1).ToString(),Channel=channel,Port=port,Result=""};
            channelList.Add(channel_ob);
            this.Invoke(new MethodInvoker(() =>
            {
                this.gridControl1.RefreshDataSource();
            }));
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ChannelSimbank_Load(object sender, EventArgs e)
        {
            channelList.Clear();
            gridControl1.DataSource = channelList;
        }
      

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        private void gridView1_ClipboardRowPasting(object sender, DevExpress.XtraGrid.Views.Grid.ClipboardRowPastingEventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            var cells = view.GetSelectedCells() as DevExpress.XtraGrid.Views.Base.GridCell[];

            if (cells.Length <= 1 || e.Values.Count > 1 || System.Windows.Forms.Clipboard.GetText().Contains(System.Environment.NewLine))
                return;

            e.Cancel = true;
            for (int i = 0; i < cells.Length; i++)
                view.SetRowCellValue(cells[i].RowHandle, cells[i].Column, e.OriginalValues[0]);
        }

        private void resetAllPort()
        {

            List<Task> tasks = new List<Task>();
            foreach (var com in gsmList)
            {
              

                    var port_name = com.PortName;
                    if (port_name != real_sim_port)
                    {
                        tasks.Add(new Task(() => { com.reConnect(); }));
                    }
                }
            
            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                Task.WaitAll(tasks.ToArray());
            }).Start();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {  if(string.IsNullOrEmpty(this.real_sim_port))
            {
                if (AppModeSetting.Locale.Equals("zh-CN"))
                {
                    MessageBox.Show("应用不发现设备渠道", "SIMBANK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (AppModeSetting.Locale.Equals("en-US"))
                {
                    MessageBox.Show("No Simbank port detected.", "SIMBANK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    MessageBox.Show("Không phát hiện cổng Simbank trên thiết bị", "Cổng Simbank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
             }
           if(this.gridView1.RowCount==0)
            {  if (AppModeSetting.Locale.Equals("zh-CN"))
                {
                    MessageBox.Show("不发现需要改变的渠道", "SIMBANK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            else if (AppModeSetting.Locale.Equals("en-US"))
                {
                    MessageBox.Show("No row detected in the change-list.", "SIMBANK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    MessageBox.Show("Không phát hiện kênh nào cần đổi cổng", "Cổng Simbank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            foreach(var com in gsmList)
            {
                int val = -1;
                string port_name = com.PortName;
                if(port_name==this.real_sim_port)
                {
                    val = 1;
                    bool handshake_port = com.checkSimBank();
                    if (handshake_port)
                    {
                        bool reset_sim_bank = com.resetAllChannel();
                        if (reset_sim_bank)
                        {  foreach (var channel in channelList)
                            {
                                bool switch_simbank_port = com.switchSimWithChannel(channel.Channel,channel.Port);
                                if (switch_simbank_port)
                                {  if (AppModeSetting.Locale.Equals("zh-CN"))
                                    {
                                        channel.Result = "改变端口成功";
                                    }
                                else if (AppModeSetting.Locale.Equals("en-US"))
                                    {
                                        channel.Result = "Change port successfully";
                                    }
                                    else
                                    {
                                        channel.Result = "Đổi cổng theo kênh thành công";
                                    }
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        this.gridControl1.RefreshDataSource();
                                    }));
                                }
                                else
                                {
                                    if (AppModeSetting.Locale.Equals("zh-CN"))
                                    {
                                        channel.Result = "改变端口失败";
                                    }
                                    else if (AppModeSetting.Locale.Equals("en-US"))
                                    {
                                        channel.Result = "Change port failed";
                                    }
                                    else
                                    {
                                        channel.Result = "Đổi cổng theo kênh thất bại";
                                    }
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        this.gridControl1.RefreshDataSource();
                                    }));
                                }
                            }
                            resetAllPort();
                        }
                    }
                    else
                    {
                        if (AppModeSetting.Locale.Equals("zh-CN"))
                        {
                            MessageBox.Show("改变的时候有错误发生，请你再来尝试", "SIMBANK", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        }
                        else if (AppModeSetting.Locale.Equals("en-US"))
                        {
                            MessageBox.Show("There is error during changing channel port", "Switch Simbank", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        else
                        {
                            MessageBox.Show("Có lỗi xảy ra khi đổi kênh sim", "Switch Simbank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if(e.Column.FieldName=="Result")
            {
                int rowHandle = e.RowHandle;
                gridView1.SetRowCellValue(rowHandle, gridView1.Columns["Result"], Color.GreenYellow);
            }
        }

        private void xóaDòngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] selected_rows = this.gridView1.GetSelectedRows();
            if (selected_rows.Any())
            {
                int count = 0;
                foreach (int row in selected_rows)
                {
                    int val = row-count;
                    var channel_row = this.gridView1.GetRow(val);
                    if (channel_row != null)
                    {
                        var channel_ob = (ChannelSimBankObject)channel_row;
                        this.channelList.Remove(channel_ob);
                    }
                    count++;
                }
                this.gridControl1.BeginInvoke(new MethodInvoker(() =>
                {
                    this.gridView1.RefreshData();
                }));
            }
            else
            {
                MessageBox.Show("Không phát hiện row nào được chọn", "Xóa row", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}