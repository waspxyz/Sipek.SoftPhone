namespace Sipek
{
  partial class ChatForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabControlChat = new System.Windows.Forms.TabControl();
      this.tabPageChat = new System.Windows.Forms.TabPage();
      this.richTextBoxChatHistory = new System.Windows.Forms.RichTextBox();
      this.buttonSendIM = new System.Windows.Forms.Button();
      this.textBoxChatInput = new System.Windows.Forms.TextBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.tabControlChat.SuspendLayout();
      this.tabPageChat.SuspendLayout();
      this.SuspendLayout();
      //
      // tabControlChat
      //
      this.tabControlChat.Controls.Add(this.tabPageChat);
      this.tabControlChat.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControlChat.Location = new System.Drawing.Point(0, 0);
      this.tabControlChat.Name = "tabControlChat";
      this.tabControlChat.SelectedIndex = 0;
      this.tabControlChat.Size = new System.Drawing.Size(374, 286);
      this.tabControlChat.TabIndex = 5;
      this.tabControlChat.TabStop = false;
      //
      // tabPageChat
      //
      this.tabPageChat.Controls.Add(this.richTextBoxChatHistory);
      this.tabPageChat.Controls.Add(this.buttonSendIM);
      this.tabPageChat.Controls.Add(this.textBoxChatInput);
      this.tabPageChat.Controls.Add(this.buttonCancel);
      this.tabPageChat.Location = new System.Drawing.Point(4, 22);
      this.tabPageChat.Name = "tabPageChat";
      this.tabPageChat.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageChat.Size = new System.Drawing.Size(366, 260);
      this.tabPageChat.TabIndex = 0;
      this.tabPageChat.Text = "Chat";
      this.tabPageChat.UseVisualStyleBackColor = true;
      //
      // richTextBoxChatHistory
      //
      this.richTextBoxChatHistory.Dock = System.Windows.Forms.DockStyle.Top;
      this.richTextBoxChatHistory.Location = new System.Drawing.Point(3, 3);
      this.richTextBoxChatHistory.Name = "richTextBoxChatHistory";
      this.richTextBoxChatHistory.ReadOnly = true;
      this.richTextBoxChatHistory.Size = new System.Drawing.Size(360, 196);
      this.richTextBoxChatHistory.TabIndex = 3;
      this.richTextBoxChatHistory.TabStop = false;
      this.richTextBoxChatHistory.Text = "";
      //
      // buttonSendIM
      //
      this.buttonSendIM.Location = new System.Drawing.Point(310, 205);
      this.buttonSendIM.Name = "buttonSendIM";
      this.buttonSendIM.Size = new System.Drawing.Size(53, 47);
      this.buttonSendIM.TabIndex = 2;
      this.buttonSendIM.Text = "Send";
      this.buttonSendIM.UseVisualStyleBackColor = true;
      this.buttonSendIM.Click += new System.EventHandler(this.buttonSendIM_Click);
      //
      // textBoxChatInput
      //
      this.textBoxChatInput.AcceptsReturn = true;
      this.textBoxChatInput.Location = new System.Drawing.Point(3, 205);
      this.textBoxChatInput.Multiline = true;
      this.textBoxChatInput.Name = "textBoxChatInput";
      this.textBoxChatInput.Size = new System.Drawing.Size(301, 47);
      this.textBoxChatInput.TabIndex = 0;
      this.textBoxChatInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxChatInput_KeyUp);
      this.textBoxChatInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxChatInput_KeyPress);
      //
      // buttonCancel
      //
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(226, 42);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "button1";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      //
      // ChatForm
      //
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(374, 286);
      this.Controls.Add(this.tabControlChat);
      this.Name = "ChatForm";
      this.Text = "Chat Room";
      this.Shown += new System.EventHandler(this.ChatForm_Shown);
      this.tabControlChat.ResumeLayout(false);
      this.tabPageChat.ResumeLayout(false);
      this.tabPageChat.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControlChat;
    private System.Windows.Forms.TabPage tabPageChat;
    private System.Windows.Forms.TextBox textBoxChatInput;
    private System.Windows.Forms.Button buttonSendIM;
    private System.Windows.Forms.RichTextBox richTextBoxChatHistory;
    private System.Windows.Forms.Button buttonCancel;
  }
}
