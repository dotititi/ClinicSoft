using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientLabResultDetailView : Page
    {
        public PatientLabResultDetailView(int labOrderId)
        {
            InitializeComponent();
            LoadResults(labOrderId);
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadResults(int labOrderId)
        {
            using var context = new ClinicSoftContext();
            var rawResults = context.LabResultItems
                .Include(ri => ri.TestType)
                    .ThenInclude(tt => tt.Unit)
                .Where(ri => ri.LabResult.LabOrderId == labOrderId)
                .ToList();
            var results = rawResults.Select(ri => new
            {
                TestName = ri.TestType.Name,
                ResultValue = ri.ResultValue,
                UnitSymbol = ri.TestType.Unit?.Symbol ?? "—",
                NormalRange = ri.TestType.NormalRange ?? "—"
            }).ToList();

            ResultsGrid.ItemsSource = results;
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && NavigationService?.CanGoBack == true)
            {
                GoBack();
                e.Handled = true;
            }
        }
        private void GoBack()
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет страниц для возврата.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}