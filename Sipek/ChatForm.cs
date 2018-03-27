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
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*
* @see http://sipekphone.googlepages.com/pjsipwrapper
* @see http://voipengine.googlepages.com/
*
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sipek.Common;


namespace Sipek
{
public partial class ChatForm : Form
{
private SipekResources _resources = null;
private SipekResources SipekResources
{
get { return _resources; }
}

private int _buddyId = -1;
public int BuddyId
{
get { return _buddyId; }
set { _buddyId = value; }
}

private string _buddyName;
public string BuddyName
{
get { return _buddyName; }
set { _buddyName = value; }
}

private string _lastMessage = "";
public string LastMessage
{
get { return _lastMessage; }
set
{
if (value.Length > 0)
{
richTextBoxChatHistory.Text += "(" + BuddyName + ") " + DateTime.Now;
richTextBoxChatHistory.Text += ": " + value;
richTextBoxChatHistory.Text += Environment.NewLine;
}
}
}

public ChatForm(SipekResources resources)
{
InitializeComponent();
_resources = resources;
}

private void ChatForm_Activated(object sender, EventArgs e)
{
}

private void buttonSendIM_Click(object sender, EventArgs e)
{
if (BuddyId == -1) return;

// get buddy data form _buddyId
CBuddyRecord buddy = CBuddyList.getInstance()[BuddyId];
if (buddy != null)
{
// Invoke SIP stack wrapper function to send message
SipekResources.Messenger.sendMessage(buddy.Number, textBoxChatInput.Text);

richTextBoxChatHistory.Text += "(me) " + DateTime.Now;

//Font orgfnt = richTextBoxChatHistory.Font;
//Font fnt = new Font("Microsoft Sans Serif",16,FontStyle.Bold);
//richTextBoxChatHistory.Font = fnt;
richTextBoxChatHistory.Text += ": " + textBoxChatInput.Text;
//richTextBoxChatHistory.Font = orgfnt;
richTextBoxChatHistory.Text += Environment.NewLine;

textBoxChatInput.Clear();

}
}

private void buttonCancel_Click(object sender, EventArgs e)
{
Close();
}

private void textBoxChatInput_KeyPress(object sender, KeyPressEventArgs e)
{
if (e.KeyChar == 0xD)
{
this.buttonSendIM_Click(sender, e);
}
}

private void ChatForm_Shown(object sender, EventArgs e)
{
// get buddy data form _buddyId
CBuddyRecord buddy = CBuddyList.getInstance()[BuddyId];
if (buddy != null)
{
tabPageChat.Text = "Chat with " + buddy.FirstName;
}
else
{
tabPageChat.Text = "Chat with " + BuddyName;
}
}

private void textBoxChatInput_KeyUp(object sender, KeyEventArgs e)
{
if (e.KeyValue == 0xD)
{
textBoxChatInput.Clear();
}
}

}
}