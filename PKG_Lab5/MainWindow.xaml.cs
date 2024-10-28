using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PKG_Lab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private double scale = 1.0;
        public MainWindow()
        {
            this.MinHeight = 1080;
            this.MinWidth = 1920;
            InitializeComponent();
            DrawGrid();
        }

        private void DrawGrid()
        {
            if (DrawingCanvas == null)
            {
                return;
            }

            DrawingCanvas.Children.Clear(); // Очистка перед рисованием
            double step = 20 * scale; // Шаг сетки в пикселях с учетом масштаба
            double canvasWidth = DrawingCanvas.ActualWidth;
            double canvasHeight = DrawingCanvas.ActualHeight;

            // Определяем центр Canvas
            double centerX = canvasWidth / 2;
            double centerY = canvasHeight / 2;

            // Рисуем вертикальные линии
            for (double x = centerX; x < canvasWidth; x += step)
            {
                Line line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = canvasHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                DrawingCanvas.Children.Add(line);
            }

            for (double x = centerX; x > 0; x -= step)
            {
                Line line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = canvasHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                DrawingCanvas.Children.Add(line);
            }

            // Рисуем горизонтальные линии
            for (double y = centerY; y < canvasHeight; y += step)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = canvasWidth,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                DrawingCanvas.Children.Add(line);
            }

            for (double y = centerY; y > 0; y -= step)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = canvasWidth,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                DrawingCanvas.Children.Add(line);
            }

            // Рисуем оси координат
            DrawAxis(centerX, centerY, canvasWidth, canvasHeight);
        }

        private void DrawAxis(double centerX, double centerY, double canvasWidth, double canvasHeight)
        {
            // Ось X
            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = canvasWidth,
                Y2 = centerY,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };

            // Ось Y
            Line yAxis = new Line
            {
                X1 = centerX,
                Y1 = 0,
                X2 = centerX,
                Y2 = canvasHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };

            DrawingCanvas.Children.Add(xAxis);
            DrawingCanvas.Children.Add(yAxis);

            // Устанавливаем шаг и диапазон значений
            double step = 100 * scale; // Каждые 5 клеток * 20 пикселей
            int range = (int)(Math.Max(canvasWidth, canvasHeight) / step); // Определяем диапазон значений на осях

            // Базовый размер шрифта
            double baseFontSize = 12;

            // Добавляем метки на ось X
            for (int i = -range; i <= range; i++)
            {
                double x = centerX + i * step; // Учитываем масштаб
                if (x >= 0 && x <= canvasWidth)
                {
                    TextBlock label = new TextBlock
                    {
                        Text = (i * 5).ToString(), // Значения 0, 5, 10 и т.д.
                        Foreground = Brushes.Black, // Цвет меток черный
                        FontSize = baseFontSize * scale // Увеличиваем размер шрифта пропорционально масштабу
                    };
                    Canvas.SetLeft(label, x);
                    Canvas.SetTop(label, centerY + 5); // Сдвигаем вниз для метки
                    DrawingCanvas.Children.Add(label);
                }
            }

            // Добавляем метки на ось Y
            for (int i = -range; i <= range; i++)
            {
                double y = centerY - i * step; // Учитываем масштаб
                if (y >= 0 && y <= canvasHeight)
                {
                    TextBlock label = new TextBlock
                    {
                        Text = (i * 5).ToString(), // Значения 0, 5, 10 и т.д.
                        Foreground = Brushes.Black, // Цвет меток черный
                        FontSize = baseFontSize * scale // Увеличиваем размер шрифта пропорционально масштабу
                    };
                    Canvas.SetLeft(label, centerX + 5); // Сдвигаем вправо для метки
                    Canvas.SetTop(label, y);
                    DrawingCanvas.Children.Add(label);
                }
            }
        }

        private void OnMidPointClipButtonClick(object sender, RoutedEventArgs e)
        {
            DrawGrid(); // Перерисовываем сетку

            string[] lines = InputTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                MessageBox.Show("Недостаточно данных.");
                return;
            }

            if (!int.TryParse(lines[0], out int n) || n <= 0 || n + 1 > lines.Length)
            {
                MessageBox.Show("Неверное количество отрезков.");
                return;
            }

            List<Line> segments = new List<Line>();
            Random random = new Random(); // Объект для генерации случайных чисел
            for (int i = 1; i <= n; i++)
            {
                var coords = lines[i].Split(' ');
                if (coords.Length != 4 ||
                    !double.TryParse(coords[0], out double x1) ||
                    !double.TryParse(coords[1], out double y1) ||
                    !double.TryParse(coords[2], out double x2) ||
                    !double.TryParse(coords[3], out double y2))
                {
                    MessageBox.Show($"Неверные координаты отрезка {i}.");
                    return;
                }

                // Преобразуем координаты в систему координат канваса
                segments.Add(new Line
                {
                    X1 = x1 * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                    Y1 = -(y1 * 20 * scale) + (DrawingCanvas.ActualHeight / 2),
                    X2 = x2 * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                    Y2 = -(y2 * 20 * scale) + (DrawingCanvas.ActualHeight / 2)
                });
            }

            var clipCoords = lines[n + 1].Split(' ');
            if (clipCoords.Length != 4 ||
                !double.TryParse(clipCoords[0], out double xMin) ||
                !double.TryParse(clipCoords[1], out double yMin) ||
                !double.TryParse(clipCoords[2], out double xMax) ||
                !double.TryParse(clipCoords[3], out double yMax))
            {
                MessageBox.Show("Неверные координаты отсеченного прямоугольника.");
                return;
            }

            // Рисуем исходные отрезки с случайными цветами
            foreach (var segment in segments)
            {
                Brush randomColor = GetRandomColor(random); // Генерация случайного цвета
                DrawLine(segment.X1, segment.Y1, segment.X2, segment.Y2, randomColor);
            }

            // Рисуем окно отсечения
            DrawClippingWindow(xMin, yMin, xMax, yMax);

            // Отсечение отрезков и отображение точек пересечения
            int clippingPointIndex = 0; // Индекс для точек отсечения
            foreach (var segment in segments)
            {
                // Преобразуем координаты окна отсечения
                double minX = xMin * 20 * scale + (DrawingCanvas.ActualWidth / 2);
                double minY = -yMax * 20 * scale + (DrawingCanvas.ActualHeight / 2);
                double maxX = xMax * 20 * scale + (DrawingCanvas.ActualWidth / 2);
                double maxY = -yMin * 20 * scale + (DrawingCanvas.ActualHeight / 2);

                var clippedSegment = ClipSegment(segment, minX, minY, maxX, maxY);

                if (clippedSegment != null)
                {
                    // Выводим координаты точек отсечения с индексом в исходной системе координат
                    DisplayClippingPoints(clippedSegment.Value, ref clippingPointIndex);

                    // Рисуем отсечённую часть красным цветом
                    DrawLine(clippedSegment.Value.Item1, clippedSegment.Value.Item2,
                             clippedSegment.Value.Item3, clippedSegment.Value.Item4, Brushes.Red);
                }
            }
        }

        private Brush GetRandomColor(Random random)
        {
            byte r = (byte)random.Next(256);
            byte g = (byte)random.Next(256);
            byte b = (byte)random.Next(256);
            return new SolidColorBrush(Color.FromRgb(r, g, b)); // Создание нового цвета
        }

        private void DrawClippingWindow(double xMin, double yMin, double xMax, double yMax)
        {
            DrawLine(xMin * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMin * 20 * scale + (DrawingCanvas.ActualHeight / 2),
                     xMax * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMin * 20 * scale + (DrawingCanvas.ActualHeight / 2), Brushes.Blue); // Нижняя сторона
            DrawLine(xMin * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMin * 20 * scale + (DrawingCanvas.ActualHeight / 2),
                     xMin * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMax * 20 * scale + (DrawingCanvas.ActualHeight / 2), Brushes.Blue); // Левая сторона
            DrawLine(xMax * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMin * 20 * scale + (DrawingCanvas.ActualHeight / 2),
                     xMax * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMax * 20 * scale + (DrawingCanvas.ActualHeight / 2), Brushes.Blue); // Правая сторона
            DrawLine(xMin * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMax * 20 * scale + (DrawingCanvas.ActualHeight / 2),
                     xMax * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                     -yMax * 20 * scale + (DrawingCanvas.ActualHeight / 2), Brushes.Blue); // Верхняя сторона
        }

        private void DisplayClippingPoints((double, double, double, double) clippedSegment, ref int index)
        {
            double xClipped1 = clippedSegment.Item1;
            double yClipped1 = clippedSegment.Item2;
            double xClipped2 = clippedSegment.Item3;
            double yClipped2 = clippedSegment.Item4;

            // Преобразуем координаты отсечённых точек обратно в исходную систему координат
            double originalX1 = (xClipped1 - (DrawingCanvas.ActualWidth / 2)) / (20 * scale);
            double originalY1 = -(yClipped1 - (DrawingCanvas.ActualHeight / 2)) / (20 * scale);
            double originalX2 = (xClipped2 - (DrawingCanvas.ActualWidth / 2)) / (20 * scale);
            double originalY2 = -(yClipped2 - (DrawingCanvas.ActualHeight / 2)) / (20 * scale);

            // Выводим координаты точек отсечения на экран в исходной системе координат
            DrawPoint(xClipped1, yClipped1, Brushes.Black);
            DrawPoint(xClipped2, yClipped2, Brushes.Black);

            // Добавляем текстовые блоки с координатами
            var textBlock1 = new TextBlock
            {
                Text = $"C{index}({originalX1:F2}, {originalY1:F2})",
                Foreground = Brushes.Black,
                FontSize = 12,
                Margin = new Thickness(xClipped1 + 3, yClipped1 - 20, 0, 0)
            };
            DrawingCanvas.Children.Add(textBlock1);

            index++; // Увеличиваем индекс для следующей точки

            var textBlock2 = new TextBlock
            {
                Text = $"C{index}({originalX2:F2}, {originalY2:F2})",
                Foreground = Brushes.Black,
                FontSize = 12,
                Margin = new Thickness(xClipped2 + 3, yClipped2 - 20, 0, 0)
            };
            DrawingCanvas.Children.Add(textBlock2);

            index++; // Увеличиваем индекс для следующей точки
        }

        private void DrawPoint(double x, double y, Brush color)
        {
            var ellipse = new Ellipse
            {
                Fill = color,
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(ellipse, x - 2.5);
            Canvas.SetTop(ellipse, y - 2.5);
            DrawingCanvas.Children.Add(ellipse);
        }

        private (double, double, double, double)? ClipSegment(Line segment, double xMin, double yMin, double xMax, double yMax)
        {
            double x1 = segment.X1;
            double y1 = segment.Y1;
            double x2 = segment.X2;
            double y2 = segment.Y2;

            // Используем алгоритм отсекающего прямоугольника
            double t0 = 0.0, t1 = 1.0;
            double dx = x2 - x1;
            double dy = y2 - y1;

            double[] p = { -dx, dx, -dy, dy };
            double[] q = { x1 - xMin, xMax - x1, y1 - yMin, yMax - y1 };

            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    if (q[i] < 0) return null; // Параллелен и вне окна
                }
                else
                {
                    double r = q[i] / p[i];
                    if (p[i] < 0)
                    {
                        if (r > t1) return null; // Удаляемая часть
                        if (r > t0) t0 = r; // Двигаем t0
                    }
                    else
                    {
                        if (r < t0) return null; // Удаляемая часть
                        if (r < t1) t1 = r; // Двигаем t1
                    }
                }
            }

            if (t0 > t1) return null;

            double xClipped1 = x1 + t0 * dx;
            double yClipped1 = y1 + t0 * dy;
            double xClipped2 = x1 + t1 * dx;
            double yClipped2 = y1 + t1 * dy;

            return (xClipped1, yClipped1, xClipped2, yClipped2);
        }

        private void OnPolygonClipButtonClick(object sender, RoutedEventArgs e)
        {
            DrawGrid(); // Перерисовываем сетку

            string[] lines = InputTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                MessageBox.Show("Недостаточно данных.");
                return;
            }

            if (!int.TryParse(lines[0], out int n) || n <= 0 || n + 1 > lines.Length)
            {
                MessageBox.Show("Неверное количество отрезков.");
                return;
            }

            List<Line> segments = new List<Line>();
            List<Brush> segmentColors = new List<Brush>(); // Список для хранения цветов отрезков
            Random random = new Random(); // Генератор случайных чисел

            for (int i = 1; i <= n; i++)
            {
                var coords = lines[i].Split(' ');
                if (coords.Length != 4 ||
                    !double.TryParse(coords[0], out double x1) ||
                    !double.TryParse(coords[1], out double y1) ||
                    !double.TryParse(coords[2], out double x2) ||
                    !double.TryParse(coords[3], out double y2))
                {
                    MessageBox.Show($"Неверные координаты отрезка {i}.");
                    return;
                }

                // Генерируем случайный цвет для отрезка
                var color = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                segmentColors.Add(color);

                // Преобразуем координаты в систему координат канваса
                segments.Add(new Line
                {
                    X1 = x1 * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                    Y1 = -(y1 * 20 * scale) + (DrawingCanvas.ActualHeight / 2),
                    X2 = x2 * 20 * scale + (DrawingCanvas.ActualWidth / 2),
                    Y2 = -(y2 * 20 * scale) + (DrawingCanvas.ActualHeight / 2)
                });
            }

            var polyCoords = lines[n + 1].Split(' ');
            if (!int.TryParse(polyCoords[0], out int k) || k <= 0 || n + k + 2 > lines.Length)
            {
                MessageBox.Show("Неверное количество точек в полигоне.");
                return;
            }

            List<(double, double)> polygonPoints = new List<(double, double)>();
            for (int i = n + 2; i < n + k + 2; i++)
            {
                var coords = lines[i].Split(' ');
                if (coords.Length != 2 ||
                    !double.TryParse(coords[0], out double x) ||
                    !double.TryParse(coords[1], out double y))
                {
                    MessageBox.Show($"Неверные координаты точки {i - n - 1}.");
                    return;
                }

                // Преобразуем координаты в систему координат канваса
                polygonPoints.Add((x * 20 * scale + (DrawingCanvas.ActualWidth / 2), -(y * 20 * scale) + (DrawingCanvas.ActualHeight / 2)));
            }

            // Рисуем исходные отрезки
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                DrawLine(segment.X1, segment.Y1, segment.X2, segment.Y2, segmentColors[i]); // Используем цвет отрезка
            }

            // Рисуем полигон
            DrawPolygon(polygonPoints, Brushes.Blue);

            // Отсечение отрезков
            int index = 0; // Индекс для отображения точек отсечения
            foreach (var segment in segments)
            {
                var clippedPoints = ClipSegmentWithPolygon(segment.X1, segment.Y1, segment.X2, segment.Y2, polygonPoints);

                if (clippedPoints != null && clippedPoints.Length > 0)
                {
                    // Проверяем количество точек отсечения
                    if (clippedPoints.Length % 2 == 0) // Четное количество точек
                    {
                        for (int i = 0; i < clippedPoints.Length - 1; i += 2)
                        {
                            double x1 = clippedPoints[i];
                            double y1 = clippedPoints[i + 1];
                            double x2 = (i + 2 < clippedPoints.Length) ? clippedPoints[i + 2] : x1;
                            double y2 = (i + 3 < clippedPoints.Length) ? clippedPoints[i + 3] : y1;

                            DrawLine(x1, y1, x2, y2, Brushes.Red); // Рисуем отсечённую часть
                            DisplayClippingPoints2((x1, y1, x2, y2), ref index); // Передаем индекс
                        }
                    }
                    else // Нечетное количество точек
                    {
                        if (clippedPoints.Length == 2) // Только одна точка отсечения
                        {
                            // Получаем координаты точки отсечения
                            double x1 = clippedPoints[0];
                            double y1 = clippedPoints[1];

                            // Проверяем, какая конечная точка находится внутри полигона
                            bool isX1Inside = IsPointInPolygon(segment.X1, segment.Y1, polygonPoints);
                            bool isX2Inside = IsPointInPolygon(segment.X2, segment.Y2, polygonPoints);

                            double endX = 0, endY = 0;

                            if (isX1Inside && !isX2Inside) // Если первая конечная точка внутри
                            {
                                endX = segment.X1;
                                endY = segment.Y1;
                            }
                            else if (isX2Inside && !isX1Inside) // Если вторая конечная точка внутри
                            {
                                endX = segment.X2;
                                endY = segment.Y2;
                            }
                            else
                            {
                                // Если обе точки внутри или обе снаружи, ничего не рисуем
                                continue;
                            }

                            // Рисуем закрашенную часть между точкой отсечения и конечной точкой
                            DrawLine(x1, y1, endX, endY, Brushes.Red);
                            DisplayClippingPoints2((x1, y1, endX, endY), ref index); // Передаем индекс
                        }
                        else // Если больше одной точки
                        {
                            for (int i = 0; i < clippedPoints.Length; i += 2)
                            {
                                if (i + 1 < clippedPoints.Length)
                                {
                                    double x1 = clippedPoints[i];
                                    double y1 = clippedPoints[i + 1];
                                    DrawPoint(x1, y1, Brushes.Black);
                                    DisplayClippingPoints2((x1, y1, x1, y1), ref index); // Передаем индекс
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsPointInPolygon(double x, double y, List<(double, double)> polygonPoints)
        {
            int n = polygonPoints.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var (xi, yi) = polygonPoints[i];
                var (xj, yj) = polygonPoints[j];

                if ((yi > y) != (yj > y) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }
            }

            return inside;
        }


        private double[] ClipSegmentWithPolygon(double x1, double y1, double x2, double y2, List<(double, double)> polygon)
        {
            List<double> clippedPoints = new List<double>();

            for (int i = 0; i < polygon.Count; i++)
            {
                var edgeStart = polygon[i];
                var edgeEnd = polygon[(i + 1) % polygon.Count];

                if (TryFindIntersection(x1, y1, x2, y2, edgeStart.Item1, edgeStart.Item2, edgeEnd.Item1, edgeEnd.Item2, out double tx, out double ty))
                {
                    clippedPoints.Add(tx);
                    clippedPoints.Add(ty);
                }
            }

            // Возвращаем массив точек отсечения
            return clippedPoints.ToArray();
        }

        private bool TryFindIntersection(double x1, double y1, double x2, double y2,
                                          double x3, double y3, double x4, double y4,
                                          out double tx, out double ty)
        {
            double denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Math.Abs(denom) < double.Epsilon)
            {
                tx = ty = 0;
                return false; // Параллельные прямые
            }

            double ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
            double ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

            if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
            {
                tx = ty = 0;
                return false; // Пересечение вне отрезков
            }

            tx = x1 + ua * (x2 - x1);
            ty = y1 + ua * (y2 - y1);
            return true; // Пересечение найдено
        }

        private void DrawPolygon(List<(double, double)> points, Brush color)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var start = points[i];
                var end = points[(i + 1) % points.Count];
                DrawLine(start.Item1, start.Item2, end.Item1, end.Item2, color);
            }
        }

        private void DisplayClippingPoints2((double, double, double, double) clippedSegment, ref int index)
        {
            double xClipped1 = clippedSegment.Item1;
            double yClipped1 = clippedSegment.Item2;
            double xClipped2 = clippedSegment.Item3;
            double yClipped2 = clippedSegment.Item4;

            // Преобразуем координаты отсечённых точек обратно в исходную систему координат
            double originalX1 = (xClipped1 - (DrawingCanvas.ActualWidth / 2)) / (20 * scale);
            double originalY1 = -(yClipped1 - (DrawingCanvas.ActualHeight / 2)) / (20 * scale);
            double originalX2 = (xClipped2 - (DrawingCanvas.ActualWidth / 2)) / (20 * scale);
            double originalY2 = -(yClipped2 - (DrawingCanvas.ActualHeight / 2)) / (20 * scale);

            // Рисуем точки отсечения
            DrawPoint(xClipped1, yClipped1, Brushes.Black);
            DrawPoint(xClipped2, yClipped2, Brushes.Black);

            // Добавляем текстовые блоки с координатами для первой точки
            var textBlock1 = new TextBlock
            {
                Text = $"C{index}({originalX1:F2}, {originalY1:F2})",
                Foreground = Brushes.Black,
                FontSize = 12,
                Margin = new Thickness(xClipped1 + 3, yClipped1 - 20, 0, 0)
            };
            DrawingCanvas.Children.Add(textBlock1);

            index++; // Увеличиваем индекс для следующей точки

            // Добавляем текстовые блоки с координатами для второй точки
            var textBlock2 = new TextBlock
            {
                Text = $"C{index}({originalX2:F2}, {originalY2:F2})",
                Foreground = Brushes.Black,
                FontSize = 12,
                Margin = new Thickness(xClipped2 + 3, yClipped2 - 20, 0, 0)
            };
            DrawingCanvas.Children.Add(textBlock2);

            index++; // Увеличиваем индекс для следующей точки
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Brush color)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = color,
                StrokeThickness = 2
            };
            DrawingCanvas.Children.Add(line);
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scale = e.NewValue; // Обновляем масштаб
            DrawGrid(); // Перерисовываем сетку с новым масштабом
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Выберите текстовый файл"
            };

            // Если пользователь выбрал файл и нажал ОК
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Читаем содержимое файла
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    // Записываем содержимое в текстовое поле
                    InputTextBox.Text = fileContent;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
                }
            }
        }
    }
}