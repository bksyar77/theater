using System;
using System.Windows;
using System.Windows.Controls;
using Theater.Models;

namespace Theater
{
    public partial class AddEventWindow : Window
    {
        public Event NewEvent { get; private set; }

        public AddEventWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Now;
            TitleTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсим время
            DateTime fullDate;
            if (!DateTime.TryParse($"{DatePicker.SelectedDate.Value.ToShortDateString()} {TimeTextBox.Text}", out fullDate))
            {
                MessageBox.Show("Введите корректное время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var typeItem = TypeComboBox.SelectedItem as ComboBoxItem;
            string type = typeItem?.Content?.ToString() ?? "Спектакль";

            NewEvent = new Event
            {
                Title = TitleTextBox.Text.Trim(),
                Date = fullDate,
                Type = type
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}