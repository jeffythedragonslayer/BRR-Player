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
	public partial class Form3 : Form
	{
		public Form3()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			int left, right;
			int louder = checkBox1.Checked ? 1 : 0;
			int w;

			try
			{
				left = Convert.ToByte(textBox2.Text, 16);
				textBox2.BackColor = Color.White;
			}
			catch
			{
				textBox2.BackColor = Color.Red;
				return;
			}

			try
			{
				right = Convert.ToByte(textBox3.Text, 16);
				textBox3.BackColor = Color.White;
			}
			catch
			{
				textBox3.BackColor = Color.Red;
				return;
			}

			try
			{
				w = Convert.ToByte(textBox4.Text);
				if (w < 0 || w > 255) throw new Exception();
				textBox4.BackColor = Color.White;
			}
			catch
			{
				textBox4.BackColor = Color.Red;
				return;
			}

			try
			{
				int[] array = VolumeCalc.FindVolume(new[] { left, right }, louder, w);

				string result = String.Format("q7{0:X} y{1},{4},{5} v{2:D3} $FA $03 ${3:X2}",
					array[0], array[1], array[2], array[3], array[7], array[8]);

				if (array[4] != 0)
				{
					result = result + String.Format("\r\n\r\n*** Note: This is the closest " +
						"result found\r\n    This actually gives L: {0:X2} and R: {1:X2}.",
						array[5], array[6]);
				}

				if (louder == 1)
				{
					result = "#louder " + result;
				}

				textBox1.Text = result;
			}
			catch
			{
				textBox1.Text = "*** Unknown Error Detected";
			}
		}
	}
}
