using ClinicSoft.Data;
using System;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
            TotalUsers.Text = "0";
            TotalPatients.Text = "0";
            ActiveAppointments.Text = "0";
            TotalDoctors.Text = "0";
            LoadStats();
        }
        private void LoadStats()
        {
            try
            {
                using var context = new ClinicSoftContext();
                TotalUsers.Text = context.Users.Count().ToString();
                TotalPatients.Text = context.Patients.Count().ToString();
                TotalDoctors.Text = context.Doctors.Count().ToString();
                ActiveAppointments.Text = context.Appointments
                    .Count(a => a.ScheduledTime >= DateTime.Today && a.Status == "scheduled")
                    .ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
            }
        }
    }
}