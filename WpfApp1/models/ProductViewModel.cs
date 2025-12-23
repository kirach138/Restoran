using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Product _selectedProduct;

        public ObservableCollection<Product> Products { get; set; }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public ProductViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            LoadProducts();

            AddCommand = new RelayCommand(AddProduct);
            EditCommand = new RelayCommand(EditProduct, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteProduct, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveChanges);
        }

        public void LoadProducts()
        {
            Products = new ObservableCollection<Product>(_context.Product.ToList());
            OnPropertyChanged(nameof(Products));
        }

        private void AddProduct(object parameter)
        {
            var newProduct = new Product
            {
                Name = "Новый продукт",
                Cost = 0,
                Count = 0,
                Measure = "шт."
            };

            // Генерация нового ID (можно реализовать более сложную логику)
            newProduct.ID = Products.Any() ? Products.Max(p => p.ID) + 1 : 1;

            Products.Add(newProduct);
            _context.Product.Add(newProduct);
            SelectedProduct = newProduct;
        }

        private void EditProduct(object parameter)
        {
            // Логика редактирования уже реализована через привязку данных
        }

        private void DeleteProduct(object parameter)
        {
            if (SelectedProduct != null)
            {
                _context.Product.Remove(SelectedProduct);
                Products.Remove(SelectedProduct);
                SaveChanges(null);
            }
        }

        private void SaveChanges(object parameter)
        {
            try
            {
                _context.SaveChanges();
                LoadProducts(); // Обновляем список после сохранения
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedProduct != null;
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

        ~ProductViewModel()
        {
            Dispose(false);
        }
    }

    // Реализация RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        
    }
}
