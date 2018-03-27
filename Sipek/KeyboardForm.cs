using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sipek
{
  public partial class KeyboardForm : Form
  {
    private MainForm _main;

    public KeyboardForm(MainForm form)
    {
      _main = form;
      InitializeComponent();
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void keypad0_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("0");
    }

    private void keypad1_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("1");
    }

    private void keypad2_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("2");
    }

    private void keypad3_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("3");
    }

    private void keypad4_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("4");
    }

    private void keypad5_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("5");
    }

    private void keypad6_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("6");
    }

    private void keypad7_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("7");
    }

    private void keypad8_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("8");
    }

    private void keypad9_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("9");
    }

    private void keypadSTAR_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("*");
    }

    private void keypadHASH_Click(object sender, EventArgs e)
    {
      _main.onUserDialDigit("#");
    }

    private void KeyboardForm_KeyPress(object sender, KeyPressEventArgs e)
    {
      switch (e.KeyChar)
      {
        case '0': this.keypad0_Click(sender, e); break;
        case '1': this.keypad1_Click(sender, e); break;
        case '2': this.keypad2_Click(sender, e); break;
        case '3': this.keypad3_Click(sender, e); break;
        case '4': this.keypad4_Click(sender, e); break;
        case '5': this.keypad5_Click(sender, e); break;
        case '6': this.keypad6_Click(sender, e); break;
        case '7': this.keypad7_Click(sender, e); break;
        case '8': this.keypad8_Click(sender, e); break;
        case '9': this.keypad9_Click(sender, e); break;
        case '*': this.keypadSTAR_Click(sender, e); break;
        case '#': this.keypadHASH_Click(sender, e); break;
      }
    }

    private void KeyboardForm_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyValue == 0x1B)
      {
        this.Close();
      }
    }

  }
}
