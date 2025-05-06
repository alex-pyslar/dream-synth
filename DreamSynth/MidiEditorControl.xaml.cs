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
        private const double gridSize = 20; // Pixels per grid unit (quarter note)
        private double _bpm = 120; // Tempo in beats per minute
        private const double BeatsPerGridUnit = 1; // One grid unit = one quarter note

        // Public BPM property with validation
        public double BPM
        {
            get => _bpm;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("BPM must be greater than zero.");
                _bpm = value;
                UpdateInterval();
            }
        }

        private double Interval { get; set; } // Duration of one grid unit in ms

        private Line PlaybackLine;
        private System.Windows.Threading.DispatcherTimer playbackTimer;
        private Stopwatch playbackStopwatch = new Stopwatch();
        private double playbackPosition = 0; // Current playback position in grid units
        private HashSet<MidiNote> activeNotes = new HashSet<MidiNote>();
        private double sequenceDuration = 0; // Total duration in grid units

        public MidiEditorControl()
        {
            InitializeComponent();
            NoteCanvas.Width = 640;
            NoteCanvas.Height = gridSize * 12;
            this.Loaded += MidiEditorControl_Loaded;

            // Initialize Interval based on initial BPM
            UpdateInterval();

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
            playbackTimer.Interval = TimeSpan.FromMilliseconds(10); // ~100 FPS
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void UpdateInterval()
        {
            Interval = 112000.0 / BPM;
            //Interval = 112000.0 / (BPM * BeatsPerGridUnit); // Duration of one grid unit in ms
            Console.WriteLine($"BPM updated to {BPM}, Interval: {Interval:F2} ms");
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

            NoteCanvas.Children.Add(PlaybackLine);

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
            noteY = RoundToGrid(noteY);

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

            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * gridSize));
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
                var newDuration = Math.Max(gridSize, newWidth) / gridSize;
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
            // Calculate playback position in grid units
            double elapsedMs = playbackStopwatch.ElapsedMilliseconds;
            playbackPosition = elapsedMs / Interval; // Grid units
            double lineX = playbackPosition * gridSize; // Pixels

            // Debug output to track timing
            double expectedLineX = (elapsedMs / Interval) * gridSize;
            Console.WriteLine($"Elapsed: {elapsedMs:F2} ms, PlaybackPosition: {playbackPosition:F3} units, LineX: {lineX:F2} px, ExpectedLineX: {expectedLineX:F2} px, BPM: {BPM}, Interval: {Interval:F2} ms");

            // Small visual lookahead for note highlighting (in grid units)
            const double visualLookahead = 0.05; // Highlight notes slightly before playback line reaches them
            const double tolerance = 0.02; // Tight tolerance for precise synchronization

            // Update note visuals and playback in a single pass
            foreach (var child in NoteCanvas.Children.OfType<Rectangle>())
            {
                var note = child.Tag as MidiNote;
                if (note == null) continue;

                double noteStart = note.StartTime;
                double noteEnd = note.StartTime + note.Duration;

                // Determine if the note should be playing and highlighted
                bool shouldBePlaying = playbackPosition >= noteStart - tolerance &&
                                      playbackPosition < noteEnd + tolerance;
                bool shouldBeHighlighted = playbackPosition >= noteStart - visualLookahead &&
                                          playbackPosition < noteEnd + tolerance;

                // Update playback state
                if (shouldBePlaying && !activeNotes.Contains(note))
                {
                    PlayNote(note);
                    activeNotes.Add(note);
                }
                else if (!shouldBePlaying && activeNotes.Contains(note))
                {
                    StopNote(note);
                    activeNotes.Remove(note);
                }

                // Update visuals
                child.Fill = shouldBeHighlighted ? Brushes.Green : Brushes.Blue;
                child.Stroke = shouldBeHighlighted ? Brushes.Yellow : Brushes.Black;
                child.StrokeThickness = shouldBeHighlighted ? 2 : 1;
            }

            // Check if playback reached the end
            if (playbackPosition >= sequenceDuration)
            {
                StopPlayback();
                StartPlayback(); // Loop playback
            }
            else
            {
                // Update playback line position
                PlaybackLine.X1 = lineX;
                PlaybackLine.X2 = lineX;
            }
        }

        private void PlayNote(MidiNote note)
        {
            double noteStartMs = note.StartTime * Interval;
            double noteDurationMs = note.Duration * Interval;
            double currentTimeMs = playbackPosition * Interval;
            Console.WriteLine($"Playing note: Pitch={note.Pitch}, StartTime={note.StartTime:F3} ({noteStartMs:F2} ms), Duration={note.Duration:F3} ({noteDurationMs:F2} ms), CurrentTime={currentTimeMs:F2} ms, ElapsedMs={playbackStopwatch.ElapsedMilliseconds:F2}");

            // Placeholder for MIDI NoteOn
            // Example: midiOut.Send(MidiMessage.StartNote(note.Pitch + 60, 127, 0).RawData);
        }

        private void StopNote(MidiNote note)
        {
            double noteEndMs = (note.StartTime + note.Duration) * Interval;
            double currentTimeMs = playbackPosition * Interval;
            Console.WriteLine($"Stopping note: Pitch={note.Pitch}, EndTime={(note.StartTime + note.Duration):F3} ({noteEndMs:F2} ms), CurrentTime={currentTimeMs:F2} ms, ElapsedMs={playbackStopwatch.ElapsedMilliseconds:F2}");

            // Placeholder for MIDI NoteOff
            // Example: midiOut.Send(MidiMessage.StopNote(note.Pitch + 60, 0, 0).RawData);
        }

        public void StartPlayback()
        {
            playbackPosition = 0;
            activeNotes.Clear();
            playbackStopwatch.Restart();
            PlaybackLine.X1 = 0;
            PlaybackLine.X2 = 0;

            sequenceDuration = Notes.Any()
                ? Math.Max(Notes.Max(n => n.StartTime + n.Duration), NoteCanvas.ActualWidth / gridSize)
                : NoteCanvas.ActualWidth / gridSize;

            playbackTimer.Start();
            Console.WriteLine($"Playback started: BPM={BPM}, Interval={Interval:F2} ms, SequenceDuration={sequenceDuration:F3} units");
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

            foreach (var child in NoteCanvas.Children.OfType<Rectangle>())
            {
                child.Fill = Brushes.Blue;
                child.Stroke = Brushes.Black;
                child.StrokeThickness = 1;
            }

            Console.WriteLine("Playback stopped");
        }
    }

    public class MidiNote
    {
        public int Pitch { get; set; }
        public double StartTime { get; set; } // In grid units (quarter notes)
        public double Duration { get; set; } // In grid units (quarter notes)
    }
}