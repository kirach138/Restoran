using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using WpfApp1.classes_bd;
using System.Data.Entity;

namespace WpfApp1.models
{
    public class BookingViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Booking _selectedBooking;
        private string _sortDirection = "ASC";
        private string _currentSortProperty = "Startb";

        public ObservableCollection<Booking> Bookings { get; set; }
        public ObservableCollection<Table> AvailableTables { get; set; }
        public ObservableCollection<Client> AvailableClients { get; set; }

        public Booking SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                _selectedBooking = value;
                OnPropertyChanged(nameof(SelectedBooking));
            }
        }

        public string SortDirection
        {
            get => _sortDirection;
            set
            {
                _sortDirection = value;
                OnPropertyChanged(nameof(SortDirection));
                LoadBookings(); // Перезагружаем при изменении направления сортировки
            }
        }

        public string CurrentSortProperty
        {
            get => _currentSortProperty;
            set
            {
                _currentSortProperty = value;
                OnPropertyChanged(nameof(CurrentSortProperty));
                LoadBookings(); // Перезагружаем при изменении свойства сортировки
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SortCommand { get; }

        public BookingViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            LoadBookings();
            LoadAvailableTables();
            LoadAvailableClients();

            AddCommand = new RelayCommand(AddBooking);
            EditCommand = new RelayCommand(EditBooking, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteBooking, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveChanges);
            SortCommand = new RelayCommand(SortBookings);
        }

        public void LoadBookings()
        {
            try
            {
                IQueryable<Booking> query = _context.Booking
                    .Include(b => b.Client) // Добавьте Include для Client
                    .Include(b => b.Table); // И для Table тоже

                // Применяем сортировку
                switch (CurrentSortProperty)
                {
                    case "Startb":
                        query = SortDirection == "ASC"
                            ? query.OrderBy(b => b.Startb)
                            : query.OrderByDescending(b => b.Startb);
                        break;
                    case "Endb":
                        query = SortDirection == "ASC"
                            ? query.OrderBy(b => b.Endb)
                            : query.OrderByDescending(b => b.Endb);
                        break;
                    case "Tableid":
                        query = SortDirection == "ASC"
                            ? query.OrderBy(b => b.Tableid)
                            : query.OrderByDescending(b => b.Tableid);
                        break;
                    default:
                        query = SortDirection == "ASC"
                            ? query.OrderBy(b => b.Startb)
                            : query.OrderByDescending(b => b.Startb);
                        break;
                }

                Bookings = new ObservableCollection<Booking>(query.ToList());
                OnPropertyChanged(nameof(Bookings));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки бронирований: {ex.Message}");
            }
        }

        private void LoadAvailableTables()
        {
            try
            {
                AvailableTables = new ObservableCollection<Table>(_context.Table.ToList());
                OnPropertyChanged(nameof(AvailableTables));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки столиков: {ex.Message}");
            }
        }

        private void LoadAvailableClients()
        {
            try
            {
                AvailableClients = new ObservableCollection<Client>(_context.Client.ToList());
                OnPropertyChanged(nameof(AvailableClients));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void AddBooking(object parameter)
        {
            var newBooking = new Booking
            {
                Startb = DateTime.Now,
                Endb = DateTime.Now.AddHours(2),
                Tableid = AvailableTables.FirstOrDefault()?.ID ?? 0,
                Clientid = AvailableClients.FirstOrDefault()?.ID,
                Code = GenerateBookingCode(),
                Count = 0
            };

            // Генерация нового ID
            newBooking.ID = Bookings.Any() ? Bookings.Max(b => b.ID) + 1 : 1;

            Bookings.Add(newBooking);
            _context.Booking.Add(newBooking);
            SelectedBooking = newBooking;
        }

        private string GenerateBookingCode()
        {
            // Берем первые 10 символов от Guid и переводим в верхний регистр
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }

        private void EditBooking(object parameter)
        {
            // Логика редактирования уже реализована через привязку данных
        }

        private void DeleteBooking(object parameter)
        {
            if (SelectedBooking != null)
            {
                _context.Booking.Remove(SelectedBooking);
                Bookings.Remove(SelectedBooking);
                SaveChanges(null);
            }
        }

        private void SaveChanges(object parameter)
        {
            try
            {
                _context.SaveChanges();
                LoadBookings(); // Обновляем список после сохранения
                System.Windows.MessageBox.Show("Изменения сохранены успешно!");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Получаем все сообщения об ошибках валидации
                var errorMessages = new List<string>();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add($"Свойство: {validationError.PropertyName} - Ошибка: {validationError.ErrorMessage}");
                    }
                }

                var fullErrorMessage = string.Join("\n", errorMessages);
                var exceptionMessage = $"Ошибка валидации:\n{fullErrorMessage}";

                System.Windows.MessageBox.Show(exceptionMessage, "Ошибка валидации",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        //private void SaveChanges(object parameter)
        //{
        //    _context.SaveChanges();
        //    LoadBookings(); // Обновляем список после сохранения
        //    System.Windows.MessageBox.Show("Изменения сохранены успешно!");
        //    //try
        //    //{
        //    //    _context.SaveChanges();
        //    //    LoadBookings(); // Обновляем список после сохранения
        //    //    System.Windows.MessageBox.Show("Изменения сохранены успешно!");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}");
        //    //}
        //}

        private void SortBookings(object parameter)
        {
            if (parameter is string sortProperty)
            {
                // Если уже сортируем по этому свойству, меняем направление
                if (CurrentSortProperty == sortProperty)
                {
                    SortDirection = SortDirection == "ASC" ? "DESC" : "ASC";
                }
                else
                {
                    CurrentSortProperty = sortProperty;
                    SortDirection = "ASC";
                }
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedBooking != null;
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

        ~BookingViewModel()
        {
            Dispose(false);
        }

        public Dictionary<string, string> SortOptions => new Dictionary<string, string>
{
    {"Дата начала", "Startb"},
    {"Дата окончания", "Endb"},
    {"Номер столика", "Tableid"}
};

        public Dictionary<string, string> SortDirections => new Dictionary<string, string>
{
    {"По возрастанию", "ASC"},
    {"По убыванию", "DESC"}
};
    }


    //public class BookingDisplayItem : Booking
    //{
    //    public string ClientName => Client?.User?.Name ?? "Не указан";

    //}
}