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

        // Метод обновления параметров генерируемой волны
        private void Update()
        {
            WaveGenerator.Waves[0].Set(
                (WaveType)WaveType1ComboBox.SelectedIndex,
                Octave1ComboBox.SelectedIndex,
                NoteComboBox.SelectedIndex,
                (float)AmplitudeSlider.Value);
            WaveGenerator.Waves[1].Set(
                (WaveType)WaveType2ComboBox.SelectedIndex,
                Octave2ComboBox.SelectedIndex,
                NoteComboBox.SelectedIndex,
                (float)AmplitudeSlider.Value);
            WaveGenerator.Waves[2].Set(
                (WaveType)WaveType2ComboBox.SelectedIndex,
                Octave3ComboBox.SelectedIndex,
                NoteComboBox.SelectedIndex,
                (float)AmplitudeSlider.Value);
        }
    }
}
