using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VilelaBot2.Util;

namespace BRRPlay.Windows
{
	public partial class frmVolumeCalc : Form
	{
		public frmVolumeCalc()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			try
			{
				int[] output = VolumeCalc.CalculateVolume(textBox1.Text);
				textBox2.Text = output[0].ToString("X2");
				textBox3.Text = output[1].ToString("X2");
				label4.Text = "OK";
			}
			catch
			{
				label4.Text = "--";
			}
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			textBox1_TextChanged(sender, e);
		}
	}
}
