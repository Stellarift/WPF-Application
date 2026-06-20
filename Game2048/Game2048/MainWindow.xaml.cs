using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Game2048
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 4;
        private int[,] grid = new int[GridSize, GridSize];
        private Border[,] tileVisuals = new Border[GridSize, GridSize];

        private int currentScore = 0;
        private int bestScore = 0;
        private Random random = new Random();
        private bool hasWonThisSession = false;
        private bool isAnimating = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBackgroundGrid();
            LoadBestScore();

            this.Loaded += (s, e) => NewGame();
        }

        private void InitializeBackgroundGrid()
        {
            BackgroundGrid.Children.Clear();
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                var emptyCell = new Border
                {
                    Margin = new Thickness(4),
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(205, 193, 180))
                };
                BackgroundGrid.Children.Add(emptyCell);
            }
        }

        private void NewGame()
        {
            ClearAnimationCanvas();
            grid = new int[GridSize, GridSize];
            tileVisuals = new Border[GridSize, GridSize];
            currentScore = 0;
            hasWonThisSession = false;
            isAnimating = false;

            UpdateScoreDisplay();
            AddRandomTile();
            AddRandomTile();
        }

        // Безопасная очистка Canvas для предотвращения утечек памяти 
        private void ClearAnimationCanvas()
        {
            foreach (UIElement child in AnimationCanvas.Children)
            {
                if (child is Border border)
                {
                    border.BeginAnimation(Canvas.LeftProperty, null);
                    border.BeginAnimation(Canvas.TopProperty, null);
                    if (border.RenderTransform is ScaleTransform scale)
                    {
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                    }
                }
            }
            AnimationCanvas.Children.Clear();
        }

        private void UpdateScoreDisplay()
        {
            txtScore.Text = currentScore.ToString();
            txtBest.Text = bestScore.ToString();
        }

        private void Reset_Click(object sender, RoutedEventArgs e) => NewGame();
        private void NewGame_Click(object sender, RoutedEventArgs e) => NewGame();

        // Динамический расчет координат плитка встанет поверх серой подложки
        private Point GetCellCoordinates(int row, int col)
        {
            if (BackgroundGrid.Children.Count == 0) return new Point(0, 0);

            int index = row * GridSize + col;
            Border backgroundCell = (Border)BackgroundGrid.Children[index];

            // Вычисление позиции серой ячейки относительно игрового поля Grid
            UIElement container = (UIElement)BackgroundGrid.Parent;
            Point relativePoint = backgroundCell.TransformToAncestor(container).Transform(new Point(0, 0));

            return relativePoint;
        }

        private void AddRandomTile()
        {
            var empty = GetEmptyCells();
            if (empty.Count == 0) return;

            var cell = empty[random.Next(empty.Count)];
            int val = random.Next(10) < 9 ? 2 : 4;

            grid[cell.Item1, cell.Item2] = val;

            Border border = CreateTileVisual(val);
            tileVisuals[cell.Item1, cell.Item2] = border;

            Point pos = GetCellCoordinates(cell.Item1, cell.Item2);
            Canvas.SetLeft(border, pos.X);
            Canvas.SetTop(border, pos.Y);

            AnimationCanvas.Children.Add(border);
            AnimateSpawn(border);
        }

        private List<Tuple<int, int>> GetEmptyCells()
        {
            var empty = new List<Tuple<int, int>>();
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    if (grid[i, j] == 0)
                        empty.Add(Tuple.Create(i, j));
            return empty;
        }

        private Border CreateTileVisual(int value)
        {
            // Берем точный размер фоновой ячейки, чтобы плитка идеально её перекрывала
            double size = 77.0;
            if (BackgroundGrid.Children.Count > 0)
            {
                size = ((Border)BackgroundGrid.Children[0]).ActualWidth;
                if (size <= 0) size = 77.0;
            }

            var border = new Border
            {
                Width = size,
                Height = size,
                CornerRadius = new CornerRadius(6),
                RenderTransformOrigin = new Point(0.5, 0.5),
                Background = GetTileColor(value),
                RenderTransform = new ScaleTransform(1, 1)
            };

            var textBlock = new TextBlock
            {
                Text = value == 0 ? "" : value.ToString(),
                FontSize = GetFontSize(value),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = GetTextColor(value)
            };

            border.Child = textBlock;
            return border;
        }

        private void AnimateMove(Border tile, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (tile == null) return;

            Point fromPos = GetCellCoordinates(fromRow, fromCol);
            Point toPos = GetCellCoordinates(toRow, toCol);

            var duration = TimeSpan.FromMilliseconds(90); // Быстрая и плавная анимация
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var animX = new DoubleAnimation(fromPos.X, toPos.X, duration) { EasingFunction = ease };
            var animY = new DoubleAnimation(fromPos.Y, toPos.Y, duration) { EasingFunction = ease };

            tile.BeginAnimation(Canvas.LeftProperty, animX);
            tile.BeginAnimation(Canvas.TopProperty, animY);
        }

        private void AnimateSpawn(Border tile)
        {
            var scaleAnim = new DoubleAnimation
            {
                From = 0.1,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var scale = (ScaleTransform)tile.RenderTransform;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void AnimateMerge(Border tile)
        {
            var anim = new DoubleAnimation
            {
                From = 1.0,
                To = 1.15,
                Duration = TimeSpan.FromMilliseconds(60),
                AutoReverse = true
            };
            var scale = (ScaleTransform)tile.RenderTransform;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void MakeMove(Func<List<MoveInfo>> moveLogic)
        {
            if (isAnimating) return;

            var moves = moveLogic();
            if (moves.Count == 0) return;

            isAnimating = true;

            foreach (var move in moves)
            {
                AnimateMove(move.TileVisual, move.FromRow, move.FromCol, move.ToRow, move.ToCol);
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(95);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                FinalizeMove(moves);
            };
            timer.Start();
        }

        private void FinalizeMove(List<MoveInfo> moves)
        {
            ClearAnimationCanvas();
            tileVisuals = new Border[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int val = grid[i, j];
                    if (val != 0)
                    {
                        var border = CreateTileVisual(val);
                        tileVisuals[i, j] = border;

                        Point pos = GetCellCoordinates(i, j);
                        Canvas.SetLeft(border, pos.X);
                        Canvas.SetTop(border, pos.Y);
                        AnimationCanvas.Children.Add(border);

                        foreach (var m in moves)
                        {
                            if (m.ToRow == i && m.ToCol == j && m.IsMergeTarget)
                            {
                                AnimateMerge(border);
                                break;
                            }
                        }
                    }
                }
            }

            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                SaveBestScore();
            }

            AddRandomTile();
            UpdateScoreDisplay();
            isAnimating = false;

            if (!hasWonThisSession && CheckWin())
            {
                hasWonThisSession = true;
                var result = MessageBox.Show("Поздравляем! Вы достигли 2048!\n\nХотите продолжить?", "Победа!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) NewGame();
            }
            else if (CheckGameOver())
            {
                MessageBox.Show($"Игра окончена!\n\nВаш счёт: {currentScore}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                NewGame();
            }
        }

        private List<MoveInfo> ExecuteMoveLeft()
        {
            var moves = new List<MoveInfo>();
            for (int i = 0; i < GridSize; i++)
            {
                var newRow = new int[GridSize];
                bool[] merged = new bool[GridSize];
                int pos = 0;

                for (int j = 0; j < GridSize; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        if (pos > 0 && newRow[pos - 1] == grid[i, j] && !merged[pos - 1])
                        {
                            var targetVisual = tileVisuals[i, j];
                            moves.Add(new MoveInfo { TileVisual = targetVisual, FromRow = i, FromCol = j, ToRow = i, ToCol = pos - 1, IsMergeTarget = true });

                            newRow[pos - 1] *= 2;
                            currentScore += newRow[pos - 1];
                            merged[pos - 1] = true;
                        }
                        else
                        {
                            var targetVisual = tileVisuals[i, j];
                            moves.Add(new MoveInfo { TileVisual = targetVisual, FromRow = i, FromCol = j, ToRow = i, ToCol = pos, IsMergeTarget = false });

                            newRow[pos] = grid[i, j];
                            pos++;
                        }
                    }
                }

                for (int j = 0; j < GridSize; j++) grid[i, j] = newRow[j];
            }

            moves.RemoveAll(m => m.FromRow == m.ToRow && m.FromCol == m.ToCol);
            return moves;
        }

        private List<MoveInfo> Move(MoveDirection direction)
        {
            int rotations = 0;
            if (direction == MoveDirection.RIGHT) rotations = 2;
            else if (direction == MoveDirection.UP) rotations = 1;
            else if (direction == MoveDirection.DOWN) rotations = 3;

            for (int r = 0; r < rotations; r++) RotateMatrix90();

            var rawMoves = ExecuteMoveLeft();

            int returnRotations = (4 - rotations) % 4;
            for (int r = 0; r < returnRotations; r++) RotateMatrix90();

            var transformedMoves = new List<MoveInfo>();
            foreach (var m in rawMoves)
            {
                var fromCoords = TransformCoordinates(m.FromRow, m.FromCol, rotations);
                var toCoords = TransformCoordinates(m.ToRow, m.ToCol, rotations);

                int realFromRow = fromCoords.Item1;
                int realFromCol = fromCoords.Item2;
                int realToRow = toCoords.Item1;
                int realToCol = toCoords.Item2;

                transformedMoves.Add(new MoveInfo
                {
                    TileVisual = tileVisuals[realFromRow, realFromCol],
                    FromRow = realFromRow,
                    FromCol = realFromCol,
                    ToRow = realToRow,
                    ToCol = realToCol,
                    IsMergeTarget = m.IsMergeTarget
                });
            }

            return transformedMoves;
        }

        private Tuple<int, int> TransformCoordinates(int r, int c, int rotationCount)
        {
            int row = r, col = c;
            for (int i = 0; i < rotationCount; i++)
            {
                int nextRow = col;
                int nextCol = GridSize - 1 - row;
                row = nextRow;
                col = nextCol;
            }
            return Tuple.Create(row, col);
        }

        private void RotateMatrix90()
        {
            int[,] rotatedGrid = new int[GridSize, GridSize];
            Border[,] rotatedVisuals = new Border[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    rotatedGrid[j, GridSize - 1 - i] = grid[i, j];
                    rotatedVisuals[j, GridSize - 1 - i] = tileVisuals[i, j];
                }
            }
            grid = rotatedGrid;
            tileVisuals = rotatedVisuals;
        }

        private bool CheckGameOver()
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (grid[i, j] == 0) return false;
                    if (j < GridSize - 1 && grid[i, j] == grid[i, j + 1]) return false;
                    if (i < GridSize - 1 && grid[i, j] == grid[i + 1, j]) return false;
                }
            }
            return true;
        }

        private bool CheckWin()
        {
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    if (grid[i, j] >= 2048) return true;
            return false;
        }

        private Brush GetTileColor(int value)
        {
            switch (value)
            {
                case 2: return new SolidColorBrush(Color.FromRgb(238, 228, 218));
                case 4: return new SolidColorBrush(Color.FromRgb(237, 224, 200));
                case 8: return new SolidColorBrush(Color.FromRgb(242, 177, 121));
                case 16: return new SolidColorBrush(Color.FromRgb(245, 149, 99));
                case 32: return new SolidColorBrush(Color.FromRgb(246, 124, 95));
                case 64: return new SolidColorBrush(Color.FromRgb(246, 94, 59));
                case 128: return new SolidColorBrush(Color.FromRgb(237, 207, 114));
                case 256: return new SolidColorBrush(Color.FromRgb(237, 204, 97));
                case 512: return new SolidColorBrush(Color.FromRgb(237, 200, 80));
                case 1024: return new SolidColorBrush(Color.FromRgb(237, 197, 63));
                case 2048: return new SolidColorBrush(Color.FromRgb(237, 194, 46));
                default: return new SolidColorBrush(Color.FromRgb(60, 58, 50));
            }
        }

        private Brush GetTextColor(int value)
        {
            if (value <= 4) return new SolidColorBrush(Color.FromRgb(119, 110, 101));
            return new SolidColorBrush(Color.FromRgb(249, 246, 242));
        }

        private int GetFontSize(int value)
        {
            if (value < 10) return 32;
            if (value < 100) return 28;
            if (value < 1000) return 22;
            return 18;
        }

        private void LoadBestScore()
        {
            try
            {
                if (System.IO.File.Exists("highscore.txt"))
                {
                    string content = System.IO.File.ReadAllText("highscore.txt");
                    int.TryParse(content, out bestScore);
                }
            }
            catch { bestScore = 0; }
        }

        private void SaveBestScore()
        {
            try { System.IO.File.WriteAllText("highscore.txt", bestScore.ToString()); }
            catch { }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (isAnimating) return;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.A:
                    MakeMove(() => Move(MoveDirection.LEFT));
                    break;
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.D:
                    MakeMove(() => Move(MoveDirection.RIGHT));
                    break;
                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.W:
                    MakeMove(() => Move(MoveDirection.UP));
                    break;
                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.S:
                    MakeMove(() => Move(MoveDirection.DOWN));
                    break;
            }
        }
    }

    public enum MoveDirection { LEFT, RIGHT, UP, DOWN }

    public class MoveInfo
    {
        public Border TileVisual { get; set; }
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public bool IsMergeTarget { get; set; }
    }
}