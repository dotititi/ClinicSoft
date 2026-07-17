using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditDocumentTemplateWindow : Window
    {
        private readonly int? _id;
        private List<DocumentType> _documentTypes = new();
        public EditDocumentTemplateWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование шаблона документа";
                FormTitle.Text = "Редактирование шаблона документа";
                BtnSave.Content = "Сохранить";
            }
            else
            {
                Title = "Добавление шаблона документа";
                FormTitle.Text = "Добавление нового шаблона документа";
                BtnSave.Content = "Добавить";
            }
            LoadDocumentTypes();
            if (_id.HasValue) LoadData();
        }
        private void LoadDocumentTypes()
        {
            using var context = new ClinicSoftContext();
            _documentTypes = context.DocumentTypes.ToList();
            CbDocumentType.ItemsSource = _documentTypes;
            CbDocumentType.DisplayMemberPath = "Name";
            CbDocumentType.SelectedValuePath = "Id";
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var template = context.DocumentTemplates
                .Include(t => t.DocumentType)
                .FirstOrDefault(t => t.Id == _id.Value);
            if (template != null)
            {
                CbDocumentType.SelectedValue = template.DocumentTypeId;
                TxtName.Text = template.Name;
                TxtContent.Text = template.Content;
                UpdateDescriptionFromSelectedType();
            }
        }
        private void CbDocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDescriptionFromSelectedType();
        }
        private void UpdateDescriptionFromSelectedType()
        {
            if (CbDocumentType.SelectedItem is DocumentType selectedType)
            {
                TxtDescription.Text = selectedType.Description ?? "";
            }
            else
            {
                TxtDescription.Text = "";
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbDocumentType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип документа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var name = TxtName.Text?.Trim();
            var content = TxtContent.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название шаблона не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Содержимое шаблона не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int documentTypeId = (int)CbDocumentType.SelectedValue;
            try
            {
                using var context = new ClinicSoftContext();
                string nameLower = name.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.DocumentTemplates
                        .AsEnumerable()
                        .Any(t => t.Id != _id.Value &&
                                 t.Name != null &&
                                 t.Name.Trim().ToLowerInvariant() == nameLower &&
                                 t.DocumentTypeId == documentTypeId);
                    if (duplicateExists)
                    {
                        var typeName = (CbDocumentType.SelectedItem as DocumentType)?.Name ?? "выбранный";
                        MessageBox.Show(
                            $"Шаблон с названием «{name}» для типа документа «{typeName}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    var template = context.DocumentTemplates.Find(_id.Value);
                    if (template != null)
                    {
                        template.DocumentTypeId = documentTypeId;
                        template.Name = name;
                        template.Description = TxtDescription.Text.Trim();
                        template.Content = content;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.DocumentTemplates
                        .AsEnumerable()
                        .Any(t => t.Name != null &&
                                 t.Name.Trim().ToLowerInvariant() == nameLower &&
                                 t.DocumentTypeId == documentTypeId);
                    if (exists)
                    {
                        var typeName = (CbDocumentType.SelectedItem as DocumentType)?.Name ?? "выбранный";
                        MessageBox.Show(
                            $"Шаблон с названием «{name}» для типа документа «{typeName}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    context.DocumentTemplates.Add(new DocumentTemplate
                    {
                        DocumentTypeId = documentTypeId,
                        Name = name,
                        Description = TxtDescription.Text.Trim(),
                        Content = content,
                        CreatedAt = DateTime.UtcNow
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