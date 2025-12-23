using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class DishProdViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Dish_prod _selectedDishProd;
        private ObservableCollection<Dish> _dishes;
        private ObservableCollection<Product> _availableProducts;

        public ObservableCollection<Dish_prod> DishProds { get; set; }

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set
            {
                _dishes = value;
                OnPropertyChanged(nameof(Dishes));
            }
        }

        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set
            {
                _availableProducts = value;
                OnPropertyChanged(nameof(AvailableProducts));
            }
        }

        public Dish_prod SelectedDishProd
        {
            get => _selectedDishProd;
            set
            {
                _selectedDishProd = value;
                OnPropertyChanged(nameof(SelectedDishProd));
                OnPropertyChanged(nameof(SelectedDish));
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public Dish SelectedDish
        {
            get => SelectedDishProd?.Dish;
            set
            {
                if (SelectedDishProd != null && value != null)
                {
                    SelectedDishProd.Dish = value;
                    SelectedDishProd.Dishid = value.ID;
                    OnPropertyChanged(nameof(SelectedDishProd));
                }
            }
        }

        public Product SelectedProduct
        {
            get => SelectedDishProd?.Product;
            set
            {
                if (SelectedDishProd != null && value != null)
                {
                    SelectedDishProd.Product = value;
                    SelectedDishProd.Productid = value.ID;
                    OnPropertyChanged(nameof(SelectedDishProd));
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadByDishCommand { get; }

        private Dish _currentDishFilter;
        private Product _selectedDefaultProduct;

        // Свойство для фильтрации по блюду
        public Dish CurrentDishFilter
        {
            get => _currentDishFilter;
            set
            {
                _currentDishFilter = value;
                OnPropertyChanged(nameof(CurrentDishFilter));
                if (value != null)
                {
                    LoadByDish(null);
                }
            }
        }

        public DishProdViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            DishProds = new ObservableCollection<Dish_prod>();

            AddCommand = new RelayCommand(AddDishProd, CanAdd);
            DeleteCommand = new RelayCommand(DeleteDishProd, CanDelete);
            SaveCommand = new RelayCommand(SaveChanges);
            LoadByDishCommand = new RelayCommand(LoadByDish);

            LoadDishes();
            LoadAvailableProducts();
        }

        public void LoadDishes()
        {
            //Dishes = new ObservableCollection<Dish>(_context.Dish.ToList());
            //_context = new Model_R(); // Пересоздаем контекст для чистоты
            //Dishes = new ObservableCollection<Dish>(_context.Dish.AsNoTracking().ToList());
            Dishes = new ObservableCollection<Dish>(_context.Dish.ToList());
        }

        public void LoadAvailableProducts()
        {
            AvailableProducts = new ObservableCollection<Product>(_context.Product.ToList());

            // Устанавливаем первый продукт как выбранный по умолчанию
            if (AvailableProducts.Count > 0)
            {
                _selectedDefaultProduct = AvailableProducts[0];
            }
        }

        public void LoadDishProds()
        {
            DishProds.Clear();
            var dishProds = _context.Dish_prod
                .Include("Dish")
                .Include("Product")
                .ToList();

            foreach (var item in dishProds)
            {
                DishProds.Add(item);
            }

            OnPropertyChanged(nameof(DishProds));
        }

        public void LoadByDish(object parameter)
        {
            if (_currentDishFilter == null) return;

            // Пересоздаем контекст для чистоты
            //_context = new Model_R();

            DishProds.Clear();
            var dishProds = _context.Dish_prod
                .Where(dp => dp.Dishid == _currentDishFilter.ID)
                .Include("Dish")
                .Include("Product")
                .ToList();

            foreach (var item in dishProds)
            {
                DishProds.Add(item);
            }

            OnPropertyChanged(nameof(DishProds));
        }

        //public void LoadByDish(object parameter)
        //{
        //    if (_currentDishFilter == null) return;

        //    DishProds.Clear();
        //    var dishProds = _context.Dish_prod
        //        .Where(dp => dp.Dishid == _currentDishFilter.ID)
        //        .Include("Dish")
        //        .Include("Product")
        //        .ToList();

        //    foreach (var item in dishProds)
        //    {
        //        DishProds.Add(item);
        //    }

        //    OnPropertyChanged(nameof(DishProds));
        //}

        //private void AddDishProd(object parameter)
        //{
        //    if (_currentDishFilter == null) return;

        //    // Используем первый продукт из списка, если не выбран другой
        //    Product productToUse = _selectedDefaultProduct;
        //    if (AvailableProducts.Count > 0 && productToUse == null)
        //    {
        //        productToUse = AvailableProducts[0];
        //    }

        //    // ПОПРАВКА 1: Присоединяем объект Dish к текущему контексту
        //    if (_context.Entry(_currentDishFilter).State == EntityState.Detached)
        //    {
        //        // Если объект отсоединен, присоединяем его как неизмененный
        //        _context.Dish.Attach(_currentDishFilter);
        //    }

        //    // ПОПРАВКА 2: То же самое для продукта
        //    if (productToUse != null && _context.Entry(productToUse).State == EntityState.Detached)
        //    {
        //        _context.Product.Attach(productToUse);
        //    }

        //    var newDishProd = new Dish_prod
        //    {
        //        // ID должен генерироваться базой данных (Identity)
        //        // Если ID не Identity, то используйте ваш подход
        //        // Но лучше перейти на Identity для PK
        //        Dishid = _currentDishFilter.ID,
        //        Dish = _currentDishFilter, // Этот объект теперь присоединен к контексту
        //        Count = 1,
        //        Product = productToUse,
        //        Productid = productToUse?.ID ?? 0
        //    };

        //    // ПОПРАВКА 3: Не устанавливаем ID вручную, если он Identity
        //    // Если не Identity, то:
        //    int newId = 1;
        //    if (_context.Dish_prod.Any())
        //    {
        //        newId = _context.Dish_prod.Max(dp => dp.ID) + 1;
        //    }
        //    newDishProd.ID = newId;

        //    DishProds.Add(newDishProd);
        //    SelectedDishProd = newDishProd;
        //    _context.Dish_prod.Add(newDishProd);
        //}

        private void AddDishProd(object parameter)
        {
            //_context = new Model_R();
            //Dishes = new ObservableCollection<Dish>(_context.Dish.ToList());
            if (_currentDishFilter == null) return;

            // Получаем максимальный ID из всех записей Dish_prod
            int newId = 1;
            if (_context.Dish_prod.Any())
            {
                newId = _context.Dish_prod.Max(dp => dp.ID) + 1;
            }

            // Используем первый продукт из списка, если не выбран другой
            Product productToUse = _selectedDefaultProduct;
            if (AvailableProducts.Count > 0 && productToUse == null)
            {
                productToUse = AvailableProducts[0];
            }

            var newDishProd = new Dish_prod
            {
                ID = newId, // ID с учетом всех записей
                Dishid = _currentDishFilter.ID,
                Dish = _currentDishFilter,
                Count = 1,
                Product = productToUse,
                Productid = productToUse?.ID ?? 0
            };

            DishProds.Add(newDishProd);
            SelectedDishProd = newDishProd;
            _context.Dish_prod.Add(newDishProd);
        }

        private void DeleteDishProd(object parameter)
        {
            if (SelectedDishProd != null)
            {
                _context.Dish_prod.Remove(SelectedDishProd);
                DishProds.Remove(SelectedDishProd);
                SaveChanges(null);
            }
        }

        private void SaveChanges(object parameter)
        {
            _context.SaveChanges();
            LoadByDish(null);
            try
            {
                _context.SaveChanges();
                LoadByDish(null); // Обновляем список после сохранения
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool CanAdd(object parameter)
        {
            return _currentDishFilter != null && AvailableProducts.Count > 0;
        }

        private bool CanDelete(object parameter)
        {
            return SelectedDishProd != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _disposed = false;

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
    }
}