using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;

namespace DreamSynth
{
    public partial class MidiEditorControl : UserControl
    {
        public static ObservableCollection<MidiNote> Notes { get; set; } = new ObservableCollection<MidiNote>();
        private MidiNote SelectedNote { get; set; }
        private Rectangle SelectedRectangle { get; set; }
        private bool isDragging = false;
        private bool isResizing = false;
        private Point mouseStartPosition;
        private double initialWidth;
        private const double noteHeight = 20;
        private const double gridSize = 20;

        public MidiEditorControl()
        {
            InitializeComponent();
            NoteCanvas.Width = 640;
            NoteCanvas.Height = gridSize * 12;
            this.Loaded += MidiEditorControl_Loaded;
        }

        private void MidiEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        private void DrawGrid()
        {
            NoteCanvas.Children.Clear();
            int numHorizontalLines = (int)(NoteCanvas.ActualHeight / gridSize);
            int numVerticalLines = (int)(NoteCanvas.ActualWidth / gridSize);

            for (int i = 0; i <= numHorizontalLines; i++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = i * gridSize,
                    X2 = NoteCanvas.ActualWidth,
                    Y2 = i * gridSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                };
                NoteCanvas.Children.Add(line);
            }

            for (int i = 0; i <= numVerticalLines; i++)
            {
                var line = new Line
                {
                    X1 = i * gridSize,
                    Y1 = 0,
                    X2 = i * gridSize,
                    Y2 = NoteCanvas.ActualHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                };
                NoteCanvas.Children.Add(line);
            }

            foreach (var note in Notes)
            {
                DrawNote(note);
            }
        }

        private void DrawNote(MidiNote note)
        {
            var noteWidth = note.Duration * gridSize;

            // Вычисляем Y-координату для каждой ноты на основе pitch
            var noteY = note.Pitch * (gridSize);  // Привязываем pitch к целому значению на сетке

            noteY = RoundToGrid(noteY);  // Применяем округление к сетке

            var rect = new Rectangle
            {
                Width = noteWidth,
                Height = noteHeight,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = note
            };

            rect.MouseLeftButtonDown += Note_MouseLeftButtonDown;
            rect.MouseRightButtonDown += Note_MouseRightButtonDown;

            // Для более точного отображения устанавливаем координаты
            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * gridSize));  // Используем округление для времени
            Canvas.SetTop(rect, noteY);  // Округление для Y-позиции

            NoteCanvas.Children.Add(rect);
        }


        private double RoundToGrid(double value)
        {
            return Math.Round(value / gridSize) * gridSize;
        }

        private void NoteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging || isResizing) return;

            var mousePosition = e.GetPosition(NoteCanvas);
            var startTime = RoundToGrid(mousePosition.X);

            // Корректный расчет pitch, всегда целое число от 0 до 11
            int pitchIndex = (int)Math.Round(mousePosition.Y / (gridSize));
            pitchIndex = Math.Max(0, Math.Min(pitchIndex, 11)); // Ограничиваем pitch от 0 до 11 (для 12 полутонов)

            var note = new MidiNote
            {
                Pitch = pitchIndex, // Устанавливаем корректный pitch
                StartTime = startTime / gridSize,
                Duration = 1
            };

            Notes.Add(note);
            DrawGrid();
        }

        
        private void Note_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                SelectedNote = rect.Tag as MidiNote;
                SelectedRectangle = rect;
                isDragging = true;
                mouseStartPosition = e.GetPosition(NoteCanvas);
                Mouse.Capture(NoteCanvas);
            }
        }


        private void Note_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                SelectedNote = rect.Tag as MidiNote;
                SelectedRectangle = rect;
                isResizing = true;
                mouseStartPosition = e.GetPosition(NoteCanvas);
                initialWidth = rect.Width;
                Mouse.Capture(NoteCanvas);
            }
        }

        private void NoteCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(NoteCanvas);

            if (isDragging && SelectedNote != null)
            {
                // Обновляем StartTime на основе позиции мыши
                var newStartTime = RoundToGrid(mousePosition.X) / gridSize;

                // Ограничиваем значение StartTime
                newStartTime = Math.Max(0, newStartTime);

                // Обновляем pitch на основе новой позиции мыши
                var newPitch = (int)Math.Round(mousePosition.Y / gridSize);
                newPitch = Math.Max(0, Math.Min(newPitch, (int)(NoteCanvas.Height / gridSize) - 1));

                // Обновляем свойства ноты
                SelectedNote.StartTime = newStartTime;
                SelectedNote.Pitch = newPitch;

                // Обновляем позицию прямоугольника на канвасе, привязанную к сетке
                Canvas.SetLeft(SelectedRectangle, RoundToGrid(SelectedNote.StartTime * gridSize));
                Canvas.SetTop(SelectedRectangle, RoundToGrid(SelectedNote.Pitch * gridSize));

                // Перерисовываем сетку (для перерисовки канваса)
                DrawGrid();
            }
        }




        private void NoteCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging || isResizing)
            {
                isDragging = false;
                isResizing = false;
                Mouse.Capture(null);
                SelectedNote = null;
                SelectedRectangle = null;
            }
        }
    }

    public class MidiNote
    {
        public int Pitch { get; set; }
        public double StartTime { get; set; }
        public double Duration { get; set; }
    }
}
