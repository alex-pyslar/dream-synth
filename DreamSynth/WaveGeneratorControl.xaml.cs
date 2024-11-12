using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DreamSynth
{
    public partial class WaveGeneratorControl : UserControl
    {
        public static WaveGenerator WaveGenerator = new WaveGenerator(new Wave[]
        {
            new Wave(WaveType.Sine, 440, 0.5f),
            new Wave(WaveType.Sine, 440, 0.5f),
            new Wave(WaveType.Sine, 440, 0.5f)
        });

        // Флаг для отслеживания, выполняется ли обновление
        private bool _isUpdating = false;

        public WaveGeneratorControl()
        {
            InitializeComponent();

            // Привязка событий изменения значений слайдеров к методам-обработчикам
            AmplitudeSlider.ValueChanged += Slider_ValueChanged;

            // Привязка событий изменения выбранного элемента ComboBox к методам-обработчикам
            WaveType1ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType2ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType3ComboBox.SelectionChanged += ComboBox_SelectionChanged;

            Octave1ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            Octave2ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            Octave3ComboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AmplitudeTextBlock.Text = AmplitudeSlider.Value.ToString("F1");
            Update(); // Обновление параметров генерируемой волны
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Update(); // Обновление параметров генерируемой волны
        }

        // Метод обновления параметров генерируемой волны
        private async void Update()
        {
            // Проверяем, выполняется ли обновление в данный момент
            if (_isUpdating) return;

            _isUpdating = true; // Устанавливаем флаг обновления

            int counter = 0; // Счётчик времени

            while (true) // Бесконечный цикл для повторения проигрывания
            {
                // Получаем список нот
                var notes = MidiEditorControl.Notes.ToList();

                // Обновляем счётчик времени
                counter++;

                // Сбрасываем счётчик, если он достиг 16000
                if (counter > 400) counter = 0;

                // Ищем все ноты, которые должны проигрываться в данный момент
                var notesToPlay = notes.Where(note =>
                    counter >= note.StartTime * 50 && counter < (note.StartTime + note.Duration) * 50).ToList();

                if (notesToPlay.Any()) // Если есть ноты для проигрывания
                {
                    foreach (var note in notesToPlay)
                    {
                        // Установка параметров для каждой волны
                        WaveGenerator.Waves[0].Set(
                            (WaveType)WaveType1ComboBox.SelectedIndex,
                            Octave1ComboBox.SelectedIndex,
                            note.Pitch,
                            (float)AmplitudeSlider.Value);

                        WaveGenerator.Waves[1].Set(
                            (WaveType)WaveType2ComboBox.SelectedIndex,
                            Octave2ComboBox.SelectedIndex,
                            note.Pitch,
                            (float)AmplitudeSlider.Value);

                        WaveGenerator.Waves[2].Set(
                            (WaveType)WaveType3ComboBox.SelectedIndex,
                            Octave3ComboBox.SelectedIndex,
                            note.Pitch,
                            (float)AmplitudeSlider.Value);

                        // Подождём 1 миллисекунду перед проигрыванием следующей ноты
                        await Task.Delay(1);
                    }
                }
                else // Если нет нот для проигрывания, устанавливаем амплитуду в 0
                {
                    WaveGenerator.Waves[0].Set(
                        (WaveType)WaveType1ComboBox.SelectedIndex,
                        Octave1ComboBox.SelectedIndex,
                        0,
                        0);

                    WaveGenerator.Waves[1].Set(
                        (WaveType)WaveType2ComboBox.SelectedIndex,
                        Octave2ComboBox.SelectedIndex,
                        0,
                        0);

                    WaveGenerator.Waves[2].Set(
                        (WaveType)WaveType3ComboBox.SelectedIndex,
                        Octave3ComboBox.SelectedIndex,
                        0,
                        0);
                }
                // Задержка 1 миллисекунда перед следующим циклом
                await Task.Delay(1);
            }
        }
    }
}
