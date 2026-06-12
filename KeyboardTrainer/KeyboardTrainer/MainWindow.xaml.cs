using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardTrainer
{
    public partial class MainWindow : Window
    {
        private string targetText = "";
        private string inputText = "";
        private int errors = 0;
        private int correctChars = 0;
        private DateTime startTime;
        private bool isTraining = false;
        private bool shiftPressed = false;
        private bool capsLockOn = false;

        // Словарь для сопоставления клавиш с кнопками
        private Dictionary<Key, Button> keyButtonMap = new Dictionary<Key, Button>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyMap();
            UpdateKeyboardLabels();
            sliderLength.ValueChanged += SliderLength_ValueChanged;
        }

        private void InitializeKeyMap()
        {
            // Буквы
            keyButtonMap[Key.Q] = btnQ; keyButtonMap[Key.W] = btnW; keyButtonMap[Key.E] = btnE;
            keyButtonMap[Key.R] = btnR; keyButtonMap[Key.T] = btnT; keyButtonMap[Key.Y] = btnY;
            keyButtonMap[Key.U] = btnU; keyButtonMap[Key.I] = btnI; keyButtonMap[Key.O] = btnO;
            keyButtonMap[Key.P] = btnP; keyButtonMap[Key.A] = btnA; keyButtonMap[Key.S] = btnS;
            keyButtonMap[Key.D] = btnD; keyButtonMap[Key.F] = btnF; keyButtonMap[Key.G] = btnG;
            keyButtonMap[Key.H] = btnH; keyButtonMap[Key.J] = btnJ; keyButtonMap[Key.K] = btnK;
            keyButtonMap[Key.L] = btnL; keyButtonMap[Key.Z] = btnZ; keyButtonMap[Key.X] = btnX;
            keyButtonMap[Key.C] = btnC; keyButtonMap[Key.V] = btnV; keyButtonMap[Key.B] = btnB;
            keyButtonMap[Key.N] = btnN; keyButtonMap[Key.M] = btnM;

            // Цифры
            keyButtonMap[Key.D1] = btn1; keyButtonMap[Key.D2] = btn2; keyButtonMap[Key.D3] = btn3;
            keyButtonMap[Key.D4] = btn4; keyButtonMap[Key.D5] = btn5; keyButtonMap[Key.D6] = btn6;
            keyButtonMap[Key.D7] = btn7; keyButtonMap[Key.D8] = btn8; keyButtonMap[Key.D9] = btn9;
            keyButtonMap[Key.D0] = btn0;

            // Специальные символы
            keyButtonMap[Key.OemMinus] = btnMinus;
            keyButtonMap[Key.OemPlus] = btnEquals;
            keyButtonMap[Key.OemOpenBrackets] = btnLBracket;
            keyButtonMap[Key.OemCloseBrackets] = btnRBracket;
            keyButtonMap[Key.OemBackslash] = btnBackslash;
            keyButtonMap[Key.OemSemicolon] = btnSemicolon;
            keyButtonMap[Key.OemQuotes] = btnQuote;
            keyButtonMap[Key.OemComma] = btnComma;
            keyButtonMap[Key.OemPeriod] = btnPeriod;
            keyButtonMap[Key.OemQuestion] = btnSlash;

            // Служебные клавиши
            keyButtonMap[Key.Space] = btnSpace;
            keyButtonMap[Key.Back] = btnBackspace;
            keyButtonMap[Key.Enter] = btnEnter;
            keyButtonMap[Key.Tab] = btnTab;
            keyButtonMap[Key.Escape] = btnEsc;
            keyButtonMap[Key.Capital] = btnCapsLock;
            keyButtonMap[Key.LeftShift] = btnShiftL;
            keyButtonMap[Key.RightShift] = btnShiftR;
            keyButtonMap[Key.LeftCtrl] = btnCtrlL;
            keyButtonMap[Key.RightCtrl] = btnCtrlR;
            keyButtonMap[Key.LeftAlt] = btnAltL;
            keyButtonMap[Key.RightAlt] = btnAltR;
            keyButtonMap[Key.LWin] = btnWinL;
            keyButtonMap[Key.RWin] = btnWinR;
        }

        private void SliderLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtLengthValue.Text = ((int)e.NewValue).ToString();
        }

        private void UpdateKeyboardLabels()
        {
            bool isUpper = shiftPressed ^ capsLockOn;
            string shiftState = isUpper ? "SHIFT" : "";

            var labelMap = new Dictionary<Button, (string normal, string shifted)>
            {
                [btn1] = ("1", "!"),
                [btn2] = ("2", "@"),
                [btn3] = ("3", "#"),
                [btn4] = ("4", "$"),
                [btn5] = ("5", "%"),
                [btn6] = ("6", "^"),
                [btn7] = ("7", "&"),
                [btn8] = ("8", "*"),
                [btn9] = ("9", "("),
                [btn0] = ("0", ")"),
                [btnMinus] = ("-", "_"),
                [btnEquals] = ("=", "+"),
                [btnLBracket] = ("[", "{"),
                [btnRBracket] = ("]", "}"),
                [btnBackslash] = ("\\", "|"),
                [btnSemicolon] = (";", ":"),
                [btnQuote] = ("'", "\""),
                [btnComma] = (",", "<"),
                [btnPeriod] = (".", ">"),
                [btnSlash] = ("/", "?"),
                [btnQ] = ("q", "Q"),
                [btnW] = ("w", "W"),
                [btnE] = ("e", "E"),
                [btnR] = ("r", "R"),
                [btnT] = ("t", "T"),
                [btnY] = ("y", "Y"),
                [btnU] = ("u", "U"),
                [btnI] = ("i", "I"),
                [btnO] = ("o", "O"),
                [btnP] = ("p", "P"),
                [btnA] = ("a", "A"),
                [btnS] = ("s", "S"),
                [btnD] = ("d", "D"),
                [btnF] = ("f", "F"),
                [btnG] = ("g", "G"),
                [btnH] = ("h", "H"),
                [btnJ] = ("j", "J"),
                [btnK] = ("k", "K"),
                [btnL] = ("l", "L"),
                [btnZ] = ("z", "Z"),
                [btnX] = ("x", "X"),
                [btnC] = ("c", "C"),
                [btnV] = ("v", "V"),
                [btnB] = ("b", "B"),
                [btnN] = ("n", "N"),
                [btnM] = ("m", "M")
            };

            foreach (var kvp in labelMap)
            {
                kvp.Key.Content = isUpper ? kvp.Value.shifted : kvp.Value.normal;
            }
        }

        private void GenerateText()
        {
            int length = (int)sliderLength.Value;
            Random rand = new Random();
            StringBuilder sb = new StringBuilder();

            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[rand.Next(chars.Length)]);
            }

            targetText = sb.ToString();
            txtTargetText.Text = targetText;
        }

        private void HighlightKey(Key key, bool highlight)
        {
            if (keyButtonMap.ContainsKey(key))
            {
                var button = keyButtonMap[key];
                if (highlight)
                    button.Background = Brushes.Gold;
                else
                    button.Background = (Brush)Application.Current.Resources["Button.Static.Background"] ?? Brushes.LightGray;
            }
        }

        private void ProcessKeyPress(Key key)
        {
            if (!isTraining || string.IsNullOrEmpty(targetText)) return;

            HighlightKey(key, true);

            string keyChar = GetCharFromKey(key);

            if (key == Key.Back && inputText.Length > 0)
            {
                inputText = inputText.Substring(0, inputText.Length - 1);
                txtInputText.Text = GetColoredInput();
                UpdateStats();
            }
            else if (key == Key.Enter)
            {
                // Игнор Enter
            }
            else if (keyChar != null)
            {
                if (inputText.Length < targetText.Length)
                {
                    char expectedChar = targetText[inputText.Length];
                    char actualChar = keyChar[0];

                    if (chkCaseSensitive.IsChecked == true)
                    {
                        if (actualChar == expectedChar)
                        {
                            inputText += actualChar;
                            correctChars++;
                        }
                        else
                        {
                            errors++;
                            inputText += actualChar;
                        }
                    }
                    else
                    {
                        if (char.ToLower(actualChar) == char.ToLower(expectedChar))
                        {
                            inputText += expectedChar;
                            correctChars++;
                        }
                        else
                        {
                            errors++;
                            inputText += expectedChar;
                        }
                    }

                    txtInputText.Text = GetColoredInput();
                    UpdateStats();

                    if (inputText.Length == targetText.Length)
                    {
                        StopTraining();
                        MessageBox.Show($"Поздравляем! Вы завершили ввод!\n\n" +
                                       $"Правильно: {correctChars}\n" +
                                       $"Ошибок: {errors}\n" +
                                       $"Скорость: {CalculateSpeed()} зн/мин",
                                       "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private string GetCharFromKey(Key key)
        {
            bool isUpper = shiftPressed ^ capsLockOn;

            var charMap = new Dictionary<Key, string>
            {
                [Key.Space] = " ",
                [Key.OemPeriod] = isUpper ? ">" : ".",
                [Key.OemComma] = isUpper ? "<" : ",",
                [Key.OemQuestion] = isUpper ? "?" : "/",
                [Key.OemSemicolon] = isUpper ? ":" : ";",
                [Key.OemQuotes] = isUpper ? "\"" : "'",
                [Key.OemMinus] = isUpper ? "_" : "-",
                [Key.OemPlus] = isUpper ? "+" : "=",
                [Key.OemOpenBrackets] = isUpper ? "{" : "[",
                [Key.OemCloseBrackets] = isUpper ? "}" : "]",
                [Key.OemBackslash] = isUpper ? "|" : "\\"
            };

            // Цифры с Shift
            var digitMap = new Dictionary<Key, string>
            {
                [Key.D1] = isUpper ? "!" : "1",
                [Key.D2] = isUpper ? "@" : "2",
                [Key.D3] = isUpper ? "#" : "3",
                [Key.D4] = isUpper ? "$" : "4",
                [Key.D5] = isUpper ? "%" : "5",
                [Key.D6] = isUpper ? "^" : "6",
                [Key.D7] = isUpper ? "&" : "7",
                [Key.D8] = isUpper ? "*" : "8",
                [Key.D9] = isUpper ? "(" : "9",
                [Key.D0] = isUpper ? ")" : "0"
            };

            if (charMap.ContainsKey(key))
                return charMap[key];
            if (digitMap.ContainsKey(key))
                return digitMap[key];

            string keyStr = key.ToString();
            if (keyStr.Length == 1 && char.IsLetter(keyStr[0]))
            {
                char c = keyStr[0];
                if (isUpper)
                    return char.ToUpper(c).ToString();
                else
                    return char.ToLower(c).ToString();
            }

            return null;
        }

        private string GetColoredInput()
        {
            if (string.IsNullOrEmpty(targetText)) return "";

            var result = new System.Text.StringBuilder();

            for (int i = 0; i < targetText.Length; i++)
            {
                if (i < inputText.Length)
                {
                    if (chkCaseSensitive.IsChecked == true)
                    {
                        if (inputText[i] == targetText[i])
                            result.Append($"<Run Foreground=\"Green\" Text=\"{inputText[i]}\"/>");
                        else
                            result.Append($"<Run Foreground=\"Red\" Text=\"{inputText[i]}\"/>");
                    }
                    else
                    {
                        if (char.ToLower(inputText[i]) == char.ToLower(targetText[i]))
                            result.Append($"<Run Foreground=\"Green\" Text=\"{targetText[i]}\"/>");
                        else
                            result.Append($"<Run Foreground=\"Red\" Text=\"{targetText[i]}\"/>");
                    }
                }
                else
                {
                    result.Append($"<Run Foreground=\"Gray\" Text=\"{targetText[i]}\"/>");
                }
            }

            return $"<Span>{result}</Span>";
        }

        private void UpdateStats()
        {
            int typedChars = inputText.Length;
            double elapsedMinutes = (DateTime.Now - startTime).TotalMinutes;
            double speed = typedChars / elapsedMinutes;

            txtSpeed.Text = $"Скорость: {(int)speed} зн/мин";
            txtErrors.Text = $"Ошибки: {errors}";
            txtCorrect.Text = $"Правильно: {correctChars}";
        }

        private int CalculateSpeed()
        {
            double elapsedMinutes = (DateTime.Now - startTime).TotalMinutes;
            if (elapsedMinutes <= 0) return 0;
            return (int)(inputText.Length / elapsedMinutes);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            GenerateText();
            inputText = "";
            errors = 0;
            correctChars = 0;
            startTime = DateTime.Now;
            isTraining = true;
            txtInputText.Text = "";

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            sliderLength.IsEnabled = false;
            chkCaseSensitive.IsEnabled = false;

            UpdateStats();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopTraining();
        }

        private void StopTraining()
        {
            isTraining = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            sliderLength.IsEnabled = true;
            chkCaseSensitive.IsEnabled = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            StopTraining();
            targetText = "";
            inputText = "";
            errors = 0;
            correctChars = 0;
            txtTargetText.Text = "";
            txtInputText.Text = "";
            txtSpeed.Text = "Скорость: 0 зн/мин";
            txtErrors.Text = "Ошибки: 0";
            txtCorrect.Text = "Правильно: 0";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = true;
                UpdateKeyboardLabels();
            }
            if (e.Key == Key.Capital)
            {
                capsLockOn = !capsLockOn;
                UpdateKeyboardLabels();
            }

            ProcessKeyPress(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = false;
                UpdateKeyboardLabels();
            }

            HighlightKey(e.Key, false);
        }
    }
}
