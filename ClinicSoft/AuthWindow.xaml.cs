using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClinicSoft
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private bool _isLoginMode = true;

        public AuthWindow()
        {
            InitializeComponent();
            UpdateMode();
        }

        private void UpdateMode()
        {
            if (_isLoginMode)
            {
                Header.Text = "Вход в систему";
                ActionButton.Content = "Войти";
                ToggleLabel.Text = "Нет аккаунта?";
                ToggleModeButton.Content = "Зарегистрироваться";
            }
            else
            {
                Header.Text = "Регистрация";
                ActionButton.Content = "Создать аккаунт";
                ToggleLabel.Text = "Уже есть аккаунт?";
                ToggleModeButton.Content = "Войти";
            }

            ErrorMessage.Text = "";
            LoginBox.Clear();
            PasswordBox.Clear();
            LoginPlaceholder.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Visible;

            // Снимаем фокус, чтобы placeholder'ы отображались корректно
            this.Focus();
        }

        private void ToggleModeButton_Click(object sender, RoutedEventArgs e)
        {
            _isLoginMode = !_isLoginMode;
            UpdateMode();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Заполните все поля.";
                return;
            }

            if (_isLoginMode)
            {
                // TODO: реализовать вход через EF Core
                ErrorMessage.Text = "Функция входа — заглушка.";
            }
            else
            {
                // TODO: реализовать регистрацию
                ErrorMessage.Text = "Функция регистрации — заглушка.";
            }
        }

        // Placeholder для логина
        private void LoginBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginBox.Text)
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoginBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // Placeholder для пароля
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordBox.SecurePassword.Length == 0
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordBox.SecurePassword.Length == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
