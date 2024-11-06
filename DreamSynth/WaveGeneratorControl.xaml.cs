using System;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
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
            
            while (true) // Бесконечный цикл для повторения проигрывания
            {
                // Получаем отсортированный список нот по времени начала
                var notes = MidiEditorControl.Notes.OrderBy(n => n.StartTime).ToList();
                for (int i = 0; i < notes.Count; i++)
                {
                    // Рассчитываем время проигрывания ноты на основе ее длительности
                    double noteDuration = notes[i].Duration * 500; // Время длительности ноты в миллисекундах
                    // Установка параметров для каждой волны
                    WaveGenerator.Waves[0].Set(
                        (WaveType)WaveType1ComboBox.SelectedIndex,
                        Octave1ComboBox.SelectedIndex,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    WaveGenerator.Waves[1].Set(
                        (WaveType)WaveType2ComboBox.SelectedIndex,
                        Octave2ComboBox.SelectedIndex,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    WaveGenerator.Waves[2].Set(
                        (WaveType)WaveType3ComboBox.SelectedIndex,
                        Octave3ComboBox.SelectedIndex,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    // Здесь добавляем задержку, учитывая длительность ноты
                    await Task.Delay((int)noteDuration); // Задержка в миллисекундах на основе длительности ноты
                }
            }
            
        }
    }
}
