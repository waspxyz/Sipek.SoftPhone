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
 * @see http://sites.google.com/site/sipekvoip
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using Sipek.Common;
using System.Runtime.InteropServices;

namespace Sipek.Sip
{

    public class pjsipPresenceAndMessaging : IPresenceAndMessaging
    {
        #region Dll declarations


#if LINUX
                internal const string PJSIP_DLL = "libpjsipDll.so";
#elif MOBILE
                internal const string PJSIP_DLL = "pjsipdll_mobile.dll";
#elif TLS
                internal const string PJSIP_DLL = "pjsipdll_tls.dll";
#else
        internal const string PJSIP_DLL = "pjsipDll.dll";
#endif

        [DllImport(PJSIP_DLL, EntryPoint = "dll_addBuddy")]
        private static extern int dll_addBuddy(string uri, bool subscribe);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_removeBuddy")]
        private static extern int dll_removeBuddy(int buddyId);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_sendMessage")]
        private static extern int dll_sendMessage(int buddyId, string uri, string message);
        [DllImport(PJSIP_DLL, EntryPoint = "dll_setStatus")]
        private static extern int dll_setStatus(int accId, int presence_state);
        #endregion

        #region Callback declarations
        delegate int OnMessageReceivedCallback(string from, string message);
        delegate int OnBuddyStatusChangedCallback(int buddyId, int status, string statusText);

        [DllImport(PJSIP_DLL)]
        private static extern int onMessageReceivedCallback(OnMessageReceivedCallback cb);
        [DllImport(PJSIP_DLL)]
        private static extern int onBuddyStatusChangedCallback(OnBuddyStatusChangedCallback cb);

        static OnMessageReceivedCallback mrdel = new OnMessageReceivedCallback(onMessageReceived);
        static OnBuddyStatusChangedCallback bscdel = new OnBuddyStatusChangedCallback(onBuddyStatusChanged);

        #endregion

        #region Constructor
        static private pjsipPresenceAndMessaging _instance = null;
        static public pjsipPresenceAndMessaging Instance
        {
            get
            {
                if (_instance == null) _instance = new pjsipPresenceAndMessaging();
                return _instance;
            }
        }

        private pjsipPresenceAndMessaging()
        {
            onBuddyStatusChangedCallback(bscdel);
            onMessageReceivedCallback(mrdel);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Add new entry in a buddy list and subscribe presence
        /// </summary>
        /// <param name="ident">Buddy address (without hostname part</param>
        /// <param name="presence">subscribe presence flag</param>
        /// <returns></returns>
        public override int addBuddy(string name, bool presence, int accId)
        {
            string sipuri = "";

            if (!pjsipStackProxy.Instance.IsInitialized) return -1;

            // check if name contains URI
            if (name.IndexOf("sip:") == 0)
            {
                // do nothing...
                sipuri = name;
            }
            else
            {
                sipuri = "sip:" + name + "@" + Config.Accounts[accId].HostName;
            }
            // check transport - if TCP add transport=TCP
            sipuri = pjsipStackProxy.Instance.SetTransport(accId, sipuri);

            return dll_addBuddy(sipuri, presence);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="buddyId"></param>
        /// <returns></returns>
        public override int delBuddy(int buddyId)
        {
            return dll_removeBuddy(buddyId);
        }

        /// <summary>
        /// Send an instance message
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override int sendMessage(string destAddress, string message, int accId)
        {
            if (!pjsipStackProxy.Instance.IsInitialized) return -1;

            string sipuri = "";

            // check if name contains URI
            if (destAddress.IndexOf("sip:") == 0)
            {
                // do nothing...
                sipuri = destAddress;
            }
            else
            {
                sipuri = "sip:" + destAddress + "@" + Config.Accounts[accId].HostName;
            }
            // set transport
            sipuri = pjsipStackProxy.Instance.SetTransport(accId, sipuri);

            return dll_sendMessage(Config.Accounts[accId].Index, sipuri, message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override int sendMessage(string destAddress, string message)
        {
            return sendMessage(destAddress, message, Config.Accounts[Config.DefaultAccountIndex].Index);
        }

        /// <summary>
        /// Set presence status
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public override int setStatus(int accId, EUserStatus status)
        {
            if ((!pjsipStackProxy.Instance.IsInitialized) || (accId < 0)) return -1;

            if ((Config.Accounts.Count > 0) && (Config.Accounts[accId].RegState != 200)) return -1;
            if (!Config.PublishEnabled) return -1;

            return dll_setStatus(Config.Accounts[accId].Index, (int)status);
        }

        #endregion

        #region Callbacks
        private static int onMessageReceived(string from, string text)
        {
            Instance.BaseMessageReceived(from.ToString(), text.ToString());
            return 1;
        }

        private static int onBuddyStatusChanged(int buddyId, int status, string text)
        {
            Instance.BaseBuddyStatusChanged(buddyId, status, text.ToString());
            return 1;
        }
        #endregion
    }
}
