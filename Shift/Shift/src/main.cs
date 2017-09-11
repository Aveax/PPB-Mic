using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;

namespace Shift.src
{
    class Cosiek
    {
        static void Main()
        {
            WaveFileReader nad = new WaveFileReader("j:\\Downloads\\BadNauk\\Sine40khzKrotki.wav");
            WaveFileReader odb = new WaveFileReader("j:\\Downloads\\BadNauk\\0002 3-Audio-1.wav");
            double skala = 0.005208;

            var inFile = "j:\\Downloads\\BadNauk\\Sine40khzKrotki.wav";
            var outFile = "j:\\Downloads\\BadNauk\\ok.wav";
            int outRate = 768000;

            Shift sample = new Shift(nad, odb, skala);

            List<double> przes = sample.obliczPrzesuniecie();

            foreach (double i in przes)
            {
                Console.WriteLine(i * (-1) + " ms");
            }

            using (var reader = new AudioFileReader(inFile))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outRate);
                WaveFileWriter.CreateWaveFile16(outFile, resampler);
            }

            Console.ReadLine();

        }
    }
}
