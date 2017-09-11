using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System;
using System.IO;

namespace Shift.src
{
    class Shift
    {
        String nad;
        String odb;
        double skala;
        int outRate;

        public Shift(String nad, String odb, double skala, int outRate)
        {
            this.nad = nad;
            this.odb = odb;
            this.skala = skala;
            this.outRate = outRate;
        }

        public List<double> obliczPrzesuniecie()
        {
            upsampler(nad, "pierwszy");
            upsampler(odb, "drugi");

            float[] nadBuffer = convert(new MediaFoundationReader("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\pierwszy.wav"));
            File.Delete("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\pierwszy.wav");

            float[] odbBuffer = convert(new MediaFoundationReader("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\drugi.wav"));
            File.Delete("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\drugi.wav");

            int x_mon = monotonicznosc(nadBuffer);
            int y_mon = monotonicznosc(odbBuffer);

            List<double> nadl = przeciecia(nadBuffer);
            List<double> odbl = przeciecia(odbBuffer);

            List<double> przes = przesuniecie(nadl, odbl, x_mon, y_mon);

            List<double> przes_m_kont = przes.ConvertAll(x => x - przes[0]);

            return przes;
        }

        public void upsampler(String x, String y)
        {
            using (var reader = new AudioFileReader(x))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outRate);
                WaveFileWriter.CreateWaveFile16("E:\\UAM\\PPB\\Mikrofon\\PPB-Mic\\Shift\\Assets\\"+y+".wav", resampler);
            }
        }

        public float[] convert(MediaFoundationReader x)
        {
            float[] floatBuffer;

            int _byteBuffer32_length = (int)x.Length * 2;
            int _floatBuffer_length = _byteBuffer32_length / sizeof(float);

            IWaveProvider stream32 = new Wave16ToFloatProvider(x);
            WaveBuffer _waveBuffer = new WaveBuffer(_byteBuffer32_length);
            stream32.Read(_waveBuffer, 0, (int)_byteBuffer32_length);
            floatBuffer = new float[_floatBuffer_length];

            for (int i = 0; i < _floatBuffer_length; i++)
            {
                floatBuffer[i] = _waveBuffer.FloatBuffer[i];
            }

            return floatBuffer;
        }

        public List<double> przeciecia(float[] x)
        {
            List<double> lista = new List<double>();

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == 0)
                {
                    lista.Add(i * skala);
                }
                if (i < x.Length - 1)
                {
                    if (x[i] < 0 && x[i + 1] > 0)
                    {
                        double temp = (0 - x[i]) / (x[i + 1] - x[i]);
                        lista.Add(i * skala + temp * skala);
                    }
                    if (x[i] > 0 && x[i + 1] < 0)
                    {
                        double temp = (0 - x[i + 1]) / (x[i] - x[i + 1]);
                        lista.Add((i + 1) * skala + skala - temp * skala);
                    }
                }
            }
            return lista;
        }

        public int monotonicznosc(float[] x)
        {
            int mon = 0;
            if (x[0] <= 0 && x[1] > 0)
            {
                mon = 1;
            }
            if (x[0] > 0 && x[1] <= 0)
            {
                mon = -1;
            }
            if (x[0] > 0 && x[1] > 0)
            {
                if (x[0] > x[1])
                {
                    mon = -1;
                }
                else
                {
                    mon = 1;
                }
            }
            if (x[0] <= 0 && x[1] <= 0)
            {
                if (x[0] > x[1])
                {
                    mon = 1;
                }
                else
                {
                    mon = -1;
                }
            }

            return mon;
        }

        public List<double> przesuniecie(List<double> x, List<double> y, int x_mon, int y_mon)
        {
            List<double> przes = new List<double>();
            int length = 0;
            if (x.Count > y.Count) { length = y.Count; } else { length = x.Count; }


            if (x_mon == y_mon)
            {
                for (int i = 0; i < length - 2; i = i + 2)
                {
                    przes.Add(x[i] - y[i]);
                }
            }
            else
            {
                if (x_mon > y_mon)
                {
                    for (int i = 0; i < length - 2; i = i + 1)
                    {
                        przes.Add(x[i] - y[i + 1]);
                    }
                }
                else
                {
                    for (int i = 0; i < length - 2; i = i + 1)
                    {
                        przes.Add(x[i + 1] - y[i + 2]);
                    }
                }
            }

            return przes;
        }
    }
}
