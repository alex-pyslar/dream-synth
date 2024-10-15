using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Midi;

namespace DreamSynth
{
    public partial class MidiEditorControl : UserControl
    {
        private MidiEventCollection midiEvents;
        private List<Rectangle> noteRectangles = new List<Rectangle>();

        public MidiEditorControl()
        {
            InitializeComponent();
            midiEvents = new MidiEventCollection(1, 120);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            midiEvents = new MidiEventCollection(1, 120);
            MidiCanvas.Children.Clear();
            noteRectangles.Clear();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "MIDI files (*.mid)|*.mid" };
            if (openFileDialog.ShowDialog() == true)
            {
                var midiFile = new MidiFile(openFileDialog.FileName, false);
                midiEvents = midiFile.Events;
                LoadMidiEvents();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "MIDI files (*.mid)|*.mid" };
            if (saveFileDialog.ShowDialog() == true)
            {
                MidiFile.Export(saveFileDialog.FileName, midiEvents);
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var noteEvent = new NoteOnEvent(0, 1, 60, 127, 480);
            midiEvents[0].Add(noteEvent);
            DrawNoteRectangle(noteEvent);
        }

        private void LoadMidiEvents()
        {
            MidiCanvas.Children.Clear();
            noteRectangles.Clear();

            foreach (var track in midiEvents)
            {
                foreach (var midiEvent in track)
                {
                    if (midiEvent is NoteOnEvent noteOn)
                    {
                        DrawNoteRectangle(noteOn);
                    }
                }
            }
        }

        private void DrawNoteRectangle(NoteOnEvent noteOn)
        {
            var rect = new Rectangle
            {
                Width = noteOn.NoteLength / 10.0,
                Height = 20,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black
            };

            Canvas.SetLeft(rect, noteOn.AbsoluteTime / 10.0);
            Canvas.SetTop(rect, (127 - noteOn.NoteNumber) * 3); // Расположение ноты по высоте
            MidiCanvas.Children.Add(rect);
            noteRectangles.Add(rect);

            rect.MouseDown += Note_MouseDown;
            rect.MouseMove += Note_MouseMove;
            rect.MouseUp += Note_MouseUp;
        }

        private bool isDragging = false;
        private Point startPoint;
        private Rectangle selectedRect;

        private void Note_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedRect = sender as Rectangle;
            startPoint = e.GetPosition(MidiCanvas);
            isDragging = true;
            selectedRect.CaptureMouse();
        }

        private void Note_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedRect != null)
            {
                var position = e.GetPosition(MidiCanvas);
                var offsetX = position.X - startPoint.X;
                Canvas.SetLeft(selectedRect, Canvas.GetLeft(selectedRect) + offsetX);
                startPoint = position;
            }
        }

        private void Note_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            selectedRect?.ReleaseMouseCapture();
        }
    }
}
