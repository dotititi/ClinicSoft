using ClinicSoft.Views.Admin;
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
using System.Windows.Shapes;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            // Загружаем главную страницу при запуске
            MainFrame.Navigate(new DashboardPage());
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

        private void BtnReference_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReferencePage());
        }
    }
}