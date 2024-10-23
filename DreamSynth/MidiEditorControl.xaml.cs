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
        private const double noteHeight = 20; // Высота ноты
        private const double gridSize = 20; // Размер ячейки сетки

        public MidiEditorControl()
        {
            InitializeComponent();
            NoteCanvas.Width = 640;  // Ширина канваса
            NoteCanvas.Height = 140;  // Высота канваса для диапазона нот от 0 до 7
            this.Loaded += MidiEditorControl_Loaded; // Загружаем сетку при инициализации
        }

        // Инициализация компонента и рисование сетки
        private void MidiEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        // Отображение сетки на Canvas
        private void DrawGrid()
        {
            NoteCanvas.Children.Clear(); // Очищаем старые элементы перед перерисовкой
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
                    Stroke = Brushes.Black, // Черный цвет
                    StrokeThickness = 0.5 // Толщина линии
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
                    Stroke = Brushes.Black, // Черный цвет
                    StrokeThickness = 0.5 // Толщина линии
                };
                NoteCanvas.Children.Add(line);
            }

            // Отрисовываем ноты после отрисовки сетки
            foreach (var note in Notes)
            {
                DrawNote(note);
            }
        }

        // Отображение ноты на Canvas
        private void DrawNote(MidiNote note)
        {
            var noteWidth = note.Duration * gridSize;
            var noteY = note.Pitch * (gridSize / 2); // Рассчитываем позицию по высоте (0-я нота сверху, 7-я снизу)

            // Убедимся, что позиция Y округлена по сетке
            noteY = RoundToGrid(noteY); // Округление по сетке

            var rect = new Rectangle
            {
                Width = noteWidth,
                Height = noteHeight,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = note // Сохраняем объект ноты в Tag
            };

            rect.MouseLeftButtonDown += Note_MouseLeftButtonDown;
            rect.MouseRightButtonDown += Note_MouseRightButtonDown;

            Canvas.SetLeft(rect, RoundToGrid(note.StartTime * gridSize));
            Canvas.SetTop(rect, noteY);

            NoteCanvas.Children.Add(rect);
        }

        // Округление до сетки
        private double RoundToGrid(double value)
        {
            return Math.Round(value / gridSize) * gridSize;
        }

        // Обработка добавления новой ноты по клику на пустое место канваса
        private void NoteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging || isResizing) return;

            var mousePosition = e.GetPosition(NoteCanvas);
            var startTime = RoundToGrid(mousePosition.X); // Прямое округление по сетке
            var pitch = (int)Math.Round(mousePosition.Y / (gridSize / 2)); // Преобразуем Y-координату в высоту (0 сверху, 7 снизу)

            pitch = Clamp(pitch, 0, 7); // Ограничиваем диапазон pitch от 0 до 7

            var note = new MidiNote
            {
                Pitch = pitch,
                StartTime = startTime / gridSize, // Длительность по умолчанию
                Duration = 1 // Длительность по умолчанию
            };

            Notes.Add(note);
            DrawGrid(); // Обновляем Canvas с новой нотой
        }

        // Захват ноты для перемещения
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

        // Изменение длительности ноты правой кнопкой мыши
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

        // Обработка перемещения или изменения размера ноты
        private void NoteCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(NoteCanvas);

            // Проверка на перемещение ноты
            if (isDragging && SelectedNote != null)
            {
                var offsetX = mousePosition.X - mouseStartPosition.X;
                var newStartTime = RoundToGrid(Canvas.GetLeft(SelectedRectangle) + offsetX) / gridSize; // Округляем до сетки
                
                // Ограничиваем перемещение по X
                if (newStartTime < 0)
                    newStartTime = 0; // Не позволяем перемещать за пределы канваса
                else if (newStartTime * gridSize + SelectedRectangle.Width > NoteCanvas.ActualWidth)
                    newStartTime = (NoteCanvas.ActualWidth / gridSize) - (SelectedRectangle.Width / gridSize); // Не позволяем перемещать за пределы канваса

                SelectedNote.StartTime = newStartTime;

                // Обновление вертикальной позиции (диапазон pitch от 0 до 7)
                var newPitch = (int)Math.Round(mousePosition.Y / (gridSize / 2)); // Преобразуем Y-координату в высоту
                
                newPitch = Clamp(newPitch, 0, 7); // Ограничиваем вертикальную позицию

                SelectedNote.Pitch = newPitch; // Устанавливаем ограниченную высоту

                DrawGrid(); // Перерисовываем Canvas
            }
            // Проверка на изменение размера ноты
            else if (isResizing && SelectedNote != null)
            {
                var offsetX = mousePosition.X - mouseStartPosition.X;
                var newWidth = Math.Max(1, initialWidth + offsetX);
                
                // Ограничиваем изменение ширины ноты
                if (newWidth + Canvas.GetLeft(SelectedRectangle) > NoteCanvas.ActualWidth)
                {
                    newWidth = NoteCanvas.ActualWidth - Canvas.GetLeft(SelectedRectangle); // Не позволяем изменять ширину за пределы канваса
                }

                // Обновление длительности ноты
                SelectedNote.Duration = RoundToGrid(newWidth) / gridSize; // Округляем длительность до сетки
                SelectedRectangle.Width = SelectedNote.Duration * gridSize; // Обновляем ширину прямоугольника

                DrawGrid(); // Перерисовываем Canvas
            }
        }

        // Метод Clamp для ограничения значений
        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // Окончание перемещения или изменения размера
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

    // Класс для представления MIDI ноты
    public class MidiNote
    {
        public int Pitch { get; set; } // Высота ноты (например, C4 = 60)
        public double StartTime { get; set; } // Время начала
        public double Duration { get; set; } // Длительность
    }
}
