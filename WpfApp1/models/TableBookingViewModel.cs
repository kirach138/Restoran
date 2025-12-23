using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class TableBookingViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private DateTime? _selectedBookingDate;
        private string _selectedBookingTime;
        private int? _selectedGuestCount;
        private TableViewModel _selectedTable;
        private string _filterStatusText;
        private System.Windows.Visibility _filterStatusVisibility = System.Windows.Visibility.Collapsed;
        private System.Windows.Visibility _selectedTableVisibility = System.Windows.Visibility.Collapsed;
        private User _currentUser;
        private Client _currentClient;

        // Коллекции для привязки
        public ObservableCollection<TableViewModel> AllTables { get; set; }
        public ObservableCollection<TableViewModel> AvailableTables { get; set; }
        public ObservableCollection<string> AvailableBookingTimes { get; set; }
        public ObservableCollection<int> GuestCounts { get; set; }

        // Свойства для фильтрации
        public DateTime? SelectedBookingDate
        {
            get => _selectedBookingDate;
            set
            {
                _selectedBookingDate = value;
                OnPropertyChanged(nameof(SelectedBookingDate));
                UpdateFilterStatus();
            }
        }

        public string SelectedBookingTime
        {
            get => _selectedBookingTime;
            set
            {
                _selectedBookingTime = value;
                OnPropertyChanged(nameof(SelectedBookingTime));
                UpdateFilterStatus();
            }
        }

        public int? SelectedGuestCount
        {
            get => _selectedGuestCount;
            set
            {
                _selectedGuestCount = value;
                OnPropertyChanged(nameof(SelectedGuestCount));
                UpdateFilterStatus();
            }
        }

        public TableViewModel SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable != null)
                    _selectedTable.IsSelected = false;

                _selectedTable = value;

                if (_selectedTable != null)
                    _selectedTable.IsSelected = true;

                OnPropertyChanged(nameof(SelectedTable));
                OnPropertyChanged(nameof(SelectedTableText));
                OnPropertyChanged(nameof(CanBookTable));
                SelectedTableVisibility = _selectedTable != null ?
                    System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public string SelectedTableText
        {
            get
            {
                if (SelectedTable == null) return "";
                return $"Выбран столик №{SelectedTable.Number} на {SelectedGuestCount ?? 1} чел.";
            }
        }

        public string FilterStatusText
        {
            get => _filterStatusText;
            set
            {
                _filterStatusText = value;
                OnPropertyChanged(nameof(FilterStatusText));
            }
        }

        public System.Windows.Visibility FilterStatusVisibility
        {
            get => _filterStatusVisibility;
            set
            {
                _filterStatusVisibility = value;
                OnPropertyChanged(nameof(FilterStatusVisibility));
            }
        }

        public System.Windows.Visibility SelectedTableVisibility
        {
            get => _selectedTableVisibility;
            set
            {
                _selectedTableVisibility = value;
                OnPropertyChanged(nameof(SelectedTableVisibility));
            }
        }

        public bool CanBookTable => SelectedTable != null &&
                                   SelectedBookingDate.HasValue &&
                                   !string.IsNullOrEmpty(SelectedBookingTime) &&
                                   SelectedGuestCount.HasValue;

        // Свойство для текущего пользователя
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));

                // Находим клиента для этого пользователя
                if (_currentUser != null)
                {
                    _currentClient = _context.Client.FirstOrDefault(c => c.ID == _currentUser.ID);
                }
                else
                {
                    _currentClient = null;
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
                OnPropertyChanged(nameof(IsUserClient));
            }
        }

        public bool IsUserLoggedIn => _currentUser != null;
        public bool IsUserClient => _currentClient != null;

        // Команды
        public ICommand SearchTablesCommand { get; }
        public ICommand BookTableCommand { get; }
        public RelayCommand SelectTableCommand { get; }

        public TableBookingViewModel(Model_R context)
        {
            _context = context ?? new Model_R();

            // Инициализация коллекций
            AllTables = new ObservableCollection<TableViewModel>();
            AvailableTables = new ObservableCollection<TableViewModel>();
            AvailableBookingTimes = new ObservableCollection<string>();
            GuestCounts = new ObservableCollection<int>();

            // Инициализация команд
            SearchTablesCommand = new RelayCommand(SearchAvailableTables);
            BookTableCommand = new RelayCommand(BookSelectedTable, CanBookTableExecute);
            SelectTableCommand = new RelayCommand(SelectTable);

            // Загрузка данных
            LoadAllTables();
            InitializeTimeSlots();
            InitializeGuestCounts();

            // Установка значений по умолчанию
            SelectedBookingDate = DateTime.Today;
            SelectedBookingTime = AvailableBookingTimes.FirstOrDefault();
            SelectedGuestCount = GuestCounts.FirstOrDefault();
        }

        // Метод для проверки возможности выполнения команды BookTable
        public bool CanBookTableExecute(object parameter)
        {
            return CanBookTable && IsUserLoggedIn && IsUserClient;
        }

        public void LoadAllTables()
        {
            AllTables.Clear();
            var tables = _context.Table.ToList();

            foreach (var table in tables)
            {
                var tableVM = new TableViewModel
                {
                    Table = table,
                    IsAvailable = true,
                    SelectTableCommand = SelectTableCommand
                };

                AllTables.Add(tableVM);
            }

            // Сначала показываем все столики
            AvailableTables = new ObservableCollection<TableViewModel>(AllTables);
            OnPropertyChanged(nameof(AvailableTables));
        }

        public void InitializeTimeSlots()
        {
            // Время работы ресторана: с 10:00 до 22:00 с интервалом в 1 час
            AvailableBookingTimes.Clear();

            for (int hour = 10; hour <= 22; hour++)
            {
                AvailableBookingTimes.Add($"{hour:D2}:00");
            }
        }

        public void InitializeGuestCounts()
        {
            GuestCounts.Clear();

            for (int i = 1; i <= 10; i++)
            {
                GuestCounts.Add(i);
            }
        }

        public void SearchAvailableTables(object parameter)
        {
            if (!SelectedBookingDate.HasValue ||
                string.IsNullOrEmpty(SelectedBookingTime) ||
                !SelectedGuestCount.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все параметры бронирования",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранные дату и время
            var selectedDateTime = ParseDateTime(SelectedBookingDate.Value, SelectedBookingTime);
            if (!selectedDateTime.HasValue)
            {
                MessageBox.Show("Неверный формат времени",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Расчет начала и окончания брони (2 часа)
            var bookingStart = selectedDateTime.Value;
            var bookingEnd = bookingStart.AddHours(2);

            // Получаем занятые столики на это время
            var bookedTableIds = _context.Booking
                .Where(b => b.Startb < bookingEnd && b.Endb > bookingStart)
                .Select(b => b.Tableid)
                .Distinct()
                .ToList();

            // Фильтруем столики
            AvailableTables.Clear();

            foreach (var tableVM in AllTables)
            {
                // Проверяем, подходит ли столик по количеству мест
                bool fitsGuests = tableVM.Places >= SelectedGuestCount.Value;

                // Проверяем, свободен ли столик
                bool isAvailable = !bookedTableIds.Contains(tableVM.ID) && fitsGuests;

                // Обновляем статус доступности
                tableVM.IsAvailable = isAvailable;
                tableVM.IsSelected = false;

                // Добавляем в доступные только свободные столики
                if (isAvailable)
                {
                    AvailableTables.Add(tableVM);
                }
            }

            // Сбрасываем выбранный столик
            SelectedTable = null;

            // Обновляем статус фильтрации
            UpdateFilterStatus();

            // Показываем сообщение, если нет доступных столиков
            if (!AvailableTables.Any())
            {
                FilterStatusText = "Нет доступных столиков по заданным параметрам";
                FilterStatusVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                FilterStatusText = $"Найдено {AvailableTables.Count} столиков";
                FilterStatusVisibility = System.Windows.Visibility.Visible;
            }

            OnPropertyChanged(nameof(AvailableTables));
        }

        public DateTime? ParseDateTime(DateTime date, string time)
        {
            if (string.IsNullOrEmpty(time)) return null;

            try
            {
                var timeParts = time.Split(':');
                if (timeParts.Length != 2) return null;

                if (int.TryParse(timeParts[0], out int hours) &&
                    int.TryParse(timeParts[1], out int minutes))
                {
                    return new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public void SelectTable(object parameter)
        {
            if (parameter is TableViewModel tableVM)
            {
                SelectedTable = tableVM;
            }
        }

        public void BookSelectedTable(object parameter)
        {
            if (!CanBookTable || SelectedTable == null)
            {
                MessageBox.Show("Выберите столик и заполните все параметры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка авторизации
            if (!IsUserLoggedIn || !IsUserClient)
            {
                MessageBox.Show("Для бронирования столика необходимо войти в систему как клиент",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedDateTime = ParseDateTime(SelectedBookingDate.Value, SelectedBookingTime);
                if (!selectedDateTime.HasValue)
                {
                    MessageBox.Show("Неверный формат времени",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Расчет времени брони
                var bookingStart = selectedDateTime.Value;
                var bookingEnd = bookingStart.AddHours(2);

                // Поиск существующей брони на этот столик, дату и время
                var existingBooking = _context.Booking
                    .FirstOrDefault(b => b.Tableid == SelectedTable.ID &&
                                        b.Startb == bookingStart &&
                                        b.Endb == bookingEnd);

                if (existingBooking != null)
                {
                    // Бронь уже существует - обновляем клиента
                    if (existingBooking.Clientid == null)
                    {
                        // Если бронь без клиента - назначаем текущего клиента
                        existingBooking.Clientid = _currentClient.ID;
                        existingBooking.Client = _currentClient;

                        _context.SaveChanges();

                        MessageBox.Show($"Вы присоединились к существующей брони столика №{SelectedTable.Number}!\n" +
                                       $"Дата: {SelectedBookingDate.Value:dd.MM.yyyy}\n" +
                                       $"Время: {SelectedBookingTime}\n" +
                                       $"Код бронирования: {existingBooking.Code}",
                                       "Присоединение к брони",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Information);
                    }
                    else if (existingBooking.Clientid == _currentClient.ID)
                    {
                        MessageBox.Show("Вы уже забронировали этот столик на выбранное время",
                            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Этот столик уже забронирован другим клиентом на выбранное время",
                            "Столик занят", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SearchAvailableTables(null);
                        return;
                    }
                }
                else
                {
                    // Проверяем, что столик всё ещё доступен
                    bool isStillAvailable = !_context.Booking
                        .Any(b => b.Tableid == SelectedTable.ID &&
                                b.Startb < bookingEnd &&
                                b.Endb > bookingStart);

                    if (!isStillAvailable)
                    {
                        MessageBox.Show("Этот столик уже забронирован на выбранное время. Пожалуйста, выберите другой столик или время.",
                            "Столик занят", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SearchAvailableTables(null);
                        return;
                    }

                    // Создаем новое бронирование
                    var newBooking = new Booking
                    {
                        ID = GetNextBookingId(),
                        Startb = bookingStart,
                        Endb = bookingEnd,
                        Tableid = SelectedTable.ID,
                        Clientid = _currentClient.ID,
                        Code = GenerateBookingCode(),
                        Count = SelectedTable.Cost
                    };

                    // Устанавливаем связь с клиентом
                    newBooking.Client = _currentClient;

                    // Добавляем в базу данных
                    _context.Booking.Add(newBooking);
                    _context.SaveChanges();

                    MessageBox.Show($"Столик №{SelectedTable.Number} успешно забронирован!\n" +
                                   $"Дата: {SelectedBookingDate.Value:dd.MM.yyyy}\n" +
                                   $"Время: {SelectedBookingTime}\n" +
                                   $"Код бронирования: {newBooking.Code}",
                                   "Бронирование подтверждено",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }

                // Обновляем список столиков
                SearchAvailableTables(null);
                SelectedTable = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNextBookingId()
        {
            return _context.Booking.Any() ?
                   _context.Booking.Max(b => b.ID) + 1 : 1;
        }

        private string GenerateBookingCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        private void UpdateFilterStatus()
        {
            if (SelectedBookingDate.HasValue &&
                !string.IsNullOrEmpty(SelectedBookingTime) &&
                SelectedGuestCount.HasValue)
            {
                FilterStatusText = $"Дата: {SelectedBookingDate.Value:dd.MM.yyyy}, " +
                                  $"Время: {SelectedBookingTime}, " +
                                  $"Гостей: {SelectedGuestCount}";
                FilterStatusVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                FilterStatusVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}