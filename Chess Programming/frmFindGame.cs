using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace Chess_Programming
{
    public partial class frmFindGame : DevComponents .DotNetBar .Office2007Form 
    {
        Thread tListenForRequest;
        bool ActiveListener = true;        
        ArrayList AlHost = new ArrayList();

        public frmFindGame()
        {
            InitializeComponent();
        }
        
        private void frmFindGame_Load(object sender, EventArgs e)
        {
            frmMain.localpc.DisconnectUDP();
            CheckForIllegalCrossThreadCalls = false;

            tListenForRequest = new Thread(new ThreadStart(ListenForRequest));
            tListenForRequest.IsBackground = true;
            tListenForRequest.Start();
        }

        void ListenForRequest()
        {
            while (ActiveListener)
            {
                string message = frmMain.localpc.ReceiveUDPDataBroadCast();
                if (message != "")
                {
                    string strType;
                    string strParnerIp = "";
                    string strParnerName, strItem;
                    frmMain.localpc.AnalysisReceiveUDPString(message, out strType, out strParnerIp, out strParnerName, out strItem);

                    if (strParnerIp != frmMain.localpc.ipAddress.ToString())
                    {
                        switch (strType.ToUpper())
                        {
                            case "HOST":
                                if (this.AlHost.IndexOf(strParnerName) != 1)
                                {
                                    this.AlHost.Add(strParnerName + ":" + strParnerIp);
                                }
                                break;
                            case "CHAT":                                
                                this.lstChat.Items.Insert(0, strParnerName + ": " + strItem);
                                this.lstChat.SelectedIndex = 0;
                                break;
                        }
                    }
                }               
            }
        }

        private void timerSendBroadcast_Tick(object sender, EventArgs e)
        {
            this.AlHost = new ArrayList();
            frmMain.localpc.SendUDPData("255.255.255.255", "FINDHOST", "");
        }

        private void btnsend_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendUDPData("255.255.255.255", "CHAT", txtchat.Text);       
            this.lstChat.Items.Add(frmMain.localpc.Profile + ": " + txtchat.Text);
            this.lstChat.TopIndex = lstChat.Items.Count - 1;
            txtchat.Clear();
        }

        private void btnHostGame_Click(object sender, EventArgs e)
        {            
            this.ActiveListener = false;
            this.tListenForRequest.Abort();
            timerSendBroadcast.Stop();
            timerUpdateHost.Stop();            

            frmMain.localpc.DisconnectUDP();
            frmMain.localpc.Function = 1;
            this.Hide();
            this.AlHost = new ArrayList();
            lstHost.Items.Clear();
            ///Join Game
            frmLanGame frmlangame = new frmLanGame();
            frmlangame.ShowDialog();
            ///Escape Game
            
            this.Show();
            timerSendBroadcast.Start();
            timerUpdateHost.Start();
            this.ActiveListener = true;
            tListenForRequest = new Thread(new ThreadStart(ListenForRequest));
            tListenForRequest.IsBackground = true;
            tListenForRequest.Start();            
        }

        private void btnJoinGame_Click(object sender, EventArgs e)
        {
            if (lstHost.SelectedItems.Count > 0)
            {            
                tListenForRequest.Abort();
                this.ActiveListener = false;
                timerSendBroadcast.Stop();
                timerUpdateHost.Stop();
                frmMain.localpc.DisconnectUDP();
                frmMain.localpc.ConnectState = 1;
                frmMain.localpc.Function = 2;

                ListViewItem li = lstHost.SelectedItems[0];
                string strHost = li.Text;
                string[] arrHostEntry = strHost.Split(':');
                frmMain.localpc.ParnerIP = arrHostEntry[1];
                frmMain.localpc.ParnerName = arrHostEntry[0];                

                clsProfile profile = new clsProfile(frmMain.localpc.Profile);                                
                string a = profile.TotalWin.ToString();
                string b = profile.TotalDraw.ToString();
                string c = profile.TotalLose.ToString();
                string d = profile.Rating.ToString();

                frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "JOIN",a + ":" + b + ":" + c + ":" + d);
                this.Hide();
                this.AlHost = new ArrayList();
                lstHost.Items.Clear();
                ///Join Game
                frmLanGame frmlangame = new frmLanGame();
                frmlangame.ShowDialog();
                ///Escape Game
                this.Show();
                this.ActiveListener = true;
                timerSendBroadcast.Start();
                timerUpdateHost.Start();
                tListenForRequest = new Thread(new ThreadStart(ListenForRequest));
                tListenForRequest.IsBackground = true;
                tListenForRequest.Start();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn máy ...", "Join Game");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.tListenForRequest.Abort();
            frmMain.localpc.DisconnectUDP();
            this.Close();
        }

        private void timerUpdateHost_Tick(object sender, EventArgs e)
        {
            //Remove Host
            for (int i = 0; i < lstHost.Items.Count; i++)
            {
                if (this.AlHost.IndexOf(lstHost.Items[i].Text) == -1)
                {
                    lstHost.Items[i].Remove();
                }
            }
            ///Add New Host
            for (int i = 0; i < this.AlHost.Count; i++)
            {
                ListViewItem li = this.lstHost.Items[this.AlHost[i].ToString()];
                if (li == null)
                {
                    li = new ListViewItem(this.AlHost[i].ToString(), 0);
                    li.Name = this.AlHost[i].ToString();
                    this.lstHost.Items.Add(li);
                }
            }
        }

        private void frmFindGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmMain.localpc.DisconnectUDP(); ;
        }

     
    }
}