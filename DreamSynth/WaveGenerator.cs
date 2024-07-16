using NAudio.Wave; // Подключаем библиотеку NAudio для работы со звуковыми волнами
using System; // Подключаем базовую библиотеку System
using System.Linq; // Подключаем библиотеку Linq для удобной работы с коллекциями

namespace DreamSynth // Объявляем пространство имен для проекта синтезатора
{
    // Перечисление типов волны
    public enum WaveType
    {
        Sine,    // Синусоидальная волна
        Square,  // Прямоугольная волна
        Triangle // Треугольная волна
    }

    // Класс генератора волн, наследующийся от WaveProvider32
    public class WaveGenerator : WaveProvider32
    {
        // Определяем три кортежа для хранения типа волны, частоты и амплитуды
        private (WaveType waveType, float frequency, float amplitude) wave1;
        private (WaveType waveType, float frequency, float amplitude) wave2;
        private (WaveType waveType, float frequency, float amplitude) wave3;

        // Эквалайзер для применения к волнам
        private Equalizer equalizer;

        // Конструктор класса, принимающий параметры для трех волн
        public WaveGenerator(
            WaveType waveType1, float frequency1, float amplitude1,
            WaveType waveType2, float frequency2, float amplitude2,
            WaveType waveType3, float frequency3, float amplitude3)
        {
            // Инициализируем волны переданными параметрами
            wave1 = (waveType1, frequency1, amplitude1);
            wave2 = (waveType2, frequency2, amplitude2);
            wave3 = (waveType3, frequency3, amplitude3);
        }

        // Метод для настройки первой волны
        public void SetWave1(WaveType waveType, float frequency, float amplitude)
        {
            wave1 = (waveType, frequency, amplitude);
        }

        // Метод для настройки второй волны
        public void SetWave2(WaveType waveType, float frequency, float amplitude)
        {
            wave2 = (waveType, frequency, amplitude);
        }

        // Метод для настройки третьей волны
        public void SetWave3(WaveType waveType, float frequency, float amplitude)
        {
            wave3 = (waveType, frequency, amplitude);
        }

        // Метод для установки эквалайзера
        public void SetEqualizer(Equalizer equalizer)
        {
            this.equalizer = equalizer;
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
                float sample1 = GenerateWaveSample(wave1.waveType, wave1.frequency, wave1.amplitude, time);
                float sample2 = GenerateWaveSample(wave2.waveType, wave2.frequency, wave2.amplitude, time);
                float sample3 = GenerateWaveSample(wave3.waveType, wave3.frequency, wave3.amplitude, time);

                // Суммируем сэмплы трех волн
                float combinedSample = sample1 + sample2 + sample3;

                // Применяем эквалайзер, если он установлен
                if (equalizer != null)
                {
                    combinedSample = equalizer.Apply(combinedSample, Math.Max(wave1.frequency, Math.Max(wave2.frequency, wave3.frequency)));
                }

                // Записываем комбинированный сэмпл в буфер
                buffer[offset + i] = combinedSample;
            }

            // Вызываем событие генерации сэмплов
            OnSampleGenerated?.Invoke(buffer.Take(sampleCount).ToArray());

            return sampleCount; // Возвращаем количество сэмплов
        }

        // Метод для генерации сэмпла в зависимости от типа волны
        private float GenerateWaveSample(WaveType waveType, float frequency, float amplitude, double time)
        {
            float sample = 0;
            switch (waveType)
            {
                case WaveType.Sine:
                    // Генерация синусоидальной волны
                    sample = amplitude * (float)Math.Sin(2 * Math.PI * frequency * time);
                    break;
                case WaveType.Square:
                    // Генерация прямоугольной волны
                    sample = amplitude * Math.Sign(Math.Sin(2 * Math.PI * frequency * time));
                    break;
                case WaveType.Triangle:
                    // Генерация треугольной волны
                    sample = amplitude * (float)(2 * Math.Abs(2 * ((time * frequency) % 1) - 1) - 1);
                    break;
            }
            return sample;
        }
    }
}