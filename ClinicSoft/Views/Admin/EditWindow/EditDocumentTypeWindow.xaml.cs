using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditDocumentTypeWindow : Window
    {
        private readonly int? _id;

        public EditDocumentTypeWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование типа документа";
                FormTitle.Text = "Редактирование типа документа";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление типа документа";
                FormTitle.Text = "Добавление нового типа документа";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var type = context.DocumentTypes.Find(_id.Value);
            if (type != null)
            {
                TxtName.Text = type.Name;
                TxtDescription.Text = type.Description ?? "";
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            var description = TxtDescription.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Описание не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                string nameLower = name.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.DocumentTypes
                        .AsEnumerable()
                        .Any(t => t.Id != _id.Value &&
                                 t.Name != null &&
                                 t.Name.Trim().ToLowerInvariant() == nameLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show(
                            $"Тип документа «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    var type = context.DocumentTypes.Find(_id.Value);
                    if (type != null)
                    {
                        type.Name = name;
                        type.Description = TxtDescription.Text.Trim();
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.DocumentTypes
                        .AsEnumerable()
                        .Any(t => t.Name != null &&
                                 t.Name.Trim().ToLowerInvariant() == nameLower);
                    if (exists)
                    {
                        MessageBox.Show(
                            $"Тип документа «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    context.DocumentTypes.Add(new DocumentType
                    {
                        Name = name,
                        Description = TxtDescription.Text.Trim()
                    });
                    context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}