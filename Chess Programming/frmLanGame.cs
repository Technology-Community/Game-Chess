using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Chess_Usercontrol;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace Chess_Programming
{
    public partial class frmLanGame : DevComponents.DotNetBar.Office2007Form
    {

        UcChessBoard Board;
        int intNotationSize;
        Label[] lblNotation = new Label[36];
        bool ActiveListenerUDP = false;
        bool ActiveListenerTCP = false;
        bool ActiveListenerBroadcast = false;
        Thread tRec;

        public static int iSide = 0;
        public static int iTotalTimer = 0;
        public static int iMoveTimer = 0;
        public static int iExtraTimer = 0;
        public static int iUndo = 0;

        bool bMyReady = false;
        bool bParReady = false;

        UcMovesHistory UcMovesHistory1;

        private void CreateChessBoard(ChessSide eOwnSide, GameMode eGameMode, string strFEN)
        {
            try
            {
                if (Board != null)
                {
                    Board.Dispose();
                    if (UcMovesHistory1 != null)
                        UcMovesHistory1.Dispose();
                }

                panel1.Visible = true;
                panel1.Controls.Clear();
                clsOptions obj = new clsOptions();
                UcMovesHistory1 = new UcMovesHistory();
                obj.CellSize = 55;
                Board = new UcChessBoard(obj.BoardStyle, obj.PieceStyle, eOwnSide, eGameMode, obj.CellSize, obj.CellSize, obj.PlaySound, strFEN);
                Board.MoveMaked += new UcChessBoard.MoveMakedHandler(Board_MoveMaked);
                Bitmap bmpBackImage = new Bitmap(Board.Width, Board.Height);
                Board.DrawToBitmap(bmpBackImage, Board.Bounds);
                Board.BackgroundImage = bmpBackImage;
                Board.BoardBitMap = bmpBackImage;
                pnlHistory.Visible = true;

                intNotationSize = (int)((obj.CellSize * 38) / 100);
                if (this.InvokeRequired)
                {
                    this.Invoke(new dlgAddItemN(AddNotation), obj.CellSize, eOwnSide);
                }
                else
                {
                    AddNotation(obj.CellSize, eOwnSide);
                }
                pnlHistory.Controls.Add(UcMovesHistory1);
                UcMovesHistory1.LoadMovesHistory(Board.stkWhiteMoves, Board.stkBlackMoves);

                Board.Location = new Point(intNotationSize, intNotationSize);

                panel1.Controls.Add(Board);
                this.panel1.ClientSize = new Size(obj.CellSize * 8 + intNotationSize * 2, obj.CellSize * 8 + intNotationSize * 2);
            }
            catch
            {


            }
        }

        void Board_MoveMaked(object sender, EventArgs e)
        {
            try
            {
                if ((Board.WhiteToMove == true && Board.OwnSide == ChessSide.Black) || (Board.WhiteToMove == false && Board.OwnSide == ChessSide.White))
                {
                    string s = Board.LastMove.ToString();
                    frmMain.localpc.SendTCPData("MOVE", Board.LastMove.ToString());
                }
                UcMovesHistory1.LoadMovesHistory(Board.stkWhiteMoves, Board.stkBlackMoves);
            }
            catch
            {


            }
        }

        private delegate void dlgAddItemN(int intCellSize, ChessSide eOwnSide);

        void AddNotation(int intCellSize, ChessSide eOwnSide)
        {
            try
            {
                for (int i = 0; i < 36; i++)
                {
                    lblNotation[i] = new Label();
                    lblNotation[i].Height = intCellSize;
                    lblNotation[i].Width = intCellSize;
                    lblNotation[i].Image = clsImageProcess.GetChessBoardBitMap(ChessSide.Black, ChessBoardStyle.BoardEdge);

                    lblNotation[i].TextAlign = ContentAlignment.MiddleCenter;
                    lblNotation[i].Font = new Font(FontFamily.GenericSansSerif, 13.0f);
                    lblNotation[i].ImageAlign = ContentAlignment.MiddleCenter;
                    lblNotation[i].ForeColor = Color.White;
                }

                lblNotation[0].Height = intNotationSize;
                lblNotation[0].Width = intNotationSize;
                lblNotation[0].Location = new Point();
                lblNotation[0].BackColor = Color.Blue;
                panel1.Controls.Add(lblNotation[0]);

                for (int i = 1; i <= 8; i++)
                {
                    lblNotation[i].Height = intNotationSize;
                    if (eOwnSide == ChessSide.White)
                    {
                        lblNotation[i].Text = Convert.ToChar(64 + i).ToString();
                    }
                    else
                    {
                        lblNotation[i].Text = Convert.ToChar(73 - i).ToString();
                    }
                    lblNotation[i].Location = new Point(intCellSize * (i - 1) + intNotationSize, 0);
                    panel1.Controls.Add(lblNotation[i]);
                }
                lblNotation[9].Height = intNotationSize;
                lblNotation[9].Width = intNotationSize;
                lblNotation[9].BackColor = Color.Blue;
                lblNotation[9].Location = new Point(0, intNotationSize + intCellSize * 8);
                panel1.Controls.Add(lblNotation[9]);

                for (int i = 1; i <= 8; i++)
                {
                    lblNotation[9 + i].Height = intNotationSize;
                    if (eOwnSide == ChessSide.White)
                    {
                        lblNotation[9 + i].Text = Convert.ToChar(64 + i).ToString();
                    }
                    else
                    {
                        lblNotation[9 + i].Text = Convert.ToChar(73 - i).ToString();
                    }
                    lblNotation[9 + i].Location = new Point(intCellSize * (i - 1) + intNotationSize, intNotationSize + intCellSize * 8);
                    panel1.Controls.Add(lblNotation[9 + i]);
                }
                lblNotation[18].Height = intNotationSize;
                lblNotation[18].Width = intNotationSize;
                lblNotation[18].BackColor = Color.Blue;
                lblNotation[18].Location = new Point(intNotationSize + intCellSize * 8, 0);
                panel1.Controls.Add(lblNotation[18]);

                for (int i = 1; i <= 8; i++)
                {
                    lblNotation[18 + i].Width = intNotationSize;
                    if (eOwnSide == ChessSide.White)
                    {
                        lblNotation[18 + i].Text = Convert.ToString(9 - i);
                    }
                    else
                    {
                        lblNotation[18 + i].Text = Convert.ToString(i);
                    }
                    lblNotation[18 + i].Location = new Point(0, intCellSize * (i - 1) + intNotationSize);
                    panel1.Controls.Add(lblNotation[18 + i]);
                }

                lblNotation[27].Height = intNotationSize;
                lblNotation[27].Width = intNotationSize;
                lblNotation[27].BackColor = Color.Blue;
                lblNotation[27].Location = new Point(intNotationSize + intCellSize * 8, intNotationSize + intCellSize * 8);
                panel1.Controls.Add(lblNotation[27]);
                for (int i = 1; i <= 8; i++)
                {
                    lblNotation[27 + i].Width = intNotationSize;
                    if (eOwnSide == ChessSide.White)
                    {
                        lblNotation[27 + i].Text = Convert.ToString(9 - i);
                    }
                    else
                    {
                        lblNotation[27 + i].Text = Convert.ToString(i);
                    }
                    lblNotation[27 + i].Location = new Point(intNotationSize + intCellSize * 8, intCellSize * (i - 1) + intNotationSize);
                    panel1.Controls.Add(lblNotation[27 + i]);
                }
            }
            catch
            {


            }
        }

        public frmLanGame()
        {
            InitializeComponent();

        }

        private void frmLanGame_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            frmMain.localpc.DisconnectUDP();

            if (frmMain.localpc.Function == 1)
            {
                frmGameOptions frm = new frmGameOptions();
                frm.ShowDialog();
                lblstatus.Text = "Đang tìm kiếm đối thủ...";
                this.ActiveListenerBroadcast = true;
                tRec = new Thread(new ThreadStart(ListenForRequestBroadcast));
                tRec.IsBackground = true;
                tRec.Start();

            }
            else
            {
                btnSanSang.Enabled = true;
                lblstatus.Text = "Đang tiến hành kết nối máy chủ...";
                this.ActiveListenerUDP = true;
                tRec = new Thread(new ThreadStart(ListenForRequestUDP));
                tRec.IsBackground = true;
                tRec.Start();
            }
            ucMyProfile.LoadProfile(frmMain.localpc.Profile);
        }

        void ListenForRequestBroadcast()
        {
            while (ActiveListenerBroadcast)
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
                            case "FINDHOST":
                                if (frmMain.localpc.ConnectState == 0)
                                {
                                    frmMain.localpc.SendUDPData(strParnerIp, "HOST", "");
                                }
                                break;
                            case "JOIN":
                                string[] arrProfileInfo = strItem.Split(':');
                                string totalwin = arrProfileInfo[0];
                                string totoldraw = arrProfileInfo[1];
                                string totallose = arrProfileInfo[2];
                                string rating = arrProfileInfo[3];
                                ucOppProfile.LoadProfile(strParnerName, totalwin, totoldraw, totallose, rating);

                                frmMain.localpc.ConnectState = 1;
                                btnkichout.Enabled = true;
                                frmMain.localpc.ParnerIP = strParnerIp;
                                frmMain.localpc.ParnerName = strParnerName;
                                frmMain.localpc.ConnectState = 1;
                                frmMain.localpc.Function = 1;
                                btnSanSang.Enabled = true;

                                clsProfile profile = new clsProfile(frmMain.localpc.Profile);
                                totalwin = profile.TotalWin.ToString();
                                totoldraw = profile.TotalDraw.ToString();
                                totallose = profile.TotalLose.ToString();
                                rating = profile.Rating.ToString();
                                frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "PROFILE", totalwin + ":" + totoldraw + ":" + totallose + ":" + rating);

                                lblstatus.Text = "Đang tiến hành kết nối với " + strParnerName;
                                this.ActiveListenerBroadcast = false;
                                this.ActiveListenerUDP = true;
                                tRec = new Thread(new ThreadStart(ListenForRequestUDP));
                                tRec.IsBackground = true;
                                tRec.Start();
                                break;
                            case "CHAT":
                                lstChatBox.Items.Add(strParnerName + " :" + strItem);
                                lstChatBox.TopIndex = lstChatBox.Items.Count - 1;
                                break;
                        }


                    }
                }
                frmMain.localpc.DisconnectUDP();
            }

        }

        void ListenForRequestUDP()
        {
            while (ActiveListenerUDP)
            {
                string message = frmMain.localpc.ReceiveUDPData(frmMain.localpc.ParnerIP);
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
                            case "PROFILE":
                                string[] arrProfileInfo = strItem.Split(':');
                                string totalwin = arrProfileInfo[0];
                                string totoldraw = arrProfileInfo[1];
                                string totallose = arrProfileInfo[2];
                                string rating = arrProfileInfo[3];
                                ucOppProfile.LoadProfile(strParnerName, totalwin, totoldraw, totallose, rating);
                                break;
                            case "CHAT":
                                lstChatBox.Items.Add(strParnerName + " :" + strItem);
                                lstChatBox.TopIndex = lstChatBox.Items.Count - 1;
                                break;
                            case "KICKOUT":
                                MessageBox.Show(strParnerName + " đã từ chối chơi với bạn.");
                                this.Close();
                                break;
                            case "READY":
                                lblstatus.Text = strParnerName + " đã sẵn sàng chơi.";
                                this.bParReady = true;
                                if ((this.bMyReady) && (frmMain.localpc.Function == 1))
                                {
                                    frmMain.localpc.InitialTCP();
                                }
                                break;

                            case "SERVERREADY":
                                ActiveListenerUDP = false;
                                ActiveListenerTCP = true;

                                frmMain.localpc.InitialTCP();
                                Thread.Sleep(5000);
                                tRec = new Thread(new ThreadStart(ListenForRequestTCP));
                                tRec.IsBackground = true;
                                tRec.Start();
                                break;

                            case "CLIENTREADY":
                                this.ActiveListenerUDP = false;
                                this.ActiveListenerTCP = true;
                                tRec = new Thread(new ThreadStart(ListenForRequestTCP));
                                tRec.IsBackground = true;
                                tRec.Start();

                                Thread.Sleep(2000);
                                frmMain.localpc.SendTCPData("OPTION", iSide + ":" + iTotalTimer + ":" + iMoveTimer + ":" + iExtraTimer + ":" + iUndo);
                                break;
                            case "OUT":
                                MessageBox.Show("Máy bạn mất kết nối với " + strParnerName, "Mất kết nối");
                                btnkichout.Enabled = false;
                                frmMain.localpc.DisposeUDP();

                                this.ActiveListenerUDP = false;
                                this.ActiveListenerBroadcast = true;
                                tRec = new Thread(new ThreadStart(ListenForRequestBroadcast));
                                tRec.IsBackground = true;
                                tRec.Start();
                                break;
                        }


                    }
                }
            }
            frmMain.localpc.DisconnectUDP();
        }

        void ListenForRequestTCP()
        {
            while (ActiveListenerTCP)
            {
                try
                {
                    string message = frmMain.localpc.ReceiveTCPData();
                    if (message != "")
                    {
                        string strType, strItem;
                        frmMain.localpc.AnalysisReceiveTCPString(message, out strType, out strItem);

                        switch (strType.ToUpper())
                        {
                            case "ASKFILE":
                                frmMain.localpc.SendTCPData("FILE", "");
                                //frmMain.localpc.SendFileTCP();
                                //Thread tsend = new Thread(new ThreadStart(frmMain.localpc.SendFileTCP));
                                //tsend.Start();
                                break;
                            case "FILE":
                                ActiveListenerTCP = false;
                                //Thread treceive = new Thread(new ThreadStart(frmMain.localpc.ReceiveFileTCP));
                                //treceive.Start();   
                                //frmMain.localpc.ReceiveFileTCP();
                                ActiveListenerTCP = true;
                                break;
                            case "SENDED":
                                if (frmMain.localpc.Function == 1) //Server
                                {
                                    ucOppProfile.LoadProfile(frmMain.localpc.ParnerName);
                                    frmMain.localpc.SendTCPData("OPTION", iSide + ":" + iTotalTimer + ":" + iMoveTimer + ":" + iExtraTimer + ":" + iUndo);
                                }
                                else
                                {
                                    frmMain.localpc.SendTCPData("ASKFILE", "");
                                }
                                break;
                            case "OPTION":
                                string[] arrOption = strItem.Split(':');
                                iSide = int.Parse(arrOption[0]);
                                if (iSide == 0)
                                {
                                    iSide = 1;
                                }
                                else
                                {
                                    iSide = 0;
                                }

                                iTotalTimer = int.Parse(arrOption[1]);
                                iMoveTimer = int.Parse(arrOption[2]);
                                iExtraTimer = int.Parse(arrOption[3]);
                                iUndo = int.Parse(arrOption[4]);
                                frmGameOptions frm = new frmGameOptions();
                                frm.ShowDialog();
                                if (frmGameOptions.bEditOption == false)
                                {
                                    btnPlay.Enabled = true;
                                }
                                break;
                            case "ENDOPTION":
                                lblstatus.Text = "Cấu hình đã xong, Xin nhấn Bắt đầu để chơi.";
                                this.btnPlay.Enabled = true;
                                break;
                            case "MOVE":
                                Board.MakeMove(strItem);
                                break;
                            case "CHAT":
                                this.lstChatBox.Items.Add(frmMain.localpc.ParnerName + ": " + strItem);
                                lstChatBox.TopIndex = lstChatBox.Items.Count - 1;
                                break;
                            case "LOSE":
                                MessageBox.Show("Chúc mừng bạn đã thắng " + frmMain.localpc.ParnerName);
                                panel1.Enabled = false;
                                break;
                            case "DRAW":
                                DialogResult drdraw = MessageBox.Show(frmMain.localpc.ParnerName + " xin Hòa. Bạn đồng ý không ?", "XIN HÒA", MessageBoxButtons.OKCancel);
                                if (drdraw == DialogResult.OK)
                                {
                                    frmMain.localpc.SendTCPData("DRAWOK", "");
                                    MessageBox.Show("Bạn đã Hòa với " + frmMain.localpc.ParnerName + ". Cố gắng thêm nhé.");
                                    panel1.Enabled = false;
                                }
                                else
                                {
                                    frmMain.localpc.SendTCPData("DRAWCANCEL", "");
                                }
                                break;
                            case "DRAWOK":
                                MessageBox.Show("Bạn đã Hòa với " + frmMain.localpc.ParnerName + ". Cố gắng thêm nhé.");
                                panel1.Enabled = false;
                                break;
                            case "DRAWCANCEL":
                                MessageBox.Show(frmMain.localpc.ParnerName + " không đồng ý HÒA.");
                                break;
                            case "UNDO":
                                DialogResult dr = MessageBox.Show(frmMain.localpc.ParnerName + " xin Đi Lại. Bạn đồng ý không ?", "XIN ĐI LẠI", MessageBoxButtons.OKCancel);
                                if (dr == DialogResult.OK)
                                {
                                    frmMain.localpc.SendTCPData("UNDOOK", "");
                                    Board.UnDoMove();
                                    UcMovesHistory1.LoadMovesHistory(Board.stkWhiteMoves, Board.stkBlackMoves);
                                }
                                else
                                {
                                    frmMain.localpc.SendTCPData("UNDOCANCEL", "");
                                }
                                break;
                            case "UNDOOK":
                                Board.UnDoMove();
                                break;
                            case "UNDOCANCEL":
                                MessageBox.Show(frmMain.localpc.ParnerName + " không đồng ý cho bạn đi lại.");
                                break;
                            case "NEWGAME":
                                DialogResult drnewgame = MessageBox.Show(frmMain.localpc.ParnerName + " mời bạn chơi lại. Bạn đồng ý không ?", "CHƠI LẠI", MessageBoxButtons.OKCancel);
                                if (drnewgame == DialogResult.OK)
                                {
                                    frmMain.localpc.SendTCPData("NEWGAMEOK", "");
                                }
                                else
                                {
                                    frmMain.localpc.SendTCPData("NEWGAMECANCEL", "");
                                }
                                break;
                            case "NEWGAMEOK":
                                frmGameOptions frmgameoption = new frmGameOptions();
                                frmgameoption.ShowDialog();
                                if (frmGameOptions.bEditOption == false)
                                {
                                    btnPlay.Enabled = true;
                                    panel1.Enabled = true;
                                }
                                break;
                            case "NEWGAMECANCEL":
                                MessageBox.Show(frmMain.localpc.ParnerName + " đã từ chối Chơi Lại với bạn.", "Chơi Lại");
                                break;
                        }
                    }
                }
                catch
                {
                    if (frmMain.localpc.ParnerName != null)
                    {
                        MessageBox.Show("Máy bạn đã mất kết nối với " + frmMain.localpc.ParnerName, "Mất kết nối");
                    }
                    else
                    {
                        MessageBox.Show("Máy bạn đã mất kết nối với máy chủ", "Mất kết nối");
                    }

                    this.ActiveListenerTCP = false;
                    this.Close();
                }
            }
        }


        private void frmLanGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (frmMain.localpc.ConnectState == 1)
            {
                frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "OUT", "");
            }            
            tRec.Abort();
            frmMain.localpc.DisconnectUDP();
            frmMain.localpc.DisposeTCPConnection();
            frmMain.localpc.DisposeUDP();

        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            lstChatBox.Items.Add(frmMain.localpc.Profile + ": " + txtchat.Text);
            lstChatBox.TopIndex = lstChatBox.Items.Count - 1;
            switch (frmMain.localpc.ConnectState)
            {
                case 2:
                    frmMain.localpc.SendTCPData("CHAT", txtchat.Text);
                    break;
                case 1:
                    frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "CHAT", txtchat.Text);
                    break;
                default:
                    frmMain.localpc.SendUDPData("255.255.255.255", "CHAT", txtchat.Text);
                    break;
            }
            txtchat.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "KICKOUT", "");
            btnkichout.Enabled = false;
            frmMain.localpc.DisposeUDP();

            this.ActiveListenerUDP = false;
            this.ActiveListenerBroadcast = true;
            tRec = new Thread(new ThreadStart(ListenForRequestBroadcast));
            tRec.IsBackground = true;
            tRec.Start();
        }

        private void btnSanSang_Click(object sender, EventArgs e)
        {
            btnSanSang.Enabled = false;
            bMyReady = true;
            frmMain.localpc.SendUDPData(frmMain.localpc.ParnerIP, "READY", "");
            if ((bParReady) && (frmMain.localpc.Function == 1))
            {
                frmMain.localpc.InitialTCP();
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (iSide == 0)
            {
                CreateChessBoard(ChessSide.White, GameMode.VsNetWorkPlayer, clsFEN.DefaultFENstring);
            }
            else
            {
                CreateChessBoard(ChessSide.Black, GameMode.VsNetWorkPlayer, clsFEN.DefaultFENstring);
            }
            btnPlay.Enabled = false;
            btnChat.Enabled = true;
            grbPlay.Enabled = true;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            frmOptions frm = new frmOptions();
            frm.ShowDialog();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendTCPData("UNDO", "");
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendTCPData("DRAW", "");
        }

        private void btnLose_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendTCPData("LOSE", "");
            MessageBox.Show("Bạn đã Thua " + frmMain.localpc.ParnerName + ". Cố gắng thêm nhé.");
            panel1.Enabled = false;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            frmMain.localpc.SendTCPData("NEWGAME", "");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if(MessageBox .Show ("Bạn có muốn thoát không?","Thông Báo", MessageBoxButtons.YesNo , MessageBoxIcon.Question  )== DialogResult.Yes )
                this.Close();
        }

    }
}