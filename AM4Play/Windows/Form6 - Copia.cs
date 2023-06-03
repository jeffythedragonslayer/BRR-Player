using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AM4Play.SNESAPU;
using AM4Play.WaveLib;

namespace AM4Play
{
    public partial class Form6 : Form
    {
		static int PitchMultiplier = 3;
		static int PitchSubMultiplier = 0;
		static int PitchNote = 0;
		static int PitchDSP = 0x1000;

        #region (Un)Initializer
        public Form6(string[] args)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			for (int i = 0; i < 256; ++i)
			{
				domainUpDown1.Items.Add(i.ToString("X2"));
			}
			domainUpDown1.Text = "03";

            IsPlaying = false;
            IsPaused = false;

            // 1 12 18 24 30 36 42 48
            // 2 11 17 23 29 35 41 47
            // 3 10 16 22 28 34 40 46
            // 4  9 15 21 27 33 39 45
            // 5  8 14 20 26 32 38 44
            // 6  7 13 19 25 31 37 43

            leftVolume = new VerticalProgressBar[8]  { verticalProgressBar1, verticalProgressBar12, verticalProgressBar18, verticalProgressBar24, verticalProgressBar30, verticalProgressBar36, verticalProgressBar42, verticalProgressBar48 };
            rightVolume = new VerticalProgressBar[8] { verticalProgressBar2, verticalProgressBar11, verticalProgressBar17, verticalProgressBar23, verticalProgressBar29, verticalProgressBar35, verticalProgressBar41, verticalProgressBar47 };
            pitch = new VerticalProgressBar[8]       { verticalProgressBar3, verticalProgressBar10, verticalProgressBar16, verticalProgressBar22, verticalProgressBar28, verticalProgressBar34, verticalProgressBar40, verticalProgressBar46 };
            envelope = new VerticalProgressBar[8]    { verticalProgressBar4, verticalProgressBar9 , verticalProgressBar15, verticalProgressBar21, verticalProgressBar27, verticalProgressBar33, verticalProgressBar39, verticalProgressBar45 };
            leftLevel = new VerticalProgressBar[8]   { verticalProgressBar5, verticalProgressBar8 , verticalProgressBar14, verticalProgressBar20, verticalProgressBar26, verticalProgressBar32, verticalProgressBar38, verticalProgressBar44 };
            rightLevel = new VerticalProgressBar[8]  { verticalProgressBar6, verticalProgressBar7 , verticalProgressBar13, verticalProgressBar19, verticalProgressBar25, verticalProgressBar31, verticalProgressBar37, verticalProgressBar43 };
            dspFlags = new AM4Player.DSPFrags[8] { dspFrags1, dspFrags2, dspFrags3, dspFrags4, dspFrags5, dspFrags6, dspFrags7, dspFrags8 };

            UpdateExternalOrNot();
            ReAssignAudioOptions();

            playToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            restartToolStripMenuItem.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            channelMuteToolStripMenuItem.Enabled = false;

            bool success = false;

        loop1:
            success = false;
            foreach (ToolStripMenuItem item in volumeToolStripMenuItem.DropDownItems)
            {
                if ((0x10000 * Convert.ToDouble((string)item.Tag) / 100) == Options.Volume)
                {
                    item.Checked = true;
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                Options.Volume = 0x10000;
                goto loop1;
            }

        loop2:
            success = false;
            foreach (ToolStripMenuItem item in interpolationToolStripMenuItem.DropDownItems)
            {
                if (((Interpolation)Convert.ToInt32(item.Tag)) == Options.INTERPOLATION)
                {
                    item.Checked = true;
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                Options.INTERPOLATION = Interpolation.INT_GAUSS;
                goto loop2;
            }

            // Filters
            if ((Options.OPTIONS & DSPOpts.DSP_ANALOG) == DSPOpts.DSP_ANALOG)
            {
                simulateAnalogAnomaliesToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_BASS) == DSPOpts.DSP_BASS)
            {
                bassBoostToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_SURND) == DSPOpts.DSP_SURND)
            {
                simulateSurroundSoundToolStripMenuItem.Checked = true;
            }

            // Advanced
            if ((Options.OPTIONS & DSPOpts.DSP_NOECHO) == DSPOpts.DSP_NOECHO)
            {
                disableEchoToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_NOFIR) == DSPOpts.DSP_NOFIR)
            {
                disableFIRFilterToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_NOPMOD) == DSPOpts.DSP_NOPMOD)
            {
                disablePitchModulationToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_NONOISE) == DSPOpts.DSP_NONOISE)
            {
                disableNoiseToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_ECHOMEM) == DSPOpts.DSP_ECHOMEM)
            {
                rewriteEchoToARAMToolStripMenuItem.Checked = true;
            }
            if ((Options.OPTIONS & DSPOpts.DSP_NOSURND) == DSPOpts.DSP_NOSURND)
            {
                disableSurroundakaNegativePansToolStripMenuItem.Checked = true;
            }

            timer1.Interval = Program.GetRate();

            if (args.Length == 1)
            {
                Open(args[0]);
                Play();

                if (Options.ForceExternalProgram)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void ReAssignAudioOptions()
        {
            bool loaded = false;

        loop:
            // load sample rate
            foreach (ToolStripMenuItem item in sampleRateToolStripMenuItem.DropDownItems)
            {
                if (Convert.ToInt32((string)item.Tag) == Options.RATE)
                {
                    item.Checked = true;
                    loaded = true;
                    break;
                }
            }

            if (!loaded)
            {
                Options.RATE = 32000;
                goto loop;
            }

        loop1:
            loaded = false;
            // load bits
            foreach (ToolStripMenuItem item in bitsToolStripMenuItem.DropDownItems)
            {
                if (Convert.ToInt32((string)item.Tag) == Options.BITS)
                {
                    item.Checked = true;
                    loaded = true;
                    break;
                }
            }

            if (!loaded)
            {
                Options.BITS = 16;
                goto loop1;
            }

        loop2:
            loaded = false;
            // channels
            foreach (ToolStripMenuItem item in channelsToolStripMenuItem.DropDownItems)
            {
                if (Convert.ToInt32((string)item.Tag) == Options.CHANNELS)
                {
                    item.Checked = true;
                    loaded = true;
                    break;
                }
            }

            if (!loaded)
            {
                Options.CHANNELS = 2;
                goto loop2;
            }
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();

            if (player != null)
            {
                player.Dispose();
                player = null;

                IsPaused = IsPlaying = false;
            }
        }
        #endregion

        #region Draw Gradients
        private void drawFormGradient(object sender, PaintEventArgs e)
        {
            if (DisplayRectangle.Width == 0 || DisplayRectangle.Height == 0)
            {
                return;
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(
				DisplayRectangle, Color.Orchid, Color.MediumPurple, 90))
            {
                e.Graphics.FillRectangle(brush, DisplayRectangle);
            }
        }

        private void drawMenuGradient(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
				menuStrip1.ClientRectangle, Color.LightPink, Color.Violet, 90))
            {
                e.Graphics.FillRectangle(brush, menuStrip1.ClientRectangle);
            }
        }
        #endregion

        #region Open/Dump/Quit
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (!IsPlaying)
                    {
                        Open(openFileDialog1.FileName);
                    }
                    else
                    {
                        Open(openFileDialog1.FileName);
                        ResetSPC();
                    }

					Play();
                    SetPlayPauseString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BRR Player - Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Play/Pause/Stop/Restart
        string[] playTable = new string[2] { "Play", "Pause" };

        private void SetPlayPauseString()
        {
            playToolStripMenuItem.Text = button1.Text = playTable[IsPaused ? 0 : 1];
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsPaused)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "BRR Player - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            SetPlayPauseString();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsPlaying)
                {
                    Stop();
                }
                SetPlayPauseString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BRR Player - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Restart();
                SetPlayPauseString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BRR Player - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Help - About AM4 Player
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string about = @"BRR Player Version 1.0 (July 2014)
by Vitor Vilela (SMWC User ID: 8251)
Thanks Alpha-II and degrade-factory for SnesAPU Emulator
Thanks also the authors of NAudio.

SNES APU - ©2003-04 Alpha-II Productions, 2001-2012 degrade-factory";

            MessageBox.Show(about, "About BRR Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Options
        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (!IsPlaying) return;

            // this is to bypass the limited shortcuts.
            switch (e.KeyCode)
            {
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                    int num = e.KeyCode - Keys.D1;
                    ToolStripMenuItem target = (channelMuteToolStripMenuItem.DropDownItems[num & 7] as ToolStripMenuItem);
                    channelMuteToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(target));
                    target.Checked = !target.Checked;
                    break;
            }
        }

        private void bitsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            uint bits = (uint)Convert.ToInt32((string)e.ClickedItem.Tag);

            if (bits >= 8 && (bits & 7) == 0)
            {
                if (bits != Options.BITS)
                {
                    Options.BITS = bits;
                    ResetSPCOptions();
                    RestartWaveOut();
                }
            }
        }

        private void channelsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            uint channels = Convert.ToUInt32((string)e.ClickedItem.Tag);

            if (channels != 0)
            {
                if (channels != Options.CHANNELS)
                {
                    Options.CHANNELS = channels;
                    ResetSPCOptions();
                    RestartWaveOut();
                }
            }
        }

        private void sampleRateToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            uint rate = Convert.ToUInt32((string)e.ClickedItem.Tag);

            if (rate >= 8000)
            {
                if (rate != Options.RATE)
                {
                    Options.RATE = rate;
                    ResetSPCOptions();
                    RestartWaveOut();
                }
            }
        }

        private void interpolationToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Interpolation inter = (Interpolation)Convert.ToInt32(e.ClickedItem.Tag);
            if (inter != Options.INTERPOLATION)
            {
                Options.INTERPOLATION = inter;
                ResetSPCOptions();
            }
        }

        private void bassBoostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(bassBoostToolStripMenuItem.Checked, DSPOpts.DSP_BASS);
        }

        private void simulateAnalogAnomaliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(simulateAnalogAnomaliesToolStripMenuItem.Checked, DSPOpts.DSP_ANALOG);
        }

        private void simulateSurroundSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(simulateSurroundSoundToolStripMenuItem.Checked, DSPOpts.DSP_SURND);
        }

        private void volumeToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            double scale = Convert.ToDouble((string)e.ClickedItem.Tag);
            scale = (double)0x10000 * scale / 100.0;
            Options.Volume = (uint)scale;
            ResetSPCOptions();
        }

        private void channelMuteToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!IsPlaying)
            {
                (e.ClickedItem as ToolStripMenuItem).Checked = !(e.ClickedItem as ToolStripMenuItem).Checked;
                return;
            }

            int channel = Convert.ToInt32((string)e.ClickedItem.Tag);
            bool enable = !(e.ClickedItem as ToolStripMenuItem).Checked;

            // for some non-sense reason, e.clickeditem.checked shows the
            // PREVIOUS state and will toggle after sometime. whatever, I XOR'd
            // the value.

            SetChannelMute(channel, enable);
            leftVolume[channel].NotGrayed = !enable;
            rightVolume[channel].NotGrayed = !enable;
            envelope[channel].NotGrayed = !enable;
            pitch[channel].NotGrayed = !enable;
            leftLevel[channel].NotGrayed = !enable;
            rightLevel[channel].NotGrayed = !enable;
        }

        private void disableSurroundakaNegativePansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(disableSurroundakaNegativePansToolStripMenuItem.Checked, DSPOpts.DSP_NOSURND);
        }

        private void disableEchoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(disableEchoToolStripMenuItem.Checked, DSPOpts.DSP_NOECHO);
        }

        private void disableFIRFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(disableFIRFilterToolStripMenuItem.Checked, DSPOpts.DSP_NOFIR);
        }

        private void disablePitchModulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(disablePitchModulationToolStripMenuItem.Checked, DSPOpts.DSP_NOPMOD);
        }

        private void disableNoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(disableNoiseToolStripMenuItem.Checked, DSPOpts.DSP_NONOISE);
        }

        private void rewriteEchoToARAMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDSPOpts(rewriteEchoToARAMToolStripMenuItem.Checked, DSPOpts.DSP_ECHOMEM);
        }

        private void forceExternalSPCPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options.ForceExternalProgram = ((ToolStripMenuItem)sender).Checked;
            UpdateExternalOrNot();
        }

        private void minuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options.MaximiumMinutes = 1;
        }

        private void minutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options.MaximiumMinutes = 3;
        }

        private void minutesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Options.MaximiumMinutes = 5;
        }

        private void minutesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Options.MaximiumMinutes = 7;
        }

        private void minutesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Options.MaximiumMinutes = 10;
        }

        private void frameRateToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int value = Int32.Parse((string)e.ClickedItem.Tag);

            if (value == 69)
            {
                Options.FrameRate = 7.5f;
            }
            else
            {
                Options.FrameRate = value;
            }

            timer1.Interval = Program.GetRate();
        }
        #endregion

        #region Util Functions
        private void SetTitle(string engine, string data, bool isFile = true)
        {
            if (isFile)
            {
                data = Path.GetFileNameWithoutExtension(data);
            }

            this.Text = string.Format("{0} - BRR Player", data);
        }
        #endregion

        #region Graphical Stuff

        // 1 12 18 24 30 36 42 48
        // 2 11 17 23 29 35 41 47
        // 3 10 16 22 28 34 40 46
        // 4  9 15 21 27 33 39 45
        // 5  8 14 20 26 32 38 44
        // 6  7 13 19 25 31 37 43
        VerticalProgressBar[] leftVolume;
        VerticalProgressBar[] rightVolume;
        VerticalProgressBar[] pitch;
        VerticalProgressBar[] envelope;
        VerticalProgressBar[] leftLevel;
        VerticalProgressBar[] rightLevel;
        AM4Player.DSPFrags[] dspFlags;

        private unsafe void timer1_Tick(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized) { return; }
            if (Program.dsp == null) { return; }

            for (int i = 0; i < 8; ++i)
            {
                leftVolume[i].Value = Program.gvoices[i].volLeft;
                rightVolume[i].Value = Program.gvoices[i].volRight;
                pitch[i].Value = Program.gvoices[i].pitch;
                envelope[i].Value = Program.gvoices[i].envelope;
                leftLevel[i].Value = Program.gvoices[i].levelLeft;
                rightLevel[i].Value = Program.gvoices[i].levelRight;
                leftVolume[i].InvertColors = Program.gvoices[i].surroundL;
                rightVolume[i].InvertColors = Program.gvoices[i].surroundR;

                dspFlags[i].Echo = (Program.dsp[0x4D] >> i & 1) != 0;
                dspFlags[i].PMON = (Program.dsp[0x2D] >> i & 1) != 0;
                dspFlags[i].Noise = (Program.dsp[0x3D] >> i & 1) != 0;
            }
        }
        #endregion

        #region Drap/Drop Stuff
        private void Form6_DragDrop(object sender, DragEventArgs e)
        {
            string[] f = e.Data.GetFormats();

            for (int x = 0, y = f.Length; x < y; x++)
            {
                if (String.Equals(f[x], "FileName"))
                {
                    try
                    {
                        object data = e.Data.GetData(f[x]);
                        string file = ((string[])data)[0];

                        if (!IsPlaying)
                        {
                            Open(file);
                            Play();
                        }
                        else
                        {
                            Open(file);
                            ResetSPC();
                        }

                        SetPlayPauseString();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };

            e.Effect = DragDropEffects.None;
            return;
        }

        private void Form6_DragEnter(object sender, DragEventArgs e)
        {
            string[] f = e.Data.GetFormats();

            for (int x = 0, y = f.Length; x < y; ++x)
            {
                if (f[x] == "FileName")
                {
                    e.Effect = DragDropEffects.All;
                    return;
                }
            }

            e.Effect = DragDropEffects.None;
        }
        #endregion

        #region Direct Functions and Fields
        private bool IsPlaying;
        private bool IsPaused;
        private byte[] currentSPC;
        private string currentFile;
        private WavePlayer player;

        private unsafe void SetChannelMute(int channel, bool mute)
        {
            Program.voice[channel].mFlg &= ~MixFlags.MFLG_MUTE;

            if (mute)
            {
                Program.voice[channel].mFlg |= MixFlags.MFLG_MUTE;
            }
        }

        private void SetDSPOpts(bool value, DSPOpts item)
        {
            DSPOpts options = Options.OPTIONS;
            if (value)
            {
                options |= item;
            }
            else
            {
                options &= ~item;
            }
            Options.OPTIONS = options;
            ResetSPCOptions();
        }

        private void UpdateExternalOrNot()
        {
            bool enable = !Options.ForceExternalProgram;

            // player
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
                IsPlaying = false;
                IsPaused = false;
            }

            // play, stop
            playToolStripMenuItem.Enabled = enable;
            stopToolStripMenuItem.Enabled = enable;
            button1.Enabled = enable;
            button2.Enabled = enable;

            // options->sample rate, interpolation, filters, volume, channel mute, advanced
            // bit, channel
            sampleRateToolStripMenuItem.Enabled = enable;
            bitToolStripMenuItem.Enabled = enable;
            channelToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in sampleRateToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            foreach (ToolStripItem item in bitToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            foreach (ToolStripItem item in channelToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            interpolationToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in interpolationToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            filtersToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in filtersToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            volumeToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in volumeToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            channelMuteToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in channelMuteToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }

            advancedToolStripMenuItem.Enabled = enable;

            foreach (ToolStripItem item in advancedToolStripMenuItem.DropDownItems)
            {
                item.Enabled = enable;
            }
        }
		#endregion

		#region BRR Player Stuff
		class ParamPattern
		{
			public string fileNamePattern;
			public string[] parametersPatterns;
		}

		List<ParamPattern> paramPatterns = new List<ParamPattern>();

		delegate void FuncCH(char c);

		private string capture(ref string str, ref int index, params char[] stop)
		{
			int startIndex = index;
			int endIndex = index;
			while (endIndex < str.Length)
			{
				foreach (char e in stop)
				{
					if (e == str[endIndex])
					{
						goto boom;
					}
				}
				++endIndex;
			}
		boom:
			index = endIndex;
			return str.Substring(startIndex, endIndex - startIndex);
		}

		private void LoadPatterns(string folder)
		{
			this.paramPatterns.Clear();

			string paramPatterns = folder + "/!patterns.txt";
			string notePatterns = folder + "/!notes.txt";

			if (File.Exists(paramPatterns))
			{
				LoadParameterPatterns(GetLines(RemoveComments(FixLines(File.ReadAllText(paramPatterns)))));
			}
			if (File.Exists(notePatterns))
			{
				LoadNotePatterns(GetLines(RemoveComments(FixLines(File.ReadAllText(notePatterns)))));
			}
		}

		private void LoadParameterPatterns(string[] contents)
		{
			// format:
			// "Piano.brr" $BF $D1 $C0 $02 $00
			// *"Piano \$[0-9A-F]{2}\.brr" $BF $D1 $C0 ${1} $00

			int lines = 1;

			foreach (string ln in contents)
			{
				string line = ln.Trim();
				string[] parameters = new string[8];
				string file;

				bool regex = false;
				ParamPattern pattern = new ParamPattern();

				int pos = 0;

				if (line.StartsWith("*"))
				{
					regex = true;
					pos = 1;
				}

				var limit = new ThreadStart(delegate()
				{
					if (line.Length == pos)
					{
						throw new Exception(String.Format("Unexpected end of line at line {0}.", lines));
					}
				});

				var skipWhiteSpace = new ThreadStart(delegate()
				{
					limit();
					while (Char.IsWhiteSpace(line[pos])) { pos++; limit(); }
				});

				var must = new FuncCH(delegate(char c)
				{
					if (line[pos] != c)
					{
						throw new Exception(String.Format("Expected '{0}' at line {1}:{2}.", c, lines, pos));
					}
					pos++;
				});

				skipWhiteSpace();
				must('"');
				file = capture(ref line, ref pos, '"');
				must('"');

				if (String.IsNullOrEmpty(file))
				{
					throw new Exception(String.Format("Empty file at line {0}.", lines));
				}

				for (int i = 0; i < parameters.Length; ++i)
				{
					skipWhiteSpace();
					must('$');
					parameters[i] = capture(ref line, ref pos, ' ', '\t');
				}

				lines++;

				if (!regex)
				{
					for (int i = 0; i < parameters.Length; ++i)
					{
						parameters[i] = Regex.Escape(parameters[i]);
					}
					file = Regex.Escape(file);
				}

				pattern.fileNamePattern = file;
				pattern.parametersPatterns = parameters;
				paramPatterns.Add(pattern);
			}
		}

		private void LoadNotePatterns(string[] contents)
		{

		}

		string[] GetLines(string data)
		{
			return data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		}

		string RemoveComments(string data)
		{
			string[] tmp = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < tmp.Length; ++i)
			{
				if (tmp[i].Contains(";"))
				{
					tmp[i] = tmp[i].Substring(0, tmp[i].IndexOf(";"));
				}
			}
			return String.Join("\r\n", tmp);
		}

		string FixLines(string data)
		{
			data = data.Replace("\r\n", "\n");
			data = data.Replace("\r", "\n");
			data = data.Replace("\n", "\r\n");
			return data;
		}

		static int[] YI_pitchtable = new int[] {
                0x085f, 0x08de, 0x0965, 0x09f4,
                0x0a8c, 0x0b2c, 0x0bd6, 0x0c8b,
                0x0d4a, 0x0e14, 0x0eea, 0x0fcd,
                0x10be,
        };

		static int YI_GetPitchOfNote(int note, int tuning, int multiplier, int unknown)
		{
			//; set DSP pitch from $10/1
			//0566: 95 61 03  adc   a,$0361+x
			//0569: 3f 1d 0b  call  $0b1d
			//056c: 3f 35 0b  call  $0b35
			//056f: 8d 00     mov   y,#$00
			//0571: e4 11     mov   a,$11
			//0573: 80        setc
			//0574: a8 34     sbc   a,#$34
			//0576: b0 09     bcs   $0581
			//0578: e4 11     mov   a,$11
			//057a: 80        setc
			//057b: a8 13     sbc   a,#$13
			//057d: b0 06     bcs   $0585
			//057f: dc        dec   y
			//0580: 1c        asl   a
			//0581: 7a 10     addw  ya,$10
			//0583: da 10     movw  $10,ya
			//0585: 4d        push  x
			//0586: e4 11     mov   a,$11

			if (note - 0x34 >= 0)
			{
				int i = note - 0x34 & 255;
				i += tuning + note * 256;

				tuning = i & 255;
				note = i >> 8 & 255;
			}
			else if (note - 0x13 < 0)
			{
				int i = (note - 0x13) * 2 & 255;
				i += -256 + tuning + note * 256;

				tuning = i & 255;
				note = i >> 8 & 255;
			}

			byte x15, x14;
			byte sp;

			byte a, x, y;
			a = (byte)note;
			a <<= 1;
			y = 0x00;
			x = 0x18;

			y = (byte)(a % x);
			a = (byte)(a / x);

			x = a;

			a = (byte)(YI_pitchtable[y / 2] >> 8);
			x15 = a;
			a = (byte)YI_pitchtable[y / 2];
			x14 = a;
			a = (byte)(YI_pitchtable[y / 2 + 1] >> 8);
			sp = a;
			a = (byte)YI_pitchtable[y / 2 + 1];
			y = sp;
			{
				ushort ya = (ushort)(y << 8 | a);
				ya -= x14;
				ya -= (ushort)(x15 << 8);
				y = (byte)(ya >> 8);
				a = (byte)ya;
			}
			y = (byte)tuning;
			{
				ushort ya = (ushort)(y * a);
				y = (byte)(ya >> 8);
				a = (byte)ya;
			}
			a = y;
			y = 0x00;
			{
				ushort ya = (ushort)(y << 8 | a);
				ya += x14;
				ya += (ushort)(x15 << 8);
				y = (byte)(ya >> 8);
				a = (byte)ya;
			}
			x15 = y;
			x15 = (byte)((x15 << 1) + ((a & 0x80) >> 7));
			a <<= 1;
			x14 = a;

			int pitch1 = x14 | (x15 << 8);

			while (x != 6)
			{
				pitch1 >>= 1;
				++x;
			}


			//0588: 1c        asl   a
			//0589: 8d 00     mov   y,#$00
			//058b: cd 18     mov   x,#$18
			//058d: 9e        div   ya,x
			//058e: 5d        mov   x,a
			//058f: f6 33 0e  mov   a,$0e33+y
			//0592: c4 15     mov   $15,a
			//0594: f6 32 0e  mov   a,$0e32+y
			//0597: c4 14     mov   $14,a             ; set $14/5 from pitch table
			//0599: f6 35 0e  mov   a,$0e35+y
			//059c: 2d        push  a
			//059d: f6 34 0e  mov   a,$0e34+y
			//05a0: ee        pop   y
			//05a1: 9a 14     subw  ya,$14
			//05a3: eb 10     mov   y,$10
			//05a5: cf        mul   ya

			//05a6: dd        mov   a,y
			//05a7: 8d 00     mov   y,#$00

			//05a9: 7a 14     addw  ya,$14
			//05ab: cb 15     mov   $15,y
			//05ad: 1c        asl   a
			//05ae: 2b 15     rol   $15
			//05b0: c4 14     mov   $14,a

			//note &= 127;

			//int pitch = NotePitch(note);
			//pitch += (NotePitch(note + 1) - pitch) * tuning >> 8;

			//int pitch1 = YI_pitchtable[subNote];
			//pitch1 += (YI_pitchtable[subNote + 1] - pitch1) * tuning >> 8;
			//pitch1 <<= 1; // * 2

			//pitch1 >>= 6 - note / 12;


			//note &= 255;
			//int pitch1 = NotePitch2(note);
			//pitch1 += (NotePitch2(note + 1) - pitch1) * tuning >> 8;
			//pitch1 <<= 1;


			//05b2: 2f 04     bra   $05b8
			//05b4: 4b 15     lsr   $15
			//05b6: 7c        ror   a
			//05b7: 3d        inc   x
			//05b8: c8 06     cmp   x,#$06
			//05ba: d0 f8     bne   $05b4
			//05bc: c4 14     mov   $14,a

			int strange_thing = unknown * (pitch1 >> 8); // $16
			int moreee = (unknown * (pitch1 & 255)) >> 8;

			int value = multiplier * (pitch1 & 255);

			strange_thing += value;

			int oloco = ((multiplier * (pitch1 >> 8)) & 255) << 8 | (moreee & 255);
			oloco += strange_thing;
			strange_thing = oloco;

			return oloco;

			//05be: ce        pop   x
			//05bf: f5 20 02  mov   a,$0220+x
			//05c2: eb 15     mov   y,$15
			//05c4: cf        mul   ya
			//05c5: da 16     movw  $16,ya

			//05c7: f5 20 02  mov   a,$0220+x
			//05ca: eb 14     mov   y,$14
			//05cc: cf        mul   ya
			//05cd: 6d        push  y

			//05ce: f5 21 02  mov   a,$0221+x
			//05d1: eb 14     mov   y,$14
			//05d3: cf        mul   ya
			//05d4: 7a 16     addw  ya,$16

			//05d6: da 16     movw  $16,ya
			//05d8: f5 21 02  mov   a,$0221+x
			//05db: eb 15     mov   y,$15
			//05dd: cf        mul   ya
			//05de: fd        mov   y,a
			//05df: ae        pop   a
			//05e0: 7a 16     addw  ya,$16
			//05e2: da 16     movw  $16,ya
			//05e4: 7d        mov   a,x               ; set voice X pitch DSP reg from $16/7
			//05e5: 9f        xcn   a                 ;  (if vbit clear in $1a)
			//05e6: 5c        lsr   a
			//05e7: 08 02     or    a,#$02
			//05e9: fd        mov   y,a               ; Y = voice X pitch DSP reg
			//05ea: e4 16     mov   a,$16
			//05ec: 3f f2 05  call  $05f2
			//05ef: fc        inc   y
			//05f0: e4 17     mov   a,$17
			//; write A to DSP reg Y if vbit clear in $1a
			//05f2: 2d        push  a
			//05f3: e4 47     mov   a,$47
			//05f5: 24 1a     and   a,$1a
			//05f7: ae        pop   a
			//05f8: d0 06     bne   $0600
			//; write A to DSP reg Y
			//05fa: cc f2 00  mov   $00f2,y
			//05fd: c5 f3 00  mov   $00f3,a
			//0600: 6f        ret
		}

		static void UpdatePitch()
		{
			if (PitchNote >= 0x80)
			{
				PitchDSP = YI_GetPitchOfNote(PitchNote - 0x80, 0, PitchMultiplier, PitchSubMultiplier);
			}
			else
			{
				PitchDSP = (PitchMultiplier << 8) | PitchSubMultiplier;
			}
		}

        private void LoadBRR(string fileName)
        {
			string folder = Path.GetDirectoryName(Path.GetFullPath(fileName));
			LoadPatterns(folder);

            byte[] brr = File.ReadAllBytes(fileName);
            int loop = 0x0000;

            loop:
            if (IsValidBRR(brr))
            {
                if ((brr.Length + 0x400) > 65536)
                {
                    throw new Exception("Sorry, but this BRR is too large to be played with BRR Player.");
                }

                byte[] spc = new byte[] {
                    0x53, 0x4E, 0x45, 0x53, 0x2D, 0x53, 0x50, 0x43, 0x37, 0x30, 0x30,
                    0x20, 0x53, 0x6F, 0x75, 0x6E, 0x64, 0x20, 0x46, 0x69, 0x6C, 0x65,
                    0x20, 0x44, 0x61, 0x74, 0x61, 0x20, 0x76, 0x30, 0x2E, 0x33, 0x30
                };

                Array.Resize<byte>(ref spc, 66048);
                brr.CopyTo(spc, 0x500); // copy to ARAM 0x400

                loop += 0x400;

				// load pattern
				string file = Path.GetFileName(Path.GetFullPath(fileName));
				Match pattern = null;
				string[] parameters = null;
				byte[] hexParams = null;

				foreach (ParamPattern p in paramPatterns)
				{
					var result = Regex.Match(file, p.fileNamePattern, RegexOptions.Singleline |
						RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

					if (result.Success)
					{
						pattern = result;
						parameters = new string[p.parametersPatterns.Length];
						hexParams = new byte[parameters.Length];

						for (int i = 0; i < parameters.Length; ++i)
						{
							parameters[i] = result.Result(p.parametersPatterns[i]);
							try
							{
								hexParams[i] = Convert.ToByte(parameters[i], 16);
							}
							catch
							{
								throw new Exception(String.Format("Pattern Error: Cannot convert '{0}'"+
									" into a 8-bit hexadecimal number.", parameters[i]));
							}
						}
						break;
					}
				}

				if (hexParams == null)
				{
					hexParams = new byte[8];
					hexParams[0] = 0;
					hexParams[1] = 0;
					hexParams[2] = 0x7F;
					hexParams[3] = 4;
					hexParams[4] = 0;
					hexParams[5] = 0;
					hexParams[6] = 0x20;
					hexParams[7] = 0x20;
				}

                // Reset Vector
                spc[0x25] = 0x00; // Jump to 0x200
                spc[0x26] = 0x02;

                // ARAM: 0x200 - small program
                spc[0x300] = 0x2F; // BRA $FE... he he
                spc[0x301] = 0xFE;

                // ARAM: 0x300 - SRC DIR
                spc[0x400] = 0x00;
                spc[0x401] = 0x04;
                spc[0x402] = (byte)(loop & 255);
                spc[0x403] = (byte)(loop >> 8 & 255);

                // S-DSP: 0x00
                spc[0x10100] = hexParams[6]; // Volume
                spc[0x10101] = hexParams[7];
				fusionTrackBar1.Value = hexParams[6];
				fusionTrackBar2.Value = hexParams[7];
                
				// Pitch processing
				PitchMultiplier = hexParams[3];
				PitchSubMultiplier = hexParams[4];
				PitchNote = hexParams[5];

				UpdatePitch();

				spc[0x10102] = (byte)(PitchDSP & 255);
				spc[0x10103] = (byte)(PitchDSP >> 8);
				fusionTrackBar6.Value = PitchDSP;

                spc[0x10104] = 0x00; // SRCN

                spc[0x10105] = hexParams[0]; // ADSR 1
                spc[0x10106] = hexParams[1]; // ADSR 2
				spc[0x10107] = hexParams[2]; // GAIN
				fusionTrackBar3.Value = hexParams[0];
				fusionTrackBar4.Value = hexParams[1];
				fusionTrackBar5.Value = hexParams[2];

                spc[0x1010C] = 0x7F; // MASTER VOLUME
                spc[0x1011C] = 0x7F;

                spc[0x1015D] = 0x03; // SAMPLE DIR 0X400

                spc[0x1014C] = 0x01; // Key On Voice 0

				UpdateTrackers();

                SetTitle("BRR Engine", fileName, true);
                currentSPC = spc;
                currentFile = fileName;
            }
            else if ((brr.Length - 2) % 9 == 0 && brr.Length != 2)
            {
                loop = brr[0] | (brr[1] << 8);
                Array.Copy(brr, 2, brr, 0, brr.Length - 2);
                Array.Resize<byte>(ref brr, brr.Length - 2);
                goto loop;
            }
            else
            {
                throw new Exception("Invalid BRR file!");
            }
        }

        private bool IsValidBRR(byte[] brr)
        {
			if (brr.Length == 0) return false;
            if (brr.Length % 9 != 0) return false;
            if ((brr[brr.Length - 9] & 1) == 0) return false;
            return true;
        }

        private bool IsValidSPC(byte[] spc)
        {
            if (spc.Length < 66048) return false;
            byte[] header = new byte[] {
                0x53, 0x4E, 0x45, 0x53, 0x2D, 0x53, 0x50, 0x43, 0x37, 0x30, 0x30,
                0x20, 0x53, 0x6F, 0x75, 0x6E, 0x64, 0x20, 0x46, 0x69, 0x6C, 0x65,
                0x20, 0x44, 0x61, 0x74, 0x61, 0x20, 0x76, 0x30, 0x2E, 0x33, 0x30
            };

            for (int i = 0; i < header.Length; ++i)
            {
                if (spc[i] != header[i]) return false;
            }

            return true;
        }
		#endregion

		#region Kernel
		private void Open(string fileName)
        {
            LoadBRR(fileName);
        }

        private unsafe void ResetSPCOptions()
        {
            APU.SetAPUOpt(1, Options.CHANNELS, Options.BITS, Options.RATE, Options.INTERPOLATION, (uint)Options.OPTIONS);
            DSP.SetDSPAmp(Options.Volume);
        }

        private unsafe void ResetSPC()
        {
            if (currentSPC != null)
            {
                fixed (byte* ptr = currentSPC)
                {
                    APU.LoadSPCFile(ptr);
                }

                APU.GetPointers();
                ResetSPCOptions();
                Program.dsp = APU.dsp;
                Program.voice = APU.voice;
            }
        }

        private unsafe void RestartWaveOut()
        {
            if (player != null && IsPlaying)
            {
                try
                {
                    player.Stop();
                    Thread.Sleep(0);
                    player.Dispose();
                    player = null;
                    player = new WavePlayer((int)Options.RATE, (int)Options.BITS, (int)Options.CHANNELS);
                    if (!IsPaused) player.Play();
                }
                catch(Exception e)
                {
                    MessageBox.Show(string.Format("{0}\nMake sure if you doesn't " + 
                        "have selected a unsupported Sample Rate, Bit or Channel. " + 
                        "Not All PCs can handle some specific Sample Rate/Bit/Channel. " + 
                        "Anyway, to avoid this error again, Sample Rate, Bit and Channel " +
                        "has been changed to 32000, 16 and 2.", e.Message), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    Options.RATE = 32000;
                    Options.BITS = 16;
                    Options.CHANNELS = 2;
                    ReAssignAudioOptions();
                }
            }
        }

        private unsafe void Play(bool force=false)
        {
            if (!IsPaused||force)
            {
                if (currentSPC != null && !IsPlaying && player == null)
                {
                    if (!Options.ForceExternalProgram)
                    {
                        try
                        {
                            ResetSPC();

                            player = new WavePlayer((int)Options.RATE, (int)Options.BITS, (int)Options.CHANNELS);
                            player.Play();

                            IsPlaying = true;
                            IsPaused = false;

                            playToolStripMenuItem.Enabled = true;
                            stopToolStripMenuItem.Enabled = true;
                            restartToolStripMenuItem.Enabled = true;
                            button1.Enabled = true;
                            button2.Enabled = true;
                            button3.Enabled = true;
                            channelMuteToolStripMenuItem.Enabled = true;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(string.Format("{0}\nMake sure if you doesn't " +
                                "have selected a unsupported Sample Rate, Bit or Channel. " +
                                "Not All PCs can handle some specific Sample Rate/Bit/Channel. " +
                                "Anyway, to avoid this error again, Sample Rate, Bit and Channel " +
                                "has been changed to 32000, 16 and 2.", e.Message), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            Options.RATE = 32000;
                            Options.BITS = 16;
                            Options.CHANNELS = 2;
                            ReAssignAudioOptions();
                        }
                    }
                    else
                    {
                        try
                        {
                            string TMP = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                            File.WriteAllBytes(TMP + "\\Temp\\music.spc", currentSPC);

                            ProcessStartInfo info = new ProcessStartInfo(TMP + "\\Temp\\music.spc");
                            using (Process process = new Process())
                            {
                                process.StartInfo = info;
                                process.Start();
                            }

                            restartToolStripMenuItem.Enabled = true;
                            button3.Enabled = true;
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                IsPaused = false;

                if (player != null)
                {
                    player.Play();
                }
            }
        }

        private void Pause()
        {
            if (player != null)
            {
                player.Pause();
                IsPaused = true;
            }
        }

        private void Stop()
        {
            if (player != null)
            {
                player.Stop();
                ResetSPC();
                IsPaused = true;
            }
        }

        private void Restart()
        {
            Open(currentFile);
            if (!Options.ForceExternalProgram)
            {
                ResetSPC();
            }

            Play();
        }
        #endregion

		private void fusionTrackBar1_Scroll(object sender, EventArgs e)
		{
			// left volume
			DSP.SetDSPReg(0x00, (byte)fusionTrackBar1.Value);
			fastLabel7.Text = "0x" + fusionTrackBar1.Value.ToString("X2");
			keyonoff();
		}

		private void fusionTrackBar2_Scroll(object sender, EventArgs e)
		{
			// right volume
			DSP.SetDSPReg(0x01, (byte)fusionTrackBar2.Value);
			fastLabel8.Text = "0x" + fusionTrackBar2.Value.ToString("X2");
			keyonoff();
		}

		private void fusionTrackBar3_Scroll(object sender, EventArgs e)
		{
			// adsr 1
			DSP.SetDSPReg(0x05, (byte)fusionTrackBar3.Value);
			fastLabel9.Text = "0x" + fusionTrackBar3.Value.ToString("X2");
			keyonoff();
		}

		private void fusionTrackBar4_Scroll(object sender, EventArgs e)
		{
			// adsr 2
			DSP.SetDSPReg(0x06, (byte)fusionTrackBar4.Value);
			fastLabel10.Text = "0x" + fusionTrackBar4.Value.ToString("X2");
			keyonoff();
		}

		private void fusionTrackBar5_Scroll(object sender, EventArgs e)
		{
			// gain
			DSP.SetDSPReg(0x07, (byte)fusionTrackBar5.Value);
			fastLabel11.Text = "0x" + fusionTrackBar5.Value.ToString("X2");
			keyonoff();
		}

		private void fusionTrackBar6_Scroll(object sender, EventArgs e)
		{
			// pitch
			DSP.SetDSPReg(0x02, (byte)(fusionTrackBar6.Value & 255));
			DSP.SetDSPReg(0x03, (byte)(fusionTrackBar6.Value >> 8));
			fastLabel12.Text = "0x" + fusionTrackBar6.Value.ToString("X4");
			keyonoff();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(delegate()
			{
				DSP.SetDSPReg(0x5C, 0x01);
				DSP.SetDSPReg(0x4C, 0x00);
				Thread.Sleep(16);
				DSP.SetDSPReg(0x5C, 0x00);
				DSP.SetDSPReg(0x4C, 0x01);
			})).Start();
		}

		private void keyonoff()
		{
			if (toolStripMenuItem8.Checked)
			{
				button5_Click(null, null);
			}
		}

		private void UpdateTrackers()
		{
			fastLabel7.Text = "0x" + fusionTrackBar1.Value.ToString("X2");
			fastLabel8.Text = "0x" + fusionTrackBar2.Value.ToString("X2");
			fastLabel9.Text = "0x" + fusionTrackBar3.Value.ToString("X2");
			fastLabel10.Text = "0x" + fusionTrackBar4.Value.ToString("X2");
			fastLabel11.Text = "0x" + fusionTrackBar5.Value.ToString("X2");
			fastLabel12.Text = "0x" + fusionTrackBar6.Value.ToString("X4");
		}

		private void toolStripMenuItem8_Click(object sender, EventArgs e)
		{
			toolStripMenuItem8.Checked = !toolStripMenuItem8.Checked;
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
		{
			try
			{
				PitchMultiplier = Convert.ToByte(domainUpDown1.Text, 16);
				domainUpDown1.BackColor = Color.White;
				UpdatePitch();
			}
			catch
			{
				domainUpDown1.BackColor = Color.Red;
			}
		}

		private void domainUpDown2_SelectedItemChanged(object sender, EventArgs e)
		{
			try
			{
				PitchSubMultiplier = Convert.ToByte(domainUpDown2.Text, 16);
				domainUpDown2.BackColor = Color.White;
				UpdatePitch();
			}
			catch
			{
				domainUpDown2.BackColor = Color.Red;
			}
		}

		private void domainUpDown3_SelectedItemChanged(object sender, EventArgs e)
		{
			try
			{
				int octave = Convert.ToByte(domainUpDown3.Text);
				PitchNote &= 0x7F;
				PitchNote %= 12;
				PitchNote += octave * 12;
				PitchNote |= 0x80;
				domainUpDown3.BackColor = Color.White;
				UpdatePitch();
			}
			catch
			{
				domainUpDown3.BackColor = Color.Red;
			}
		}

		static string[] notes = new string[] { "c", "c+", "d", "d+", "e", "f",
            "f+", "g", "g+", "a", "a+", "b" };

		private void domainUpDown4_SelectedItemChanged(object sender, EventArgs e)
		{
			try
			{
				int note = Array.IndexOf<string>(notes, domainUpDown4.Text);
				if (note == -1) throw new Exception();
				PitchNote &= 0x7F;
				PitchNote = PitchNote / 12 * 12;
				PitchNote += note;
				PitchNote |= 0x80;
				domainUpDown4.BackColor = Color.White;
				UpdatePitch();
			}
			catch
			{
				domainUpDown4.BackColor = Color.Red;
			}
		}
    }
}
