using System;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Wave;
using AM4Play.SNESAPU;

namespace AM4Play.WaveLib
{
    class SPC700Provider : IWaveProvider
    {
        public static bool IsEmulating
        {
            get
            {
                return emu;
            }
        }

        public static bool CanEmulate
        {
            get { return canemu; }
            set { canemu = value; }
        }

        static bool emu = false, canemu = true;

        WaveFormat format;

        public SPC700Provider(int rate, int bits, int channels)
        {
            if (bits == -32)
            {
                format = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            }
            else
            {
                format = new WaveFormat(rate, bits, channels);
            }
        }

        public unsafe int Read(byte[] buffer, int offset, int count)
        {
            if (canemu)
            {
                emu = true;

                fixed (byte* ptr = &buffer[offset])
                {
                    uint samples = (uint)(count / format.BlockAlign);

                    APU.EmuAPU(ptr, samples, 1);
                }

                emu = false;

                Program.GetInfo2(0);
                Program.GetInfo2(1);
                Program.GetInfo2(2);
                Program.GetInfo2(3);
                Program.GetInfo2(4);
                Program.GetInfo2(5);
                Program.GetInfo2(6);
                Program.GetInfo2(7);

                if (MixerData.copyChannelPointers)
                {
                    MixerData.chnMem[0] = (ushort)(APU.ram[0x0031] << 8 | APU.ram[0x0030]);
                    MixerData.chnMem[1] = (ushort)(APU.ram[0x0033] << 8 | APU.ram[0x0032]);
                    MixerData.chnMem[2] = (ushort)(APU.ram[0x0035] << 8 | APU.ram[0x0034]);
                    MixerData.chnMem[3] = (ushort)(APU.ram[0x0037] << 8 | APU.ram[0x0036]);
                    MixerData.chnMem[4] = (ushort)(APU.ram[0x0039] << 8 | APU.ram[0x0038]);
                    MixerData.chnMem[5] = (ushort)(APU.ram[0x003B] << 8 | APU.ram[0x003A]);
                    MixerData.chnMem[6] = (ushort)(APU.ram[0x003D] << 8 | APU.ram[0x003C]);
                    MixerData.chnMem[7] = (ushort)(APU.ram[0x003F] << 8 | APU.ram[0x003E]);
                }

                return count;
            }
            else
            {
                buffer[0] = 0;
                return 1;
            }
        }

        public WaveFormat WaveFormat
        {
            get { return format; }
        }
    }
}
