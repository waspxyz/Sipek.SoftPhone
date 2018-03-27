/*
 * Copyright (C) 2007 Sasa Coh <sasacoh@gmail.com>
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
 */

using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Sipek.Common;

namespace Sipek
{
    ////////////////////////////////////////////
    //
    /// <summary>
    /// CBuddyMessage
    /// </summary>
    public class CBuddyMessage
    {
        private DateTime _datetime;
        private string _text;

        public string Content
        {
            get { return _text; }
        }

        public CBuddyMessage(DateTime datetime, string text)
        {
            _text = text;
            _datetime = datetime;
        }
    }

    /// <summary>
    /// CBuddyRecord
    /// </summary>
    public class CBuddyRecord
    {
        #region Variables

        private int _id;
        private string _firstName;
        private string _lastName = "";
        private string _number;
        //private string _email;
        private int _accountId;
        private string _uri;
        private bool _presenceEnabled;

        private int _status = 0;
        private string _statusText;
        private Stack<CBuddyMessage> _messageList;

        #endregion

        #region Properties

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        public string LastMessage
        {
            get
            {
                if (_messageList.Count == 0) return "";
                return _messageList.Peek().Content;
            }
        }

        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string StatusText
        {
            get { return _statusText; }
            set { _statusText = value; }
        }

        public bool PresenceEnabled
        {
            get { return _presenceEnabled; }
            set { _presenceEnabled = value; }
        }
        /////////////////////////////////////////////////////

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public int Accountid
        {
            get { return _accountId; }
            set { _accountId = value; }
        }

        #endregion

        #region Constructor

        public CBuddyRecord()
        {
            _messageList = new Stack<CBuddyMessage>();
        }

        #endregion

        #region public methods

        public void addMessage(DateTime datetime, string text)
        {
            CBuddyMessage msg = new CBuddyMessage(datetime, text);
            _messageList.Push(msg);
        }

        public void clearAllMessages()
        {
            _messageList.Clear();
        }

        #endregion

    }

    /// <summary>
    /// CBuddyList
    /// </summary>
    public class CBuddyList
    {
        private static CBuddyList _instance = null;

        private Dictionary<int, CBuddyRecord> _buddyList = new Dictionary<int, CBuddyRecord>();

        private string XMLPhonebookFile = "phonebook.xml";

        private const string FIRSTNAME = "FirstName";
        private const string LASTNAME = "LastName";
        private const string NUMBER = "phonenumber";
        private const string URI = "uri";
        private const string PENABLED = "presence";
        private const string PHONE_URI = "Phone/uri";
        private const string PHONE_NUMBER = "Phone/phonenumber";
        private const string PHONE_PRESENCE = "Phone/presence";

        #region Properties
        public CBuddyRecord this[int index]
        {
            get
            {
                if (!_buddyList.ContainsKey(index)) return null;
                return _buddyList[index];
            }
        }

        public int BuddyCount
        {
            get { return _buddyList.Count; }
        }

        private IPresenceAndMessaging _messenger = null;
        public IPresenceAndMessaging Messenger
        {
            get { return _messenger; }
            set { _messenger = value; }
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////
        #region Constructor
        public static CBuddyList getInstance()
        {
            if (_instance == null)
            {
                _instance = new CBuddyList();
            }
            return _instance;
        }

        protected CBuddyList()
        {
        }

        #endregion

        #region Private Methods

        public void initialize()
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(XMLPhonebookFile);
            }
            catch (System.IO.FileNotFoundException ee)
            {
                System.Console.WriteLine(ee.Message);

                XmlNode root = xmlDocument.CreateNode("element", "Phonebook", "");
                xmlDocument.AppendChild(root);

            }
            catch (System.Xml.XmlException e) { System.Console.WriteLine(e.Message); }


            XmlNodeList list = xmlDocument.SelectNodes("/Phonebook/Record");

            foreach (XmlNode item in list)
            {
                CBuddyRecord record = new CBuddyRecord();

                XmlNode snode = item.SelectSingleNode(FIRSTNAME);
                if ((snode != null) && (snode.FirstChild.Value != null)) record.FirstName = snode.FirstChild.Value;

                snode = item.SelectSingleNode(LASTNAME);
                if ((snode != null) && (snode.FirstChild != null) && (snode.FirstChild.Value != null)) record.LastName = snode.FirstChild.Value;

                snode = item.SelectSingleNode(PHONE_NUMBER);
                if ((snode != null) && (snode.FirstChild.Value != null)) record.Number = snode.FirstChild.Value;

                snode = item.SelectSingleNode(PHONE_URI);
                if ((snode != null) && (snode.FirstChild != null)) record.Uri = snode.FirstChild.Value;

                snode = item.SelectSingleNode(PHONE_PRESENCE);
                if ((snode != null) && (snode.FirstChild != null)) record.PresenceEnabled = (snode.FirstChild.Value == "" ? false : true);

                this.addRecord(record);
            }
        }

        #endregion


        #region Public methods

        public Dictionary<int, CBuddyRecord> getList()
        {
            return _buddyList;
        }

        public CBuddyRecord getRecord(int id)
        {
            if (_buddyList.ContainsKey(id)) return _buddyList[id];
            return null;
        }

        public int getBuddyId(string buddy)
        {
            int buddyId = -1;
            Dictionary<int, CBuddyRecord>.ValueCollection col = _buddyList.Values;
            foreach (CBuddyRecord item in col)
            {
                if ((item.FirstName.Contains(buddy) == true) || (item.LastName.Contains(buddy) == true)
                    || (item.Number.Contains(buddy) == true))
                {
                    return item.Id;
                }
            }
            return buddyId;
        }

        public void save()
        {
            try
            {
                // serialize data
                XmlDocument xmldoc = new XmlDocument();

                XmlNode nodeRoot = xmldoc.CreateNode("element", "Phonebook", "");

                foreach (KeyValuePair<int, CBuddyRecord> kvp in _buddyList)
                {
                    XmlNode nodeRecord = xmldoc.CreateNode("element", "Record", "");

                    XmlElement ellastname = xmldoc.CreateElement(LASTNAME);
                    ellastname.InnerText = kvp.Value.LastName;
                    nodeRecord.AppendChild(ellastname);

                    XmlElement elname = xmldoc.CreateElement(FIRSTNAME);
                    elname.InnerText = kvp.Value.FirstName;
                    nodeRecord.AppendChild(elname);

                    XmlElement phelem = xmldoc.CreateElement("Phone");
                    {
                        XmlElement elNumber = xmldoc.CreateElement(NUMBER);
                        elNumber.InnerText = kvp.Value.Number;
                        phelem.AppendChild(elNumber);

                        XmlElement elUri = xmldoc.CreateElement(URI);
                        elUri.InnerText = kvp.Value.Uri;
                        phelem.AppendChild(elUri);

                        XmlElement elEnabled = xmldoc.CreateElement(PENABLED);
                        elEnabled.InnerText = kvp.Value.PresenceEnabled == true ? "1" : "";
                        phelem.AppendChild(elEnabled);
                    }

                    nodeRecord.AppendChild(phelem);

                    nodeRoot.AppendChild(nodeRecord);
                }
                xmldoc.AppendChild(nodeRoot);

                xmldoc.Save(XMLPhonebookFile);
            }
            catch (System.IO.FileNotFoundException ee) { System.Console.WriteLine(ee.Message); }
            catch (System.Xml.XmlException e) { System.Console.WriteLine(e.Message); }
        }

        public void addRecord(CBuddyRecord record)
        {
            int buddyindex = -1;

            buddyindex = Messenger.addBuddy(record.Number, record.PresenceEnabled);

            // shouldn't happen!!!!
            if (buddyindex == -1) return; //System.Diagnostics.Debug.WriteLine("");

            record.Id = buddyindex;
            // add record to buddylist
            _buddyList.Add(record.Id, record);
        }


        public void deleteRecord(int id)
        {
            if (_buddyList.ContainsKey(id))
            {
                _buddyList.Remove(id);
            }
        }

        #endregion

    }

} // namespace Sipek
