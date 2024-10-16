using System;
using System.ComponentModel;
using OxyPlot;
using System.Windows;
using NAudio.Wave;

namespace DreamSynth
{
    public partial class MainWindow : Window
    {
        private WaveOutEvent waveOut;
        private AudioVisualizer audioVisualizer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAudioVisualizer();
            
            waveOut = new WaveOutEvent();
            waveOut.Init(WaveGeneratorControl.WaveGenerator);
            waveOut.DesiredLatency = 50; // Уменьшение задержки

            WaveGeneratorControl.WaveGenerator.OnSampleGenerated += samples =>
            {
                audioVisualizer.Update(samples);
            };
        }

        private void InitializeAudioVisualizer()
        {
            var plotModel = new PlotModel { Title = "Audio Signal" };
            audioVisualizer = new AudioVisualizer(plotModel);
            plotView.Model = plotModel;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Stop();
        }
    }
}