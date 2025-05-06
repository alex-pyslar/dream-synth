using NAudio.Wave;
using System;
using System.Linq;

namespace DreamSynth
{
    public class WaveGenerator : WaveProvider32
    {
        public Wave[] Waves = new Wave[3];
        private double phase1, phase2, phase3; // Фазы для каждой волны
        private const float SAFETY_FACTOR = 0.8f; // Коэффициент для предотвращения клиппинга
        private readonly Equalizer equalizer;

        public WaveGenerator(Wave[] waves, Equalizer equalizer)
        {
            Waves = waves;
            this.equalizer = equalizer;
            phase1 = phase2 = phase3 = 0.0;
        }

        public event Action<float[]> OnSampleGenerated;

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            double sampleRateD = (double)sampleRate;

            for (int i = 0; i < sampleCount; i++)
            {
                // Генерируем сэмплы для каждой волны с использованием фаз
                float sample1 = GenerateWaveSample(Waves[0], ref phase1, sampleRateD);
                float sample2 = GenerateWaveSample(Waves[1], ref phase2, sampleRateD);
                float sample3 = GenerateWaveSample(Waves[2], ref phase3, sampleRateD);

                // Микшируем сэмплы с применением коэффициента безопасности
                float combinedSample = (sample1 + sample2 + sample3) * SAFETY_FACTOR / 3.0f;
                
                // Мягкое ограничение амплитуды
                combinedSample = SoftClip(combinedSample);
                
                // Применяем эквалайзер и дисторшн
                combinedSample = equalizer.ProcessSample(combinedSample);
                
                buffer[offset + i] = combinedSample;
            }

            OnSampleGenerated?.Invoke(buffer.Take(sampleCount).ToArray());
            return sampleCount;
        }

        private float GenerateWaveSample(Wave wave, ref double phase, double sampleRate)
        {
            float sample = 0;
            double frequency = wave.Frequency;
            float amplitude = wave.Amplitude;

            switch (wave.Type)
            {
                case WaveType.Sine:
                    sample = amplitude * (float)Math.Sin(phase);
                    break;
                case WaveType.Square:
                    sample = amplitude * (Math.Sin(phase) >= 0 ? 0.9f : -0.9f);
                    break;
                case WaveType.Triangle:
                    sample = amplitude * (float)(2.0 * (Math.Abs(phase / Math.PI - 
                        Math.Floor(phase / Math.PI + 0.5)) - 0.5));
                    break;
            }

            // Обновляем фазу
            phase += 2.0 * Math.PI * frequency / sampleRate;
            if (phase >= 2.0 * Math.PI)
                phase -= 2.0 * Math.PI;

            return sample;
        }

        private float SoftClip(float sample)
        {
            // Мягкое ограничение для предотвращения резких переходов
            if (sample > 1.0f)
                return 1.0f - (1.0f / (sample + 1.0f));
            else if (sample < -1.0f)
                return -1.0f + (1.0f / (-sample + 1.0f));
            return sample;
        }
    }
}