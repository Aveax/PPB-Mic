using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace Shift.src
{
    class Shift
    {
        //Sciezka do folderu z plikami
        String path;
        //Nazwa pliku z nadawanym dzwiekiem
        String transmitter;
        //Nazwa pliku z odebranym dzwiekiem
        String receiver;
        //Skala (odstep pomiedzy samplami po upsamplingu)(gdybysmy chcieli w ms)
        double scale;
        //Zmienna do upsampler (do jakiej ilosci sampli zwiekszyc)
        int outRate;
        //Lista przesuniec
        List<double> shiftX;
        //Od ktorego sampla zaczac
        int start = 1000000;
        //Zmienna aby zakonczyc liczenie przesuniec wczesniej (czasem liczenie do konca psuje wyswietlanie w pliku wav)
        int end = 1000;

        public Shift(String path, String transmitter, String receiver, int outRate)
        {
            this.path = path;
            this.transmitter = transmitter;
            this.receiver = receiver;
            this.scale = Convert.ToDouble(1000) / Convert.ToDouble(outRate);
            this.outRate = outRate;
        }

        //Init
        public void calculateShift()
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

            List<double[]> transmitterList = intersections(transmitterBuffer);
            List<double[]> receiverList = intersections(receiverBuffer);

            List<double> calc = shift(transmitterList, receiverList, x_mon, y_mon);

            List<double> calc_minus_cont = calc.ConvertAll(x => (x - calc[0])/* * scale*/);

            this.shiftX = calc_minus_cont;
        }

        //Zapisanie do wav
        public void saveResult()
        {
            if (File.Exists(path + "result.wav"))
            {
                File.Delete(path + "result.wav");
            }

            double[] doubleArray = this.shiftX.ToArray();

            double max = 0;

            foreach (double x in doubleArray)
            {
                if (Math.Abs(x) > max) { max = Math.Abs(x); }
            }

            double[] doubleArrayX = new double[doubleArray.Length];

            for (int i = 0; i < doubleArray.Length; i++)
            {
                //doubleArrayX[i] = (doubleArray[i] / max) *(-1);
                doubleArrayX[i] = (doubleArray[i] / max);
            }

            float[] floatArray = doubleArrayX.Select(s => (float)s).ToArray();

            WaveFileReader reader = new WaveFileReader(path + "Sine40khzKrotki.wav");
            WaveFormat temp = reader.WaveFormat;

            WaveFormat waveFormat = new WaveFormat(/*temp.SampleRate*/44100, temp.BitsPerSample, temp.Channels);
            using (WaveFileWriter writer = new WaveFileWriter(path + "result.wav", waveFormat))
            {
                writer.WriteSamples(floatArray, 0, floatArray.Length);
            }
        }

        //Upsampling (wav -> wav)
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

        //Do float array
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

        //Znalezienie przeciec przez 0
        public List<double[]> intersections(float[] s)
        {
            List<double[]> list = new List<double[]>();

            double[] x = s.Select(a => (double)a).ToArray();
            
            for (int i = start; i < x.Length; i++)
            {
                if (x[i] == 0)
                {
                    double[] temp = new double[2];
                    //temp[0] = i * scale;
                    temp[0] = i;
                    temp[1] = 0;
                    list.Add(temp);
                }
                if (i < x.Length - 1)
                {
                    if (x[i] < 0 && x[i + 1] > 0)
                    {
                        double[] temp = new double[2];
                        temp[0] = i;
                        temp[1] = ((0 - x[i]) / (x[i + 1] - x[i]));
                        list.Add(temp);
                    }
                    if (x[i] > 0 && x[i + 1] < 0)
                    {
                        double[] temp = new double[2];
                        temp[0] = i;
                        temp[1] = 1 - ((0 - x[i + 1]) / (x[i] - x[i + 1]));
                        list.Add(temp);
                    }
                }
            }

            return list;
        }

        //Monotonicznosc do pierwszego przeciecia przez 0 (od wyznaczonego sampla)
        public int monotonicity(float[] x)
        {
            int mon = 0;
            if (x[start] <= 0 && x[start+1] > 0)
            {
                mon = 1;
            }
            if (x[start] > 0 && x[start+1] <= 0)
            {
                mon = -1;
            }
            if (x[start] > 0 && x[start+1] > 0)
            {
                if (x[start] > x[start+1])
                {
                    mon = -1;
                }
                else
                {
                    mon = 1;
                }
            }
            if (x[start] <= 0 && x[start+1] <= 0)
            {
                if (x[start] > x[start+1])
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

        //Obliczenie przesuniec pomiedzy 2 plikami
        public List<double> shift(List<double[]> x, List<double[]> y, int x_mon, int y_mon)
        {
            double[,] a = new double[2, x.Count];
            double[,] b = new double[2, y.Count];

            int counter = 0;
            foreach (double[] s in x)
            {
                a[0, counter] = s[0];
                a[1, counter] = s[1];
                counter++;
            }
            counter = 0;
            foreach (double[] s in y)
            {
                b[0, counter] = s[0];
                b[1, counter] = s[1];
                counter++;
            }

            List<double> shift = new List<double>();
            int length = 0;

            if (x.Count > y.Count) { length = y.Count; } else { length = x.Count; }


            if (x_mon == y_mon)
            {
                for (int i = 0; i < length - end; i = i + 2)
                {
                    double temp1 = a[0, i] - b[0, i];
                    double temp2 = a[1, i] - b[1, i];
                    shift.Add(temp1 + temp2);
                }
            }
            else
            {
                if (x_mon > y_mon)
                {
                    for (int i = 0; i < length - end; i = i + 1)
                    {
                        double temp1 = a[0, i] - b[0, i + 1];
                        double temp2 = a[1, i] - b[1, i + 1];
                        shift.Add(temp1 + temp2);
                    }
                }
                else
                {
                    for (int i = 0; i < length - end; i = i + 1)
                    {
                        double temp1 = a[0, i + 1] - b[0, i + 2];
                        double temp2 = a[1, i + 1] - b[1, i + 2];
                        shift.Add(temp1 + temp2);
                    }
                }
            }

            return shift;
        }
    }
}
