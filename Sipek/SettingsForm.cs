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
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sipek.Common.CallControl;
using Sipek.Common;
#if LINUX
#else
using WaveLib.AudioMixer; // see http://www.codeproject.com/KB/graphics/AudioLib.aspx
#endif

namespace Sipek
{
  public partial class SettingsForm : Form
  {
    private Mixers mMixers;
    private bool mAvoidEvents;
    private int _lastMicVolume = 0;

    private SipekResources _resources = null;
    public SipekResources SipekResources
    {
      get { return _resources; }
    }

    private bool _restart = false;
    private bool RestartRequired
    {
      get { return _restart; }
      set { _restart = value;}
    }

    private bool _reregister = false;
    private bool ReregisterRequired
    {
      get { return _reregister; }
      set { _reregister = value; }
    }

    public SettingsForm(SipekResources resources)
    {
      InitializeComponent();

      _resources = resources;
    }

    private void updateAccountList()
    {
      int size = SipekResources.Configurator.Accounts.Count;
      comboBoxAccounts.Items.Clear();
      for (int i = 0; i < size; i++)
      {
        IAccount acc = SipekResources.Configurator.Accounts[i];

        if (acc.AccountName.Length == 0)
        {
          comboBoxAccounts.Items.Add("--empty--");
        }
        else
        {
          comboBoxAccounts.Items.Add(acc.AccountName);
        }
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void comboBoxAccounts_SelectedIndexChanged(object sender, EventArgs e)
    {
      int index = comboBoxAccounts.SelectedIndex;

      if (SipekResources.Configurator.DefaultAccountIndex == index)
        checkBoxDefault.Checked = true;
      else
        checkBoxDefault.Checked = false;

      IAccount acc = SipekResources.Configurator.Accounts[index];

      if (acc == null)
      {
        clearAll();
        // error!!!
        return;
      }
      textBoxAccountName.Text = acc.AccountName;
     
      textBoxDisplayName.Text = acc.DisplayName;
      textBoxUsername.Text = acc.UserName;
      textBoxPassword.Text = acc.Password;
      textBoxRegistrarAddress.Text = acc.HostName;
      textBoxProxyAddress.Text = acc.ProxyAddress;
      textBoxDomain.Text = acc.DomainName;
      comboBoxSIPTransport.SelectedIndex = (int)acc.TransportMode;
    }
 
    private void clearAll()
    {
      textBoxAccountName.Text = "";
      textBoxDisplayName.Text = "";
      textBoxUsername.Text = "";
      textBoxPassword.Text = "";
      textBoxRegistrarAddress.Text = "";
      textBoxProxyAddress.Text = "";
      textBoxDomain.Text = "*";
    }

    private void buttonApply_Click(object sender, EventArgs e)
    {
      int index = this.comboBoxAccounts.SelectedIndex;
      if (index >= 0)
      {
        IAccount account = SipekResources.Configurator.Accounts[index];

        account.HostName = textBoxRegistrarAddress.Text;
        account.ProxyAddress = textBoxProxyAddress.Text;
        account.AccountName = textBoxAccountName.Text;
        account.DisplayName = textBoxDisplayName.Text;
        account.Id = textBoxUsername.Text;
        account.UserName = textBoxUsername.Text;
        account.Password = textBoxPassword.Text;
        account.DomainName = textBoxDomain.Text;
        account.TransportMode = (ETransportMode)comboBoxSIPTransport.SelectedIndex;

        if (checkBoxDefault.Checked) SipekResources.Configurator.DefaultAccountIndex = index;
      }
      // Settings
      SipekResources.Configurator.DNDFlag = checkBoxDND.Checked;
      SipekResources.Configurator.AAFlag = checkBoxAA.Checked;
      SipekResources.Configurator.CFUFlag = checkBoxCFU.Checked;
      SipekResources.Configurator.CFNRFlag = checkBoxCFNR.Checked;
      SipekResources.Configurator.CFBFlag = checkBoxCFB.Checked;

      SipekResources.Configurator.CFUNumber = textBoxCFU.Text;
      SipekResources.Configurator.CFNRNumber = textBoxCFNR.Text;
      SipekResources.Configurator.CFBNumber = textBoxCFB.Text;
     
      // additional settings
      SipekResources.Configurator.SIPPort = Int16.Parse(textBoxListenPort.Text);
      SipekResources.Configurator.StunServerAddress = textBoxStunServerAddress.Text;
      SipekResources.Configurator.PublishEnabled = checkBoxPublish.Checked;
      SipekResources.Configurator.Expires = Int32.Parse(textBoxExpires.Text);
      SipekResources.Configurator.VADEnabled = checkBoxVAD.Checked;
      SipekResources.Configurator.ECTail = Int32.Parse(textBoxECTail.Text);
      SipekResources.Configurator.NameServer = textBoxNameServer.Text;

      //////////////////////////////////////////////////////////////////////////
      // skip if stack not initialized
      if (SipekResources.StackProxy.IsInitialized)
      {
        // check if at least 1 codec selected
        if (listBoxEnCodecs.Items.Count == 0)
        {
          (new ErrorDialog("Settings Warning", "No codec selected!")).ShowDialog();
          return;
        }

        // save enabled codec list
        List<string> cl = new List<string>();
        foreach (string item in listBoxEnCodecs.Items)
        {
          cl.Add(item);
        }
        SipekResources.Configurator.CodecList = cl;
      }

    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      buttonApply_Click(sender, e);

      SipekResources.Configurator.Save();

      if (SipekResources.StackProxy.IsInitialized)
      {
        // set codecs priority...
        foreach (string item in listBoxDisCodecs.Items)
        {
          SipekResources.StackProxy.setCodecPriority(item, 0);
        }
        int i = 0;
        foreach (string item in listBoxEnCodecs.Items)
        {
          SipekResources.StackProxy.setCodecPriority(item, 128 - i);
          i++;
        }      
      }

      // reinitialize stack
      if (RestartRequired) SipekResources.StackProxy.initialize();
       
      if (ReregisterRequired) SipekResources.Registrar.registerAccounts();

      // set device Id
      SipekResources.StackProxy.setSoundDevice(mMixers.Playback.DeviceDetail.MixerName, mMixers.Recording.DeviceDetail.MixerName);

      Close();
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
      // Continued
      updateAccountList();

      comboBoxAccounts.SelectedIndex = SipekResources.Configurator.DefaultAccountIndex;

      /////
      checkBoxDND.Checked = SipekResources.Configurator.DNDFlag;
      checkBoxAA.Checked = SipekResources.Configurator.AAFlag;
      checkBoxCFU.Checked = SipekResources.Configurator.CFUFlag;
      checkBoxCFNR.Checked = SipekResources.Configurator.CFNRFlag;
      checkBoxCFB.Checked = SipekResources.Configurator.CFBFlag;

      textBoxCFU.Text = SipekResources.Configurator.CFUNumber;
      textBoxCFNR.Text = SipekResources.Configurator.CFNRNumber;
      textBoxCFB.Text = SipekResources.Configurator.CFBNumber;

      textBoxListenPort.Text = SipekResources.Configurator.SIPPort.ToString();

      textBoxStunServerAddress.Text = SipekResources.Configurator.StunServerAddress;
      comboBoxDtmfMode.SelectedIndex = (int)SipekResources.Configurator.DtmfMode;
      checkBoxPublish.Checked = SipekResources.Configurator.PublishEnabled;
      textBoxExpires.Text = SipekResources.Configurator.Expires.ToString();
      checkBoxVAD.Checked = SipekResources.Configurator.VADEnabled;
      textBoxECTail.Text = SipekResources.Configurator.ECTail.ToString();
      textBoxNameServer.Text = SipekResources.Configurator.NameServer;

      // init audio
                        try {
        mMixers = new Mixers();

                                mMixers.Playback.MixerLineChanged += new WaveLib.AudioMixer.Mixer.MixerLineChangeHandler(mMixer_MixerLineChanged);
        mMixers.Recording.MixerLineChanged += new WaveLib.AudioMixer.Mixer.MixerLineChangeHandler(mMixer_MixerLineChanged);
                       
        LoadDeviceCombos(mMixers);                      
                        }
      catch (Exception ex)
                        {
                          ///report error
        (new ErrorDialog("Initialize Error " + ex.Message, "Audio Mixer cannot initialize! \r\nCheck audio configuration and start again!")).ShowDialog();
      }        

      // load codecs from system
      if (SipekResources.StackProxy.IsInitialized)
      {
        int noofcodecs = SipekResources.StackProxy.getNoOfCodecs();
        for (int i = 0; i < noofcodecs; i++)
        {
          string name = SipekResources.StackProxy.getCodec(i);
          listBoxDisCodecs.Items.Add(name);
        }
        // load enabled codecs from settings
        List<string> codeclist = SipekResources.Configurator.CodecList;
        foreach (string item in codeclist)
        {
          // item match with disabled list (all supported codec)
          if (listBoxDisCodecs.Items.Contains(item))
          {
            // move item from disabled list to enabled
            listBoxDisCodecs.Items.Remove(item);
            listBoxEnCodecs.Items.Add(item);
          }
        }
      }
     
      // set stack flags
      ReregisterRequired = false;
      RestartRequired = false;
    }


    //////////////////////////////////////////////////////////////////////////////////
    /// Audio controls
    ///
    ///
    #region Audio
    private void comboBoxPlaybackDevices_SelectedIndexChanged(object sender, EventArgs e)
    {
      LoadValues(MixerType.Playback);
    }

    private void comboBoxRecordingDevices_SelectedIndexChanged(object sender, EventArgs e)
    {
      LoadValues(MixerType.Recording);
    }

    private void LoadValues(MixerType mixerType)
    {
      MixerLine line;

      //Get info about the lines
      if (mixerType == MixerType.Recording)
      {
        mMixers.Recording.DeviceId = ((MixerDetail)comboBoxRecordingDevices.SelectedItem).DeviceId;
        line = mMixers.Recording.UserLines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.SRC_MICROPHONE);
        trackBarRecordingVolume.Tag = line;
        trackBarRecordingBalance.Tag = line;
        //checkBoxSelectMic.Tag = line;
        checkBoxRecordingMute.Tag = line;
        _lastMicVolume = line.Volume;
        line.Selected = true;
        this.checkBoxRecordingMute.Checked = line.Volume == 0 ? true : false;
      }
      else
      {
        mMixers.Playback.DeviceId = ((MixerDetail)comboBoxPlaybackDevices.SelectedItem).DeviceId;
        line = mMixers.Playback.UserLines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
        trackBarPlaybackVolume.Tag = line;
        trackBarPlaybackBalance.Tag = line;
        checkBoxPlaybackMute.Tag = line;
      }
     
      //If it is 2 channels then ask both and set the volume to the bigger but keep relation between them (Balance)
      int volume = 0;
      float balance = 0;
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
          balance = (volume > 0) ? -(1 - (right / (float)left)) : 0;
        }
        else
        {
          volume = right;
          balance = (volume > 0) ? (1 - (left / (float)right)) : 0;
        }
      }

      if (mixerType == MixerType.Recording)
      {
        if (volume >= 0)
            this.trackBarRecordingVolume.Value = volume;
          else
            this.trackBarRecordingVolume.Enabled = false;
         
        //MONO OR MORE THAN 2 CHANNELS, then let disable balance
        if (line.Channels != 2)
          this.trackBarRecordingBalance.Enabled = false;
        else
          this.trackBarRecordingBalance.Value = (int)(trackBarRecordingBalance.Maximum * balance);
      }
      else
      {
        if (volume >= 0)
          this.trackBarPlaybackVolume.Value = volume;
        else
          this.trackBarPlaybackVolume.Enabled = false;

        //MONO OR MORE THAN 2 CHANNELS, then let disable balance
        if (line.Channels != 2)
          this.trackBarPlaybackBalance.Enabled = false;
        else
          this.trackBarPlaybackBalance.Value = (int)(trackBarPlaybackBalance.Maximum * balance);
      }
     
      // checkbox
      this.checkBoxPlaybackMute.Checked = line.Mute;
      //this.checkBoxSelectMic.Checked = line.Selected;
    }

    private void LoadDeviceCombos(Mixers mixers)
    {
      //Load Output Combo
      MixerDetail mixerDetailDefault = new MixerDetail();
      mixerDetailDefault.DeviceId = -1;
      mixerDetailDefault.MixerName = "Default";
      mixerDetailDefault.SupportWaveOut = true;
      comboBoxPlaybackDevices.Items.Add(mixerDetailDefault);
      foreach (MixerDetail mixerDetail in mixers.Playback.Devices)
      {
        comboBoxPlaybackDevices.Items.Add(mixerDetail);
      }
      comboBoxPlaybackDevices.SelectedIndex = 0;

      //Load Input Combo
      mixerDetailDefault = new MixerDetail();
      mixerDetailDefault.DeviceId = -1;
      mixerDetailDefault.MixerName = "Default";
      mixerDetailDefault.SupportWaveIn = true;
      comboBoxRecordingDevices.Items.Add(mixerDetailDefault);
      foreach (MixerDetail mixerDetail in mixers.Recording.Devices)
      {
        comboBoxRecordingDevices.Items.Add(mixerDetail);
      }
      comboBoxRecordingDevices.SelectedIndex = 0;
    }

    private void mMixer_MixerLineChanged(Mixer mixer, MixerLine line)
    {
      mAvoidEvents = true;

      try
      {
        if (line.Direction == MixerType.Playback)
        {
          float balance = adjustValues(line, trackBarPlaybackVolume);

          //Set the balance
          if (balance != -1)
          {
            if ((MixerLine)trackBarPlaybackBalance.Tag == line)
            {
              trackBarPlaybackBalance.Value = (int)(trackBarPlaybackBalance.Maximum * balance);
            }
          }

          // adjust checkboxes
          checkBoxPlaybackMute.Checked = line.Mute;
        }
        else if (line.Direction == MixerType.Recording)
        {
          line.Channel = Channel.Uniform;
          // adjust recording
          float balance = adjustValues(line, trackBarRecordingVolume);
          //Set the balance
          //if (balance != -1)
          //{
          //  if ((MixerLine)trackBarRecordingBalance.Tag == line)
          //  {
          //    trackBarRecordingBalance.Value = (int)(trackBarRecordingBalance.Maximum * balance);
          //  }
          //}
          // adjust checkboxes
          checkBoxRecordingMute.Checked = (line.Volume == 0 ? true : false);

        }
      }
      finally
      {
        mAvoidEvents = false;
      }
    }

    private float adjustValues(MixerLine line, TrackBar tBar)
    {
      float balance = -1;
      MixerLine frontEndLine = (MixerLine)tBar.Tag;
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
          tBar.Value = volume;

      }
      return balance;
    }

    private void trackBarPlaybackVolume_ValueChanged(object sender, EventArgs e)
    {
      if (mAvoidEvents)
        return;
     
      TrackBar tBar = (TrackBar)sender;
      MixerLine line = (MixerLine)tBar.Tag;
      if (line.Channels != 2)
      {
        // One channel or more than two let set the volume uniform
        line.Channel = Channel.Uniform;
        line.Volume = tBar.Value;
      }
      else
      {
        TrackBar tBarBalance = trackBarPlaybackBalance;
        //Set independent volume
        //foreach (TrackBar tBarBalance in tBarBalanceArray[(int)line.Mixer.MixerType])
        {
          MixerLine frontEndLine = (MixerLine)tBarBalance.Tag;
          if (frontEndLine == line)
          {
            if (tBarBalance.Value == 0)
            {
              line.Channel = Channel.Uniform;
              line.Volume = tBar.Value;
            }
            if (tBarBalance.Value <= 0)
            {
              // Left channel is bigger
              line.Channel = Channel.Left;
              line.Volume = tBar.Value;
              line.Channel = Channel.Right;
              line.Volume = (int)(tBar.Value * (1 + (tBarBalance.Value / (float)tBarBalance.Maximum)));
            }
            else
            {
              // Right channel is bigger
              line.Channel = Channel.Right;
              line.Volume = tBar.Value;
              line.Channel = Channel.Left;
              line.Volume = (int)(tBar.Value * (1 - (tBarBalance.Value / (float)tBarBalance.Maximum)));
            }
            //break;
          }
        }
      }
    }

    private void trackBarPlaybackBalance_ValueChanged(object sender, EventArgs e)
    {
      if (mAvoidEvents)
        return;

      TrackBar tBarBalance = (TrackBar)sender;
      MixerLine line = (MixerLine)tBarBalance.Tag;

      //This demo just set balance when they are just 2 channels
      if (line.Channels == 2)
      {
        //Set independent volume
        TrackBar tBarVolume = trackBarPlaybackVolume;
        //foreach (TrackBar tBarVolume in tBarArray[(int)line.Mixer.MixerType])
        {
          MixerLine frontEndLine = (MixerLine)tBarVolume.Tag;
          if (frontEndLine == line)
          {
            if (tBarBalance.Value == 0)
            {
              line.Channel = Channel.Uniform;
              line.Volume = tBarVolume.Value;
            }
            if (tBarBalance.Value <= 0)
            {
              // Left channel is bigger
              line.Channel = Channel.Left;
              line.Volume = tBarVolume.Value;
              line.Channel = Channel.Right;
              line.Volume = (int)(tBarVolume.Value * (1 + (tBarBalance.Value / (float)tBarBalance.Maximum)));
            }
            else
            {
              // Rigth channel is bigger
              line.Channel = Channel.Right;
              line.Volume = tBarVolume.Value;
              line.Channel = Channel.Left;
              line.Volume = (int)(tBarVolume.Value * (1 - (tBarBalance.Value / (float)tBarBalance.Maximum)));
            }
            //break;
          }
        }
      }

    }

    private void trackBarRecordingVolume_ValueChanged(object sender, EventArgs e)
    {
      if (mAvoidEvents)
        return;

      TrackBar tBar = (TrackBar)sender;
      MixerLine line = (MixerLine)tBar.Tag;
      // One channel or more than two let set the volume uniform
      line.Channel = Channel.Uniform;
      line.Volume = tBar.Value;

      _lastMicVolume = line.Volume;

      this.checkBoxRecordingMute.Checked = line.Volume == 0 ? true : false;
    }

    private void checkBoxPlaybackMute_CheckedChanged(object sender, EventArgs e)
    {
      CheckBox chkBox = (CheckBox)sender;
      MixerLine line = (MixerLine)chkBox.Tag;
      if (line.Direction == MixerType.Recording)
      {
        line.Channel = Channel.Uniform;
        //line.Selected = chkBox.Checked;
        if (checkBoxRecordingMute.Checked == true)
        {
          _lastMicVolume = line.Volume;
          line.Volume = 0;
        }
        else
        {
          line.Volume = _lastMicVolume;
        }
      }
      // set mute if possible
      if (line.ContainsMute)  line.Mute = chkBox.Checked;
    }
   
    #endregion Audio

    public void activateTab(int index)
    {
      this.tabControlSettings.SelectTab(index);
    }

    private void buttonEnable_Click(object sender, EventArgs e)
    {
      if (listBoxDisCodecs.SelectedItems.Count > 0)
      {
        // get selected item from disabled codecs
        listBoxEnCodecs.Items.Add(listBoxDisCodecs.SelectedItem);
        // remove from disabled list
        listBoxDisCodecs.Items.Remove(listBoxDisCodecs.SelectedItem);
      }
    }

    private void buttonDisable_Click(object sender, EventArgs e)
    {
      if (listBoxEnCodecs.SelectedItems.Count > 0)
      {
        // get selected item from enabled codecs
        listBoxDisCodecs.Items.Add(listBoxEnCodecs.SelectedItem);
        // remove from enabled list
        listBoxEnCodecs.Items.Remove(listBoxEnCodecs.SelectedItem);
      }
    }

    private void reregistrationRequired_TextChanged(object sender, EventArgs e)
    {
      ReregisterRequired = true;
    }

    private void restartRequired_TextChanged(object sender, EventArgs e)
    {
      RestartRequired = true;
      ReregisterRequired = true;
    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      SipekResources.Configurator.DtmfMode = (EDtmfMode)comboBoxDtmfMode.SelectedIndex;
    }

    private void checkBoxDefault_CheckedChanged(object sender, EventArgs e)
    {
      ReregisterRequired = true;
    }

    private void comboBoxSIPTransport_SelectedIndexChanged(object sender, EventArgs e)
    {
      // get current account
      int accountId = this.comboBoxAccounts.SelectedIndex;
      if ((accountId >= 0)&&(accountId < 5))
      {
        SipekResources.Configurator.Accounts[accountId].TransportMode = (ETransportMode)comboBoxSIPTransport.SelectedIndex;
      }
    }

    private void checkBoxPublish_CheckedChanged(object sender, EventArgs e)
    {
      SipekResources.Configurator.PublishEnabled = checkBoxPublish.Checked;
      RestartRequired = true;
      ReregisterRequired = true;
    }


    private void numEvaluate_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '\b'))
      {
        e.Handled = true;
      }
    }

  }
}
