using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        private Line PlaybackLine;
        private static System.Windows.Threading.DispatcherTimer playbackTimer;
        private double playbackPosition = 0; // Текущее положение воспроизведения


        public MidiEditorControl()
        {
            InitializeComponent();
            NoteCanvas.Width = 640;
            NoteCanvas.Height = gridSize * 12;
            this.Loaded += MidiEditorControl_Loaded;

            // Инициализация линии воспроизведения
            PlaybackLine = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = NoteCanvas.Height
            };
            NoteCanvas.Children.Add(PlaybackLine);

            // Инициализация таймера воспроизведения
            playbackTimer = new System.Windows.Threading.DispatcherTimer();
            playbackTimer.Interval = TimeSpan.FromMilliseconds(1); // Интервал обновления (50 мс)
            playbackTimer.Tick += PlaybackTimer_Tick;
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

            NoteCanvas.Children.Add(PlaybackLine); // Добавляем линию воспроизведения поверх сетки

            foreach (var note in Notes)
            {
                DrawNote(note);
            }
        }

        private void DrawNote(MidiNote note)
        {
            var noteWidth = note.Duration * gridSize;
            var noteY = note.Pitch * gridSize;
            noteY = RoundToGrid(noteY); // Применяем округление к сетке

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

            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * gridSize)); // Округление для времени
            Canvas.SetTop(rect, noteY);

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
            int pitchIndex = (int)Math.Round(mousePosition.Y / gridSize);
            pitchIndex = Math.Max(0, Math.Min(pitchIndex, 11));

            var note = new MidiNote
            {
                Pitch = pitchIndex,
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
                var newStartTime = RoundToGrid(mousePosition.X) / gridSize;
                newStartTime = Math.Max(0, newStartTime);
                var newPitch = (int)Math.Round(mousePosition.Y / gridSize);
                newPitch = Math.Max(0, Math.Min(newPitch, (int)(NoteCanvas.Height / gridSize) - 1));
                SelectedNote.StartTime = newStartTime;
                SelectedNote.Pitch = newPitch;
                Canvas.SetLeft(SelectedRectangle, RoundToGrid(SelectedNote.StartTime * gridSize));
                Canvas.SetTop(SelectedRectangle, RoundToGrid(SelectedNote.Pitch * gridSize));
                DrawGrid();
            }
            else if (isResizing && SelectedNote != null)
            {
                var widthChange = mousePosition.X - mouseStartPosition.X;
                var newWidth = RoundToGrid(initialWidth + widthChange);
                var newDuration = Math.Max(0, newWidth / gridSize);
                SelectedNote.Duration = newDuration;
                SelectedRectangle.Width = newWidth;
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

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            // Увеличиваем положение воспроизведения на основе скорости и интервала
            playbackPosition += gridSize * (playbackTimer.Interval.TotalMilliseconds / (MainWindow.interval));
            
            // Рассчитываем текущее положение линии воспроизведения
            double lineX = RoundToGrid(playbackPosition * gridSize);
    
            // Проверяем, не вышла ли линия за пределы холста
            if (lineX >= NoteCanvas.ActualWidth)
            {
                StopPlayback();
                StartPlayback();
            }
            else
            {
                PlaybackLine.X1 += lineX;
                PlaybackLine.X2 += lineX;
            }
        }

        public void StartPlayback()
        {
            playbackPosition = 0;
            playbackTimer.Start();
        }

        public void StopPlayback()
        {
            playbackTimer.Stop();
            playbackPosition = 0;
            PlaybackLine.X1 = 0;
            PlaybackLine.X2 = 0;
        }
    }

    public class MidiNote
    {
        public int Pitch { get; set; }
        public double StartTime { get; set; }
        public double Duration { get; set; }
    }
}
