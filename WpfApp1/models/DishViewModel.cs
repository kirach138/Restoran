using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class DishViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Dish _selectedDish;

        public ObservableCollection<Dish> Dishes { get; set; }

        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public DishViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            LoadDishes();

            AddCommand = new RelayCommand(AddDish);
            EditCommand = new RelayCommand(EditDish, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteDish, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveChanges);
        }

        public void LoadDishes()
        {
            Dishes = new ObservableCollection<Dish>(_context.Dish.ToList());
            OnPropertyChanged(nameof(Dishes));
        }

        

        private void AddDish(object parameter)
        {
            var newDish = new Dish
            {
                Name = "Новый продукт",
                Price = 0,
                Description = "",
                Availability = 0
            };

            // Генерация нового ID (можно реализовать более сложную логику)
            newDish.ID = Dishes.Any() ? Dishes.Max(p => p.ID) + 1 : 1;

            Dishes.Add(newDish);
            _context.Dish.Add(newDish);
            SelectedDish = newDish;
        }

        private void EditDish(object parameter)
        {
            // Логика редактирования уже реализована через привязку данных
        }

        private void DeleteDish(object parameter)
        {
            if (SelectedDish != null)
            {
                _context.Dish.Remove(SelectedDish);
                Dishes.Remove(SelectedDish);
                SaveChanges(null);
            }
        }

        private void SaveChanges(object parameter)
        {
            _context.SaveChanges();
            LoadDishes();
            try
            {
                _context.SaveChanges();
                LoadDishes(); // Обновляем список после сохранения
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedDish != null;
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

        ~DishViewModel()
        {
            Dispose(false);
        }
    }
}
