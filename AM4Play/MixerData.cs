using System;
using System.Collections.Generic;
using System.Text;

namespace AM4Play
{
	unsafe struct MixerData
	{
		public static bool copyChannelPointers = false;
		public static ushort[] chnMem = new ushort[8];
	}
}
