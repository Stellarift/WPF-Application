using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace TextEditor
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = "";
        private bool isDirty = false;

        public MainWindow()
        {
            InitializeComponent();
            UpdateTitle();
            UpdateStatus();
        }

        #region Меню "Файл"

        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            if (isDirty)
            {
                var result = MessageBox.Show("Сохранить изменения?", "Текстовый редактор",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    mnuSave_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            txtEditor.Clear();
            currentFilePath = "";
            isDirty = false;
            UpdateTitle();
            UpdateStatus();
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
                    currentFilePath = openFileDialog.FileName;
                    isDirty = false;
                    UpdateTitle();
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                mnuSaveAs_Click(sender, e);
                return;
            }

            try
            {
                File.WriteAllText(currentFilePath, txtEditor.Text);
                isDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.DefaultExt = "txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
                    currentFilePath = saveFileDialog.FileName;
                    isDirty = false;
                    UpdateTitle();
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Меню "Правка"

        private void mnuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditor.CanUndo)
            {
                txtEditor.Undo();
            }
        }

        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            txtEditor.Cut();
        }

        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            txtEditor.Copy();
        }

        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            txtEditor.Paste();
        }

        private void mnuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            txtEditor.SelectAll();
        }

        #endregion

        #region Меню "Настройки"

        private void mnuFont_Click(object sender, RoutedEventArgs e)
        {
            
            MessageBox.Show("Функция выбора шрифта.\n\n" +
                           "Текущий шрифт: " + txtEditor.FontFamily.Source + "\n" +
                           "Размер: " + txtEditor.FontSize,
                           "Настройки шрифта", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void mnuColor_Click(object sender, RoutedEventArgs e)
        {
            
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (txtEditor.Background is SolidColorBrush brush)
            {
                System.Drawing.Color currentColor = System.Drawing.Color.FromArgb(
                    brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                colorDialog.Color = currentColor;
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtEditor.Background = new SolidColorBrush(
                    Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }

        private void mnuForeColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (txtEditor.Foreground is SolidColorBrush brush)
            {
                System.Drawing.Color currentColor = System.Drawing.Color.FromArgb(
                    brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                colorDialog.Color = currentColor;
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtEditor.Foreground = new SolidColorBrush(
                    Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }

        #endregion

        #region Вспомогательные методы

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath)
                ? "(новый документ)"
                : System.IO.Path.GetFileName(currentFilePath);

            string dirtyMarker = isDirty ? "*" : "";
            this.Title = $"{fileName}{dirtyMarker} - Текстовый редактор";

            statusPath.Text = $"Путь: {currentFilePath}";
        }

        private void UpdateStatus()
        {
            if (txtEditor.Text.Length > 0)
            {
                int line = txtEditor.GetLineIndexFromCharacterIndex(txtEditor.CaretIndex) + 1;
                int col = txtEditor.CaretIndex - txtEditor.GetCharacterIndexFromLineIndex(line - 1) + 1;
                statusCursor.Text = $"Строка: {line}, Колонка: {col}";
            }
            else
            {
                statusCursor.Text = "Строка: 1, Колонка: 1";
            }
            statusChars.Text = $"Символов: {txtEditor.Text.Length}";
        }

        private void txtEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            isDirty = true;
            UpdateTitle();
            UpdateStatus();
        }

        private void txtEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatus();
        }

        #endregion

        #region Закрытие окна

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isDirty)
            {
                var result = MessageBox.Show("Сохранить изменения перед выходом?", "Текстовый редактор",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    mnuSave_Click(this, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }

        #endregion
    }
}