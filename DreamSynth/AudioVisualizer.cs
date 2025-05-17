using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace DreamSynth
{
    public class AudioVisualizer
    {
        private readonly PlotModel timePlotModel;
        private readonly PlotModel freqPlotModel;
        private readonly LineSeries timeLineSeries;
        private readonly LineSeries freqLineSeries;

        public AudioVisualizer(PlotModel timePlotModel, PlotModel freqPlotModel)
        {
            this.timePlotModel = timePlotModel;
            this.freqPlotModel = freqPlotModel;
            
            timePlotModel.Title = null;
            timePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 1000,
                Title = "Время"
            });
            timePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -3,
                Maximum = 3,
                Title = "Амплитуда"
            });

            timeLineSeries = new LineSeries
            {
                Color = OxyColors.Blue,
                LineStyle = LineStyle.Solid
            };
            timePlotModel.Series.Add(timeLineSeries);
            
            freqPlotModel.Title = null;
            freqPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 5000,
                Title = "Частота (Гц)"
            });
            freqPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 10,
                Title = "Амплитуда"
            });

            freqLineSeries = new LineSeries
            {
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid
            };
            freqPlotModel.Series.Add(freqLineSeries);
        }

        public void Update(float[] audioData, int sampleRate = 44100)
        {
            timeLineSeries.Points.Clear();
            for (int i = 0; i < audioData.Length; i++)
            {
                timeLineSeries.Points.Add(new DataPoint(i, audioData[i]));
            }
            timePlotModel.InvalidatePlot(true);
            
            MathNet.Numerics.Complex32[] complexData = new MathNet.Numerics.Complex32[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                complexData[i] = new MathNet.Numerics.Complex32(audioData[i], 0);
            }
            
            Fourier.Forward(complexData, FourierOptions.Matlab);
            
            freqLineSeries.Points.Clear();
            int n = audioData.Length / 2;
            float frequencyStep = sampleRate / (float)audioData.Length;
            float maxFrequency = 5000;
            int maxIndex = (int)(maxFrequency / frequencyStep);
            maxIndex = Math.Min(maxIndex, n);
            
            float maxMagnitude = 0;
            for (int i = 0; i < maxIndex; i++)
            {
                float magnitude = complexData[i].Magnitude;
                if (magnitude > maxMagnitude) maxMagnitude = magnitude;
            }

            for (int i = 0; i < maxIndex; i++)
            {
                float frequency = i * frequencyStep;
                float magnitude = complexData[i].Magnitude;
                float scaledMagnitude = maxMagnitude > 0 ? 10 * (float)Math.Log10(1 + magnitude / maxMagnitude) : 0;
                freqLineSeries.Points.Add(new DataPoint(frequency, scaledMagnitude));
            }
            
            freqPlotModel.Axes[0].Maximum = maxFrequency;
            freqPlotModel.Axes[1].Maximum = 10;
            freqPlotModel.InvalidatePlot(true);
        }
    }
}