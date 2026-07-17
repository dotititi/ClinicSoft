using ClinicSoft.Data;
using System.Linq;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientDashboardPage : Page
    {
        public PatientDashboardPage(int patientUserId)
        {
            InitializeComponent();
            LoadData(patientUserId);
        }
        private void LoadData(int patientUserId)
        {
            using var context = new ClinicSoftContext();
            var patient = context.Patients.First(p => p.UserId == patientUserId);
            WelcomeMessage.Text = $"Здравствуйте, {patient.FirstName} {patient.LastName}!\n\n" +
                                "Вы можете записаться на приём, просмотреть свои записи или обновить данные в профиле.";
        }
    }
}