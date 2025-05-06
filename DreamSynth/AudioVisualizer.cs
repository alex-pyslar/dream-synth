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

            // Настройка временного графика
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

            // Настройка частотного графика
            freqPlotModel.Title = null;
            freqPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 5000, // Ограничиваем до 5 кГц
                Title = "Частота (Гц)"
            });
            freqPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 10, // Для логарифмического масштаба амплитуд
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
            // Обновление временного графика
            timeLineSeries.Points.Clear();
            for (int i = 0; i < audioData.Length; i++)
            {
                timeLineSeries.Points.Add(new DataPoint(i, audioData[i]));
            }
            timePlotModel.InvalidatePlot(true);

            // Преобразование в Complex32[]
            MathNet.Numerics.Complex32[] complexData = new MathNet.Numerics.Complex32[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                complexData[i] = new MathNet.Numerics.Complex32(audioData[i], 0);
            }

            // Выполнение FFT
            Fourier.Forward(complexData, FourierOptions.Matlab);

            // Обновление частотного графика
            freqLineSeries.Points.Clear();
            int n = audioData.Length / 2; // Только первая половина спектра
            float frequencyStep = sampleRate / (float)audioData.Length; // Шаг частоты в Гц
            float maxFrequency = 5000; // Ограничиваем до 5 кГц
            int maxIndex = (int)(maxFrequency / frequencyStep); // Индекс для ограничения частот
            maxIndex = Math.Min(maxIndex, n); // Не превышаем половину спектра

            // Нормализация амплитуд
            float maxMagnitude = 0;
            for (int i = 0; i < maxIndex; i++)
            {
                float magnitude = complexData[i].Magnitude;
                if (magnitude > maxMagnitude) maxMagnitude = magnitude;
            }

            for (int i = 0; i < maxIndex; i++)
            {
                float frequency = i * frequencyStep; // Частота в Гц
                float magnitude = complexData[i].Magnitude;
                // Логарифмическое масштабирование амплитуды для лучшей видимости
                float scaledMagnitude = maxMagnitude > 0 ? 10 * (float)Math.Log10(1 + magnitude / maxMagnitude) : 0;
                freqLineSeries.Points.Add(new DataPoint(frequency, scaledMagnitude));
            }

            // Настройка осей частотного графика
            freqPlotModel.Axes[0].Maximum = maxFrequency; // Ограничение по частоте
            freqPlotModel.Axes[1].Maximum = 10; // Для масштабированных амплитуд
            freqPlotModel.InvalidatePlot(true);
        }
    }
}