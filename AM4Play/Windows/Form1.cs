using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BRRPlay.Windows
{
	public partial class Form1 : Form
	{
		public Form1(string text, string title)
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
