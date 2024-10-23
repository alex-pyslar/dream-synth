using System;
using System.Data;
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

        // Метод преобразования pitch в частоту
        private float PitchToFrequency(int pitch, int octave)
        {
            // Устанавливаем базовые частоты для ноты A (440 Гц)
            float baseFrequency = 440.0f; // A4
            int midiNoteNumber = (pitch + (octave * 7)) + 57; // Преобразуем в MIDI-номер, A4 (octave 4) = 69
            return baseFrequency * (float)Math.Pow(2.0, (midiNoteNumber - 69) / 12.0);
        }

        // Метод обновления параметров генерируемой волны
        private void Update()
        {
            int octave1 = Octave1ComboBox.SelectedIndex;
            int octave2 = Octave2ComboBox.SelectedIndex;
            int octave3 = Octave3ComboBox.SelectedIndex;

            WaveGenerator.Waves[0].Set(
                (WaveType)WaveType1ComboBox.SelectedIndex,
                octave1,
                MidiEditorControl.Notes[0].Pitch, // Используем pitch от 0 до 7
                (float)AmplitudeSlider.Value);

            WaveGenerator.Waves[1].Set(
                (WaveType)WaveType2ComboBox.SelectedIndex,
                octave2,
                MidiEditorControl.Notes[0].Pitch, // Используем pitch от 0 до 7
                (float)AmplitudeSlider.Value);

            WaveGenerator.Waves[2].Set(
                (WaveType)WaveType3ComboBox.SelectedIndex,
                octave3,
                MidiEditorControl.Notes[0].Pitch, // Используем pitch от 0 до 7
                (float)AmplitudeSlider.Value);
        }

    }
}
