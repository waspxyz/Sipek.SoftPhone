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
 *
 * @see http://sites.google.com/site/sipekvoip
 *
 */

using System;
using System.Text;
using Sipek.Common;
using System.Runtime.InteropServices;

namespace Sipek.Sip
{

    // callback delegates
    delegate int OnCallStateChanged(int callId, ESessionState stateId);
    delegate int OnCallIncoming(int callId, string number);
    delegate int OnCallHoldConfirm(int callId);

    /// <summary>
    /// Implementation of ICallProxyInterface interface use by call state machine.
    /// Each call (session) contains an instance of a call proxy.
    /// Current session is identified by SessionId property.
    /// pjsipCallProxy communicates with pjsip stack using API functions and callbacks.
    /// </summary>
    internal class pjsipCallProxy : ICallProxyInterface
    {
        #region DLL declarations

#if LINUX
                internal const string PJSIP_DLL = "libpjsipDll.so";
#elif MOBILE
                internal const string PJSIP_DLL = "pjsipdll_mobile.dll";
#elif TLS
                internal const string PJSIP_DLL = "pjsipdll_tls.dll";
#else
        internal const string PJSIP_DLL = "pjsipDll.dll";
#endif

        // call API
        [DllImport(PJSIP_DLL, EntryPoint = "dll_makeCall")]
        private static extern int dll_makeCall(int accountId, string uri);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_releaseCall")]
        private static extern int dll_releaseCall(int callId);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_answerCall")]
        private static extern int dll_answerCall(int callId, int code);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_holdCall")]
        private static extern int dll_holdCall(int callId);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_retrieveCall")]
        private static extern int dll_retrieveCall(int callId);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_xferCall")]
        private static extern int dll_xferCall(int callId, string uri);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_xferCallWithReplaces")]
        private static extern int dll_xferCallWithReplaces(int callId, int dstSession);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_serviceReq")]
        private static extern int dll_serviceReq(int callId, int serviceCode, string destUri);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_dialDtmf")]
        private static extern int dll_dialDtmf(int callId, string digits, int mode);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_getCurrentCodec")]
        private static extern int dll_getCurrentCodec(int callId, StringBuilder codec);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_makeConference")]
        private static extern int dll_makeConference(int callId);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_sendCallMessage")]
        private static extern int dll_sendCallMessage(int callId, string message);

        #endregion

        #region Callback Declarations
        // passing delegates to unmanaged code (.dll)
        [DllImport(PJSIP_DLL)]
        private static extern int onCallStateCallback(OnCallStateChanged cb);
        [DllImport(PJSIP_DLL)]
        private static extern int onCallIncoming(OnCallIncoming cb);
        [DllImport(PJSIP_DLL)]
        private static extern int onCallHoldConfirmCallback(OnCallHoldConfirm cb);

        // Static declaration because of CallbackonCollectedDelegate exception!
        static OnCallStateChanged csDel = new OnCallStateChanged(onCallStateChanged);
        static OnCallIncoming ciDel = new OnCallIncoming(onCallIncoming);
        static OnCallHoldConfirm chDel = new OnCallHoldConfirm(onCallHoldConfirm);

        #endregion

        #region Properties
        private IConfiguratorInterface _config = new NullConfigurator();

        private IConfiguratorInterface Config
        {
            get { return _config; }
        }

        private int _sessionId;
        public override int SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor called by pjsipWrapper with Config parameter.
        /// Make sure you set Config in pjsipWrapper before using pjsipCallProxy
        /// </summary>
        /// <param name="config"></param>
        internal pjsipCallProxy(IConfiguratorInterface config)
        {
            _config = config;
        }

        /// <summary>
        /// Static initializer. Call this method to set callbacks from SIP stack.
        /// </summary>
        public static void initialize()
        {
            // assign callbacks
            onCallIncoming(ciDel);
            onCallStateCallback(csDel);
            onCallHoldConfirmCallback(chDel);
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Method makeCall creates call session. Checks the 1st parameter
        /// format is SIP URI, if not build one.  
        /// </summary>
        /// <param name="dialedNo"></param>
        /// <param name="accountId"></param>
        /// <returns>SessionId chosen by pjsip stack</returns>
        public override int makeCall(string dialedNo, int accountId)
        {
            string sipuri = "";

            // check if call by URI
            if (dialedNo.IndexOf("sip:") == 0)
            {
                // do nothing...
                sipuri = dialedNo;
            }
            else
            {
                // prepare URI
                sipuri = "sip:" + dialedNo + "@" + Config.Accounts[accountId].HostName;
            }
            // Select configured transport for this account: udp, tcp, tls
            sipuri = pjsipStackProxy.Instance.SetTransport(accountId, sipuri);

            // Don't forget to convert accontId here!!!
            // Store session identification for further requests
            SessionId = dll_makeCall(Config.Accounts[accountId].Index, sipuri);

            return SessionId;
        }

        /// <summary>
        /// End call for a given session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override bool endCall()
        {
            dll_releaseCall(SessionId);
            return true;
        }

        /// <summary>
        /// Signals sip stack that device is alerted (ringing)
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override bool alerted()
        {
            dll_answerCall(SessionId, 180);
            return true;
        }

        /// <summary>
        /// Signals that user accepts the call (asnwer)
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override bool acceptCall()
        {
            dll_answerCall(SessionId, 200);
            return true;
        }

        /// <summary>
        /// Hold request for a given session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override bool holdCall()
        {
            dll_holdCall(SessionId);
            return true;
        }

        /// <summary>
        /// Retrieve request for a given session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override bool retrieveCall()
        {
            dll_retrieveCall(SessionId);
            return true;
        }

        /// <summary>
        /// Trasfer call to number
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public override bool xferCall(string number)
        {
            string uri = "sip:" + number + "@" + Config.Accounts[Config.DefaultAccountIndex].HostName;
            dll_xferCall(SessionId, uri);
            return true;
        }

        /// <summary>
        /// Transfer call to other session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public override bool xferCallSession(int session)
        {
            dll_xferCallWithReplaces(SessionId, session);
            return true;
        }

        /// <summary>
        /// Make conference with given session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public override bool threePtyCall(int session)
        {
            dll_serviceReq(SessionId, (int)EServiceCodes.SC_3PTY, "");
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="code"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public override bool serviceRequest(int code, string dest)
        {
            string destUri = "<sip:" + dest + "@" + Config.Accounts[Config.DefaultAccountIndex].HostName + ">";
            dll_serviceReq(SessionId, (int)code, destUri);
            return true;
        }

        /// <summary>
        /// Send dtmf digit
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="digits"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool dialDtmf(string digits, EDtmfMode mode)
        {
            int status = dll_dialDtmf(SessionId, digits, (int)mode);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string getCurrentCodec()
        {
            StringBuilder codec = new StringBuilder(256);
            int status = dll_getCurrentCodec(SessionId, codec);
            return codec.ToString();
        }

        /// <summary>
        /// Make a conference call
        /// </summary>
        /// <returns></returns>
        public override bool conferenceCall()
        {
            int status = dll_makeConference(SessionId);
            return status == 1 ? true : false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool sendCallMessage(string message)
        {
            int status = dll_sendCallMessage(SessionId, message);
            return (status == 1) ? true : false;
        }

        #endregion Methods

        #region Callbacks

        /// <summary>
        ///
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="callState"></param>
        /// <returns></returns>
        private static int onCallStateChanged(int callId, ESessionState callState)
        {
            ICallProxyInterface.BaseCallStateChanged(callId, callState, "");
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="sturi"></param>
        /// <returns></returns>
        private static int onCallIncoming(int callId, string sturi)
        {
            string uri = sturi;
            string display = "";
            string number = "";

            if (null != uri)
            {
                // get indices
                int startNum = uri.IndexOf("<sip:");
                int atPos = uri.IndexOf('@');
                // search for number
                if ((startNum >= 0) && (atPos > startNum))
                {
                    number = uri.Substring(startNum + 5, atPos - startNum - 5);
                }

                // extract display name if exists
                if (startNum >= 0)
                {
                    display = uri.Remove(startNum, uri.Length - startNum).Trim();
                }
                else
                {
                    int semiPos = display.IndexOf(';');
                    if (semiPos >= 0)
                    {
                        display = display.Remove(semiPos, display.Length - semiPos);
                    }
                    else
                    {
                        int colPos = display.IndexOf(':');
                        if (colPos >= 0)
                        {
                            display = display.Remove(colPos, display.Length - colPos);
                        }
                    }

                }
            }
            // invoke callback
            ICallProxyInterface.BaseCallStateChanged(callId, ESessionState.SESSION_STATE_INCOMING, "");
            ICallProxyInterface.BaseIncomingCall(callId, number, display);
            return 1;
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="callId"></param>
        /// <returns></returns>
        private static int onCallHoldConfirm(int callId)
        {
            //if (sm != null) sm.getState().onHoldConfirm();
            // TODO:::implement proper callback
            BaseCallNotification(callId, ECallNotification.CN_HOLDCONFIRM, "");
            return 1;
        }
        #endregion
    }

}
