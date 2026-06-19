using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SurveyApp 
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Пожалуйста, заполните поле 'ФИО'", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            // Проверка согласия
            if (chkAgree.IsChecked != true)
            {
                MessageBox.Show("Пожалуйста, дайте согласие на обработку данных", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сбор данных
            StringBuilder result = new StringBuilder();
            result.AppendLine("-----------------------------------------------");
            result.AppendLine("           РЕЗУЛЬТАТЫ АНКЕТЫ");
            result.AppendLine("-----------------------------------------------");
            result.AppendLine();

            // ФИО
            result.AppendLine($"ФИО: {txtFullName.Text.Trim()}");
            result.AppendLine();

            // Возраст
            if (int.TryParse(txtAge.Text, out int age))
            {
                result.AppendLine($"Возраст: {age} лет");
            }
            else
            {
                result.AppendLine($"Возраст: {txtAge.Text}");
            }
            result.AppendLine();

            // Пол
            string gender = rbMale.IsChecked == true ? "Мужской" : "Женский";
            result.AppendLine($"Пол: {gender}");
            result.AppendLine();

            // Город
            if (cmbCity.SelectedItem is ComboBoxItem selectedCity)
            {
                result.AppendLine($"Город: {selectedCity.Content}");
            }
            result.AppendLine();

            // Интересы
            var selectedInterests = lstInterests.SelectedItems.Cast<ListBoxItem>()
                                                  .Select(item => item.Content.ToString())
                                                  .ToList();
            if (selectedInterests.Any())
            {
                result.AppendLine($"Интересы: {string.Join(", ", selectedInterests)}");
            }
            else
            {
                result.AppendLine("Интересы: не указаны");
            }
            result.AppendLine();

            // О себе
            if (!string.IsNullOrWhiteSpace(txtAbout.Text))
            {
                result.AppendLine($"О себе: {txtAbout.Text.Trim()}");
            }
            else
            {
                result.AppendLine("О себе: не указано");
            }
            result.AppendLine();

            result.AppendLine("-----------------------------------------------");
            result.AppendLine($"Дата заполнения: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            // Вывод результата
            txtResult.Text = result.ToString();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех полей
            txtFullName.Clear();
            txtAge.Text = "18";
            rbMale.IsChecked = true;
            cmbCity.SelectedIndex = 0;
            lstInterests.UnselectAll();
            txtAbout.Clear();
            chkAgree.IsChecked = false;
            txtResult.Text = "Результаты будут отображены здесь...";
        }
    }
}