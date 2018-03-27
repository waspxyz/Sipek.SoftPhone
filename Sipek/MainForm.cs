/*
 * Copyright (C) 2008 Sasa Coh <sasacoh@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * WaveLib library sources http://www.codeproject.com/KB/graphics/AudioLib.aspx
 *
 * Visit SipekSDK page at http://voipengine.googlepages.com/
 *
 * Visit SIPek's home page at http://sipekphone.googlepages.com/
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using Sipek.Common;
using Sipek.Common.CallControl;

#if LINUX
#else
using WaveLib.AudioMixer; // see http://www.codeproject.com/KB/graphics/AudioLib.aspx
#endif

namespace Sipek
{
    public partial class MainForm : Form
    {

        #region Properties

        private Timer tmr = new Timer();  // Refresh Call List
        private EUserStatus _lastUserStatus = EUserStatus.AVAILABLE;
        private string HEADER_TEXT;

        private SipekResources _resources = null;
        private SipekResources SipekResources
        {
            get { return _resources; }
        }

        public bool IsInitialized
        {
            get { return SipekResources.StackProxy.IsInitialized; }
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();

#if TLS
      this.Text += " TLS";
      HEADER_TEXT = this.Text;
#else
            HEADER_TEXT = this.Text;
#endif

            // Check if settings upgrade needed?
            //if (Properties.Settings.Default.cfgUpdgradeSettings == true)
            //{
            //    Properties.Settings.Default.Upgrade();
            //    Properties.Settings.Default.cfgUpdgradeSettings = false;
            //}
            // Create resource object containing SipekSdk and other Sipek related data
            _resources = new SipekResources(this);
        }

        /////////////////////////////////////////////////////////////////////////////////

        private void RefreshForm()
        {
            if (IsInitialized)
            {
                // Update Call Status
                UpdateCallLines(-1);

                // Update Call Register
                UpdateCallRegister();

                // Update Buddy List
                UpdateBuddyList();

                // Update account list
                UpdateAccountList();
            }

            // Refresh toolstripbuttons
            toolStripButtonDND.Checked = SipekResources.Configurator.DNDFlag;
            toolStripButtonAA.Checked = SipekResources.Configurator.AAFlag;

            unconditionalToolStripMenuItem.Checked = SipekResources.Configurator.CFUFlag;
            toolStripTextBoxCFUNumber.Text = SipekResources.Configurator.CFUNumber;

            noReplyToolStripMenuItem.Checked = SipekResources.Configurator.CFNRFlag;
            toolStripTextBoxCFNRNumber.Text = SipekResources.Configurator.CFNRNumber;

            busyToolStripMenuItem.Checked = SipekResources.Configurator.CFBFlag;
            toolStripTextBoxCFBNumber.Text = SipekResources.Configurator.CFBNumber;

            // check if user status available
            toolStripComboBoxUserStatus.Enabled = SipekResources.Configurator.PublishEnabled;
        }

        private void UpdateAccountList()
        {
            listViewAccounts.Items.Clear();

            for (int i = 0; i < SipekResources.Configurator.Accounts.Count; i++)
            {
                IAccount acc = SipekResources.Configurator.Accounts[i];
                string name;

                if (acc.AccountName.Length == 0)
                {
                    name = "--empty--";
                }
                else
                {
                    name = acc.AccountName;
                }
                String regstate;
                switch (acc.RegState)
                {
                    case -1:
                        if (!acc.Enabled)
                        {
                            regstate = "Disabled";
                        }
                        else if (acc.HostName.Length == 0)
                        {
                            regstate = "Empty";
                        }
                        else
                        {
                            regstate = "Error";
                        }

                        break;
                    case 0:
                        regstate = "Trying";
                        break;
                    case 200:
                        regstate = "Registered";
                        break;
                    default:
                        regstate = "Registration Error (" + acc.RegState.ToString() + ")";
                        break;
                }

                ListViewItem item = new ListViewItem(new string[] { name, regstate });
                // mark default account
                if (i == SipekResources.Configurator.DefaultAccountIndex)
                {
                    // Mark default account; todo!!! Coloring!
                    item.BackColor = Color.LightGray;

                    string label = "";
                    // check registration status
                    if (acc.RegState == 200)
                    {
                        this.Text = HEADER_TEXT + " - " + acc.AccountName + " (" + acc.DisplayName + ")"; ;
                        label = "Registered" + " - " + acc.AccountName + " (" + acc.DisplayName + ")";
                    }
                    else if (acc.RegState == 0)
                    {
                        label = "Trying..." + " - " + acc.AccountName;
                    }
                    else
                    {
                        label = "Not registered" + " - " + acc.AccountName;
                    }
                    toolStripStatusLabel.Text = label;
                }
                else
                {
                }

                listViewAccounts.Items.Add(item);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdateCallRegister()
        {
            lock (this)
            {
                listViewCallRegister.Items.Clear();
                // Update Dial field
                toolStripComboDial.Items.Clear();

                Stack<CCallRecord> results = SipekResources.CallLogger.getList();

                int cnt = 0; int dialedcnt = 0;
                foreach (CCallRecord item in results)
                {
                    string duration = item.Duration.ToString();
                    if (duration.IndexOf('.') > 0) duration = duration.Remove(duration.IndexOf('.')); // remove miliseconds

                    string recorditem = item.Number;
                    CBuddyRecord rec = null;
                    int buddyId = CBuddyList.getInstance().getBuddyId(item.Number);
                    if (buddyId > -1)
                    {
                        string name = "";
                        rec = CBuddyList.getInstance()[buddyId];
                        name = rec.FirstName + " " + rec.LastName;
                        name = name.Trim();
                        recorditem = name + ", " + item.Number;
                    }
                    else if (item.Name.Length > 0)
                    {
                        recorditem = item.Name + ", " + item.Number;
                    }

                    ListViewItem lvi = new ListViewItem(new string[] {
               item.Type.ToString(), recorditem.Trim(), item.Time.ToString(), duration});

                    lvi.Tag = item;

                    listViewCallRegister.Items.Insert(cnt, lvi);

                    // add item to dial combo (if dialed)
                    if (item.Type == ECallType.EDialed)
                    {
                        toolStripComboDial.Items.Insert(dialedcnt++, item.Number);
                    }
                    // increase counter
                    cnt++;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        /// Register callbacks and synchronize threads
        ///
        delegate void DRefreshForm();
        delegate void DCallStateChanged(int sessionId);
        delegate void MessageReceivedDelegate(string from, string message);
        delegate void BuddyStateChangedDelegate(int buddyId, int status, string text);
        delegate void DMessageWaiting(int mwi, string text);
        delegate void DIncomingCall(int sessionId, string number, string info);

        void CallManager_IncomingCallNotification(int sessionId, string number, string info)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DIncomingCall(this.OnIncomingCall), new object[] { sessionId, number, info });
            else
                OnIncomingCall(sessionId, number, info);
        }

        public void onCallStateChanged(int sessionId)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DRefreshForm(this.RefreshForm));
            else
                RefreshForm();
        }

        public void onMessageReceived(string from, string message)
        {
            if (InvokeRequired)
                this.BeginInvoke(new MessageReceivedDelegate(this.MessageReceived), new object[] { from, message });
            else
                MessageReceived(from, message);
        }

        public void onBuddyStateChanged(int buddyId, int status, string text)
        {
            if (InvokeRequired)
                this.BeginInvoke(new BuddyStateChangedDelegate(this.BuddyStateChanged), new object[] { buddyId, status, text });
            else
                BuddyStateChanged(buddyId, status, text);
        }

        public void onAccountStateChanged(int accId, int accState)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DRefreshForm(this.RefreshForm));
            else
                RefreshForm();
        }

        public void onMessageWaitingIndication(int mwi, string text)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DMessageWaiting(this.MessageWaiting), new object[] { mwi, text });
            else
                MessageWaiting(mwi, text);
        }

        private void OnIncomingCall(int sessionId, string number, string info)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();

            // notifyIcon.ShowBalloonTip(30, "Sipek Softphone", "Incoming call from " + number + "!", ToolTipIcon.Info);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        /// Buddy List Methods
        #region Buddy List Methods

        private void UpdateBuddyList()
        {
            if (!IsInitialized) return;

            Dictionary<int, CBuddyRecord> results = CBuddyList.getInstance().getList();
            listViewBuddies.Items.Clear();
            foreach (KeyValuePair<int, CBuddyRecord> kvp in results)
            {
                string status = "?";

                if (kvp.Value.PresenceEnabled)
                {
                    switch (kvp.Value.Status)
                    {
                        case 0: status = "unknown"; break;
                        case 1: status = "online"; break;
                        case 2: status = "offline"; break;
                        default: status = kvp.Value.StatusText; break;
                    }
                }

                ListViewItem item = new ListViewItem(new string[] { kvp.Value.FirstName + kvp.Value.LastName, status, kvp.Value.StatusText });
                item.Tag = kvp.Value;
                //item.BackColor = Color.Blue;
                listViewBuddies.Items.Add(item);
            }
        }

        private void toolStripMenuItemAdd_Click(object sender, EventArgs e)
        {
            (new BuddyForm()).ShowDialog();
        }

        private void tabPageBuddies_Enter(object sender, EventArgs e)
        {
            UpdateBuddyList();
        }

        private void BuddyStateChanged(int buddyId, int status, string text)
        {
            CBuddyRecord record = CBuddyList.getInstance()[buddyId];

            if (null == record) return;

            record.Status = status;
            record.StatusText = text;

            // update list...
            UpdateBuddyList();
        }

        private void MessageReceived(string from, string message)
        {
            // extract buddy ID
            string buddyId = parseFrom(from);

            // check if ChatForm already opened
            foreach (Form ctrl in Application.OpenForms)
            {
                if (ctrl.Name == "ChatForm")
                {
                    ((ChatForm)ctrl).BuddyName = buddyId;
                    ((ChatForm)ctrl).LastMessage = message;
                    ctrl.Focus();
                    return;
                }
            }

            // if not, create new instance
            ChatForm bf = new ChatForm(SipekResources);
            int id = CBuddyList.getInstance().getBuddyId(buddyId);
            if (id >= 0)
            {
                //_buddyId = id;        
                CBuddyRecord buddy = CBuddyList.getInstance()[id];
                //_titleText.Caption = buddy.FirstName + ", " + buddy.LastName;
                bf.BuddyId = (int)id;
            }
            bf.BuddyName = buddyId;
            bf.LastMessage = message;
            bf.ShowDialog();
        }

        private string parseFrom(string from)
        {
            string number = from.Replace("<sip:", "");

            int atPos = number.IndexOf('@');
            if (atPos >= 0)
            {
                number = number.Remove(atPos);
                int first = number.IndexOf('"');
                if (first >= 0)
                {
                    int last = number.LastIndexOf('"');
                    number = number.Remove(0, last + 1);
                    number = number.Trim();
                }
            }
            else
            {
                int semiPos = number.IndexOf(';');
                if (semiPos >= 0)
                {
                    number = number.Remove(semiPos);
                }
                else
                {
                    int colPos = number.IndexOf(':');
                    if (colPos >= 0)
                    {
                        number = number.Remove(colPos);
                    }
                }
            }
            return number;
        }

        private void MessageWaiting(int mwi, string info)
        {
            // "Messages-Waiting: yes\r\nMessage-Account: sip:*97@192.168.60.211\r\nVoice-Message: 5/2 (0/0)\r\n"
            // extract values
            string[] parts = info.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string vmaccount = "";
            string noofvms = "";

            if (parts.Length == 3)
            {
                int index = parts[1].IndexOf("Message-Account: ");
                if (index == 0)
                {
                    vmaccount = parts[1].Substring("Message-Account: ".Length);
                }

                if (parts[2].IndexOf("Voice-Message: ") >= 0)
                {
                    noofvms = parts[2].Substring("Voice-Message: ".Length);
                }

            }

            if (mwi > 0)
                toolStripStatusLabelMessages.Text = "Message Waiting: " + noofvms + " - Account: " + vmaccount;
            else
                toolStripStatusLabelMessages.Text = "No Messages!";

        }

        private void toolStripMenuItemIM_Click(object sender, EventArgs e)
        {
            if (listViewBuddies.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewBuddies.SelectedItems[0];
                ChatForm bf = new ChatForm(SipekResources);
                bf.BuddyId = ((CBuddyRecord)lvi.Tag).Id;
                bf.ShowDialog();
            }

        }

        private void toolStripMenuItemEdit_Click(object sender, EventArgs e)
        {
            if (listViewBuddies.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewBuddies.SelectedItems[0];

                BuddyForm bf = new BuddyForm();
                bf.BuddyId = ((CBuddyRecord)lvi.Tag).Id;
                bf.ShowDialog();
            }
        }
        #endregion


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShutdownVoIP();

            this.Close();
        }

        private void ShutdownVoIP()
        {
            if (IsInitialized)
            {
                SipekResources.CallLogger.save();
                CBuddyList.getInstance().save();
            }
            SipekResources.Configurator.Save();

            // shutdown stack
            SipekResources.CallManager.Shutdown();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // (new AboutBox()).ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            (new SettingsForm(this.SipekResources)).ShowDialog();
            RefreshForm();
        }

        /// <summary>
        /// Enable or disable menu items regarding to call state...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripCalls_Opening(object sender, CancelEventArgs e)
        {
            // Hide all items...
            foreach (ToolStripMenuItem mi in contextMenuStripCalls.Items)
            {
                mi.Visible = false;
            }

            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];

                if (SipekResources.CallManager.Count <= 0)
                {
                    return;
                }
                else
                {
                    EStateId stateId = ((CStateMachine)lvi.Tag).StateId;
                    switch (stateId)
                    {
                        case EStateId.INCOMING:
                            acceptToolStripMenuItem.Visible = true;
                            transferToolStripMenuItem.Visible = true;
                            break;
                        case EStateId.ACTIVE:
                            holdRetrieveToolStripMenuItem.Text = "Hold";
                            holdRetrieveToolStripMenuItem.Visible = true;
                            transferToolStripMenuItem.Visible = true;
                            //sendMessageToolStripMenuItem.Visible = true;
                            attendedTransferToolStripMenuItem.Visible = true;
                            break;
                        case EStateId.HOLDING:
                            holdRetrieveToolStripMenuItem.Text = "Retrieve";
                            holdRetrieveToolStripMenuItem.Visible = true;
                            break;
                    }

                }
                // call
                releaseToolStripMenuItem.Visible = true;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // Call Related Methods
        #region Call Related Methods

        /// <summary>
        /// UpdateCallLines delegate
        /// </summary>
        private void UpdateCallLines(int sessionId)
        {
            listViewCallLines.Items.Clear();

            try
            {
                // get entire call list
                Dictionary<int, IStateMachine> callList = SipekResources.CallManager.CallList;

                foreach (KeyValuePair<int, IStateMachine> kvp in callList)
                {
                    string number = kvp.Value.CallingNumber;
                    string name = kvp.Value.CallingName;

                    string duration = kvp.Value.Duration.ToString();
                    if (duration.IndexOf('.') > 0) duration = duration.Remove(duration.IndexOf('.')); // remove miliseconds
                    // show name & number or just number
                    string display = name.Length > 0 ? name + " / " + number : number;
                    string stateName = kvp.Value.StateId.ToString();
                    if (SipekResources.CallManager.Is3Pty) stateName = "CONFERENCE";
                    ListViewItem lvi = new ListViewItem(new string[] {
            stateName, display, duration});

                    lvi.Tag = kvp.Value;
                    listViewCallLines.Items.Add(lvi);
                    lvi.Selected = true;

                    // display info
                    //toolStripStatusLabel1.Text = item.Value.lastInfoMessage;
                }

                if (callList.Count > 0)
                {
                    // control refresh timer
                    tmr.Start();

                    // Remember last status
                    if (toolStripComboBoxUserStatus.SelectedIndex != (int)EUserStatus.OTP)
                        _lastUserStatus = (EUserStatus)toolStripComboBoxUserStatus.SelectedIndex;

                    // Set user status "On the Phone"
                    toolStripComboBoxUserStatus.SelectedIndex = (int)EUserStatus.OTP;
                }
                else
                {
                    toolStripComboBoxUserStatus.SelectedIndex = (int)_lastUserStatus;
                }

            }
            catch (Exception e)
            {
                // TODO!!!!!!!!!!! Sychronize SHARED RESOURCES!!!!
            }
            //listViewCallLines.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            // close balloon tip
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
        }

        public void UpdateCallTimeout(object sender, EventArgs e)
        {
            if (listViewCallLines.Items.Count == 0) return;

            for (int i = 0; i < listViewCallLines.Items.Count; i++)
            {
                ListViewItem item = listViewCallLines.Items[i];
                IStateMachine sm = (IStateMachine)item.Tag;
                if (sm.IsNull) continue;

                string duration = sm.RuntimeDuration.ToString();
                if (duration.IndexOf('.') > 0) duration = duration.Remove(duration.IndexOf('.')); // remove miliseconds

                item.SubItems[2].Text = duration;
            }
            // restart timer
            if (listViewCallLines.Items.Count > 0)
            {
                tmr.Start();
            }

        }

        private void placeACallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewBuddies.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewBuddies.SelectedItems[0];

                CBuddyRecord rec = (CBuddyRecord)lvi.Tag;
                if (rec != null)
                {
                    SipekResources.CallManager.createOutboundCall(rec.Number);
                }
            }
        }

        private void toolStripButtonHoldRetrieve_Click(object sender, EventArgs e)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];

                SipekResources.CallManager.onUserHoldRetrieve(((CStateMachine)lvi.Tag).Session);
            }
        }

        private void toolStripButtonCall_Click(object sender, EventArgs e)
        {
            // TODO check if incoming call!!!
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                CStateMachine call = (CStateMachine)lvi.Tag;
                if (call.Incoming)
                {
                    SipekResources.CallManager.onUserAnswer(call.Session);
                    return;
                }
            }
            if (toolStripComboDial.Text.Length > 0)
            {
                SipekResources.CallManager.createOutboundCall(toolStripComboDial.Text);
            }
        }

        private void releaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                SipekResources.CallManager.onUserRelease(((CStateMachine)lvi.Tag).Session);
            }
        }

        private void attendedTransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                CStateMachine call = (CStateMachine)lvi.Tag;
                // check call states
                if (SipekResources.CallManager.Count >= 2)
                {
                    // get ACTIVE call
                    List<IStateMachine> activeCalls = SipekResources.CallManager[EStateId.ACTIVE];
                    if (activeCalls.Count == 0) return;

                    // get HOLDING call
                    List<IStateMachine> holdingCalls = SipekResources.CallManager[EStateId.HOLDING];
                    if (holdingCalls.Count == 0) return;

                    // transfer ACTIVE to HOLDING
                    SipekResources.CallManager.OnUserTransferAttendant(activeCalls[0].Session, holdingCalls[0].Session);
                }
            }
        }

        private void toolStripComboDial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 0x0d)
            {
                if (toolStripComboDial.Text.Length > 0)
                {
                    SipekResources.CallManager.createOutboundCall(toolStripComboDial.Text);
                }
            }
        }

        private void listViewCallRegister_DoubleClick(object sender, EventArgs e)
        {
            if (listViewCallRegister.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallRegister.SelectedItems[0];
                CCallRecord record = (CCallRecord)lvi.Tag;
                SipekResources.CallManager.createOutboundCall(record.Number);
            }
        }

        private void acceptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                SipekResources.CallManager.onUserAnswer(((CStateMachine)lvi.Tag).Session);
            }
        }

        #endregion

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCallRegister.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallRegister.SelectedItems[0];
                CCallRecord record = (CCallRecord)lvi.Tag;
                SipekResources.CallLogger.deleteRecord(record);
            }
            this.UpdateCallRegister();

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutdownVoIP();
        }

        private void toolStripTextBoxTransferTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 0x0d)
            {
                if (listViewCallLines.SelectedItems.Count > 0)
                {
                    ListViewItem lvi = listViewCallLines.SelectedItems[0];
                    if (toolStripTextBoxTransferTo.Text.Length > 0)
                    {
                        SipekResources.CallManager.OnUserTransfer(((CStateMachine)lvi.Tag).Session, toolStripTextBoxTransferTo.Text);
                    }
                }
                contextMenuStripCalls.Close();
            }
        }

        private void toolStripButtonDND_Click(object sender, EventArgs e)
        {
            SipekResources.Configurator.DNDFlag = toolStripButtonDND.Checked;
        }

        private void toolStripButtonAA_Click(object sender, EventArgs e)
        {
            SipekResources.Configurator.AAFlag = toolStripButtonAA.Checked;
        }

        private void sendInstantMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCallRegister.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallRegister.SelectedItems[0];
                CCallRecord record = (CCallRecord)lvi.Tag;
                int id = CBuddyList.getInstance().getBuddyId(record.Number);
                if (id > 0)
                {
                    ChatForm bf = new ChatForm(SipekResources);
                    bf.BuddyId = id;
                    bf.ShowDialog();
                }
            }
        }

        private void toolStripComboBoxUserStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
             * AVAILABLE, BUSY, OTP, IDLE, AWAY, BRB, OFFLINE
             *
            Available
            Busy
            On the Phone
            Idle
            Away
            Be Right Back
            Offline
             */

            EUserStatus status = (EUserStatus)toolStripComboBoxUserStatus.SelectedIndex;

            SipekResources.Messenger.setStatus(SipekResources.Configurator.DefaultAccountIndex, status);
        }

        private void toolStripKeyboardButton_Click(object sender, EventArgs e)
        {
            (new KeyboardForm(this)).ShowDialog();
        }

        public void onUserDialDigit(string digits)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                SipekResources.CallManager.OnUserDialDigit(((CStateMachine)lvi.Tag).Session, digits, SipekResources.Configurator.DtmfMode);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /// Audio Control

        private Mixers mMixers;
        private bool mAvoidEvents = false;

        private void LoadAudioValues()
        {
            try
            {
                mMixers = new Mixers();
            }
            catch (Exception e)
            {
                ///report error
                (new ErrorDialog("Initialize Error ", "Audio Mixer cannot initialize! \r\nCheck audio configuration and start again!\r\n" + e.Message)).ShowDialog();
                return;
            }
            // set callback
            mMixers.Playback.MixerLineChanged += new WaveLib.AudioMixer.Mixer.MixerLineChangeHandler(mMixer_MixerLineChanged);
            mMixers.Recording.MixerLineChanged += new WaveLib.AudioMixer.Mixer.MixerLineChangeHandler(mMixer_MixerLineChanged);

            MixerLine pbline = mMixers.Playback.UserLines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.SRC_WAVEOUT);

            toolStripTrackBar1.Tag = pbline;
            toolStripMuteButton.Tag = pbline;
            MixerLine recline = mMixers.Recording.UserLines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.SRC_MICROPHONE);
            toolStripTrackBar2.Tag = recline;
            toolStripMicMuteButton.Tag = recline;

            //If it is 2 channels then ask both and set the volume to the bigger but keep relation between them (Balance)
            int volume = 0;
            float balance = 0;
            if (pbline.Channels != 2)
                volume = pbline.Volume;
            else
            {
                pbline.Channel = Channel.Left;
                int left = pbline.Volume;
                pbline.Channel = Channel.Right;
                int right = pbline.Volume;
                if (left > right)
                {
                    volume = left;
                    balance = (volume > 0) ? -(1 - (right / (float)left)) : 0;
                }
                else
                {
                    volume = right;
                    balance = (volume > 0) ? (1 - (left / (float)right)) : 0;
                }
            }

            if (volume >= 0)
                this.toolStripTrackBar1.Value = volume;
            else
                this.toolStripTrackBar1.Enabled = false;

            // toolstrip checkboxes
            this.toolStripMuteButton.Checked = pbline.Mute;
            this.toolStripMicMuteButton.Checked = recline.Volume == 0 ? true : false;
            _lastMicVol = recline.Volume;
        }

        /// <summary>
        /// Callback from Windows Volume Control
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="line"></param>
        private void mMixer_MixerLineChanged(Mixer mixer, MixerLine line)
        {
            mAvoidEvents = true;

            try
            {
                float balance = -1;
                MixerLine frontEndLine = (MixerLine)toolStripTrackBar1.Tag;
                if (frontEndLine == line)
                {
                    int volume = 0;
                    if (line.Channels != 2)
                        volume = line.Volume;
                    else
                    {
                        line.Channel = Channel.Left;
                        int left = line.Volume;
                        line.Channel = Channel.Right;
                        int right = line.Volume;
                        if (left > right)
                        {
                            volume = left;
                            // TIP: Do not reset the balance if both left and right channel have 0 value
                            if (left != 0 && right != 0)
                                balance = (volume > 0) ? -(1 - (right / (float)left)) : 0;
                        }
                        else
                        {
                            volume = right;
                            // TIP: Do not reset the balance if both left and right channel have 0 value
                            if (left != 0 && right != 0)
                                balance = (volume > 0) ? 1 - (left / (float)right) : 0;
                        }
                    }

                    if (volume >= 0)
                        toolStripTrackBar1.Value = volume;

                }

                // adjust toolstrip checkboxes
                if ((MixerLine)toolStripMicMuteButton.Tag == line)
                {
                    toolStripMicMuteButton.Checked = line.Volume == 0 ? true : false;

                    toolStripMicMuteButton.Image = line.Volume == 0 ? Sipek.Properties.Resources.ico_mutedin : Sipek.Properties.Resources.ico_mutein;
                }
                else if ((MixerLine)toolStripMuteButton.Tag == line)
                {
                    toolStripMuteButton.Checked = line.Mute;

                    toolStripMuteButton.Image = line.Mute ? Sipek.Properties.Resources.ico_mutedout : Sipek.Properties.Resources.ico_muteout;

                }
            }
            finally
            {
                mAvoidEvents = false;
            }
        }

        private void toolStripTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (mAvoidEvents)
                return;

            TrackBar tBar = (TrackBar)sender;
            MixerLine line = (MixerLine)tBar.Tag;
            if (line.Direction == MixerType.Recording)
            {
                line.Volume = toolStripTrackBar2.Value;
                line.Mute = line.Volume == 0;
                toolStripMicMuteButton.Image = line.Mute ? Sipek.Properties.Resources.ico_mutedin : Sipek.Properties.Resources.ico_mutein;
                toolStripMicMuteButton.Checked = line.Mute;
            }
            else
            {
                if (line.Channels != 2)
                {
                    // One channel or more than two let set the volume uniform
                    line.Channel = Channel.Uniform;
                    line.Volume = tBar.Value;
                }
                else
                {
                    //Set independent volume
                    line.Channel = Channel.Uniform;
                    line.Volume = toolStripTrackBar1.Value;
                }

                line.Mute = line.Volume == 0;
                toolStripMuteButton.Image = line.Mute ? Sipek.Properties.Resources.ico_mutedout : Sipek.Properties.Resources.ico_muteout;
                toolStripMuteButton.Checked = line.Mute;
            }
        }

        private int _lastMicVol = 0;
        private int _lastVol = 0;
        private void toolStripMuteButton_Click(object sender, EventArgs e)
        {
            ToolStripButton chkBox = (ToolStripButton)sender;
            MixerLine line = (MixerLine)chkBox.Tag;
            if (line.Direction == MixerType.Recording)
            {
                if (chkBox.Checked == true)
                {
                    _lastMicVol = line.Volume;
                    line.Volume = 0;
                }
                else
                {
                    line.Volume = _lastMicVol;
                }

                toolStripTrackBar2.Value = line.Volume;

                toolStripMicMuteButton.Image = line.Volume == 0 ? Sipek.Properties.Resources.ico_mutedin : Sipek.Properties.Resources.ico_mutein;
            }
            else
            {
                if (chkBox.Checked == true)
                {
                    _lastVol = line.Volume;
                    line.Volume = 0;
                }
                else
                {
                    line.Volume = _lastVol;
                }
                line.Mute = chkBox.Checked;
                toolStripMuteButton.Image = line.Mute ? Sipek.Properties.Resources.ico_mutedout : Sipek.Properties.Resources.ico_muteout;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadAudioValues();

            // Register callbacks from callcontrol
            SipekResources.CallManager.CallStateRefresh += onCallStateChanged;
            SipekResources.CallManager.IncomingCallNotification += new DIncomingCallNotification(CallManager_IncomingCallNotification);
            // Register callbacks from pjsipWrapper
            //SipekFactory.getCommonProxy().CallStateChanged += onTelephonyRefresh;
            SipekResources.Messenger.MessageReceived += onMessageReceived;
            SipekResources.Messenger.BuddyStatusChanged += onBuddyStateChanged;
            SipekResources.Registrar.AccountStateChanged += onAccountStateChanged;
            SipekResources.StackProxy.MessageWaitingIndication += onMessageWaitingIndication;

            // Initialize and set factory for CallManager

            int status = SipekResources.CallManager.Initialize();
            SipekResources.CallManager.CallLogger = SipekResources.CallLogger;

            if (status != 0)
            {
                (new ErrorDialog("Initialize Error", "Init SIP stack problem! \r\nPlease, check configuration and start again! \r\nStatus code " + status)).ShowDialog();
                return;
            }

            // initialize Stack
            int t = SipekResources.Registrar.registerAccounts();

            // Initialize BuddyList
            CBuddyList.getInstance().Messenger = SipekResources.Messenger;
            CBuddyList.getInstance().initialize();

            //////////////////////////////////////////////////////////////////////////
            // load settings

            this.UpdateCallRegister();

            // Buddy list
            this.UpdateBuddyList();

            this.UpdateAccountList();

            // Set user status
            toolStripComboBoxUserStatus.SelectedIndex = (int)EUserStatus.AVAILABLE;

            // scoh::::03.04.2008:::pjsip ISSUE??? At startup codeclist is different as later
            // set codecs priority...
            // initialize/reset codecs - enable PCMU and PCMA only
            int noOfCodecs = SipekResources.StackProxy.getNoOfCodecs();
            for (int i = 0; i < noOfCodecs; i++)
            {
                string codecname = SipekResources.StackProxy.getCodec(i);
                if (SipekResources.Configurator.CodecList.Contains(codecname))
                {
                    // leave default
                    SipekResources.StackProxy.setCodecPriority(codecname, 128);
                }
                else
                {
                    // disable
                    SipekResources.StackProxy.setCodecPriority(codecname, 0);
                }
            }

            // timer
            tmr.Interval = 1000;
            tmr.Tick += new EventHandler(UpdateCallTimeout);
        }

        private void listViewAccounts_DoubleClick(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm(this.SipekResources);
            //sf.activateTab("");
            sf.ShowDialog();
        }

        private void unconditionalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFUFlag = unconditionalToolStripMenuItem.Checked;
        }

        private void noReplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFNRFlag = noReplyToolStripMenuItem.Checked;
        }

        private void busyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFBFlag = busyToolStripMenuItem.Checked;
        }

        private void toolStripTextBoxCFUNumber_TextChanged(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFUNumber = toolStripTextBoxCFUNumber.Text;
        }

        private void toolStripTextBoxCFNRNumber_TextChanged(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFNRNumber = toolStripTextBoxCFNRNumber.Text;
        }

        private void toolStripTextBoxCFBNumber_TextChanged(object sender, EventArgs e)
        {
            SipekResources.Configurator.CFBNumber = toolStripTextBoxCFBNumber.Text;
        }

        private void toolStrip3PtyButton_Click(object sender, EventArgs e)
        {
            if (listViewCallLines.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listViewCallLines.SelectedItems[0];
                // TODO implement 3Pty
                SipekResources.CallManager.onUserConference(((CStateMachine)lvi.Tag).Session);
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (IsInitialized)
            {
                //UpdateAccountList();
                UpdateBuddyList();
            }
        }

        private void toolStripMenuItemRemove_Click(object sender, EventArgs e)
        {
            if (listViewBuddies.SelectedItems.Count > 0)
            {
                CBuddyRecord item = (CBuddyRecord)listViewBuddies.SelectedItems[0].Tag;

                CBuddyList.getInstance().deleteRecord(item.Id);
                UpdateBuddyList();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }


    }

    //[System.ComponentModel.DesignerCategory("code")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public partial class ToolStripTrackBar : ToolStripControlHost
    {
        public ToolStripTrackBar()
            : base(CreateControlInstance())
        {

        }

        /// <summary>
        /// Create a strongly typed property called TrackBar - handy to prevent casting everywhere.
        /// </summary>
        public TrackBar TrackBar
        {
            get
            {
                return Control as TrackBar;
            }
        }

        /// <summary>
        /// Create the actual control, note this is static so it can be called from the
        /// constructor.
        ///
        /// </summary>
        /// <returns></returns>
        private static Control CreateControlInstance()
        {
            TrackBar t = new TrackBar();
            t.AutoSize = false;
            t.Height = 16;
            t.TickFrequency = 6553;
            t.SmallChange = 6553;
            t.LargeChange = 10000;
            t.Minimum = 0;
            t.Maximum = 65535;

            // Add other initialization code here.
            return t;
        }

        [DefaultValue(0)]
        public int Value
        {
            get { return TrackBar.Value; }
            set { TrackBar.Value = value; }
        }

        [DefaultValue(0)]
        public new object Tag
        {
            get { return TrackBar.Tag; }
            set { TrackBar.Tag = value; }
        }

        /// <summary>
        /// Attach to events we want to re-wrap
        /// </summary>
        /// <param name="control"></param>
        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            TrackBar trackBar = control as TrackBar;
            trackBar.ValueChanged += new EventHandler(trackBar_ValueChanged);
        }

        /// <summary>
        /// Detach from events.
        /// </summary>
        /// <param name="control"></param>
        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            TrackBar trackBar = control as TrackBar;
            trackBar.ValueChanged -= new EventHandler(trackBar_ValueChanged);

        }


        /// <summary>
        /// Routing for event
        /// TrackBar.ValueChanged -> ToolStripTrackBar.ValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void trackBar_ValueChanged(object sender, EventArgs e)
        {
            // when the trackbar value changes, fire an event.
            if (this.ValueChanged != null)
            {
                ValueChanged(sender, e);
            }
        }

        // add an event that is subscribable from the designer.
        public event EventHandler ValueChanged;


        // set other defaults that are interesting
        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 16);
            }
        }

    }
}
