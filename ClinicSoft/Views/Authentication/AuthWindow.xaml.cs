using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.Admin;
using ClinicSoft.Views.DoctorViews;
using ClinicSoft.Views.PatientViews;
using ClinicSoft.Views.Registrator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using static BCrypt.Net.BCrypt;

namespace ClinicSoft.Views.Authentication
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private bool _isInitialSetupMode;
        public AuthWindow()
        {
            InitializeComponent();
            using var context = new ClinicSoftContext();
            _isInitialSetupMode = !context.Users
                .Any(u => u.Role != null && u.Role.Name == "admin");

            if (_isInitialSetupMode)
            {
                ToggleModeButton.Visibility = Visibility.Collapsed;
                Header.Text = "Первоначальная настройка системы";
            }
            UpdateMode();
        }
        private void UpdateMode()
        {
            if (_isInitialSetupMode && !_isLoginMode)
            {
                Header.Text = "Создание администратора";
                ActionButton.Content = "Создать аккаунт";
                ToggleModeButton.Visibility = Visibility.Collapsed;
            }
            else if (_isLoginMode)
            {
                Header.Text = "Вход в систему";
                ActionButton.Content = "Войти";
                ToggleLabel.Text = "Нет аккаунта?";
                ToggleModeButton.Content = "Зарегистрироваться";
                ToggleModeButton.Visibility = Visibility.Visible;
            }
            else
            {
                Header.Text = "Регистрация";
                ActionButton.Content = "Зарегистрироваться";
                ToggleLabel.Text = "Есть аккаунт?";
                ToggleModeButton.Content = "Войти";
                ToggleModeButton.Visibility = Visibility.Visible;
            }
            ErrorMessage.Text = "";
            LoginBox.Clear();
            PasswordBox.Clear();
            LoginPlaceholder.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Visible;
            this.Focus();
        }
        private bool _isLoginMode = true;
        private void ToggleModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialSetupMode)
                return;
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
                User? user = null;
                try
                {
                    using var context = new ClinicSoftContext();
                    user = context.Users
                        .Include(u => u.Role)
                        .FirstOrDefault(u => u.Login == login);
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = $"Ошибка подключения к БД: {ex.Message}";
                    return;
                }
                if (user == null || user.Role == null || !Verify(password, user.PasswordHash))
                {
                    ErrorMessage.Text = "Неверный логин или пароль.";
                    return;
                }
                HandleLoginByRole(user);
            }
            else
            {
                if (_isInitialSetupMode)
                {
                    CreateInitialAdmin(login, password);
                }
                else
                {
                    RegisterNewPatient(login, password);
                }
            }
        }
        private void HandleLoginByRole(User user)
        {
            try
            {
                switch (user.Role.Name)
                {
                    case "patient":
                        HandlePatientLogin(user);
                        break;

                    case "doctor":
                        HandleDoctorLogin(user);
                        break;

                    case "admin":
                        HandleAdminLogin(user);
                        break;

                    case "registrator":
                        HandleRegistratorLogin(user);
                        break;
                    default:
                        ErrorMessage.Text = $"Неизвестная роль: {user.Role.Name}";
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка входа: {ex.Message}";
            }
        }
        private void HandleAdminLogin(User user)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var admin = context.Admins.FirstOrDefault(a => a.UserId == user.Id);

                if (admin == null)
                {
                    this.Hide();
                    var profileWindow = new AdminProfileSettingsWindow(user.Id);
                    if (profileWindow.ShowDialog() == true)
                    {
                        CompleteLogin(user);
                    }
                    else
                    {
                        this.Show();
                        ErrorMessage.Text = "Для работы в системе необходимо заполнить профиль администратора.";
                    }
                }
                else
                {
                    CompleteLogin(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка проверки профиля администратора: {ex.Message}";
                this.Show();
            }
        }
        private void HandleRegistratorLogin(User user)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var registrator = context.Registrators.FirstOrDefault(r => r.UserId == user.Id);
                if (registrator == null)
                {
                    this.Hide();
                    var profileWindow = new RegistratorProfileSettingsWindow(user.Id);
                    if (profileWindow.ShowDialog() == true)
                    {
                        CompleteLogin(user);
                    }
                    else
                    {
                        this.Show();
                        ErrorMessage.Text = "Для работы в системе необходимо заполнить профиль регистратора.";
                    }
                }
                else
                {
                    CompleteLogin(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка проверки профиля регистратора: {ex.Message}";
                this.Show();
            }
        }
        private void HandlePatientLogin(User user)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);

                if (patient == null)
                {
                    this.Hide();
                    var profileWindow = new PatientProfileSettingsWindow(user.Id, isInitialSetup: true);
                    if (profileWindow.ShowDialog() == true)
                    {
                        CompleteLogin(user);
                    }
                    else
                    {
                        this.Show();
                        ErrorMessage.Text = "Для работы в системе необходимо заполнить профиль пациента.";
                    }
                }
                else
                {
                    CompleteLogin(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка проверки профиля пациента: {ex.Message}";
                this.Show();
            }
        }
        private void HandleDoctorLogin(User user)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var doctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);

                if (doctor == null)
                {
                    this.Hide();
                    var profileWindow = new DoctorProfileSettingsWindow(user.Id, isInitialSetup: true);
                    if (profileWindow.ShowDialog() == true)
                    {
                        CompleteLogin(user);
                    }
                    else
                    {
                        this.Show();
                        ErrorMessage.Text = "Для работы в системе необходимо заполнить базовые данные профиля.";
                    }
                }
                else
                {
                    CompleteLogin(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка проверки профиля врача: {ex.Message}";
                this.Show();
            }
        }
        private void CompleteLogin(User user)
        {
            try
            {
                switch (user.Role.Name)
                {
                    case "admin":
                        new AdminWindow(user.Id).Show();
                        break;

                    case "registrator":
                        new RegistratorWindow(user.Id).Show();
                        break;

                    case "doctor":
                        new DoctorWindow(user.Id).Show();
                        break;

                    case "patient":
                        new PatientWindow(user.Id).Show();
                        break;
                    default:
                        ErrorMessage.Text = $"Неизвестная роль: {user.Role.Name}";
                        return;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка открытия окна: {ex.Message}";
                this.Show();
            }
        }
        private void RegisterNewPatient(string login, string password)
        {
            try
            {
                using var context = new ClinicSoftContext();

                if (context.Users.Any(u => u.Login == login))
                {
                    ErrorMessage.Text = "Пользователь с таким логином уже существует.";
                    return;
                }
                var patientRole = context.Roles.FirstOrDefault(r => r.Name == "patient");
                if (patientRole == null)
                {
                    ErrorMessage.Text = "Роль 'patient' не найдена в системе.";
                    return;
                }
                var newUser = new User
                {
                    Login = login,
                    PasswordHash = HashPassword(password),
                    RoleId = patientRole.Id
                };
                context.Users.Add(newUser);
                context.SaveChanges();
                this.Hide();
                var profileWindow = new PatientProfileSettingsWindow(newUser.Id, isInitialSetup: true);
                if (profileWindow.ShowDialog() == true)
                {
                    CompleteLogin(newUser);
                }
                else
                {
                    this.Show();
                    ErrorMessage.Text = "Регистрация отменена. Для использования системы необходимо заполнить профиль.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка регистрации:\n{ex.Message}";
                this.Show();
            }
        }
        private void CreateInitialAdmin(string login, string password)
        {
            try
            {
                using var context = new ClinicSoftContext();

                if (context.Users.Any(u => u.Login == login))
                {
                    ErrorMessage.Text = "Пользователь с таким логином уже существует.";
                    return;
                }
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "admin");
                if (adminRole == null)
                {
                    ErrorMessage.Text = "Роль 'admin' не найдена. Выполните миграцию БД.";
                    return;
                }
                var newUser = new User
                {
                    Login = login,
                    PasswordHash = HashPassword(password),
                    RoleId = adminRole.Id
                };
                context.Users.Add(newUser);
                context.SaveChanges();
                MessageBox.Show(
                    "Аккаунт администратора успешно создан!\n" +
                    "Теперь необходимо заполнить профиль администратора.",
                    "Первый вход",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                this.Hide();
                var profileWindow = new AdminProfileSettingsWindow(newUser.Id);
                if (profileWindow.ShowDialog() == true)
                {
                    CompleteLogin(newUser);
                }
                else
                {
                    using var cleanupContext = new ClinicSoftContext();
                    var userToDelete = cleanupContext.Users.Find(newUser.Id);
                    if (userToDelete != null)
                    {
                        cleanupContext.Users.Remove(userToDelete);
                        cleanupContext.SaveChanges();
                    }
                    this.Show();
                    ErrorMessage.Text = "Создание администратора отменено. Система не настроена.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка создания администратора:\n{ex.Message}";
                this.Show();
            }
        }
        private void LoginBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordBox.SecurePassword.Length == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}