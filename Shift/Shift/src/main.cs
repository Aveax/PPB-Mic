using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace Shift.src
{
    class Cosiek
    {
        static void Main()
        {
            WaveFileReader nad = new WaveFileReader("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\Sine40khzKrotki.wav");
            WaveFileReader odb = new WaveFileReader("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\0002 3-Audio-1.wav");
            double skala = 0.005208;


            Shift sample = new Shift(nad, odb, skala);

            List<double> przes = sample.obliczPrzesuniecie();

            foreach (double i in przes)
            {
                Console.WriteLine(i * (-1) + " ms");
            }

            Console.ReadLine();

        }
    }
}
