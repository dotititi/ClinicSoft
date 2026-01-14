using ClinicSoft.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicSoft.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadStats();
        }

        private void LoadStats()
        {
            using var context = new ClinicSoftContext();
            var today = DateTime.Today;

            var todays = context.Appointments
                .Count(a => a.ScheduledTime.Date == today);

            var totalPatients = context.Patients.Count();

            TodaysAppointments.Text = todays.ToString();
            TotalPatients.Text = totalPatients.ToString();
        }
    }
}
