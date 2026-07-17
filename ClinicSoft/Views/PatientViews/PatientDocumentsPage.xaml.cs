using ClinicSoft.Data;
using ClinicSoft.Views.DoctorViews;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientDocumentsPage : Page
    {
        private readonly int _patientUserId;
        private string _currentSearch = "";
        public PatientDocumentsPage(int patientUserId)
        {
            InitializeComponent();
            _patientUserId = patientUserId;
            LoadDocuments();
        }
        private void LoadDocuments(string searchQuery = "")
        {
            try
            {
                using var context = new ClinicSoftContext();
                var documents = context.Documents
                    .Where(d => d.Patient.UserId == _patientUserId)
                    .Include(d => d.DocumentTemplate)
                        .ThenInclude(t => t.DocumentType)
                    .AsEnumerable()
                    .Select(d => new
                    {
                        Id = d.Id,
                        CreatedAt = d.CreatedAt,
                        DocumentTypeName = d.DocumentTemplate?.DocumentType?.Name ?? "Без типа"
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var term = searchQuery.Trim().ToLowerInvariant();
                    documents = documents
                        .Where(d => d.DocumentTypeName.ToLowerInvariant().Contains(term))
                        .ToList();
                }
                DocumentsGrid.ItemsSource = documents;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = TxtSearch.Text;
            UpdateSearchPlaceholder();
            LoadDocuments(_currentSearch);
        }
        private void UpdateSearchPlaceholder()
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(TxtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void BtnOpenDocument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int documentId)
            {
                var viewWindow = new DocumentViewWindow(documentId, isEditable: false);
                viewWindow.ShowDialog();
            }
        }
    }
}