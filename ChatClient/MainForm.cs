using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace ChatClient
{
    public partial class MainForm : Form
    {
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;

        private delegate void UpdateLogCallback(string strMessage);
        private delegate void CloseConnectionCallback(string strReason);
        private Thread th;
        private Thread thStatus;
        private IPAddress ipAddr;
        private bool Connected;
        private bool ServRestart = false;
        public string Version = Properties.Settings.Default.Version.ToString();
        private bool sound;

        public MainForm(StreamWriter _sender, StreamReader _reader,TcpClient _server)
        {
            swSender = _sender;
            srReceiver = _reader;
            tcpServer = _server;
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            if(swSender != null && srReceiver != null)
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            Connect();
        }
        private void Connect()
        {
            Connected = true;
            UserName = Properties.Settings.Default.Name;
            th = new Thread(new ThreadStart(ReceiveMessage));
            th.Name = "Receive Message Main";
            th.Start();
        }
        private void ReceiveMessage() // требует измененний
        {

            string[] mess = null;
            while (Connected)
            {
                try
                {
                    string ss = srReceiver.ReadLine();
                    int count = ss.IndexOf(";");
                    if (count > 0)
                    {
                        mess = ss.Split(';');
                        Debug.WriteLine(1,mess[0]);
                        if (mess[1] == "CMDclients")
                        {
                            Debug.WriteLine(1,mess[1]+ " " +mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                //OnCMDReiceve(mess[0]);
                                
                            });
                        }
                        if (mess[1] == "Status")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                //ColorMessageAndView(mess[0]);
                                
                            });
                        }
                        if (mess[1] == "OnChatRestart")
                        {
                            ServRestart = true;
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                //CloseConnection("Server Restart!");
                                

                            });
                        }
                        if (mess[1] == "OnKickAdmin")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                //CloseConnection("Server: You kicked the admin");
                                Debug.WriteLine(1, "Kick Admin: " + mess[0]);
                            });
                        }
                        if (mess[1] == "OnStatusReported")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                if (mess[0] == "Offline")
                                {
                                    //MessageBox.Show("Status Server Reported: " + mess[0], "Warring", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                    //Reported.sReported = mess[0];
                                }
                                else if (mess[0] == "Online")
                                {
                                    //Reported.sReported = mess[0];
                                }

                            });
                        }
                        if (mess[1] == "OnListClientAdd")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                OnClientsList(mess[0], "add");
                            });
                        }
                        if (mess[1] == "OnListClientDell")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                //OnClientsList(mess[0], "dell");
                            });
                        }
                        //Debug.WriteLineIf(mess[0].Length > 0, "1:" + mess[0]);
                        //Debug.WriteLineIf(mess[1].Length > 0, "2:" + mess[1]);
                        //Debug.WriteLineIf(mess[2].Length > 0, "3:" + mess[2]);
                        for (int i = 0; i < mess.Length; i++)
                            mess[i] = null;
                    }
                    else
                    {
                        if (ss[0] == ';')
                        {

                        }
                        else
                            this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { ss });
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Length > 118)
                        Invoke((MethodInvoker)delegate ()
                        {
                            //CloseConnection("Server down!");
                        });
                }
            }
        }
        private void OnClientsList(string user, string key)
        {
            if(key=="add")
            {
                if (!listB_Users.Items.Contains(user))
                    listB_Users.Items.Add(user);
            }
        }
        private void UpdateLog(string message) // требует измененний
        {
            int count = message.IndexOf(" says:");
            int endcount = message.LastIndexOf(" says:");
            int lenght = endcount - count + 1;
            Debug.WriteLine(1, "UpdateLog: "+message);
            Debug.WriteLine(1,"count: " + count + " endcount: " + endcount + " lenght: " + lenght);
            if (count > 0)
            {
                string index = message.Substring(0, count + 6);
                string name = message.Substring(0, count);
                string endindex = message.Substring(index.Length);

                if (name == UserName)
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                    richTextBox1.AppendText(index, Color.Blue);
                }
                else
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                    richTextBox1.AppendText(index, Color.Red);
                    //Sound.play_sms();
                }
                richTextBox1.AppendText(endindex + "\r\n", ForeColor);

                Debug.WriteLine("Name: " + name + " Messeage: " + endindex);
            }
            else
            {
                int countAdm = message.IndexOf(":");

                if (countAdm > 0)
                {
                    string Aname = message.Substring(0, countAdm);
                    if (Aname == "Administrator")
                    {
                        richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                        richTextBox1.AppendText(Aname, Color.Red);
                        richTextBox1.AppendText(message.Substring(countAdm) + "\r\n", ForeColor);
                        //Sound.play_sms();
                    }
                    else if (Aname == "Server")
                    {
                        richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                        richTextBox1.AppendText(Aname, Color.Red);
                        richTextBox1.AppendText(message.Substring(countAdm) + "\r\n", ForeColor);
                        //Sound.play_sms();
                    }
                    else
                    {
                        richTextBox1.AppendText(message + "\r\n");
                        //Sound.play_sms();
                    }
                }
                else
                {
                    richTextBox1.AppendText(message + "\r\n");
                    //Sound.play_sms();
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
        private void SendMessage()// требует измененний
        {
            if (txtMessage.Lines.Length >= 1)
            {
                string message = txtMessage.Text;
                int count = message.IndexOf("/");
                string[] mess = null;
                if (message[0] == '/') // не используется
                {
                    mess = message.Split('/');
                    swSender.WriteLine(mess[1] + "|OnCmd");
                    Debug.WriteLine("OnCmd|" + mess[1]);
                    swSender.Flush();
                    txtMessage.Lines = null;
                }
                else
                {

                    swSender.WriteLine(txtMessage.Text);
                    Debug.WriteLine(txtMessage.Text);
                    swSender.Flush();
                    txtMessage.Lines = null;
                }
            }
            txtMessage.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            th.Abort();
            Application.Exit();
        }

        private void listB_Users_SelectedIndexChanged(object sender, EventArgs e) // тут нужно описать метот загрузки истории сообщений.
                                                                                  // желательно хранить ее в файле рядом с клиентом
        {
            if (listB_Users.SelectedIndex == -1) return; 
            lbl_ChatTo.Text = "Chat to " + listB_Users.SelectedItem;
        }
    }
}
