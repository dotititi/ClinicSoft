using System.Windows;
using ClinicSoft.Views.Admin;
using ClinicSoft.Views.Authentication;
using ClinicSoft.Views.Shared;

namespace ClinicSoft.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private readonly int _currentUserId;

        public AdminWindow(int currentUserId)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            MainFrame.Navigate(new AdminDashboardPage());
            UserMenuButton.Checked += (s, e) => UserMenuPopup.IsOpen = true;
            UserMenuButton.Unchecked += (s, e) => UserMenuPopup.IsOpen = false;
            UserMenuPopup.Closed += (s, e) => UserMenuButton.IsChecked = false;
        }
        private void NavigateTo(object page)
        {
            MainFrame.Navigate(page);
        }
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDashboardPage());
        private void BtnPatients_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminPatientPage());
        private void BtnDoctors_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDoctorPage());
        private void BtnRegistrators_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminRegistratorPage());
        private void BtnAdmins_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminAdminPage());
        private void BtnUsers_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminUserPage());
        private void BtnDepartments_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDepartmentPage());
        private void BtnSpecialities_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminSpecialityPage());
        private void BtnOffices_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminOfficePage());
        private void BtnDoctorStatuses_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminStatusPage());
        private void BtnDosageForms_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDosageFormPage());
        private void BtnMedications_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminMedicationPage());
        private void BtnLabTestTypes_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminLabTestTypePage());
        private void BtnUnits_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminUnitOfMeasurementPage());
        private void BtnDocumentTypes_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDocumentTypePage());
        private void BtnDocumentsTemplate_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDocumentTemplatePage());
        private void BtnDiagnoses_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDiagnosisPage());
        private void BtnGenders_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminGenderPage());
        private void BtnDoctorSchedules_Click(object sender, RoutedEventArgs e) => NavigateTo(new AdminDoctorSchedulesPage());
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var changePasswordWindow = new ChangePasswordWindow(
                _currentUserId,
                ChangePasswordWindow.UserRole.Admin
            );
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            try
            {
                var profileWindow = new AdminProfileSettingsWindow(_currentUserId);
                profileWindow.Owner = this;
                profileWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось открыть окно настроек профиля:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;

            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
                var authWindow = new AuthWindow();
                authWindow.Show();
            }
        }
    }
}