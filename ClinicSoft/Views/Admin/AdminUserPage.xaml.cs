using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminUserPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 680;
        private string _currentSearchText = "";
        public AdminUserPage()
        {
            InitializeComponent();
            LoadUsers();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < MIN_WIDTH_FOR_SINGLE_LINE)
            {
                SingleLineToolbar.Visibility = Visibility.Collapsed;
                MultiLineToolbar.Visibility = Visibility.Visible;
            }
            else
            {
                SingleLineToolbar.Visibility = Visibility.Visible;
                MultiLineToolbar.Visibility = Visibility.Collapsed;
            }
            UpdateSearchDisplay();
        }
        private void LoadUsers(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allUsers = context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Login,
                    RoleName = u.Role != null ? u.Role.Name : "—",
                    RoleId = u.RoleId
                })
                .OrderBy(u => u.Id)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allUsers = allUsers
                    .Where(u => u.Login != null &&
                               u.Login.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            UserDataGrid.ItemsSource = allUsers;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadUsers(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers(_currentSearchText);
        }
        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = UserDataGrid.SelectedItem != null;
            BtnChangeRole.IsEnabled = hasSelection;
            BtnResetPassword.IsEnabled = hasSelection;
            BtnChangeRole2.IsEnabled = hasSelection;
            BtnResetPassword2.IsEnabled = hasSelection;
        }
        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedItem == null) return;
            try
            {
                var selectedItem = UserDataGrid.SelectedItem;
                var idProperty = selectedItem.GetType().GetProperty("Id");
                var roleIdProperty = selectedItem.GetType().GetProperty("RoleId");
                if (idProperty == null || roleIdProperty == null)
                {
                    MessageBox.Show("Ошибка получения данных пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int userId = (int)idProperty.GetValue(selectedItem)!;
                int currentRoleId = (int)roleIdProperty.GetValue(selectedItem)!;
                var window = new Admin.EditWindow.AdminEditUserRoleWindow(userId, currentRoleId);
                if (window.ShowDialog() == true)
                {
                    LoadUsers(_currentSearchText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна изменения роли:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedItem == null) return;
            try
            {
                var selectedItem = UserDataGrid.SelectedItem;
                var idProperty = selectedItem.GetType().GetProperty("Id");
                var loginProperty = selectedItem.GetType().GetProperty("Login");
                var roleNameProperty = selectedItem.GetType().GetProperty("RoleName");
                if (idProperty == null || loginProperty == null || roleNameProperty == null)
                {
                    MessageBox.Show("Ошибка получения данных пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int userId = (int)idProperty.GetValue(selectedItem)!;
                string login = (string)loginProperty.GetValue(selectedItem)!;
                string roleName = (string)roleNameProperty.GetValue(selectedItem)!;
                string defaultPassword = roleName.ToLower() switch
                {
                    "admin" => "admin123",
                    "registrator" => "registrator123",
                    "doctor" => "doctor123",
                    "patient" => "patient123",
                    _ => throw new Exception($"Неизвестная роль: {roleName}")
                };

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите сбросить пароль пользователя?\n\n" +
                    $"Логин: {login}\n" +
                    $"Роль: {roleName}\n" +
                    $"Новый пароль: {defaultPassword}\n\n",
                    "Подтверждение сброса пароля",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                ResetUserPassword(userId, defaultPassword, login, roleName);
                MessageBox.Show(
                    $"Пароль пользователя \"{login}\" успешно сброшен!\n" +
                    $"Новый пароль: {defaultPassword}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadUsers(_currentSearchText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сброса пароля:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ResetUserPassword(int userId, string newPassword, string login, string roleName)
        {
            using var context = new ClinicSoftContext();
            var user = context.Users.Find(userId);
            if (user == null)
                throw new Exception($"Пользователь с ID {userId} не найден.");
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordHash = passwordHash;
            context.SaveChanges();
            bool hasProfile = CheckUserProfileExists(userId, roleName);
            if (!hasProfile)
            {
                MessageBox.Show(
                    $"Внимание: У пользователя \"{login}\" отсутствует профиль {roleName}.\n" +
                    $"При первом входе система запросит заполнение профиля.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        private bool CheckUserProfileExists(int userId, string roleName)
        {
            using var context = new ClinicSoftContext();
            return roleName.ToLower() switch
            {
                "admin" => context.Admins.Any(a => a.UserId == userId),
                "registrator" => context.Registrators.Any(r => r.UserId == userId),
                "doctor" => context.Doctors.Any(d => d.UserId == userId),
                "patient" => context.Patients.Any(p => p.UserId == userId),
                _ => true
            };
        }
    }
}