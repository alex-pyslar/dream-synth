using System;
using System.Linq;
using NAudio.Wave;

namespace DreamSynth
{
    public enum WaveType
    {
        Sine,
        Square,
        Triangle
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
            int midiNoteNumber = 11 - (int)pitch + (octave * 12);
            Frequency = (float)(440 * Math.Pow(2, (midiNoteNumber - 69) / 12.0));
            Amplitude = amplitude;
        }
    }
}