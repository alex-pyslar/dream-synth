using NAudio.Wave;
using System;
using System.Linq;

namespace DreamSynth
{
    // Класс генератора волн, наследующийся от WaveProvider32
    public class WaveGenerator: WaveProvider32
    {
        public Wave[] Waves = new Wave[3];

        // Конструктор класса, принимающий параметры для трех волн
        public WaveGenerator(Wave[] waves)
        {
            Waves = waves;
        }
        
        // Событие, вызываемое при генерации сэмплов
        public event Action<float[]> OnSampleGenerated;

        // Переопределенный метод для чтения сэмплов
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate; // Получаем частоту дискретизации

            for (int i = 0; i < sampleCount; i++)
            {
                double time = (double)(i + offset) / sampleRate; // Рассчитываем время текущего сэмпла
                // Генерируем сэмплы для каждой из трех волн
                float sample1 = GenerateWaveSample(Waves[0], time);
                float sample2 = GenerateWaveSample(Waves[1], time);
                float sample3 = GenerateWaveSample(Waves[2], time);
                
                // Суммируем сэмплы трех волн
                float combinedSample = sample1 + sample2 + sample3;
                
                buffer[offset + i] = combinedSample;
            }

            // Вызываем событие генерации сэмплов
            OnSampleGenerated?.Invoke(buffer.Take(sampleCount).ToArray());

            return sampleCount; // Возвращаем количество сэмплов
        }
        
        private float GenerateWaveSample(Wave wave, double time)
        {
            float sample = 0;
            switch (wave.Type)
            {
                case WaveType.Sine:
                    // Генерация синусоидальной волны
                    sample = wave.Amplitude * (float)Math.Sin(2 * Math.PI * wave.Frequency * time);
                    break;
                case WaveType.Square:
                    // Генерация прямоугольной волны
                    sample = wave.Amplitude * Math.Sign(Math.Sin(2 * Math.PI * wave.Frequency * time));
                    break;
                case WaveType.Triangle:
                    // Генерация треугольной волны
                    sample = wave.Amplitude * (float)(2 * Math.Abs(2 * ((time * wave.Frequency) % 1) - 1) - 1);
                    break;
            }
            return sample;
        }
    }
}