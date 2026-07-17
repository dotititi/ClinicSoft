using ClinicSoft.Data;
using ClinicSoft.Views.DoctorViews;
using ClinicSoft.Views.PatientViews;
using ClinicSoft.Views.Registrator;
using ClinicSoft.Views.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    /// <summary>
    /// Логика взаимодействия для DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        private readonly int _doctorId;
        private readonly int _userId;
        private bool _isLabSpecialist;
        public DoctorWindow(int userId)
        {
            _userId = userId;
            try
            {
                using var context = new ClinicSoftContext();
                var doctor = context.Doctors
                    .Include(d => d.Speciality)
                    .FirstOrDefault(d => d.UserId == userId);
                if (doctor == null)
                {
                    MessageBox.Show("Врач не найден. Возможно, профиль не настроен администратором.",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                _doctorId = doctor.Id;
                _isLabSpecialist = IsLabSpecialist(doctor.Speciality?.Name);
                InitializeComponent();
                UserMenuButton.Click += UserMenuButton_Click;
                UpdateNavigation();
                if (_isLabSpecialist)
                {
                    NavigateTo(new LabSpecialistPage(_doctorId, _isLabSpecialist));
                }
                else
                {
                    NavigateTo(new DoctorDashboardPage(_doctorId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private bool IsLabSpecialist(string? specialityName)
        {
            if (string.IsNullOrEmpty(specialityName))
                return false;

            var labSpecialities = new[]
            {
                "Лаборант",
                "Лабораторная диагностика",
                "Клиническая лабораторная диагностика",
                "Лаб. диагностика"
            };
            return labSpecialities.Any(s =>
                specialityName.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
        private void UpdateNavigation()
        {
            if (_isLabSpecialist)
            {
                HeaderTitle.Text = "Лаборатория";
                BtnDashboard.Visibility = Visibility.Collapsed;
                BtnAppointments.Visibility = Visibility.Collapsed;
                BtnTreatment.Visibility = Visibility.Collapsed;
                DocumentsHeader.Visibility = Visibility.Collapsed;
                BtnCreateDocument.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderTitle.Text = "Панель врача";
                BtnDashboard.Visibility = Visibility.Visible;
                BtnAppointments.Visibility = Visibility.Visible;
                BtnTreatment.Visibility = Visibility.Visible;
                DocumentsHeader.Visibility = Visibility.Visible;
                BtnCreateDocument.Visibility = Visibility.Visible;
            }
        }
        private void NavigateTo(Page page)
        {
            MainFrame.Navigate(page);
        }
        private void LoadDashboard() => NavigateTo(new DoctorDashboardPage(_doctorId));
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) => LoadDashboard();
        private void BtnAppointments_Click(object sender, RoutedEventArgs e) => NavigateTo(new DoctorAppointmentPage(_doctorId));
        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new DoctorPatientPage(_doctorId, _isLabSpecialist));
        }
        private void BtnLabTests_Click(object sender, RoutedEventArgs e) => NavigateTo(new LabSpecialistPage(_doctorId, _isLabSpecialist));
        private void BtnTreatment_Click(object sender, RoutedEventArgs e) => NavigateTo(new DoctorTreatmentPage(_doctorId));
        private void BtnCreateDocument_Click(object sender, RoutedEventArgs e) => NavigateTo(new DoctorDocumentsPage(_doctorId));
        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
        }
        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new DoctorProfileSettingsWindow(_userId);
            settingsWindow.ShowDialog();
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var changePasswordWindow = new ChangePasswordWindow(_userId, ChangePasswordWindow.UserRole.Doctor);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new Views.Authentication.AuthWindow().Show();
        }
    }
}