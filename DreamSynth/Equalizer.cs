namespace DreamSynth // Объявляем пространство имен для проекта синтезатора
{
    // Класс эквалайзера для обработки аудиосигнала путем изменения уровня громкости различных частотных диапазонов
    public class Equalizer
    {
        // Поля для хранения текущего значения усиления для низких, средних и высоких частот
        private float lowGain;
        private float midGain;
        private float highGain;

        // Поля для хранения частот разделения низких и высоких частот
        private float lowFrequency;
        private float highFrequency;

        // Константы для значений частот разделения по умолчанию
        private const float DefaultLowFrequency = 150.0f;  // Частота разделения для низких частот по умолчанию
        private const float DefaultHighFrequency = 1500.0f; // Частота разделения для высоких частот по умолчанию

        // Конструктор класса Equalizer, инициализирующий усиление и частоты разделения
        public Equalizer(float initialLowGain, float initialMidGain, float initialHighGain)
        {
            this.lowGain = initialLowGain;
            this.midGain = initialMidGain;
            this.highGain = initialHighGain;
            this.lowFrequency = DefaultLowFrequency;
            this.highFrequency = DefaultHighFrequency;
        }

        // Метод для установки усиления низких частот
        public void SetLowGain(float gain)
        {
            this.lowGain = gain;
        }

        // Метод для установки усиления средних частот
        public void SetMidGain(float gain)
        {
            this.midGain = gain;
        }

        // Метод для установки усиления высоких частот
        public void SetHighGain(float gain)
        {
            this.highGain = gain;
        }

        // Метод для установки частоты разделения для низких частот
        public void SetLowFrequency(float frequency)
        {
            this.lowFrequency = frequency;
        }

        // Метод для установки частоты разделения для высоких частот
        public void SetHighFrequency(float frequency)
        {
            this.highFrequency = frequency;
        }

        // Метод для применения эквалайзера к отдельному сэмплу аудиосигнала
        // sample - входной аудиосигнал
        // frequency - частота входного сэмпла
        public float Apply(float sample, float frequency)
        {
            // Переменная для хранения скорректированного значения сэмпла
            float adjustedSample = sample;

            // Условие для применения усиления низких частот
            if (frequency < lowFrequency)
            {
                adjustedSample *= lowGain; // Применение усиления низких частот
            }
            // Условие для применения усиления средних частот
            else if (frequency >= lowFrequency && frequency < highFrequency)
            {
                adjustedSample *= midGain; // Применение усиления средних частот
            }
            // Условие для применения усиления высоких частот
            else
            {
                adjustedSample *= highGain; // Применение усиления высоких частот
            }

            // Возвращение скорректированного значения сэмпла
            return adjustedSample;
        }
    }
}
