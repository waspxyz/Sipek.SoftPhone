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
using System.Text;
using System.Timers;
using System.Runtime.InteropServices;
using System.Media;
using Sipek.Common;
using Sipek.Common.CallControl;
using Sipek.Sip;
using ServiceStack.Text;

namespace Sipek
{
    /// <summary>
    /// ConcreteFactory
    /// Implementation of AbstractFactory.
    /// </summary>
    public class SipekResources : AbstractFactory
    {
        MainForm _form; // reference to MainForm to provide timer context
        IMediaProxyInterface _mediaProxy = new CMediaPlayerProxy();
        ICallLogInterface _callLogger = new CCallLog();
        pjsipStackProxy _stackProxy = pjsipStackProxy.Instance;
        SipekConfigurator _config = new SipekConfigurator();

        #region Constructor
        public SipekResources(MainForm mf)
        {
            _form = mf;

            System.IO.StreamReader config = new System.IO.StreamReader("config.txt", System.Text.Encoding.GetEncoding("utf-8"));
            _config = config.ReadToEnd().FromJson<SipekConfigurator>();
            config.Close();
            config.Dispose();


            //List<IAccount> acc = new List<IAccount>();
            //acc.Add(new SipekAccount(0) { UserName = "1001", Enabled = true, AccountName = "1001", DisplayName = "1001", DomainName = "1001", HostName = "1001", Id = "1001", Password = "1001", ProxyAddress = "192.168.9.199", RegState = -1, TransportMode = Common.ETransportMode.TM_UDP });

            //_config.Accounts = acc;

            //string ss = _config.ToJson();
            // initialize sip struct at startup
            SipConfigStruct.Instance.stunServer = this.Configurator.StunServerAddress;
            SipConfigStruct.Instance.publishEnabled = this.Configurator.PublishEnabled;
            SipConfigStruct.Instance.expires = this.Configurator.Expires;
            SipConfigStruct.Instance.VADEnabled = this.Configurator.VADEnabled;
            SipConfigStruct.Instance.ECTail = this.Configurator.ECTail;
            SipConfigStruct.Instance.nameServer = this.Configurator.NameServer;

            // initialize modules
            _callManager.StackProxy = _stackProxy;
            _callManager.Config = _config;
            _callManager.Factory = this;
            _callManager.MediaProxy = _mediaProxy;
            _stackProxy.Config = _config;
            _registrar.Config = _config;
            _messenger.Config = _config;


        }
        #endregion Constructor

        #region AbstractFactory methods
        public ITimer createTimer()
        {
            return new GUITimer(_form);
        }

        public IStateMachine createStateMachine()
        {
            // TODO: check max number of calls
            return new CStateMachine();
        }

        #endregion

        #region Other Resources
        public pjsipStackProxy StackProxy
        {
            get { return _stackProxy; }
            set { _stackProxy = value; }
        }

        public SipekConfigurator Configurator
        {
            get { return _config; }
            set { }
        }

        // getters
        public IMediaProxyInterface MediaProxy
        {
            get { return _mediaProxy; }
            set { }
        }

        public ICallLogInterface CallLogger
        {
            get { return _callLogger; }
            set { }
        }

        private IRegistrar _registrar = pjsipRegistrar.Instance;
        public IRegistrar Registrar
        {
            get { return _registrar; }
        }

        private IPresenceAndMessaging _messenger = pjsipPresenceAndMessaging.Instance;
        public IPresenceAndMessaging Messenger
        {
            get { return _messenger; }
        }

        private CCallManager _callManager = CCallManager.Instance;
        public CCallManager CallManager
        {
            get { return CCallManager.Instance; }
        }
        #endregion
    }

    #region Concrete implementations

    public class GUITimer : ITimer
    {
        Timer _guiTimer;
        MainForm _form;


        public GUITimer(MainForm mf)
        {
            _form = mf;
            _guiTimer = new Timer();
            if (this.Interval > 0) _guiTimer.Interval = this.Interval;
            _guiTimer.Interval = 100;
            _guiTimer.Enabled = true;
            _guiTimer.Elapsed += new ElapsedEventHandler(_guiTimer_Tick);
        }

        void _guiTimer_Tick(object sender, EventArgs e)
        {
            _guiTimer.Stop();
            //_elapsed(sender, e);
            // Synchronize thread with GUI because SIP stack works with GUI thread only
            if ((_form.IsDisposed) || (_form.Disposing) || (!_form.IsInitialized))
            {
                return;
            }
            _form.Invoke(_elapsed, new object[] { sender, e });
        }

        public bool Start()
        {
            _guiTimer.Start();
            return true;
        }

        public bool Stop()
        {
            _guiTimer.Stop();
            return true;
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; _guiTimer.Interval = value; }
        }

        private TimerExpiredCallback _elapsed;
        public TimerExpiredCallback Elapsed
        {
            set
            {
                _elapsed = value;
            }
        }
    }


    // Accounts
    [Serializable]
    public class SipekAccount : IAccount
    {
        private int _index = -1;
        //  private int _accountIdentification = -1;

        public SipekAccount(int index)
        {
            this._index = index;
        }

        public int Index
        {
            get;
            set;
        }

        public string AccountName
        {
            get;
            set;
        }

        public bool Enabled
        { set; get; }

        public string HostName
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string DomainName
        {
            get;
            set;
        }

        public int RegState
        {
            get;
            set;
        }

        public string ProxyAddress
        {
            get;
            set;
        }

        public ETransportMode TransportMode
        {
            get;
            set;
        }

    }

    [Serializable]
    public class SipekConfigurator : IConfiguratorInterface
    {


        public SipekConfigurator()
        {
        }

        public bool IsNull
        {
            get
            {
                return false;
            }
        }

        private bool _CFUFlag = false;
        public bool CFUFlag
        {
            get { return _CFUFlag; }
            set { _CFUFlag = value; }
        }

        private string _CFUNumber = string.Empty;
        public string CFUNumber
        {
            get { return _CFUNumber; }
            set { _CFUNumber = value; }
        }

        private bool _CFNRFlag = false;
        public bool CFNRFlag
        {
            get { return _CFNRFlag; }
            set { _CFNRFlag = value; }
        }

        private string _CFNRNumber = string.Empty;
        public string CFNRNumber
        {
            get { return _CFNRNumber; }
            set { _CFNRNumber = value; }
        }

        private bool _DNDFlag = false;
        public bool DNDFlag
        {
            get { return _DNDFlag; }
            set { _DNDFlag = value; }
        }

        private bool _AAFlag = false;
        public bool AAFlag
        {
            get { return _AAFlag; }
            set { _AAFlag = value; }
        }

        private bool _CFBFlag = false;
        public bool CFBFlag
        {
            get { return _CFBFlag; }
            set { _CFBFlag = value; }
        }

        private string _CFBNumber = string.Empty;
        public string CFBNumber
        {
            get { return _CFBNumber; }
            set { _CFBNumber = value; }
        }

        private int _SIPPort = 5060;
        public int SIPPort
        {
            get { return _SIPPort; }
            set { _SIPPort = value; }
        }

        private bool _PublishEnabled = false;
        public bool PublishEnabled
        {
            get { return _PublishEnabled; }
            set { _PublishEnabled = value; }
        }

        private string _StunServerAddress = string.Empty;
        public string StunServerAddress
        {
            get { return _StunServerAddress; }
            set { _StunServerAddress = value; }
        }

        private EDtmfMode _DtmfMode = EDtmfMode.DM_Outband;
        public EDtmfMode DtmfMode
        {
            get { return _DtmfMode; }
            set { _DtmfMode = value; }
        }

        private int _Expires = 3600;
        public int Expires
        {
            get { return _Expires; }
            set { _Expires = value; }
        }

        private int _ECTail = 200;
        public int ECTail
        {
            get { return _ECTail; }
            set { _ECTail = value; }
        }

        private bool _VADEnabled = true;
        public bool VADEnabled
        {
            get { return _VADEnabled; }
            set { _VADEnabled = value; }
        }

        private string _NameServer = string.Empty;
        public string NameServer
        {
            get { return _NameServer; }
            set { _NameServer = value; }
        }

        private int _DefaultAccountIndex = 0;
        public int DefaultAccountIndex
        {
            get { return _DefaultAccountIndex; }
            set { _DefaultAccountIndex = value; }
        }


        private List<IAccount> _Accounts = new List<IAccount>();
        public List<IAccount> Accounts
        {
            get { return _Accounts; }
            set { _Accounts = value; }
            //get
            //{
            //    List<IAccount> list = new List<IAccount>();
            //    for (int index = 0; index < 5; ++index)
            //    {
            //        IAccount account = new SipekAccount(index);
            //        list.Add(account);
            //    }
            //    return list;
            //}
        }


        private List<string> _CodecList = new List<string>();
        public List<string> CodecList
        {
            //get
            //{
            //    List<string> list = new List<string>();
            //    foreach (string str in Settings.Default.cfgCodecList)
            //        list.Add(str);
            //    return list;
            //}
            //set
            //{
            //    Settings.Default.cfgCodecList.Clear();
            //    foreach (string str in value)
            //        Settings.Default.cfgCodecList.Add(str);
            //}

            get { return _CodecList; }
            set { _CodecList = value; }
        }

        public void Save()
        {
            string cfgPath = "config.txt";
            if (System.IO.File.Exists(cfgPath))
            {
                System.IO.File.Delete(cfgPath);
            }
            System.IO.FileStream f = System.IO.File.Create(cfgPath);
            f.Close();
            f.Dispose();
            string cfg = this.ToJson();
            System.IO.StreamWriter f2 = new System.IO.StreamWriter(cfgPath, true, System.Text.Encoding.UTF8);
            f2.WriteLine(cfg);
            f2.Close();
            f2.Dispose();
        }
    }


    //////////////////////////////////////////////////////
    // Media proxy
    // internal class
    public class CMediaPlayerProxy : IMediaProxyInterface
    {
        SoundPlayer player = new SoundPlayer();

        #region Methods

        public int playTone(ETones toneId)
        {
            string fname;

            switch (toneId)
            {
                case ETones.EToneDial:
                    fname = "Sounds/dial.wav";
                    break;
                case ETones.EToneCongestion:
                    fname = "Sounds/congestion.wav";
                    break;
                case ETones.EToneRingback:
                    fname = "Sounds/ringback.wav";
                    break;
                case ETones.EToneRing:
                    fname = "Sounds/ring.wav";
                    break;
                default:
                    fname = "";
                    break;
            }

            player.SoundLocation = fname;
            player.Load();
            player.PlayLooping();

            return 1;
        }

        public int stopTone()
        {
            player.Stop();
            return 1;
        }

        #endregion

    }

    #endregion Concrete Implementations

}
