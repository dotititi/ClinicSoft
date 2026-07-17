using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Shared
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly int _userId;
        private readonly UserRole _userRole;
        public enum UserRole
        {
            Patient,
            Doctor,
            Admin,
            Registrator
        }
        public ChangePasswordWindow(int userId, bool isPatient)
        {
            InitializeComponent();
            _userId = userId;
            _userRole = isPatient ? UserRole.Patient : UserRole.Doctor;
            SetWindowTitle();
        }
        public ChangePasswordWindow(int userId, UserRole role)
        {
            InitializeComponent();
            _userId = userId;
            _userRole = role;
            SetWindowTitle();
        }
        private void SetWindowTitle()
        {
            string roleText = _userRole switch
            {
                UserRole.Patient => "пациента",
                UserRole.Doctor => "врача",
                UserRole.Admin => "администратора",
                UserRole.Registrator => "регистратора",
                _ => "пользователя"
            };
            Title = $"Смена пароля {roleText}";
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var currentPassword = TxtCurrentPassword.Password;
            var newPassword = TxtNewPassword.Password;
            var confirmPassword = TxtConfirmPassword.Password;
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                ShowError("Текущий пароль обязателен для заполнения.");
                TxtCurrentPassword.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowError("Новый пароль обязателен для заполнения.");
                TxtNewPassword.Focus();
                return;
            }
            if (newPassword.Length < 6)
            {
                ShowError("Новый пароль должен содержать минимум 6 символов.");
                TxtNewPassword.Focus();
                return;
            }
            if (newPassword != confirmPassword)
            {
                ShowError("Новые пароли не совпадают.");
                TxtConfirmPassword.Focus();
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                User user = null;
                switch (_userRole)
                {
                    case UserRole.Patient:
                        var patient = context.Patients
                            .Include(p => p.User)
                            .FirstOrDefault(p => p.UserId == _userId);
                        if (patient == null || patient.User == null)
                            throw new Exception("Профиль пациента не найден.");
                        user = patient.User;
                        break;
                    case UserRole.Doctor:
                        var doctor = context.Doctors
                            .Include(d => d.User)
                            .FirstOrDefault(d => d.UserId == _userId);
                        if (doctor == null || doctor.User == null)
                            throw new Exception("Профиль врача не найден.");
                        user = doctor.User;
                        break;
                    case UserRole.Admin:
                        var admin = context.Admins
                            .Include(a => a.User)
                            .FirstOrDefault(a => a.UserId == _userId);
                        if (admin == null || admin.User == null)
                            throw new Exception("Профиль администратора не найден.");
                        user = admin.User;
                        break;
                    case UserRole.Registrator:
                        var registrator = context.Registrators
                            .Include(r => r.User)
                            .FirstOrDefault(r => r.UserId == _userId);
                        if (registrator == null || registrator.User == null)
                            throw new Exception("Профиль регистратора не найден.");
                        user = registrator.User;
                        break;
                }
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    ShowError("Текущий пароль неверен.");
                    TxtCurrentPassword.Focus();
                    TxtCurrentPassword.SelectAll();
                    return;
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                context.SaveChanges();
                MessageBox.Show(
                    "Пароль успешно изменён!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при смене пароля:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Ошибка валидации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}