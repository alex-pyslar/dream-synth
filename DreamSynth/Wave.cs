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

        public void Set(WaveType type, int octave, int note, float amplitude)
        {
            Type = type;

            Frequency = (float)(440 * Math.Pow(2, (note - 9 + (octave - 4) * 12) / 12.0));
            
            Amplitude = amplitude;
        }
    }
}