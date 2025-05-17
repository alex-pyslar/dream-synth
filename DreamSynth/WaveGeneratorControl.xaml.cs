using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DreamSynth
{
    
    public partial class WaveGeneratorControl : UserControl
    {
        public static int counter = 0;
        
        public static WaveGenerator WaveGenerator;
        
        private bool _isUpdating = false;

        public WaveGeneratorControl()
        {
            InitializeComponent();
            
            AmplitudeSlider.ValueChanged += Slider_ValueChanged;

            WaveType1ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType2ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            WaveType3ComboBox.SelectionChanged += ComboBox_SelectionChanged;

            Octave1ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            Octave2ComboBox.SelectionChanged += ComboBox_SelectionChanged;
            Octave3ComboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        public void InitializeWaveGenerator(Equalizer equalizer)
        {
            WaveGenerator = new WaveGenerator(new Wave[]
            {
                new Wave(WaveType.Sine, 440, 0.5f),
                new Wave(WaveType.Sine, 440, 0.5f),
                new Wave(WaveType.Sine, 440, 0.5f)
            }, equalizer);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AmplitudeTextBlock.Text = AmplitudeSlider.Value.ToString("F1");
            Update();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Update();
        }

        public async void Update()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            while (true)
            {
                var notes = MidiEditorControl.Notes.ToList();
                counter++;

                if (counter > 1600) counter = 0;

                var notesToPlay = notes
                    .Where(note => counter >= note.StartTime * 50 && counter < (note.StartTime + note.Duration) * 50)
                    .ToList();

                if (notesToPlay.Any())
                {
                    for (int i = 0; i < WaveGenerator.Waves.Length; i++)
                    {
                        var note = notesToPlay[i % notesToPlay.Count];

                        WaveGenerator.Waves[i].Set(
                            (WaveType)GetWaveTypeFromComboBox(i),
                            GetOctaveFromComboBox(i),
                            note.Pitch,
                            (float)AmplitudeSlider.Value
                        );
                    }
                }
                else
                {
                    foreach (var wave in WaveGenerator.Waves)
                    {
                        wave.Set(WaveType.Sine, 0, 0, 0);
                    }
                }
                await Task.Delay((int)MainWindow.interval);
            }
        }

        private WaveType GetWaveTypeFromComboBox(int waveIndex)
        {
            switch (waveIndex)
            {
                case 0:
                    return (WaveType)WaveType1ComboBox.SelectedIndex;
                case 1:
                    return (WaveType)WaveType2ComboBox.SelectedIndex;
                case 2:
                    return (WaveType)WaveType3ComboBox.SelectedIndex;
                default:
                    return WaveType.Sine;
            }
        }

        private int GetOctaveFromComboBox(int waveIndex)
        {
            switch (waveIndex)
            {
                case 0:
                    return Octave1ComboBox.SelectedIndex;
                case 1:
                    return Octave2ComboBox.SelectedIndex;
                case 2:
                    return Octave3ComboBox.SelectedIndex;
                default:
                    return 0;
            }
        }
    }
}