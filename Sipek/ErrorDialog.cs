using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sipek
{
  public partial class ErrorDialog : Form
  {
    public ErrorDialog(string title, string text)
    {
      InitializeComponent();

      this.Text = title;
      this.textBox.Text = text;

    }
  }
}
