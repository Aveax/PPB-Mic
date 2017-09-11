using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System;
using System.IO;

namespace Shift.src
{
    class Shift
    {
        String path;
        String transmitter;
        String receiver;
        double scale;
        int outRate;

        public Shift(String path, String transmitter, String receiver, int outRate)
        {
            this.path = path;
            this.transmitter = transmitter;
            this.receiver = receiver;
            this.scale = Convert.ToDouble(1000) / Convert.ToDouble(outRate);
            this.outRate = outRate;
        }

        public List<double> calculateShift()
        {
            if (File.Exists(path + "first.wav"))
            {
                File.Delete(path + "first.wav");
            }
            if (File.Exists(path + "second.wav"))
            {
                File.Delete(path + "second.wav");
            }

            upsampler(path + transmitter, "first");
            upsampler(path + receiver, "second");

            float[] transmitterBuffer = convert(new MediaFoundationReader(path + "first.wav"));

            float[] receiverBuffer = convert(new MediaFoundationReader(path + "second.wav"));

            int x_mon = monotonicity(transmitterBuffer);
            int y_mon = monotonicity(receiverBuffer);

            List<double> transmitterList = intersections(transmitterBuffer);
            List<double> receiverList = intersections(receiverBuffer);

            List<double> calc = shift(transmitterList, receiverList, x_mon, y_mon);

            List<double> calc_minus_cont = calc.ConvertAll(x => x - calc[0]);

            return calc_minus_cont;
        }

        public void upsampler(String x, String y)
        {
            Console.WriteLine("UPSAMPLER START");
            using (var reader = new AudioFileReader(x))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outRate);
                WaveFileWriter.CreateWaveFile16(path + y + ".wav", resampler);
            }
            Console.WriteLine("UPSAMPLER END");
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

        public List<double> intersections(float[] x)
        {
            List<double> list = new List<double>();

            for (int i = 10; i < x.Length; i++)
            {
                if (x[i] == 0)
                {
                    list.Add(i * scale);
                }
                if (i < x.Length - 1)
                {
                    if (x[i] < 0 && x[i + 1] > 0)
                    {
                        double temp = (0 - x[i]) / (x[i + 1] - x[i]);
                        list.Add(i * scale + temp * scale);
                    }
                    if (x[i] > 0 && x[i + 1] < 0)
                    {
                        double temp = (0 - x[i + 1]) / (x[i] - x[i + 1]);
                        list.Add((i + 1) * scale + scale - temp * scale);
                    }
                }
            }
            return list;
        }

        public int monotonicity(float[] x)
        {
            int mon = 0;
            if (x[10] <= 0 && x[11] > 0)
            {
                mon = 1;
            }
            if (x[10] > 0 && x[11] <= 0)
            {
                mon = -1;
            }
            if (x[10] > 0 && x[11] > 0)
            {
                if (x[10] > x[11])
                {
                    mon = -1;
                }
                else
                {
                    mon = 1;
                }
            }
            if (x[10] <= 0 && x[11] <= 0)
            {
                if (x[10] > x[11])
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

        public List<double> shift(List<double> x, List<double> y, int x_mon, int y_mon)
        {
            List<double> shift = new List<double>();
            int length = 0;
            if (x.Count > y.Count) { length = y.Count; } else { length = x.Count; }


            if (x_mon == y_mon)
            {
                for (int i = 0; i < length - 2; i = i + 2)
                {
                    shift.Add(x[i] - y[i]);
                }
            }
            else
            {
                if (x_mon > y_mon)
                {
                    for (int i = 0; i < length - 2; i = i + 1)
                    {
                        shift.Add(x[i] - y[i + 1]);
                    }
                }
                else
                {
                    for (int i = 0; i < length - 2; i = i + 1)
                    {
                        shift.Add(x[i + 1] - y[i + 2]);
                    }
                }
            }

            return shift;
        }
    }
}
