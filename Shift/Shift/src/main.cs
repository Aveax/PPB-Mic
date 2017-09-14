using System;

namespace Shift.src
{
    class Cosiek
    {
        static void Main()
        {
            //Sciezka do folderu z plikami
            String path = "E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\" ;
            //Nazwa pliku z nadawanym dzwiekiem
            String transmitter = "Sine40khzKrotki.wav";
            //Nazwa pliku z odebranym dzwiekiem
            String receiver = "Ruszanie.wav";
            //Zmienna do upsampler (do jakiej ilosci sampli zwiekszyc)
            int outRate = 768000;

            Shift sample = new Shift(path, transmitter, receiver, outRate);

            sample.calculateShift();
            sample.saveResult();

            //Console.ReadLine();
        }
    }
}
