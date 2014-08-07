using System;
using System.Windows.Forms;
using StreamingNS;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private void DFT(double[] timeSignal, ref double[] real, ref double[] imag, int SampleNumbers, int Offset)
        {
            for (int sampleIndex = 0; sampleIndex < SampleNumbers; sampleIndex++)
            {
                real[sampleIndex] = 0;
                imag[sampleIndex] = 0;
            }

            for (int freqIndex = 0; freqIndex < SampleNumbers / 2; freqIndex++)
            {
                for (int sampleIndex = 0; sampleIndex < SampleNumbers; sampleIndex++)
                {
                    real[freqIndex] = real[freqIndex] + timeSignal[sampleIndex + Offset] * Math.Cos(2 * Math.PI * freqIndex * sampleIndex / SampleNumbers);
                    imag[freqIndex] = imag[freqIndex] - timeSignal[sampleIndex + Offset] * Math.Sin(2 * Math.PI * freqIndex * sampleIndex / SampleNumbers);
                }
            }
        }

        private void FFT(double[] timeSignal, ref double[] rex, ref double[] imx, int N, int Offset)
        {
            //int[] index = new int[N];
            for (int sampleIndex = 0; sampleIndex < N; sampleIndex++)
            {
                //index[sampleIndex] = sampleIndex;
                rex[sampleIndex] = timeSignal[sampleIndex + Offset];
                imx[sampleIndex] = 0;
            }
            //ShowValues(index, rex, imx, N, richTextBox1);

            int nm1 = N - 1;
            int nd2 = N / 2;
            int m = (int)(Math.Log(N) / Math.Log(2));
            int j = nd2;

            //for (int i = 1; i <= N - 2; i++) // bit reversal
            //{
            //    if (i >= j) goto Label1190;
            //    double tr = rex[j];
            //    double ti = imx[j];
            //    int tindex = index[j];
            //    rex[j] = rex[i];
            //    imx[j] = imx[i];
            //    index[j] = index[i];
            //    rex[i] = tr;
            //    imx[i] = ti;
            //    index[i] = tindex;
            //Label1190:
            //    int k = nd2;
            //Label1200:
            //    if (k > j) goto Label1240;
            //    j = j - k;
            //    k = k / 2;
            //    goto Label1200;
            //Label1240:
            //    j = j + k;
            //}

            for (int i = 1; i <= N - 2; i++) // bit reversal
            {
                if (i < j)
                {
                    double tr = rex[j];
                    double ti = imx[j];
                    //int tindex = index[j];
                    rex[j] = rex[i];
                    imx[j] = imx[i];
                    //index[j] = index[i];
                    rex[i] = tr;
                    imx[i] = ti;
                    //index[i] = tindex;
                }
                int k = nd2;
                while (k <= j)
                {
                    j = j - k;
                    k = k / 2;
                }
                j = j + k;
            }

            for (int l = 1; l <= m; l++)  // loop for each stage
            {
                int le = (int)Math.Pow(2, l);
                int le2 = le / 2;
                double ur = 1;
                double ui = 0;
                double sr = Math.Cos(Math.PI / le2);
                double si = -Math.Sin(Math.PI / le2);
                for (j = 1; j <= le2; j++) // loop for each sub DFT
                {
                    double tr;
                    double ti;

                    int jm1 = j - 1;
                    for (int i = jm1; i <= nm1; i = i + le) // loop for each butterfly
                    {
                        int ip = i + le2;
                        tr = rex[ip] * ur - imx[ip] * ui; // butterfly calculation
                        ti = rex[ip] * ui + imx[ip] * ur;
                        rex[ip] = rex[i] - tr;
                        imx[ip] = imx[i] - ti;
                        rex[i] = rex[i] + tr;
                        imx[i] = imx[i] + ti;
                    }
                    tr = ur;
                    ur = tr * sr - ui * si;
                    ui = tr * si + ui * sr;
                }
            }
            //ShowValues(index, rex, imx, N, richTextBox2);

        }

        //private void CrossSpectra(int lines, double[] a_r, double[] a_i, double[] b_r, double[] b_i, ref double[] c_r, ref double[] c_i )
        //{
        //    for (int l = 0; l < lines; l++)
        //    {
        //        c_r[l] = a_r[l] * b_r[l];
        //        c_i[l] = -a_i[l] * b_i[l];
        //    }
        //}

        private void FreqResponseH1(int lines, double[] a_r, double[] a_i, double[] b_r, double[] b_i, ref double[] h_r, ref double[] h_i)
        {
            // H1 = Gab / Gaa = (FFTb x FFT*a) / (FFTa x FFT*a)
            for (int l = 0; l < lines; l++)
            {
                double Gab_r = (a_r[l] * b_r[l] + a_i[l] * b_i[l]);
                double Gab_i = (a_r[l] * b_i[l] - b_r[l] * a_i[l]);

                double Gaa = a_r[l] * a_r[l] + a_i[l] * a_i[l];
                
                h_r[l] = Gab_r / Gaa;
                h_i[l] = Gab_i / Gaa;
            }
        }

        private void Coherence(int lines, double[] a_r, double[] a_i, double[] b_r, double[] b_i, ref double[] coh)
        {
//G_xy = X ' * Y

//RE(G_xy) = X_r * Y_r + X_i * Y_i
//IM(G_xy) = X_r * Y_i - X_i * Y_r

//Coherence  = ( (RE(G_xy)^2 +  (IM(G_xy)^2 ) / (G_xx * G_yy)


            
            for (int l = 0; l < lines; l++)
            {
                double Gab_r = (a_r[l] * b_r[l] + a_i[l] * b_i[l]);
                double Gab_i = (a_r[l] * b_i[l] - b_r[l] * a_i[l]);

                double Gaa = a_r[l] * a_r[l] + a_i[l] * a_i[l];
                double Gbb = b_r[l] * b_r[l] + b_i[l] * b_i[l];

                // Coh = Gab x G*ab / Gaa * Gbb
                coh[l] = (Gab_r * Gab_r + Gab_i * Gab_i) / (Gaa * Gbb);
            }
        }

        private void GenerateSignal(int signalType, int SampleNumbers, double frequency, double ampl, double startphase, double frequency2, double ampl2, ref double[] timeSignalOut, double[] timeSignalIn)
        {
            const int SCALE = 1000000;

            switch (signalType)
            {
                case 1:
                    {
                        // Sinus
                        double startRadiant = 2 * Math.PI * startphase / 360;
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] = SCALE * Math.Sin(2 * Math.PI * frequency * ((double)SampleIndex / SampleNumbers) - startRadiant) * ampl;
                        }

                    }
                    break;
                case 2:
                    {
                        // Trekant
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] = SCALE * ((SampleIndex % frequency) / frequency) * ampl;
                        }

                    }
                    break;
                case 3:
                    {
                        // Firkant
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            if ((SampleIndex % frequency) > (frequency / 2))
                                timeSignalOut[SampleIndex] = SCALE * ampl;
                            else
                                timeSignalOut[SampleIndex] = 0;
                        }
                    }
                    break;
                case 4:
                    {
                        Random rnd1 = new System.Random(1);
                        int rndRange = 1000;
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] = SCALE * ((rnd1.Next(rndRange) / (double)rndRange) - 0.5) * ampl;
                        }
                    }
                    break;
                case 5:
                    {
                        // ToToneSin
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] = SCALE * Math.Sin(2 * Math.PI * frequency * ((double)SampleIndex / SampleNumbers)) * ampl +
                                Math.Cos(2 * Math.PI * frequency2 * ((double)SampleIndex / SampleNumbers)) * ampl2;
                        }

                    }
                    break;
                case 6:
                    {
                        // Dirac
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] = 0;
                        }
                        timeSignalOut[SampleNumbers / 2] = SCALE * 1;

                    }
                    break;
                case 7:
                    {
                        // FIR Filter of Sig 1
                        int kernelSize = 5;
                        double[] coeff = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                        for (int SampleIndex = kernelSize; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            double sum = 0;
                            for (int a = 0; a < kernelSize; a++)
                            {
                                sum += coeff[kernelSize - 1 - a] * timeSignalIn[SampleIndex - a];
                            }
                            timeSignalOut[SampleIndex] = sum / kernelSize;
                        }

                    }
                    break;
                case 8:
                    {
                        // FIR Filter of Sig 1 plus noise
                        int kernelSize = 5;
                        double[] coeff = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                        for (int SampleIndex = kernelSize; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            double sum = 0;
                            for (int a = 0; a < kernelSize; a++)
                            {
                                sum += coeff[kernelSize - 1 - a] * timeSignalIn[SampleIndex - a];
                            }
                            timeSignalOut[SampleIndex] = sum / kernelSize;
                        }

                        Random rnd1 = new System.Random(100);
                        int rndRange = 1000;
                        for (int SampleIndex = 0; SampleIndex < SampleNumbers; SampleIndex++)
                        {
                            timeSignalOut[SampleIndex] += ((rnd1.Next(rndRange) / (double)rndRange) - 0.5) * ampl / 10;
                        }

                    }
                    break;
                case 9:
                    {
                        InputStream MyStream = new InputStream();
                        MyStream.Start("10.10.10.101", SampleNumbers);
                        int z = MyStream.NumberOfSamples; 
                        for (int i = 0; i < SampleNumbers; i++) //MyStream.NumberOfSamples;
                        {
                            timeSignalOut[i] = MyStream.GetSample(0, i);
                        }

                    }
                    break;
            }
        }


    }
}

/* See also
http://www.codeproject.com/Articles/6855/FFT-of-waveIn-audio-signals#
http://www.lumerink.com/courses/ece697/docs/Papers/The%20Fundamentals%20of%20FFT-Based%20Signal%20Analysis%20and%20Measurements.pdf
http://www.barrgroup.com/Embedded-Systems/How-To/Digital-Filters-FIR-IIR
*/