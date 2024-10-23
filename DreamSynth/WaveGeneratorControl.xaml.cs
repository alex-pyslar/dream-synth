using System;
using System.Data;
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

        public WaveGeneratorControl()
        {
            InitializeComponent();

            // Привязка событий изменения значений слайдеров к методам-обработчикам
            AmplitudeSlider.ValueChanged += Slider_ValueChanged;

            // Привязка событий изменения выбранного элемента ComboBox к методам-обработчикам
            WaveType1ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType2ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType3ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            NoteComboBox.SelectionChanged += ComboBox_SelectionChanged;
            
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
            int octave1 = Octave1ComboBox.SelectedIndex;
            int octave2 = Octave2ComboBox.SelectedIndex;
            int octave3 = Octave3ComboBox.SelectedIndex;

            while (true) // Бесконечный цикл для повторения проигрывания
            {
                var notes = MidiEditorControl.Notes.ToList();

                for (int i = 0; i < notes.Count; i++)
                {
                    // Установка параметров для каждой волны
                    WaveGenerator.Waves[0].Set(
                        (WaveType)WaveType1ComboBox.SelectedIndex,
                        octave1,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    WaveGenerator.Waves[1].Set(
                        (WaveType)WaveType2ComboBox.SelectedIndex,
                        octave2,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    WaveGenerator.Waves[2].Set(
                        (WaveType)WaveType3ComboBox.SelectedIndex,
                        octave3,
                        notes[i].Pitch, // Используем pitch от 0 до 7
                        (float)AmplitudeSlider.Value);

                    // Здесь добавляем задержку
                    await Task.Delay(500); // Задержка в 500 миллисекунд
                }
            }
        }


    }
}
