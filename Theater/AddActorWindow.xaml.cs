using System;
using System.Windows;
using Theater.Models;

namespace Theater
{
    public partial class AddActorWindow : Window
    {
        public Actor NewActor { get; private set; }

        public AddActorWindow()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите имя актера", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(BalanceTextBox.Text, out double balance))
            {
                MessageBox.Show("Введите корректный баланс", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewActor = new Actor
            {
                Name = NameTextBox.Text.Trim(),
                Role = RoleTextBox.Text.Trim(),
                Balance = balance
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