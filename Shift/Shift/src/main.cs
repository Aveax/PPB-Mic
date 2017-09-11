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
            String nad = "C:\\Users\\Dawid\\Desktop\\x\\PPB-Mic\\Shift\\Assets\\Sine40khzKrotki.wav";
            String odb = "C:\\Users\\Dawid\\Desktop\\x\\PPB-Mic\\Shift\\Assets\\0002 3-Audio-1.wav";
            double skala = 0.005208;
            int outRate = 768000;

            Shift sample = new Shift(nad, odb, skala, outRate);

            List<double> przes = sample.obliczPrzesuniecie();

            foreach (double i in przes)
            {
                Console.WriteLine(i * (-1) + " ms");
            }

            //for(int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine(przes[i] * (-1) + " ms");
            //}

            Console.ReadLine();

        }
    }
}
