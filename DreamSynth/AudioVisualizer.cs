using OxyPlot; // Подключение библиотеки OxyPlot, которая используется для построения графиков
using OxyPlot.Axes; // Подключение пространств имен, связанных с осями графиков в OxyPlot
using OxyPlot.Series; // Подключение пространств имен, связанных с сериями данных в OxyPlot

namespace DreamSynth // Определение пространства имен для проекта DreamSynth
{
    public class AudioVisualizer // Определение публичного класса AudioVisualizer, который будет визуализировать аудио данные
    {
        private readonly PlotModel plotModel; // Поле для хранения модели графика OxyPlot
        private readonly LineSeries lineSeries; // Поле для хранения серии данных линии

        // Конструктор класса AudioVisualizer, принимающий объект PlotModel в качестве параметра
        public AudioVisualizer(PlotModel plotModel)
        {
            this.plotModel = plotModel; // Инициализация plotModel
            plotModel.Title = null; // Установка заголовка графика в null (без заголовка)

            // Добавление горизонтальной оси (временной оси) к модели графика
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom, // Расположение оси внизу графика
                Minimum = 0, // Минимальное значение оси
                Maximum = 1000, // Максимальное значение оси
                Title = "Время" // Заголовок оси
            });

            // Добавление вертикальной оси (амплитудной оси) к модели графика
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left, // Расположение оси слева графика
                Minimum = -3, // Минимальное значение оси
                Maximum = 3, // Максимальное значение оси
                Title = "Амплитуда" // Заголовок оси
            });

            // Инициализация lineSeries - объекта, представляющего серию данных линии
            this.lineSeries = new LineSeries
            {
                Color = OxyColors.Blue, // Установка цвета линии в синий
                LineStyle = LineStyle.Solid // Установка стиля линии как сплошной
            };

            // Добавление серии данных линии к модели графика
            plotModel.Series.Add(lineSeries);
        }

        // Метод для обновления графика новыми аудио данными
        public void Update(float[] audioData)
        {
            lineSeries.Points.Clear(); // Очистка текущих точек данных серии
            // Цикл по аудио данным для добавления каждой точки данных в серию линии
            for (int i = 0; i < audioData.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, audioData[i])); // Добавление новой точки данных
            }
            plotModel.InvalidatePlot(true); // Обновление графика для отображения новых данных
        }
    }
}
