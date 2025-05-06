using System;

namespace DreamSynth
{
    public class Equalizer
    {
        private readonly BiquadFilter lowShelf;
        private readonly BiquadFilter midPeak;
        private readonly BiquadFilter highShelf;
        private readonly Distortion distortion;
        private readonly int sampleRate;

        public float LowGain { get; set; } // dB
        public float MidGain { get; set; } // dB
        public float HighGain { get; set; } // dB
        public float DistortionAmount { get; set; } // 0 to 1

        public Equalizer(int sampleRate)
        {
            this.sampleRate = sampleRate;
            lowShelf = new BiquadFilter();
            midPeak = new BiquadFilter();
            highShelf = new BiquadFilter();
            distortion = new Distortion();
            UpdateFilters();
        }

        public void UpdateFilters()
        {
            // Low shelf: 200 Hz, Q=0.7
            lowShelf.SetLowShelf(200.0f, sampleRate, LowGain, 0.7f);
            // Mid peak: 1000 Hz, Q=1.0
            midPeak.SetPeaking(1000.0f, sampleRate, MidGain, 1.0f);
            // High shelf: 5000 Hz, Q=0.7
            highShelf.SetHighShelf(5000.0f, sampleRate, HighGain, 0.7f);
        }

        public float ProcessSample(float sample)
        {
            // Apply EQ bands
            sample = lowShelf.Process(sample);
            sample = midPeak.Process(sample);
            sample = highShelf.Process(sample);
            // Apply distortion
            sample = distortion.Process(sample, DistortionAmount);
            return sample;
        }
    }

    public class BiquadFilter
    {
        private float a0, a1, a2, b0, b1, b2;
        private float x1, x2, y1, y2;

        public void SetLowShelf(float frequency, float sampleRate, float gainDb, float Q)
        {
            float A = (float)Math.Pow(10.0, gainDb / 40.0);
            float w0 = 2.0f * (float)Math.PI * frequency / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2.0f * Q);

            float cosW0 = (float)Math.Cos(w0);
            float sqrtA = (float)Math.Sqrt(A);

            b0 = A * ((A + 1.0f) - (A - 1.0f) * cosW0 + 2.0f * sqrtA * alpha);
            b1 = 2.0f * A * ((A - 1.0f) - (A + 1.0f) * cosW0);
            b2 = A * ((A + 1.0f) - (A - 1.0f) * cosW0 - 2.0f * sqrtA * alpha);
            a0 = (A + 1.0f) + (A - 1.0f) * cosW0 + 2.0f * sqrtA * alpha;
            a1 = -2.0f * ((A - 1.0f) + (A + 1.0f) * cosW0);
            a2 = (A + 1.0f) + (A - 1.0f) * cosW0 - 2.0f * sqrtA * alpha;

            Normalize();
        }

        public void SetPeaking(float frequency, float sampleRate, float gainDb, float Q)
        {
            float A = (float)Math.Pow(10.0, gainDb / 40.0);
            float w0 = 2.0f * (float)Math.PI * frequency / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2.0f * Q);

            float cosW0 = (float)Math.Cos(w0);

            b0 = 1.0f + alpha * A;
            b1 = -2.0f * cosW0;
            b2 = 1.0f - alpha * A;
            a0 = 1.0f + alpha / A;
            a1 = -2.0f * cosW0;
            a2 = 1.0f - alpha / A;

            Normalize();
        }

        public void SetHighShelf(float frequency, float sampleRate, float gainDb, float Q)
        {
            float A = (float)Math.Pow(10.0, gainDb / 40.0);
            float w0 = 2.0f * (float)Math.PI * frequency / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2.0f * Q);

            float cosW0 = (float)Math.Cos(w0);
            float sqrtA = (float)Math.Sqrt(A);

            b0 = A * ((A + 1.0f) + (A - 1.0f) * cosW0 + 2.0f * sqrtA * alpha);
            b1 = -2.0f * A * ((A - 1.0f) + (A + 1.0f) * cosW0);
            b2 = A * ((A + 1.0f) + (A - 1.0f) * cosW0 - 2.0f * sqrtA * alpha);
            a0 = (A + 1.0f) - (A - 1.0f) * cosW0 + 2.0f * sqrtA * alpha;
            a1 = 2.0f * ((A - 1.0f) - (A + 1.0f) * cosW0);
            a2 = (A + 1.0f) - (A - 1.0f) * cosW0 - 2.0f * sqrtA * alpha;

            Normalize();
        }

        private void Normalize()
        {
            b0 /= a0;
            b1 /= a0;
            b2 /= a0;
            a1 /= a0;
            a2 /= a0;
            a0 = 1.0f;
        }

        public float Process(float input)
        {
            float output = b0 * input + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1;
            x1 = input;
            y2 = y1;
            y1 = output;
            return output;
        }
    }

    public class Distortion
    {
        public float Process(float sample, float amount)
        {
            // Hyperbolic tangent distortion
            float gain = amount * 10.0f; // Scale 0-1 to 0-10 for noticeable effect
            return (float)Math.Tanh(sample * gain) / gain;
        }
    }
}