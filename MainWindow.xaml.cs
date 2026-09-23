using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _inputPath = Path.Combine(AppContext.BaseDirectory, "input.txt");
        private readonly string _outputPath = Path.Combine(AppContext.BaseDirectory, "output.txt");
        private GraphNode[,]? _graph;
        private bool[,]? _mines;
        private int[,]? _distances;
        private int _rows;
        private int _columns;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBoard();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBoard();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_distances is null)
                {
                    LoadBoard();
                    return;
                }

                WriteResult();
                StatusText.Text = $"Результат збережено у {_outputPath}";
            }
            catch (Exception exception)
            {
                ShowError(exception.Message);
            }
        }

        private void LoadBoard()
        {
            try
            {
                string input = File.ReadAllText(_inputPath);
                (int rows, int columns, bool[,] mines) = ParseInput(input);

                _rows = rows;
                _columns = columns;
                _mines = mines;
                _graph = BuildGraph(mines);
                _distances = CalculateDistances(_graph, mines);

                RenderBoard();
                WriteResult();
                FileInfoText.Text = $"input.txt: {_rows} × {_columns} | output.txt створено автоматично";
                StatusText.Text = $"Знайдено {CountMines(mines)} мін.";// Використано граф і багатоджерельний BFS.
            }
            catch (Exception exception)
            {
                BoardGrid.Children.Clear();
                FileInfoText.Text = string.Empty;
                ShowError(exception.Message);
            }
        }

        private static (int Rows, int Columns, bool[,] Mines) ParseInput(string input)
        {
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                throw new InvalidDataException("Файл input.txt порожній.");
            }

            string[] size = lines[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (size.Length != 2 || !int.TryParse(size[0], out int rows) || !int.TryParse(size[1], out int columns)
                || rows <= 0 || columns <= 0 || rows > 100 || columns > 100)
            {
                throw new InvalidDataException("Перший рядок має містити N і M у межах від 1 до 100.");
            }

            string cells = string.Concat(lines.Skip(1)).Replace(" ", string.Empty).Replace("\t", string.Empty);
            if (cells.Length != rows * columns || cells.Any(cell => cell is not ('0' or '1')))
            {
                throw new InvalidDataException($"Після розмірів очікується рівно {rows * columns} чисел 0 або 1.");
            }

            bool[,] mines = new bool[rows, columns];
            bool hasMine = false;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    mines[row, column] = cells[row * columns + column] == '1';
                    hasMine |= mines[row, column];
                }
            }

            if (!hasMine)
            {
                throw new InvalidDataException("У таблиці має бути хоча б одна міна (1).");
            }

            return (rows, columns, mines);
        }

        private static GraphNode[,] BuildGraph(bool[,] mines)
        {
            int rows = mines.GetLength(0);
            int columns = mines.GetLength(1);
            GraphNode[,] graph = new GraphNode[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    graph[row, column] = new GraphNode(row, column);
                }
            }

            int[] rowChanges = { -1, 1, 0, 0 };
            int[] columnChanges = { 0, 0, -1, 1 };
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    for (int direction = 0; direction < rowChanges.Length; direction++)
                    {
                        int neighborRow = row + rowChanges[direction];
                        int neighborColumn = column + columnChanges[direction];
                        if (neighborRow >= 0 && neighborRow < rows && neighborColumn >= 0 && neighborColumn < columns)
                        {
                            graph[row, column].Neighbors.Add(graph[neighborRow, neighborColumn]);
                        }
                    }
                }
            }

            return graph;
        }

        private static int[,] CalculateDistances(GraphNode[,] graph, bool[,] mines)
        {
            int rows = graph.GetLength(0);
            int columns = graph.GetLength(1);
            int[,] distances = new int[rows, columns];
            Queue<GraphNode> queue = new();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    distances[row, column] = -1;
                    if (mines[row, column])
                    {
                        distances[row, column] = 0;
                        queue.Enqueue(graph[row, column]);
                    }
                }
            }

            while (queue.Count > 0)
            {
                GraphNode current = queue.Dequeue();
                int currentDistance = distances[current.Row, current.Column];
                foreach (GraphNode neighbor in current.Neighbors)
                {
                    if (distances[neighbor.Row, neighbor.Column] == -1)
                    {
                        distances[neighbor.Row, neighbor.Column] = currentDistance + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return distances;
        }

        private void WriteResult()
        {
            if (_distances is null)
            {
                return;
            }

            StringBuilder output = new();
            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    if (column > 0)
                    {
                        output.Append(' ');
                    }

                    output.Append(_distances[row, column]);
                }

                output.AppendLine();
            }

            File.WriteAllText(_outputPath, output.ToString());
        }

        private void RenderBoard()
        {
            if (_mines is null || _distances is null)
            {
                return;
            }

            BoardGrid.Children.Clear();
            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();
            for (int row = 0; row < _rows; row++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            }

            for (int column = 0; column < _columns; column++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            }

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    bool isMine = _mines[row, column];
                    Button cell = new()
                    {
                        Content = isMine ? "✹" : _distances[row, column].ToString(),
                        FontSize = isMine ? 22 : 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = isMine ? Brushes.White : GetDistanceBrush(_distances[row, column]),
                        Background = isMine ? Brushes.Firebrick : Brushes.White,
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        ToolTip = isMine ? "Міна" : $"Відстань до міни: {_distances[row, column]}"
                    };
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, column);
                    BoardGrid.Children.Add(cell);
                }
            }
        }

        private static Brush GetDistanceBrush(int distance) => distance switch
        {
            0 or 1 => Brushes.DodgerBlue,
            2 or 3 => Brushes.ForestGreen,
            4 or 5 => Brushes.DarkOrange,
            _ => Brushes.Purple
        };

        private static int CountMines(bool[,] mines)
        {
            int count = 0;
            foreach (bool mine in mines)
            {
                if (mine)
                {
                    count++;
                }
            }

            return count;
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private sealed class GraphNode(int row, int column)
        {
            public int Row { get; } = row;
            public int Column { get; } = column;
            public List<GraphNode> Neighbors { get; } = new();
        }
    }
}