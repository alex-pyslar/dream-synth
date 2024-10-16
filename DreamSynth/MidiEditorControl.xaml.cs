using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;

namespace MidiEditor
{
    public partial class MidiEditorControl : UserControl
    {
        private ObservableCollection<MidiNote> Notes { get; set; } = new ObservableCollection<MidiNote>();
        private MidiNote SelectedNote { get; set; }
        private bool isDragging = false;
        private const double noteHeight = 20; // Высота ноты
        private const double gridSize = 20; // Размер ячейки сетки

        public MidiEditorControl()
        {
            InitializeComponent();
            DrawGrid(); // Рисуем сетку при инициализации
        }

        private void DrawGrid()
        {
            int numHorizontalLines = (int)(NoteCanvas.ActualHeight / gridSize); // Количество горизонтальных линий
            int numVerticalLines = (int)(NoteCanvas.ActualWidth / gridSize); // Количество вертикальных линий

            // Рисуем горизонтальные линии
            for (int i = 0; i <= numHorizontalLines; i++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = i * gridSize,
                    X2 = NoteCanvas.ActualWidth,
                    Y2 = i * gridSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                NoteCanvas.Children.Add(line);
            }

            // Рисуем вертикальные линии
            for (int i = 0; i <= numVerticalLines; i++)
            {
                var line = new Line
                {
                    X1 = i * gridSize,
                    Y1 = 0,
                    X2 = i * gridSize,
                    Y2 = NoteCanvas.ActualHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                NoteCanvas.Children.Add(line);
            }
        }

        private void DrawNote(MidiNote note)
        {
            var noteWidth = note.Duration * 10; // Ширина в зависимости от длительности
            var noteY = (100 - note.Pitch) * gridSize / noteHeight; // Вычисление вертикальной позиции с учетом высоты ноты

            var rect = new Rectangle
            {
                Width = noteWidth,
                Height = noteHeight,
                Fill = Brushes.Blue,
                Tag = note // Сохраняем объект ноты в тег
            };

            rect.MouseLeftButtonDown += Note_MouseLeftButtonDown;

            // Установите горизонтальную позицию на ближайшую сетку
            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * 10));
            Canvas.SetTop(rect, noteY);

            NoteCanvas.Children.Add(rect);
            Notes.Add(note); // Добавляем ноту в коллекцию
        }

        private double RoundToGrid(double value)
        {
            return Math.Round(value / gridSize) * gridSize; // Округляем до ближайшей линии сетки
        }

        private void Note_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                SelectedNote = rect.Tag as MidiNote;
                isDragging = true;
                Mouse.Capture(rect);
            }
        }

        private void NoteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging) return; // Игнорируем добавление нот, если мы перетаскиваем

            var mousePosition = e.GetPosition(NoteCanvas);
            var note = new MidiNote
            {
                Pitch = 60, // Например, C4
                StartTime = RoundToGrid(mousePosition.X / 10), // Преобразуем в время и округляем
                Duration = 1 // Устанавливаем длительность
            };

            DrawNote(note);
        }

        private void NoteCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && SelectedNote != null)
            {
                var mousePosition = e.GetPosition(NoteCanvas);
                SelectedNote.StartTime = RoundToGrid(mousePosition.X / 10); // Обновляем время начала и округляем
                UpdateCanvas(); // Перерисовываем канвас
            }
        }

        private void NoteCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            Mouse.Capture(null); // Освобождаем захват мыши
            SelectedNote = null; // Сбрасываем выделение
        }

        private void UpdateCanvas()
        {
            NoteCanvas.Children.Clear(); // Очищаем канвас
            DrawGrid(); // Рисуем сетку снова
            foreach (var note in Notes)
            {
                DrawNote(note); // Перерисовываем все ноты
            }
        }
    }

    public class MidiNote
    {
        public int Pitch { get; set; } // Нота (например, C4 = 60)
        public double StartTime { get; set; } // Время начала
        public double Duration { get; set; } // Длительность
    }
}
