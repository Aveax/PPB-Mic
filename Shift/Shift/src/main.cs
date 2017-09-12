using System;
using System.Collections.Generic;

namespace Shift.src
{
    class Cosiek
    {
        static void Main()
        {
            String path = "E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\" ;
            String transmitter = "Sine40khzKrotki.wav";
            String receiver = "0002 3-Audio-1.wav";
            int outRate = 768000;

            Shift sample = new Shift(path, transmitter, receiver, outRate);

            sample.calculateShift();
            sample.saveResult();


            //List<double> shift = sample.calculateShift();

            //foreach (double i in shift)
            //{
            //    Console.WriteLine(i * (-1) + " ms");
            //}

            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine(shift[i] * (-1) + " ms");
            //}

            //Console.ReadLine();

        }
    }
}
