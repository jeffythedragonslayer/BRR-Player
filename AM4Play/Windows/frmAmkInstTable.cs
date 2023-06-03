using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BRRPlay.Windows
{
	public partial class frmAmkInstTable : Form
	{
		public frmAmkInstTable(string text, string title)
		{
			InitializeComponent();
			Text = title;
			textBox1.Text = text;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
