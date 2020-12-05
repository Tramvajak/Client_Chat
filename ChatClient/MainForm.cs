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
                    int _count = ss.LastIndexOf("|");
                    if (count > 0 && _count == -1)
                    {
                        mess = ss.Split(';');
                        Debug.WriteLine(1, mess[0]);
                        if (mess[1] == "CMDclients")
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
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
                        if (mess[1] == "OnListClientOnline")// new
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                OnClientsList(mess[0], "setOnline");
                            });
                        }
                        if (mess[1] == "OnListClientOffline")// new
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate ()
                            {
                                OnClientsList(mess[0], "setOffline");
                            });
                        }
                        if (mess[1] == "OnCurrentUserFullName")//new command
                        {
                            Debug.WriteLine(1, mess[1] + " " + mess[0]);
                            Invoke((MethodInvoker)delegate () 
                            {
                                OnCurrentUserName(mess[0]);
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

                    try
                    {
                        Invoke((MethodInvoker)delegate ()
                        {
                            th.Abort();
                            Thread.Sleep(2000);
                            LoginForm login = new LoginForm("Connection Lost");
                            login.Show();
                            this.Hide();
                            //CloseConnection("Server down!");
                        });
                    }
                    catch
                    {

                    }
                }
            }
        }
        private void OnCurrentUserName(string name)
        {
            UserName = name;
            lbl_UserName.Text = UserName;
            //MainForm fm = new MainForm();
            //fm.Text = UserName;
        }
        private void OnClientsList(string user, string key)
        {
            if(key=="add")
            {
                if (!listB_Users.Items.Contains(user))
                    listB_Users.Items.Add(user);
            }
            if(key=="setOnline")
            {
                for (int i = 0; i < listB_Users.Items.Count; i++)
                {
                    int indexfound = listB_Users.FindString(user);
                    if(indexfound >= 0)
                    {
                        if (listB_Users.Items[indexfound].ToString().Substring(user.Length) == "[Online]") continue;
                        listB_Users.Items[indexfound] +=  "[Online]";
                        return;
                    }
                }
            }
            if (key == "setOffline")
            {
                for (int i = 0; i < listB_Users.Items.Count; i++)
                {
                    if (listB_Users.Items[i].ToString().Contains(user+"[Online]"))
                    {
                        listB_Users.Items[i] = listB_Users.Items[i].ToString().Replace("[Online]",String.Empty);
                    }
                }
            }
        }
        private void UpdateLog(string message) // требует измененний
        {
            //message (Testing1;Testing2|фывфыв)
            int count = message.IndexOf(";");
            int endcount = message.LastIndexOf("|");
            int lenght = message.Length;
            Debug.WriteLine(1, "UpdateLog: "+message);
            Debug.WriteLine(1,"count: " + count + " endcount: " + endcount + " lenght: " + lenght);
            if (count > 0)
            {
                string[] msg = message.Split(';'); // [Name1];[Name2|message]
                string[] _msg = msg[1].Split('|'); // [Name2]|[message]
                string sendName = msg[0];
                string recvName = _msg[0];
                string _message = _msg[1];

                if (sendName == UserName)
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                    richTextBox1.AppendText(sendName+" says: ", Color.Blue);
                }
                else
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                    richTextBox1.AppendText(recvName+ " says: ", Color.Red);
                    //Sound.play_sms();
                }
                richTextBox1.AppendText(_message + "\r\n", ForeColor);

                Debug.WriteLine("Name: " + msg[0] +" RecvName: " +_msg[0]+" Messeage: " + _msg[1]);
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
            if(txtMessage.Lines.Length >= 1)
            {
                if (listB_Users.SelectedIndex != -1)
                {
                    string _message = String.Format(UserName + ";" +
                        listB_Users.Items[listB_Users.SelectedIndex].ToString()+"|"+txtMessage.Text);
                    swSender.WriteLine(_message);
                    Debug.WriteLine(1,"Send Message: "+_message );
                    swSender.Flush();
                    UpdateLog(_message);
                    txtMessage.Lines = null;
                }
                if (listB_Users.SelectedIndex == -1) MessageBox.Show("Выберите пользователя для отправки сообщения");
            }
            else MessageBox.Show("Нельзя отправить пустое сообщение");
            //if (txtMessage.Lines.Length >= 1)
            //{
            //    string message = txtMessage.Text;
            //    int count = message.IndexOf("/");
            //    string[] mess = null;
            //    if (message[0] == '/') // не используется
            //    {
            //        mess = message.Split('/');
            //        swSender.WriteLine(mess[1] + "|OnCmd");
            //        Debug.WriteLine("OnCmd|" + mess[1]);
            //        swSender.Flush();
            //        txtMessage.Lines = null;
            //    }
            //    else
            //    {

            //        swSender.WriteLine(txtMessage.Text);
            //        Debug.WriteLine(txtMessage.Text);
            //        swSender.Flush();
            //        txtMessage.Lines = null;
            //    }
            //}
            //txtMessage.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            swSender.Close();
            swSender = null;
            srReceiver.Close();
            srReceiver = null;
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
