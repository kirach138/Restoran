using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class TablesViewModel
    {
        private Model_R _context;
        private Table _selectedTable;

        public ObservableCollection<Table> Tables { get; set; }

        public Table SelectedTable
        {
            get => _selectedTable;
            set
            {
                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public TablesViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            LoadTables();

            AddCommand = new RelayCommand(AddTable);
            EditCommand = new RelayCommand(EditTable, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteTable, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveChanges);
        }

        public void LoadTables()
        {
            Tables = new ObservableCollection<Table>(_context.Table.ToList());
            OnPropertyChanged(nameof(Tables));
        }


        private void AddTable(object parameter)
        {
            var newTable = new Table
            {
                Number = 0,
                Places = 0,
                Cost = 0
            };

            // Генерация нового ID (можно реализовать более сложную логику)
            newTable.ID = Tables.Any() ? Tables.Max(p => p.ID) + 1 : 1;

            Tables.Add(newTable);
            _context.Table.Add(newTable);
            SelectedTable = newTable;
        }

        private void EditTable(object parameter)
        {
            // Логика редактирования уже реализована через привязку данных
        }

        private void DeleteTable(object parameter)
        {
            if (SelectedTable != null)
            {
                _context.Table.Remove(SelectedTable);
                Tables.Remove(SelectedTable);
                SaveChanges(null);
            }
        }

        private void SaveChanges(object parameter)
        {
            try
            {
                _context.SaveChanges();
                LoadTables(); // Обновляем список после сохранения
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedTable != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _disposed = false;

        // Добавьте метод Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }

        ~TablesViewModel()
        {
            Dispose(false);
        }
    }
}
