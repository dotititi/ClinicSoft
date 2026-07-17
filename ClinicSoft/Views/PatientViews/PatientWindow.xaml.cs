using System.Windows;
using System.Windows.Controls;
using ClinicSoft.Views.PatientViews;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientWindow : Window
    {
        private readonly int _patientUserId;
        public PatientWindow(int patientUserId)
        {
            _patientUserId = patientUserId;
            InitializeComponent();
            UserMenuButton.Click += UserMenuButton_Click;
            LoadDashboard();
        }
        private void NavigateTo(Page page)
        {
            MainFrame.Navigate(page);
        }
        private void LoadDashboard() => NavigateTo(new PatientDashboardPage(_patientUserId));
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) => LoadDashboard();
        private void BtnAppointments_Click(object sender, RoutedEventArgs e) => NavigateTo(new MyAppointmentsPage(_patientUserId));
        private void BtnBook_Click(object sender, RoutedEventArgs e) => NavigateTo(new BookAppointmentPage(_patientUserId));
        private void BtnMyDocuments_Click(object sender, RoutedEventArgs e) => NavigateTo(new PatientDocumentsPage(_patientUserId));
        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
        }
        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new PatientProfileSettingsWindow(_patientUserId);
            settingsWindow.ShowDialog();
        }
        private void BtnLabTests_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new PatientLabTestsPage(_patientUserId));
        }
        private void BtnTreatment_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new PatientTreatmentPage(_patientUserId));
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new Shared.ChangePasswordWindow(_patientUserId, isPatient: true);
            changePasswordWindow.ShowDialog();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new Views.Authentication.AuthWindow().Show();
        }
    }
}