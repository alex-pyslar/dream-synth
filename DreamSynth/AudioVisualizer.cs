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
        private readonly LinearAxis timeXAxis;
        private readonly LinearAxis timeYAxis;
        private readonly LinearAxis freqXAxis;
        private readonly LinearAxis freqYAxis;

        public AudioVisualizer(PlotModel timePlotModel, PlotModel freqPlotModel)
        {
            this.timePlotModel = timePlotModel;
            this.freqPlotModel = freqPlotModel;
            
            timePlotModel.Title = null;
            timeXAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 46.44,
                Title = "Время (мс)",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            timeYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1.5,
                Maximum = 1.5,
                Title = "Амплитуда",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            timePlotModel.Axes.Add(timeXAxis);
            timePlotModel.Axes.Add(timeYAxis);

            timeLineSeries = new LineSeries
            {
                Color = OxyColors.Blue,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2
            };
            timePlotModel.Series.Add(timeLineSeries);
            
            freqPlotModel.Title = null;
            freqXAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 5000,
                Title = "Частота (Гц)",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            freqYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -60,
                Maximum = 0,
                Title = "Амплитуда (дБ)",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            freqPlotModel.Axes.Add(freqXAxis);
            freqPlotModel.Axes.Add(freqYAxis);

            freqLineSeries = new LineSeries
            {
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2
            };
            freqPlotModel.Series.Add(freqLineSeries);
        }

        private float[] ApplyModulation(float[] audioData, int sampleRate)
        {
            float[] modulatedData = new float[audioData.Length];
            const float modulationFrequency = 10f;
            const float modulationDepth = 0.5f;
            double modulationIncrement = 2 * Math.PI * modulationFrequency / sampleRate;
            double currentPhase = 0f;

            for (int i = 0; i < audioData.Length; i++)
            {
                float modulation = 1 + modulationDepth * (float)Math.Sin(currentPhase);
                modulatedData[i] = audioData[i] * modulation;
                currentPhase += modulationIncrement;
            }
            return modulatedData;
        }

        private float[] ApplyDistortion(float[] audioData)
        {
            float[] distortedData = new float[audioData.Length];
            const float distortionAmount = 0.5f;
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                distortedData[i] = (float)Math.Tanh(distortionAmount * sample);
            }
            return distortedData;
        }

        public void Update(float[] audioData, int sampleRate = 44100)
        {
            float[] modulatedData = ApplyModulation(audioData, sampleRate);
            float[] processedData = ApplyDistortion(modulatedData);
            
            timeLineSeries.Points.Clear();
            double totalTimeMs = audioData.Length * 1000.0 / sampleRate;
            float maxAmplitude = 0f;
            for (int i = 0; i < processedData.Length; i++)
            {
                double timeMs = i * 1000.0 / sampleRate;
                timeLineSeries.Points.Add(new DataPoint(timeMs, processedData[i]));
                float absAmp = Math.Abs(processedData[i]);
                if (absAmp > maxAmplitude) maxAmplitude = absAmp;
            }
            
            timeXAxis.Maximum = totalTimeMs;
            timeXAxis.Minimum = 0;
            if (maxAmplitude < 0.01f) maxAmplitude = 0.01f;
            timeYAxis.Maximum = maxAmplitude * 1.1;
            timeYAxis.Minimum = -maxAmplitude * 1.1;

            timePlotModel.InvalidatePlot(true);
            
            MathNet.Numerics.Complex32[] complexData = new MathNet.Numerics.Complex32[processedData.Length];
            for (int i = 0; i < processedData.Length; i++)
            {
                complexData[i] = new MathNet.Numerics.Complex32(processedData[i], 0);
            }
            
            Fourier.Forward(complexData, FourierOptions.Matlab);
            
            freqLineSeries.Points.Clear();
            int n = processedData.Length / 2;
            float frequencyStep = sampleRate / (float)processedData.Length;
            float maxFrequency = 0f;
            float maxMagnitudeDB = -60f;
            
            for (int i = 0; i < n; i++)
            {
                float magnitude = complexData[i].Magnitude;
                if (magnitude > 0)
                {
                    float dB = 20 * (float)Math.Log10(magnitude);
                    if (dB > maxMagnitudeDB) maxMagnitudeDB = dB;
                    if (dB > -60)
                    {
                        float frequency = i * frequencyStep;
                        if (frequency > maxFrequency) maxFrequency = frequency;
                    }
                }
            }
            
            maxFrequency = maxFrequency == 0 ? 1000 : maxFrequency * 2;
            maxFrequency = Math.Min(maxFrequency, sampleRate / 2);
            int maxIndex = (int)(maxFrequency / frequencyStep);
            maxIndex = Math.Min(maxIndex, n);
            
            for (int i = 0; i < maxIndex; i++)
            {
                float frequency = i * frequencyStep;
                float magnitude = complexData[i].Magnitude;
                float dB = magnitude > 0 ? 20 * (float)Math.Log10(magnitude) : -60;
                dB = Math.Max(dB, -60);
                freqLineSeries.Points.Add(new DataPoint(frequency, dB));
            }
            
            freqXAxis.Maximum = maxFrequency;
            freqXAxis.Minimum = 0;
            freqYAxis.Maximum = maxMagnitudeDB == -60 ? 0 : maxMagnitudeDB + 10;
            freqYAxis.Minimum = -60;

            freqPlotModel.InvalidatePlot(true);
        }
    }
}