using BRRPlay.Properties;
using AM4Play.SNESAPU;

namespace AM4Play
{
	/// <summary>
	/// Contains all options of AM4 Player
	/// </summary>
	static unsafe class Options
	{
		private static void save()
		{
			Settings.Default.Save();
		}

		public static Interpolation INTERPOLATION
		{
			get
			{
				uint inter = Settings.Default.Interpolation;
				
				if (inter == 0xFFFFFFFF)
				{
					return SNESAPU.Interpolation.INT_GAUSS;
				}

				return (Interpolation)inter;
			}
			set
			{
				Settings.Default.Interpolation = (uint)value;
				save();
			}
		}

		public static DSPOpts OPTIONS
		{
			get
			{
				return (DSPOpts)Settings.Default.Options;
			}
			set
			{
				Settings.Default.Options = (uint)value;
				save();
			}
		}

        public static uint RATE
        {
            get
            {
                return Settings.Default.RATE;
            }
            set
            {
                Settings.Default.RATE = value;
                save();
            }
        }

        public static uint BITS
        {
            get
            {
                return Settings.Default.BITS;
            }
            set
            {
                Settings.Default.BITS = value;
                save();
            }
        }

        public static uint CHANNELS
        {
            get
            {
                return Settings.Default.CHANNELS;
            }
            set
            {
                Settings.Default.CHANNELS = value;
                save();
            }
        }

        public static bool ForceExternalProgram
        {
            get
            {
                return Settings.Default.ExternalPlayer;
            }
            set
            {
                Settings.Default.ExternalPlayer = value;
                save();
            }
        }

        public static bool EnableFastSpcSeek
        {
            get
            {
                return Settings.Default.FastSeek;
            }
            set
            {
                Settings.Default.FastSeek = value;
                save();
            }
        }

        public static int AddmusicEngine
        {
            get
            {
                return Settings.Default.Engine;
            }
            set
            {
                Settings.Default.Engine = value;
                save();
            }
        }

        public static uint Volume
        {
            get
            {
                return Settings.Default.Volume;
            }
            set
            {
                Settings.Default.Volume = value;
                save();
            }
        }

        public static bool EnableTimer
        {
            get
            {
                return Settings.Default.Timer;
            }
            set
            {
                Settings.Default.Timer = value;
                save();
            }
        }

        public static int MaximiumMinutes
        {
            get
            {
                return Settings.Default.Minutes;
            }
            set
            {
                Settings.Default.Minutes = value;
                save();
            }
        }

        public static float FrameRate
        {
            get
            {
                return Settings.Default.FrameRate;
            }
            set
            {
                Settings.Default.FrameRate = value;
                save();
            }
        }
    }
}
