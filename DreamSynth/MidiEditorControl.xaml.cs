using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private const double gridSize = 20; // Пикселей на единицу сетки (четвертная нота)
        private double BPM { get; set; } = 120; // Темп в ударах в минуту
        private double BeatsPerGridUnit { get; set; } = 1; // Одна единица сетки = одна четвертная нота
        private double Interval => 60000.0 / (BPM * BeatsPerGridUnit); // Длительность единицы сетки в мс

        private Line PlaybackLine;
        private System.Windows.Threading.DispatcherTimer playbackTimer;
        private Stopwatch playbackStopwatch = new Stopwatch(); // Tracks precise playback time
        private double playbackPosition = 0; // Current playback position in grid units
        private HashSet<MidiNote> activeNotes = new HashSet<MidiNote>(); // Tracks currently playing notes
        private double sequenceDuration = 0; // Total duration of the note sequence in grid units

        public MidiEditorControl()
        {
            InitializeComponent();
            NoteCanvas.Width = 640;
            NoteCanvas.Height = gridSize * 12;
            this.Loaded += MidiEditorControl_Loaded;

            // Initialize playback line
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

            // Initialize playback timer
            playbackTimer = new System.Windows.Threading.DispatcherTimer();
            playbackTimer.Interval = TimeSpan.FromMilliseconds(10); // ~100 FPS for smoother animation
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

            NoteCanvas.Children.Add(PlaybackLine); // Add playback line on top of grid

            // Update sequence duration (include extra space after last note)
            sequenceDuration = Notes.Any() 
                ? Math.Max(Notes.Max(n => n.StartTime + n.Duration), NoteCanvas.ActualWidth / gridSize) 
                : NoteCanvas.ActualWidth / gridSize;

            foreach (var note in Notes)
            {
                DrawNote(note);
            }
        }

        private void DrawNote(MidiNote note)
        {
            var noteWidth = note.Duration * gridSize;
            var noteY = note.Pitch * gridSize;
            noteY = RoundToGrid(noteY); // Snap to grid

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

            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * gridSize)); // Snap to grid for time
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
                var newDuration = Math.Max(gridSize, newWidth) / gridSize; // Ensure minimum duration
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
            // Получаем текущее время воспроизведения в миллисекундах и преобразуем в единицы сетки
            double elapsedMs = playbackStopwatch.ElapsedMilliseconds;
            playbackPosition = elapsedMs / Interval; // Grid units
            double lineX = playbackPosition * gridSize;

            // Отладочный вывод для проверки синхронизации
            Console.WriteLine($"Elapsed: {elapsedMs:F2} ms, PlaybackPosition: {playbackPosition:F2}, LineX: {lineX:F2}");

            // Подсвечиваем активные ноты и обновляем визуализацию
            foreach (var child in NoteCanvas.Children.OfType<Rectangle>())
            {
                var note = child.Tag as MidiNote;
                if (note != null)
                {
                    bool isPlaying = activeNotes.Contains(note);
                    child.Fill = isPlaying ? Brushes.Green : Brushes.Blue; // Подсветка активных нот
                    child.Stroke = isPlaying ? Brushes.Yellow : Brushes.Black; // Желтая рамка для активных нот
                    child.StrokeThickness = isPlaying ? 2 : 1;
                }
            }

            // Проверяем, какие ноты нужно воспроизвести или остановить
            foreach (var note in Notes)
            {
                double noteStart = note.StartTime;
                double noteEnd = note.StartTime + note.Duration;

                // Если позиция воспроизведения достигла начала ноты
                if (!activeNotes.Contains(note) && playbackPosition >= noteStart && playbackPosition < noteEnd)
                {
                    PlayNote(note);
                    activeNotes.Add(note);
                }
                // Если позиция воспроизведения прошла конец ноты
                else if (activeNotes.Contains(note) && playbackPosition >= noteEnd)
                {
                    StopNote(note);
                    activeNotes.Remove(note);
                }
            }

            // Проверяем, достигло ли воспроизведение конца последовательности
            if (playbackPosition >= sequenceDuration)
            {
                StopPlayback();
                StartPlayback(); // Зацикливаем воспроизведение
            }
            else
            {
                // Обновляем позицию линии воспроизведения
                PlaybackLine.X1 = lineX;
                PlaybackLine.X2 = lineX;
            }
        }

        private void PlayNote(MidiNote note)
        {
            // Вычисляем длительность ноты в миллисекундах для отладки
            double noteDurationMs = note.Duration * Interval;
            Console.WriteLine($"Playing note: Pitch={note.Pitch}, StartTime={note.StartTime}, Duration={note.Duration}, DurationMs={noteDurationMs:F2}, ElapsedMs={playbackStopwatch.ElapsedMilliseconds:F2}");

            // Для MIDI-синтезатора (например, NAudio или DryWetMidi):
            // - Отправить NoteOn
            // Пример: midiOut.Send(MidiMessage.StartNote(note.Pitch + 60, 127, 0).RawData);
        }

        private void StopNote(MidiNote note)
        {
            Console.WriteLine($"Stopping note: Pitch={note.Pitch}, ElapsedMs={playbackStopwatch.ElapsedMilliseconds:F2}");

            // Для MIDI-синтезатора:
            // - Отправить NoteOff
            // Пример: midiOut.Send(MidiMessage.StopNote(note.Pitch + 60, 0, 0).RawData);
        }

        public void StartPlayback()
        {
            // Сбрасываем состояние воспроизведения
            playbackPosition = 0;
            activeNotes.Clear();
            playbackStopwatch.Restart();
            PlaybackLine.X1 = 0;
            PlaybackLine.X2 = 0;

            // Обновляем длительность последовательности
            sequenceDuration = Notes.Any() 
                ? Math.Max(Notes.Max(n => n.StartTime + n.Duration), NoteCanvas.ActualWidth / gridSize) 
                : NoteCanvas.ActualWidth / gridSize;

            playbackTimer.Start();
        }

        public void StopPlayback()
        {
            playbackTimer.Stop();
            playbackStopwatch.Stop();
            foreach (var note in activeNotes.ToList())
            {
                StopNote(note);
                activeNotes.Remove(note);
            }
            playbackPosition = 0;
            PlaybackLine.X1 = 0;
            PlaybackLine.X2 = 0;

            // Сбрасываем подсветку нот
            foreach (var child in NoteCanvas.Children.OfType<Rectangle>())
            {
                child.Fill = Brushes.Blue;
                child.Stroke = Brushes.Black;
                child.StrokeThickness = 1;
            }
        }
    }

    public class MidiNote
    {
        public int Pitch { get; set; }
        public double StartTime { get; set; } // В единицах сетки (четвертные ноты)
        public double Duration { get; set; } // В единицах сетки (четвертные ноты)
    }
}