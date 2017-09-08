using NAudio.Wave;
using System.Collections.Generic;

namespace Shift.src
{
    class Shift
    {
        WaveFileReader nad;
        WaveFileReader odb;
        double skala;

        public Shift(WaveFileReader nad, WaveFileReader odb, double skala)
        {
            this.nad = nad;
            this.odb = odb;
            this.skala = skala;
        }

        public List<double> obliczPrzesuniecie()
        {

            byte[] nadbyte = new byte[nad.Length];
            this.nad.Read(nadbyte, 0, nadbyte.Length);

            byte[] odbbyte = new byte[odb.Length];
            this.odb.Read(odbbyte, 0, odbbyte.Length);

            int x_mon = monotonicznosc(nadbyte);
            int y_mon = monotonicznosc(odbbyte);

            List<double> nadl = przeciecia(nadbyte);
            List<double> odbl = przeciecia(odbbyte);

            List<double> przes = przesuniecie(nadl, odbl, x_mon, y_mon);

            List<double> przes_m_kont = przes.ConvertAll(x => x - przes[0]);

            return przes_m_kont;
        }

        public List<double> przeciecia(byte[] x)
        {
            List<double> lista = new List<double>();

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == 127)
                {
                    lista.Add(i * skala);
                }
                if (i < x.Length - 1)
                {
                    if (x[i] < 127 && x[i + 1] > 127)
                    {
                        double temp = (127 - x[i]) / (x[i + 1] - x[i]);
                        lista.Add(i * skala + temp * skala);
                    }
                    if (x[i] > 127 && x[i + 1] < 127)
                    {
                        double temp = (127 - x[i + 1]) / (x[i] - x[i + 1]);
                        lista.Add((i + 1) * skala + skala - temp * skala);
                    }
                }
            }
            return lista;
        }

        public int monotonicznosc(byte[] x)
        {
            int mon = 0;
            if (x[0] <= 127 && x[1] > 127)
            {
                mon = 1;
            }
            if (x[0] > 127 && x[1] <= 127)
            {
                mon = -1;
            }
            if (x[0] > 127 && x[1] > 127)
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
            if (x[0] <= 127 && x[1] <= 127)
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
                    for (int i = 0; i < length - 2; i = i + 2)
                    {
                        przes.Add(x[i] - y[i + 1]);
                    }
                }
                else
                {
                    for (int i = 0; i < length - 2; i = i + 2)
                    {
                        przes.Add(x[i] - y[i + 2]);
                    }
                }
            }

            return przes;
        }
    }
}
