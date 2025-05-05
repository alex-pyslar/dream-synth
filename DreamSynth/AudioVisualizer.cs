using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics; // Для Complex32
using System.Numerics; // Только если нужно где-то ещё (не используется в FFT)

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
                Maximum = 500,
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

        public void Update(float[] audioData)
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
            for (int i = 0; i < n; i++)
            {
                float magnitude = complexData[i].Magnitude;
                freqLineSeries.Points.Add(new DataPoint(i, magnitude));
            }
            freqPlotModel.InvalidatePlot(true);
        }
    }
}
