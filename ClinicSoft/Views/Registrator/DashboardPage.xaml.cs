using ClinicSoft.Data;
using System;
using System.Windows.Controls;

namespace ClinicSoft.Views.Registrator
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            TodaysAppointments.Text = "0";
            TotalPatients.Text = "0";
            ActiveDoctors.Text = "0";
            PendingLabTests.Text = "0";
            LoadStats();
        }
        private void LoadStats()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var today = DateTime.Today;
                var todays = context.Appointments
                    .Count(a => a.ScheduledTime.Date == today);
                var totalPatients = context.Patients.Count();
                var activeDoctors = context.Doctors
                    .Count(d => d.StatusId == 1);
                var pendingLabTests = context.LabOrders
                    .Count(l => l.Status == "in_progress");
                TodaysAppointments.Text = todays.ToString();
                TotalPatients.Text = totalPatients.ToString();
                ActiveDoctors.Text = activeDoctors.ToString();
                PendingLabTests.Text = pendingLabTests.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
            }
        }
    }
}