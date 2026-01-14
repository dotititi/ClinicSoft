using ClinicSoft.Data;
using ClinicSoft.Models;
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
    /// <summary>
    /// Логика взаимодействия для SpecialityWindow.xaml
    /// </summary>
    public partial class SpecialityWindow : Window
    {
        private ClinicSoftContext _context = new();

        public SpecialityWindow()
        {
            InitializeComponent();
            LoadSpecialities();
        }

        private void LoadSpecialities()
        {
            var specs = _context.MedicalSpecialities.ToList();
            SpecialityGrid.ItemsSource = specs;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewSpeciality.Text)) return;

            var spec = new MedicalSpeciality { Name = TxtNewSpeciality.Text };
            _context.MedicalSpecialities.Add(spec);
            _context.SaveChanges();
            LoadSpecialities();
            TxtNewSpeciality.Clear();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
