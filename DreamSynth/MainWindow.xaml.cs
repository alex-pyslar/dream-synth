using OxyPlot; // Подключение библиотеки для построения графиков
using System.Windows; // Подключение библиотеки для работы с элементами интерфейса WPF
using NAudio.Wave; // Подключение библиотеки для работы с аудио в .NET

namespace DreamSynth // Объявляем пространство имен для проекта синтезатора
{
    public partial class MainWindow : Window
    {
        // Поля для хранения объектов, управляющих аудио и визуализацией
        private WaveOutEvent waveOut;
        private WaveGenerator waveGenerator;
        private Equalizer equalizer;
        private AudioVisualizer audioVisualizer;

        // Конструктор основного окна приложения
        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов интерфейса
            InitializeAudioVisualizer(); // Инициализация визуализатора аудио

            // Привязка событий изменения значений слайдеров к методам-обработчикам
            Frequency1Slider.ValueChanged += FrequencySlider_ValueChanged;
            Amplitude1Slider.ValueChanged += AmplitudeSlider_ValueChanged;
            Frequency2Slider.ValueChanged += FrequencySlider_ValueChanged;
            Amplitude2Slider.ValueChanged += AmplitudeSlider_ValueChanged;
            Frequency3Slider.ValueChanged += FrequencySlider_ValueChanged;
            Amplitude3Slider.ValueChanged += AmplitudeSlider_ValueChanged;
            LowGainSlider.ValueChanged += EqualizerSlider_ValueChanged;
            MidGainSlider.ValueChanged += EqualizerSlider_ValueChanged;
            HighGainSlider.ValueChanged += EqualizerSlider_ValueChanged;
            LowFreqSlider.ValueChanged += EqualizerFrequencySlider_ValueChanged;
            HighFreqSlider.ValueChanged += EqualizerFrequencySlider_ValueChanged;
        }

        // Метод инициализации визуализатора аудио
        private void InitializeAudioVisualizer()
        {
            var plotModel = new PlotModel { Title = "Audio Signal" }; // Создание модели графика с заголовком
            audioVisualizer = new AudioVisualizer(plotModel); // Создание объекта визуализатора с моделью графика
            plotView.Model = plotModel; // Установка модели графика в элемент отображения
        }

        // Обработчик изменения частоты на любом из слайдеров
        private void FrequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновление текстовых блоков для отображения текущих значений частот
            Frequency1TextBlock.Text = Frequency1Slider.Value.ToString("F0");
            Frequency2TextBlock.Text = Frequency2Slider.Value.ToString("F0");
            Frequency3TextBlock.Text = Frequency3Slider.Value.ToString("F0");
            UpdateWave(); // Обновление параметров генерируемой волны
        }

        // Обработчик изменения амплитуды на любом из слайдеров
        private void AmplitudeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновление текстовых блоков для отображения текущих значений амплитуд
            Amplitude1TextBlock.Text = Amplitude1Slider.Value.ToString("F1");
            Amplitude2TextBlock.Text = Amplitude2Slider.Value.ToString("F1");
            Amplitude3TextBlock.Text = Amplitude3Slider.Value.ToString("F1");
            UpdateWave(); // Обновление параметров генерируемой волны
        }

        // Обработчик изменения уровней усиления эквалайзера
        private void EqualizerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновление текстовых блоков для отображения текущих значений усиления
            LowGainTextBlock.Text = LowGainSlider.Value.ToString("F1");
            MidGainTextBlock.Text = MidGainSlider.Value.ToString("F1");
            HighGainTextBlock.Text = HighGainSlider.Value.ToString("F1");
            UpdateEqualizer(); // Обновление параметров эквалайзера
        }

        // Обработчик изменения частот эквалайзера
        private void EqualizerFrequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновление текстовых блоков для отображения текущих значений частот
            LowFreqTextBlock.Text = LowFreqSlider.Value.ToString("F0");
            HighFreqTextBlock.Text = HighFreqSlider.Value.ToString("F0");
            UpdateEqualizer(); // Обновление параметров эквалайзера
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

            // Получение выбранных типов волн из комбобоксов
            var waveType1 = (WaveType)WaveType1ComboBox.SelectedIndex;
            var waveType2 = (WaveType)WaveType2ComboBox.SelectedIndex;
            var waveType3 = (WaveType)WaveType3ComboBox.SelectedIndex;

            // Получение текущих значений частот и амплитуд из слайдеров
            float frequency1 = (float)Frequency1Slider.Value;
            float amplitude1 = (float)Amplitude1Slider.Value;
            float frequency2 = (float)Frequency2Slider.Value;
            float amplitude2 = (float)Amplitude2Slider.Value;
            float frequency3 = (float)Frequency3Slider.Value;
            float amplitude3 = (float)Amplitude3Slider.Value;

            // Создание генератора волн с указанными параметрами
            waveGenerator = new WaveGenerator(
                waveType1, frequency1, amplitude1,
                waveType2, frequency2, amplitude2,
                waveType3, frequency3, amplitude3
            );

            // Получение текущих значений усиления из слайдеров
            float lowGain = (float)LowGainSlider.Value;
            float midGain = (float)MidGainSlider.Value;
            float highGain = (float)HighGainSlider.Value;

            // Получение текущих значений частот из слайдеров
            float lowFrequency = (float)LowFreqSlider.Value;
            float highFrequency = (float)HighFreqSlider.Value;

            // Создание эквалайзера с указанными параметрами
            equalizer = new Equalizer(lowGain, midGain, highGain);
            equalizer.SetLowFrequency(lowFrequency);
            equalizer.SetHighFrequency(highFrequency);

            // Установка эквалайзера в генераторе волн
            waveGenerator.SetEqualizer(equalizer);

            // Инициализация и запуск воспроизведения аудио
            waveOut = new WaveOutEvent();
            waveOut.Init(waveGenerator);
            waveOut.Play();

            // Запуск визуализации аудио
            waveGenerator.OnSampleGenerated += (samples) =>
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

        // Метод обновления параметров генерируемой волны
        private void UpdateWave()
        {
            if (waveGenerator != null)
            {
                waveGenerator.SetWave1(
                    (WaveType)WaveType1ComboBox.SelectedIndex,
                    (float)Frequency1Slider.Value,
                    (float)Amplitude1Slider.Value
                );
                waveGenerator.SetWave2(
                    (WaveType)WaveType2ComboBox.SelectedIndex,
                    (float)Frequency2Slider.Value,
                    (float)Amplitude2Slider.Value
                );
                waveGenerator.SetWave3(
                    (WaveType)WaveType3ComboBox.SelectedIndex,
                    (float)Frequency3Slider.Value,
                    (float)Amplitude3Slider.Value
                );
            }
        }

        // Метод обновления параметров эквалайзера
        private void UpdateEqualizer()
        {
            if (waveGenerator != null)
            {
                // Установка новых значений усиления
                equalizer.SetLowGain((float)LowGainSlider.Value);
                equalizer.SetMidGain((float)MidGainSlider.Value);
                equalizer.SetHighGain((float)HighGainSlider.Value);
                // Установка новых значений частот
                equalizer.SetLowFrequency((float)LowFreqSlider.Value);
                equalizer.SetHighFrequency((float)HighFreqSlider.Value);

                // Применение эквалайзера к генератору волн
                waveGenerator.SetEqualizer(equalizer);
            }
        }
    }
}