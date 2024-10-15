using System;
using System.ComponentModel;
using OxyPlot; // Подключение библиотеки для построения графиков
using System.Windows; // Подключение библиотеки для работы с элементами интерфейса WPF
using NAudio.Wave; // Подключение библиотеки для работы с аудио в .NET

namespace DreamSynth // Объявляем пространство имен для проекта синтезатора
{
    public partial class MainWindow : Window
    {
        // Поля для хранения объектов, управляющих аудио и визуализацией
        private WaveOutEvent waveOut;
        private AudioVisualizer audioVisualizer;

        // Конструктор основного окна приложения
        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов интерфейса
            InitializeAudioVisualizer(); // Инициализация визуализатора аудио
        }

        // Метод инициализации визуализатора аудио
        private void InitializeAudioVisualizer()
        {
            var plotModel = new PlotModel { Title = "Audio Signal" }; // Создание модели графика с заголовком
            audioVisualizer = new AudioVisualizer(plotModel); // Создание объекта визуализатора с моделью графика
            plotView.Model = plotModel; // Установка модели графика в элемент отображения
        }

        // Обработчик нажатия кнопки воспроизведения
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Остановка и освобождение ресурса waveOut, если он уже используется
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            // Инициализация и запуск воспроизведения аудио
            waveOut = new WaveOutEvent();
            waveOut.Init(WaveGeneratorControl.WaveGenerator);
            waveOut.Play();

            // Запуск визуализации аудио
            WaveGeneratorControl.WaveGenerator.OnSampleGenerated += (samples) =>
            {
                audioVisualizer.Update(samples);
            };
        }

        // Обработчик нажатия кнопки остановки воспроизведения
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Остановка и освобождение ресурса waveOut, если он используется
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
        }
    }
}