using System;
using System.Windows.Forms;
using AM4Play.SNESAPU;
using System.Runtime.InteropServices;
using System.Diagnostics;
using BRRPlay;

namespace AM4Play
{
	static class Program
	{
        public struct GVoices
        {
            public int volLeft;
            public int volRight;
            public int pitch;
            public int envelope;
            public int levelLeft;
            public int levelRight;
            public bool surroundL;
            public bool surroundR;
        }

        public static GVoices[] gvoices;

		public static unsafe byte* dsp;
		public static unsafe Voice* voice;

        private static int GetVolumeLevel(int level)
        {
            return (int)Math.Max(0, Math.Min(int.MaxValue, Math.Round(37.5 * Math.Log10(level * 0.010 + 1))));
        }

		public static unsafe void GetInfo2(int chn)
		{
			int b = chn << 4;

            try
            {
                gvoices[chn].volLeft = Percent(0x80, 100, Math.Abs(unchecked((sbyte)dsp[b])));
                gvoices[chn].volRight = Percent(0x80, 100, Math.Abs(unchecked((sbyte)dsp[b+1])));
            }
            catch (OverflowException)
            {
                if (dsp[b] == 0x80 && dsp[b + 1] == 0x80)
                {
                    gvoices[chn].volLeft = 100;
                    gvoices[chn].volRight = 100;
                }
                else if (dsp[b] == 0x80)
                {
                    gvoices[chn].volLeft = 100;
                    gvoices[chn].volRight = Percent(0x80, 100, Math.Abs(((sbyte*)dsp)[b + 1]));
                }
                else
                {
                    gvoices[chn].volLeft = Percent(0x80, 100, Math.Abs(((sbyte*)dsp)[b]));
                    gvoices[chn].volRight = 100;
                }
            }

            gvoices[chn].pitch = Percent(0x4000, 100, (dsp[b + 2] | (dsp[b + 3] << 8)) & 0x3fff);
            gvoices[chn].envelope = Percent(0x80, 100, dsp[b + 8] & 127);

            gvoices[chn].levelLeft = Math.Max(GetVolumeLevel(voice[chn].vMaxL), gvoices[chn].levelLeft - 5);
            gvoices[chn].levelRight = Math.Max(GetVolumeLevel(voice[chn].vMaxR), gvoices[chn].levelRight - 5);

            gvoices[chn].surroundL = (dsp[b] & 0x80) != 0;
            gvoices[chn].surroundR = (dsp[b + 1] & 0x80) != 0;

            voice[chn].vMaxL = 0;
            voice[chn].vMaxR = 0;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			string arg0 = "NULL";
			if (args.Length == 1)
			{
				arg0 = args[0];
			}

			if (!EnsureSingleInstance(arg0))
			{
				return;
			}

            gvoices = new GVoices[8];
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form6(args));
		}

		static bool EnsureSingleInstance(string arg0)
		{
			Process currentProcess = Process.GetCurrentProcess();

			foreach (Process p in Process.GetProcesses())
			{
				if (p.Id != currentProcess.Id && p.ProcessName.Equals(
					currentProcess.ProcessName, StringComparison.Ordinal))
				{
					SendArgs(p.MainWindowHandle, arg0);
					return false;
				}
			}

			return true;
		}

		[DllImport("user32", EntryPoint = "SetForegroundWindow")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32")]
		private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

		public static bool SendArgs(IntPtr targetHWnd, string args)
		{
			Win32.CopyDataStruct cds = new Win32.CopyDataStruct();
			try
			{
				cds.cbData = (args.Length + 1) * 2;
				cds.lpData = Win32.LocalAlloc(0x40, cds.cbData);
				Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
				cds.dwData = (IntPtr)1;
				Win32.SendMessage(targetHWnd, Win32.WM_COPYDATA, IntPtr.Zero, ref cds);
			}
			finally
			{
				cds.Dispose();
			}

			return true;
		}

        /// <summary>
        /// Returns the frame rate.
        /// </summary>
        /// <returns>Frame Rate</returns>
        public static int GetRate()
        {
            return (int)Math.Ceiling(1000.0 / Options.FrameRate);
        }

		/// <summary>
		/// Convert a value from a specific scale to another scale.
		/// </summary>
		/// <param name="origBase">Scale original</param>
		/// <param name="newBase">New scale</param>
		/// <param name="value">Value to scale</param>
		/// <returns>The new value</returns>
		public static int Percent(double origBase, double newBase, double value)
		{
			return (int)Math.Ceiling(value * newBase / origBase);
		}
	}
}
