using System;
using OxyPlot;
using System.Windows;
using NAudio.Wave;

namespace DreamSynth
{
    public partial class MainWindow : Window
    {
        public static WaveOutEvent waveOut;
        private AudioVisualizer audioVisualizer;
        public static int bpm;
        public static double interval;

        public MainWindow()
        {
            InitializeComponent();
            bpm = 120;
            InitializeAudioVisualizer();
            
            EqualizerControl.PlayBtn.Click += PlayButton_Click;
            EqualizerControl.StopBtn.Click += StopButton_Click;

            waveOut = new WaveOutEvent();
            WaveGeneratorControl.InitializeWaveGenerator(EqualizerControl.Equalizer);
            waveOut.Init(WaveGeneratorControl.WaveGenerator);

            WaveGeneratorControl.WaveGenerator.OnSampleGenerated += samples =>
            {
                audioVisualizer.Update(samples);
            };
        }

        private void InitializeAudioVisualizer()
        {
            var timeModel = new PlotModel { Title = "Временной сигнал" };
            var freqModel = new PlotModel { Title = "Частотный спектр" };
            audioVisualizer = new AudioVisualizer(timeModel, freqModel);

            TimePlotView.Model = timeModel;
            FreqPlotView.Model = freqModel;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Play();
            MidiEditorControl.StartPlayback();
            WaveGeneratorControl.counter = 0;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Stop();
            MidiEditorControl.StopPlayback();
            WaveGeneratorControl.counter = 0;
        }

        private void BpmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bpm = (int)e.NewValue;
            interval = 1200.0 / bpm;
            MidiEditorControl.BPM = bpm;
        }
    }
}