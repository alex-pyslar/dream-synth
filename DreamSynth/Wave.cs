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

            // Преобразование pitch от 0-7 в MIDI номер
            float midiNoteNumber = pitch + (octave * 7) + 57; // A4 = MIDI номер 69
            
            // Вычисляем частоту на основе MIDI номера
            Frequency = (float)(440 * Math.Pow(2, (midiNoteNumber - 69) / 12.0));
            Amplitude = amplitude;
        }
    }
}