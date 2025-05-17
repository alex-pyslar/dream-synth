using System;
using System.Windows;
using System.Windows.Controls;

namespace DreamSynth
{
    public partial class EqualizerControl : UserControl
    {
        public Equalizer Equalizer { get; private set; }
        
        public bool IsModulationEnabled => ModulationToggle.IsChecked ?? false;
        
        public Button PlayBtn => PlayButton;
        public Button StopBtn => StopButton;
        
        public EqualizerControl()
        {
            InitializeComponent();
            Equalizer = new Equalizer(44100);
            ModulationToggle.Checked += ModulationToggle_CheckedChanged;
            ModulationToggle.Unchecked += ModulationToggle_CheckedChanged;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Equalizer.LowGain = (float)LowGainSlider.Value;
            Equalizer.MidGain = (float)MidGainSlider.Value;
            Equalizer.HighGain = (float)HighGainSlider.Value;
            Equalizer.DistortionAmount = (float)DistortionSlider.Value;

            LowGainTextBlock.Text = $"{LowGainSlider.Value:F1} dB";
            MidGainTextBlock.Text = $"{MidGainSlider.Value:F1} dB";
            HighGainTextBlock.Text = $"{HighGainSlider.Value:F1} dB";
            DistortionTextBlock.Text = $"{DistortionSlider.Value:F1}";

            Equalizer.UpdateFilters(IsModulationEnabled);
        }
        
        private void ModulationToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Equalizer.UpdateFilters(IsModulationEnabled);
        }
    }
}