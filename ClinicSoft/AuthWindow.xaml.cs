using System;
using System.Windows;
using ClinicSoft.Data;        // Убедись, что путь к DbContext правильный
using ClinicSoft.Models;
using static BCrypt.Net.BCrypt;            // Для хеширования паролей

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
                // === ВХОД ===
                User? user = null;
                try
                {
                    using (var context = new ClinicSoftContext())
                    {
                        user = context.Users.FirstOrDefault(u => u.Login == login);
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = $"Ошибка подключения к БД: {ex.Message}";
                    return;
                }

                if (user == null || !Verify(password, user.PasswordHash))
                {
                    ErrorMessage.Text = "Неверный логин или пароль.";
                    return;
                }

                // === ПЕРЕХОД В ЗАВИСИМОСТИ ОТ РОЛИ ===
                switch (user.Role)
                {
                    case "admin":
                        new Views.Admin.AdminWindow().Show();
                        break;

                    case "registrator":
                        new Views.Registrator.RegistratorWindow().Show();
                        break;

                    case "doctor":
                        new Views.Doctor.DoctorWindow().Show();
                        break;

                   /* case "nurse":
                        new Views.Nurse.NurseWindow().Show();
                        break;*/

                    case "patient":
                        new Views.Patient.PatientWindow().Show();
                        break;

                    default:
                        ErrorMessage.Text = "Неизвестная роль пользователя.";
                        return;
                }

                this.Close(); // закрываем окно авторизации
            }
            else
            {
                // === РЕГИСТРАЦИЯ (только как пациент) ===
                try
                {
                    using (var context = new ClinicSoftContext())
                    {
                        if (context.Users.Any(u => u.Login == login))
                        {
                            ErrorMessage.Text = "Пользователь с таким логином уже существует.";
                            return;
                        }

                        var newUser = new User
                        {
                            Login = login,
                            PasswordHash = HashPassword(password),
                            Role = "patient" // саморегистрация → только пациент
                        };

                        context.Users.Add(newUser);
                        context.SaveChanges();

                        ErrorMessage.Text = "Аккаунт создан! Теперь войдите.";
                        _isLoginMode = true;
                        UpdateMode();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = $"Ошибка регистрации: {ex.Message}";
                }
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