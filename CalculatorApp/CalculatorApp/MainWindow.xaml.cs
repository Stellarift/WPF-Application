using System;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        private double result = 0;           // Результат предыдущей операции
        private string currentOperator = ""; // Текущий оператор (+, -, *, /)
        private bool newInput = true;        // Флаг нового ввода

        public MainWindow()
        {
            InitializeComponent();
            txtCurrent.Text = "0";
            txtExpression.Text = "";
        }

        // Цифры 0-9
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string number = btn.Content.ToString();

            if (newInput)
            {
                txtCurrent.Text = number;
                newInput = false;
            }
            else
            {
                // Нельзя начинать с нескольких нулей
                if (txtCurrent.Text == "0" && number == "0")
                    return;

                if (txtCurrent.Text == "0")
                    txtCurrent.Text = number;
                else
                    txtCurrent.Text += number;
            }
        }

        // Десятичная точка
        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (newInput)
            {
                txtCurrent.Text = "0.";
                newInput = false;
            }
            else
            {
                if (!txtCurrent.Text.Contains("."))
                    txtCurrent.Text += ".";
            }
        }

        // Операторы (+, -, *, /)
        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string op = btn.Content.ToString();

            if (currentOperator != "" && !newInput)
            {
                Calculate();
            }

            result = double.Parse(txtCurrent.Text);
            currentOperator = op;
            txtExpression.Text = $"{result} {op}";
            newInput = true;
        }

        // Вычисление
        private void Calculate()
        {
            double currentNumber = double.Parse(txtCurrent.Text);

            switch (currentOperator)
            {
                case "+":
                    result += currentNumber;
                    break;
                case "-":
                    result -= currentNumber;
                    break;
                case "*":
                    result *= currentNumber;
                    break;
                case "/":
                    if (currentNumber != 0)
                        result /= currentNumber;
                    else
                    {
                        MessageBox.Show("Ошибка: Деление на ноль!", "Калькулятор",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        result = 0;
                    }
                    break;
            }

            txtCurrent.Text = result.ToString();
            currentOperator = "";
        }

        // Кнопка "="
        private void Equal_Click(object sender, RoutedEventArgs e)
        {
            if (currentOperator != "")
            {
                Calculate();
                txtExpression.Text = "";
                newInput = true;
            }
        }

        // CE - очистить текущее число
        private void CE_Click(object sender, RoutedEventArgs e)
        {
            txtCurrent.Text = "0";
            newInput = true;
        }

        // C - очистить всё
        private void C_Click(object sender, RoutedEventArgs e)
        {
            txtCurrent.Text = "0";
            txtExpression.Text = "";
            result = 0;
            currentOperator = "";
            newInput = true;
        }

        // ⌫ - удалить последний символ
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (!newInput && txtCurrent.Text.Length > 1)
            {
                txtCurrent.Text = txtCurrent.Text.Substring(0, txtCurrent.Text.Length - 1);
            }
            else if (!newInput && txtCurrent.Text.Length == 1)
            {
                txtCurrent.Text = "0";
                newInput = true;
            }
        }
    }
}
