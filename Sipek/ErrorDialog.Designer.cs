namespace Sipek
{
  partial class ErrorDialog
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
      this.button1 = new System.Windows.Forms.Button();
      this.textBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      //
      // button1
      //
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.Location = new System.Drawing.Point(120, 86);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Ok";
      this.button1.UseVisualStyleBackColor = true;
      //
      // textBox
      //
      this.textBox.Location = new System.Drawing.Point(33, 15);
      this.textBox.Multiline = true;
      this.textBox.Name = "textBox";
      this.textBox.ReadOnly = true;
      this.textBox.Size = new System.Drawing.Size(264, 65);
      this.textBox.TabIndex = 2;
      this.textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      //
      // ErrorDialog
      //
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.button1;
      this.ClientSize = new System.Drawing.Size(325, 121);
      this.Controls.Add(this.textBox);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "ErrorDialog";
      this.Text = "Error Dialog";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox textBox;
  }
}
