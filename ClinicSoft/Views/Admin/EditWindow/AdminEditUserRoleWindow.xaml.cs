using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class AdminEditUserRoleWindow : Window
    {
        private readonly int _userId;
        private readonly int _currentRoleId;
        private List<RoleDisplayItem> _availableRoles = new(); 
        private class RoleDisplayItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string DisplayName { get; set; } = null!;
        }
        public AdminEditUserRoleWindow(int userId, int currentRoleId)
        {
            InitializeComponent();
            _userId = userId;
            _currentRoleId = currentRoleId;
            LoadRoles();
            DisplayCurrentRole();
        }
        private void LoadRoles()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var roles = context.Roles
                    .Where(r => r.Name != "patient") 
                    .OrderBy(r => r.Name)
                    .ToList();
                _availableRoles = roles.Select(r => new RoleDisplayItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    DisplayName = GetRoleDisplayName(r.Name)
                }).ToList();
                CbNewRole.ItemsSource = _availableRoles;
                if (_availableRoles.Any())
                {
                    CbNewRole.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private void DisplayCurrentRole()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var role = context.Roles.FirstOrDefault(r => r.Id == _currentRoleId);
                TxtCurrentRole.Text = role != null
                    ? GetRoleDisplayName(role.Name)
                    : "Неизвестная роль";
            }
            catch (Exception ex)
            {
                TxtCurrentRole.Text = $"Ошибка: {ex.Message}";
            }
        }
        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "admin" => "Администратор",
                "registrator" => "Регистратор",
                "doctor" => "Врач",
                "patient" => "Пациент",
                _ => role
            };
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbNewRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите новую роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedRole = (RoleDisplayItem)CbNewRole.SelectedItem;
            int newRoleId = selectedRole.Id;
            if (newRoleId == _currentRoleId)
            {
                MessageBox.Show("Новая роль совпадает с текущей.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                var user = context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Id == _userId);
                if (user == null) throw new Exception("Пользователь не найден.");
                if (user.Role?.Name == "patient")
                {
                    MessageBox.Show("Нельзя изменить роль пациента. Пациенты управляются через их профиль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (selectedRole.Name == "doctor")
                {
                    var doctorExists = context.Doctors.Any(d => d.UserId == _userId);
                    if (!doctorExists)
                    {
                        var result = MessageBox.Show(
                            "У пользователя нет профиля врача. Создать его автоматически?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            context.Doctors.Add(new Doctor
                            {
                                UserId = _userId,
                                LastName = "Фамилия",
                                FirstName = "Имя",
                                MiddleName = "Отчество",
                                DepartmentId = 1,
                                SpecialityId = 1,
                            });
                            context.SaveChanges();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                user.RoleId = newRoleId;
                context.SaveChanges();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении роли:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}