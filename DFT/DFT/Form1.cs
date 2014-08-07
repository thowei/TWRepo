using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private const int SampleOffset = 0; //100;
        private const int SampleNumbers = 4096; // * 4
        private double[] timeSignal1 = new double[SampleNumbers + SampleOffset];
        private double[] timeSignal2 = new double[SampleNumbers + SampleOffset];
        private double[] rex1 = new double[SampleNumbers];
        private double[] imx1 = new double[SampleNumbers];
        private double[] rex2 = new double[SampleNumbers];
        private double[] imx2 = new double[SampleNumbers];
        //private int SeriesIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = 1;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void ShowTimeSignal(int signalIndex, double[] timeSignal)
        {
            chart1.Series[signalIndex].Points.Clear();
            for (int SampleIndex = 0; SampleIndex < SampleNumbers + SampleOffset; SampleIndex++)
            {
                chart1.Series[signalIndex].Points.AddXY((double)SampleIndex - SampleOffset, timeSignal[SampleIndex]);
            }
        }

        private void CalculateSpectra(int SignalIndex, int signalType, double freq, double ampl, double startphase, double freq2, double ampl2)
        {
            bool UseDFT = (comboBox3.SelectedIndex == 0);
            if (SignalIndex == 0)
            {
                GenerateSignal(signalType, SampleNumbers + SampleOffset, freq, ampl, startphase, freq2, ampl2, ref timeSignal1, null);
                ShowTimeSignal(SignalIndex, timeSignal1);
                if (UseDFT)
                {
                    // --- DFT -----------------------
                    DFT(timeSignal1, ref rex1, ref imx1, SampleNumbers, SampleOffset);
                    ShowSpectra(chart2, chart4, rex1, imx1, SampleNumbers / 2, SampleNumbers / 2);
                }
                else 
                {
                    //// --- FFT --------------------------
                    FFT(timeSignal1, ref rex1, ref imx1, SampleNumbers, SampleOffset);
                    ShowSpectra(chart2, chart4, rex1, imx1, SampleNumbers / 2, SampleNumbers / 2);
                }
            }
            else if (SignalIndex == 1)
            {
                GenerateSignal(signalType, SampleNumbers + SampleOffset, freq, ampl, startphase, freq2, ampl2, ref timeSignal2, timeSignal1);
                //timeSignal2[50] = 0;
                //timeSignal2[51] = 0;
                //timeSignal2[53] = 0;
                //timeSignal2[54] = 0;
                //timeSignal2[55] = 0;
                //timeSignal2[56] = 0;
                //timeSignal2[57] = 0;
                ShowTimeSignal(SignalIndex, timeSignal2);
                if (UseDFT)
                {
                    // --- DFT -----------------------
                    DFT(timeSignal2, ref rex2, ref imx2, SampleNumbers, SampleOffset);
                    ShowSpectra(chart3, chart5, rex2, imx2, SampleNumbers / 2, SampleNumbers / 2);
                }
                else 
                {
                    // --- FFT --------------------------
                    FFT(timeSignal2, ref rex2, ref imx2, SampleNumbers, SampleOffset);
                    ShowSpectra(chart3, chart5, rex2, imx2, SampleNumbers / 2, SampleNumbers / 2);
                }
            }
        }

        private void CalculateFrequencyResponse()
        {
            double[] rex3 = new double[SampleNumbers];
            double[] imx3 = new double[SampleNumbers];

            FreqResponseH1(SampleNumbers, rex1, imx1, rex2, imx2, ref rex3, ref imx3);
            ShowSpectra(chart6, chart7, rex3, imx3, SampleNumbers / 2, 1);

            double[] coh = new double[SampleNumbers];
            Coherence(SampleNumbers, rex1, imx1, rex2, imx2, ref coh);
            ShowCoherence(chart8, coh, SampleNumbers / 2);
        }

        private void ShowSpectra(Chart amplChart, Chart phaseChart, double[] real, double[] imag, int size, double scale)
        {

            //amplChart.Series.Add("Ampl " + SeriesIndex.ToString());
            //phaseChart.Series.Add("Phase " + SeriesIndex.ToString());
            //amplChart.Series[SeriesIndex].ChartType = SeriesChartType.FastLine;
            //phaseChart.Series[SeriesIndex].ChartType = SeriesChartType.FastLine;

            amplChart.Series[0].Points.Clear();
            phaseChart.Series[0].Points.Clear();
            for (int freqIndex = 0; freqIndex < size; freqIndex++)
            {
                double ampl = Math.Sqrt(Math.Pow(real[freqIndex], 2) + Math.Pow(imag[freqIndex], 2)) / scale;
                double phase = 180 * Math.Atan(imag[freqIndex] / real[freqIndex]) / Math.PI;
                amplChart.Series[0].Points.AddXY(freqIndex, ampl);
                phaseChart.Series[0].Points.AddXY(freqIndex, phase);
            }
            //SeriesIndex++;
        }

        private void ShowCoherence(Chart amplChart, double[] coh, int size)
        {
            amplChart.Series[0].Points.Clear();
            for (int freqIndex = 0; freqIndex < size; freqIndex++)
            {
                amplChart.Series[0].Points.AddXY(freqIndex, coh[freqIndex]);
            }
        }

        private void ShowValues(int[] index, double[] rex, double[] imx, int size, RichTextBox textbox)
        {
            textbox.Clear();
            for (int i = 0; i < size; i++)
            {
                textbox.AppendText(index[i].ToString() + "\t" + rex[i].ToString("F03") + "\t" + imx[i].ToString("F03") + "\r\n");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateSpectra(0, comboBox1.SelectedIndex + 1, (double)numericUpDown2.Value, (double)numericUpDown1.Value, (double)numericUpDown9.Value, (double)numericUpDown3.Value, (double)numericUpDown4.Value);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateSpectra(1, comboBox2.SelectedIndex + 1, (double)numericUpDown7.Value, (double)numericUpDown8.Value, (double)numericUpDown10.Value, (double)numericUpDown5.Value, (double)numericUpDown6.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CalculateFrequencyResponse();
        }

        
    }
}
