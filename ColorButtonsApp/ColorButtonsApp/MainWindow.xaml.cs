using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorButtonsApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                string colorName = clickedButton.Tag.ToString();
                try
                {
                    SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorName);
                    txtInfo.Background = brush;
                    txtInfo.Text = $"Вы выбрали цвет: {colorName}";
                    if (colorName == "Black" || colorName == "Navy" || colorName == "Maroon" || colorName == "Purple")
                        txtInfo.Foreground = Brushes.White;
                    else
                        txtInfo.Foreground = Brushes.Black;
                }
                catch
                {
                    txtInfo.Text = $"Ошибка: не удалось применить цвет {colorName}";
                }
            }
        }
    }
}
