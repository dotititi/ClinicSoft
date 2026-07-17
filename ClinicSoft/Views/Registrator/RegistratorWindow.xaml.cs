using ClinicSoft.Views.Authentication;
using ClinicSoft.Views.DoctorViews;
using ClinicSoft.Views.Registrator;
using ClinicSoft.Views.Shared;
using System.Windows;

namespace ClinicSoft.Views.Registrator
{
    /// <summary>
    /// Логика взаимодействия для RegistratorWindow.xaml
    /// </summary>
    public partial class RegistratorWindow : Window
    {
        private readonly int _currentUserId;
        public RegistratorWindow(int currentUserId)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            MainFrame.Navigate(new DashboardPage());
            UserMenuButton.Checked += (s, e) => UserMenuPopup.IsOpen = true;
            UserMenuButton.Unchecked += (s, e) => UserMenuPopup.IsOpen = false;
            UserMenuPopup.Closed += (s, e) => UserMenuButton.IsChecked = false;
        }
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }
        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientPage());
        }
        private void BtnAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AppointmentPage());
        }
        private void BtnAllAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientAppointmentPage());
        }
        private void BtnDoctors_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DoctorsListPage());
        }
        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var profileWindow = new RegistratorProfileSettingsWindow(_currentUserId);
            profileWindow.Owner = this;
            profileWindow.ShowDialog();
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var changePasswordWindow = new ChangePasswordWindow(
                _currentUserId,
                ChangePasswordWindow.UserRole.Registrator
            );
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
                var authWindow = new AuthWindow();
                authWindow.Show();
            }
        }
    }
}