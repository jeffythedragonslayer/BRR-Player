using System;
using NAudio.Wave;

namespace AM4Play.WaveLib
{
    class WavePlayer : IDisposable
    {
        private WaveOut player;
        private SPC700Provider provider;

        public WavePlayer(int sampleRate, int bits, int channels)
        {
            provider = new SPC700Provider(sampleRate, bits, channels);
            player = new WaveOut();
            player.DesiredLatency = 100;
            player.NumberOfBuffers = 2;
            player.Init(provider);
        }

        public void Dispose()
        {
            player.Stop();
            player.Dispose();
            player = null;
            provider = null;
        }

        public void Play()
        {
            player.Play();
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Stop()
        {
            player.Stop();
        }
    }
}
