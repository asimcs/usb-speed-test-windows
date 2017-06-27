using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using USBClassLibrary;

namespace usb_speed_test
{    
    public partial class Form1 : Form
    {
        int buffer_len = 50*1024*1024; //50Mbytes


        Nullable<UInt32> MI = 0;



        public Form1()
        {
            InitializeComponent();


            //USB Connection
            MI = null;

            USBPort = new USBClass();
            ListOfUSBDeviceProperties = new List<USBClass.DeviceProperties>();
            USBPort.USBDeviceAttached += new USBClass.USBDeviceEventHandler(USBPort_USBDeviceAttached);
            USBPort.USBDeviceRemoved += new USBClass.USBDeviceEventHandler(USBPort_USBDeviceRemoved);
            USBPort.RegisterForDeviceChange(true, this.Handle);
            USBTryMyDeviceConnection();
            MyUSBDeviceConnected = false;
        }
        private bool USBTryMyDeviceConnection()
        {            
            if (USBClass.GetUSBDevice(uint.Parse(vidText1.Text, System.Globalization.NumberStyles.AllowHexSpecifier), uint.Parse(pidText111.Text, System.Globalization.NumberStyles.AllowHexSpecifier), ref ListOfUSBDeviceProperties, false, MI))
            {
                //My Device is attached
                richTextBox1.AppendText("Number of found devices: " + ListOfUSBDeviceProperties.Count.ToString() + "\n");
                
                
                for (int i = 0; i < ListOfUSBDeviceProperties.Count; i++)
                {
                    richTextBox1.AppendText("Device " + i.ToString());
                }                
                Connect();

                return true;
            }
            else
            {
                Disconnect();
                return false;
            }
        }

        private void USBPort_USBDeviceAttached(object sender, USBClass.USBDeviceEventArgs e)
        {
            if (!MyUSBDeviceConnected)
            {
                if (USBTryMyDeviceConnection())
                {
                    MyUSBDeviceConnected = true;
                }
            }
        }

        private void USBPort_USBDeviceRemoved(object sender, USBClass.USBDeviceEventArgs e)
        {
            if (!USBClass.GetUSBDevice(MyDeviceVID, MyDevicePID, ref ListOfUSBDeviceProperties, false))
            {
                //My Device is removed
                MyUSBDeviceConnected = false;
                Disconnect();
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool IsHandled = false;

            USBPort.ProcessWindowsMessage(m.Msg, m.WParam, m.LParam, ref IsHandled);

            base.WndProc(ref m);
        }

        private void Connect()
        {
            //TO DO: Insert your connection code here
            MessageBox.Show("Connected!");
            //ConnectionToolStripStatusLabel.Text = "Connected";
        }

        private void Disconnect()
        {
            //TO DO: Insert your disconnection code here
            MessageBox.Show("Disconnected!");
            //ConnectionToolStripStatusLabel.Text = "Disconnected";
            //InitializeDeviceTextBoxes();
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 4;
        }

        private void label1_Click(object sender, EventArgs e)
        {


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
        }

        private void usb_test_btn_Click(object sender, EventArgs e)
        {
            usb_test_btn.Text = "Stop";
            usb_test_btn.Enabled = false;
            long length = 0;
            if (textBox1.Text == "") {
                MessageBox.Show("Open File Failed");
                usb_test_btn.Text = "Start";
                return;
            }
                
            try {
                length = (int)(new System.IO.FileInfo(textBox1.Text).Length);
            }
            catch(IOException e1)
            {
                MessageBox.Show("Open File Failed");
                usb_test_btn.Text = "Start";
                usb_test_btn.Enabled = true;
                return;
            }
            textBox2.Text = (length/(1024*1024)).ToString();
            if (length > buffer_len)
            {
                MessageBox.Show("Selected file size is over limitation");
                usb_test_btn.Text = "Start";
                usb_test_btn.Enabled = true;
                return;
            }
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                byte[] array = new byte[length];
                const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
                FileStream infile = new FileStream(openFileDialog1.FileName,
                                    FileMode.Open, FileAccess.Read, FileShare.None, 8
                                , FileFlagNoBuffering | FileOptions.SequentialScan);
                long x = length / 512;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                //byte[] array = File.ReadAllBytes(openFileDialog1.FileName);

                //no cach file read                
                infile.Read(array, 0, (int)x*512);                
                sw.Stop();
                infile.Close();
                infile.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                long elapsed_time = sw.ElapsedMilliseconds > 1 ? sw.ElapsedMilliseconds : 1;
                double speed = ((length * 1000) / elapsed_time) / (1024 * 1024);
                label2.Text = DateTime.Now + " " + speed + "MBytes";
                this.Refresh();
                richTextBox1.AppendText(Environment.NewLine + DateTime.Now + "Speed: " + speed + " MBytes/second" + ", File: " + textBox1.Text + ", size: " + textBox2.Text + "MBytes, Elapsed Time: " + sw.Elapsed);
                Thread.Sleep(1);
                
            }
            usb_test_btn.Text = "Start";
            usb_test_btn.Enabled = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //1MBytes 
            //5MBytes
            //50MBytes (default)
            //100MBytes
            //300MBytes
            //1GBytes            

            if (comboBox2.SelectedIndex == 0)
                buffer_len = 1 * 1024 * 1024;
            else if (comboBox2.SelectedIndex == 1)
                buffer_len = 5 * 1024 * 1024;
            else if (comboBox2.SelectedIndex == 2)
                buffer_len = 50 * 1024 * 1024;
            else if (comboBox2.SelectedIndex == 3)
                buffer_len = 100 * 1024 * 1024;
            else if (comboBox2.SelectedIndex == 4)
                buffer_len = 300 * 1024 * 1024;
            else if (comboBox2.SelectedIndex == 5)
                buffer_len = 1024 * 1024 * 1024;
            else { }            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            USBTryMyDeviceConnection();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void pid_TextChanged(object sender, EventArgs e)
        {

        }

        private void vidText_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
