using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            Debug.WriteLine(1,"Open Login Form");
        }
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        private Thread th;
        private Thread thStatus;
        private IPAddress ipAddr;
        private void Read_Settings()
        {
            try
            {


                if (!IPAddress.TryParse(Properties.Settings.Default.Ip_Server, out ipAddr))
                {
                    string _ip = Properties.Settings.Default.Ip_Server = "127.0.0.1";
                    string _port = Properties.Settings.Default.Port_Server = "7770";
                    Debug.WriteLine("Set Setting to default ip:{0} port:{1}", _ip, _port);
                }
                if (Properties.Settings.Default.Name != null)
                {
                    // txt_UserName.Text = Properties.Settings.Default.Name;

                }
                Debug.WriteLine(1,"Save Properties");
                Properties.Settings.Default.Save();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(3,ex.Message);
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoginForm_Load(object sender, EventArgs e)
        {
            Read_Settings();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            Connect();
        }
        private void Connect()
        {
            try
            {
                Debug.WriteLine(1, "Connection...");
                tcpServer = new TcpClient();
                tcpServer.Connect(ipAddr, 7770);
                Debug.WriteLine(1, "Connect");
                UserName = txt_Login.Text;
                string password = txt_Password.Text;
                swSender = new StreamWriter(tcpServer.GetStream());
                swSender.WriteLine(UserName + "|" + password + "|" + "0.1");
                swSender.Flush();
                Properties.Settings.Default.Name = UserName;
                Properties.Settings.Default.Save();
                Debug.WriteLine(1, "Login...");
                th = new Thread(new ThreadStart(ReceiveMessage));
                th.Name = "Receive Message Login";
                th.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(3, ex.Message);
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReceiveMessage()
        {
            try
            {
                srReceiver = new StreamReader(tcpServer.GetStream());
                string conMesseage = srReceiver.ReadLine();

                string[] mess = null;
                if (conMesseage[0] == '1')
                {
                    Debug.WriteLine(1, "Login success");
                    Invoke((MethodInvoker)delegate ()
                    {
                        MainForm frm = new MainForm(swSender, srReceiver, tcpServer);
                        frm.Show();
                        th.Abort();
                        this.Hide();

                    });
                }
                else
                {
                    string Reason;

                    int count = conMesseage.IndexOf("|");
                    if (count > 0)
                    {
                        mess = conMesseage.Split('|');
                        Reason = mess[1];
                        Debug.WriteLine(2,conMesseage);
                        Invoke((MethodInvoker)delegate ()
                        {
                            th.Abort();
                            MessageBox.Show(Reason, "Not Connected!");
                        });
                        return;
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate ()
                        {
                            th.Abort();
                            MessageBox.Show(conMesseage, "Not Connected!");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Поток находился в процессе прерывания.") return;
                Debug.WriteLine(3, ex.Message);
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                 th.Abort();
                Application.Exit();
            }
            catch
            {

            }
        }
    }
}
