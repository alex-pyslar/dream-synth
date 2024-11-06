using System;
using System.Linq;
using NAudio.Wave;

namespace DreamSynth
{
    public enum WaveType
    {
        Sine,    // Синусоидальная волна
        Square,  // Прямоугольная волна
        Triangle // Треугольная волна
    }

    public class Wave
    {
        public WaveType Type { get; set; }
        public float Frequency { get; set; }
        public float Amplitude { get; set; }

        public Wave(WaveType type, float frequency, float amplitude)
        {
            Type = type;
            Frequency = frequency;
            Amplitude = amplitude;
        }

        public void Set(WaveType type, int octave, float pitch, float amplitude)
        {
            Type = type;

            // Получаем MIDI номер для ноты с учетом pitch и октавы
            int midiNoteNumber = 11 - (int)pitch + (octave * 12); // Учитываем октаву

            // Вычисляем частоту на основе MIDI номера
            Frequency = (float)(440 * Math.Pow(2, (midiNoteNumber - 69) / 12.0)); // A4 = 440 Гц
            Amplitude = amplitude;
        }


    }
}