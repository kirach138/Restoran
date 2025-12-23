using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Order _selectedOrder;
        private User _currentUser;
        private Client _currentClient;
        private ObservableCollection<Client> _availableClients;
        private ObservableCollection<Status> _availableStatuses;
        private ObservableCollection<Waiter> _availableWaiters;
        private ObservableCollection<Delivery> _availableDeliveries;
        private ObservableCollection<Booking> _availableBookings;

        public ObservableCollection<Order> Orders { get; set; }

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

                LoadData(); // Загружаем заказы при изменении пользователя
            }
        }

        public RelayCommand CancelOrderCommand { get; }

        public OrderViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Orders = new ObservableCollection<Order>();
            AvailableClients = new ObservableCollection<Client>();
            AvailableStatuses = new ObservableCollection<Status>();
            AvailableWaiters = new ObservableCollection<Waiter>();
            AvailableDeliveries = new ObservableCollection<Delivery>();
            AvailableBookings = new ObservableCollection<Booking>();

            // Инициализация команд
            AddCommand = new RelayCommand(AddOrder);
            DeleteCommand = new RelayCommand(DeleteOrder, CanDeleteOrder);
            SaveCommand = new RelayCommand(SaveOrders);
            RefreshCommand = new RelayCommand(RefreshOrders);
            CancelOrderCommand = new RelayCommand(CancelOrderWithClientReset);

            // Загружаем данные при инициализации
            LoadData();
        }

        // Метод для загрузки всех данных (используется в XAML)
        public void LoadData()
        {
            try
            {
                // Очищаем коллекции
                Orders.Clear();
                AvailableClients.Clear();
                AvailableStatuses.Clear();
                AvailableWaiters.Clear();
                AvailableDeliveries.Clear();
                AvailableBookings.Clear();

                // Загружаем заказы
                var orders = _context.Order
                    .Include(o => o.Client)
                    .Include(o => o.Client.User)
                    .Include(o => o.Status)
                    .Include(o => o.Waiter)
                    .Include(o => o.Waiter.User)
                    .Include(o => o.Delivery)
                    .Include(o => o.Delivery.User)
                    .Include(o => o.Booking)
                    .Include(o => o.Booking.Table)
                    .OrderByDescending(o => o.Time)
                    .ToList();

                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                // Загружаем связанные данные
                LoadRelatedData();

                OnPropertyChanged(nameof(Orders));
                OnPropertyChanged(nameof(AvailableClients));
                OnPropertyChanged(nameof(AvailableStatuses));
                OnPropertyChanged(nameof(AvailableWaiters));
                OnPropertyChanged(nameof(AvailableDeliveries));
                OnPropertyChanged(nameof(AvailableBookings));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных заказов: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для загрузки связанных данных
        private void LoadRelatedData()
        {
            try
            {
                // Клиенты
                var clients = _context.Client
                    .Include(c => c.User)
                    .ToList();
                foreach (var client in clients)
                {
                    AvailableClients.Add(client);
                }

                // Статусы
                foreach (var status in _context.Status.ToList())
                {
                    AvailableStatuses.Add(status);
                }

                // Официанты
                var waiters = _context.Waiter
                    .Include(w => w.User)
                    .ToList();
                foreach (var waiter in waiters)
                {
                    AvailableWaiters.Add(waiter);
                }

                // Доставщики
                var deliveries = _context.Delivery
                    .Include(d => d.User)
                    .ToList();
                foreach (var delivery in deliveries)
                {
                    AvailableDeliveries.Add(delivery);
                }

                // Брони
                var bookings = _context.Booking
                    .Include(b => b.Table)
                    .ToList();
                foreach (var booking in bookings)
                {
                    AvailableBookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки связанных данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавление нового заказа
        private void AddOrder(object parameter)
        {
            try
            {
                // Получаем следующий ID
                int nextId = GetNextId();

                var newOrder = new Order
                {
                    ID = nextId,
                    Time = DateTime.Now,
                    Count = 0,
                    Statusid = AvailableStatuses.FirstOrDefault()?.ID,
                    Clientid = AvailableClients.FirstOrDefault()?.ID,
                    Adress = ""
                };

                // Устанавливаем связи
                if (newOrder.Clientid.HasValue)
                {
                    newOrder.Client = AvailableClients.FirstOrDefault(c => c.ID == newOrder.Clientid.Value);
                }

                if (newOrder.Statusid.HasValue)
                {
                    newOrder.Status = AvailableStatuses.FirstOrDefault(s => s.ID == newOrder.Statusid.Value);
                }

                // Добавляем в контекст и коллекцию
                _context.Order.Add(newOrder);
                Orders.Add(newOrder);
                SelectedOrder = newOrder;

                MessageBox.Show($"Создан новый заказ #{newOrder.ID}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNextId()
        {
            try
            {
                return _context.Order.Any() ? _context.Order.Max(o => o.ID) + 1 : 1;
            }
            catch
            {
                return Orders.Any() ? Orders.Max(o => o.ID) + 1 : 1;
            }
        }

        // Проверка возможности удаления
        private bool CanDeleteOrder(object parameter)
        {
            return SelectedOrder != null;
        }

        // Удаление заказа
        private void DeleteOrder(object parameter)
        {
            if (SelectedOrder == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ #{SelectedOrder.ID}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderToDelete = _context.Order.Find(SelectedOrder.ID);
                    if (orderToDelete != null)
                    {
                        // Удаляем связанные блюда в заказе
                        var orderDishes = _context.Ord_dish.Where(od => od.Orderid == orderToDelete.ID).ToList();
                        foreach (var dish in orderDishes)
                        {
                            _context.Ord_dish.Remove(dish);
                        }

                        _context.Order.Remove(orderToDelete);
                        Orders.Remove(SelectedOrder);
                        SelectedOrder = null;

                        SaveOrders(null);
                        MessageBox.Show("Заказ успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Сохранение изменений
        private void SaveOrders(object parameter)
        {
            try
            {
                _context.SaveChanges();
                LoadData(); // Обновляем список
                MessageBox.Show("Изменения сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление списка
        public void RefreshOrders(object parameter = null)
        {
            LoadData();
            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Метод для отмены заказа с обнулением Clientid
        private void CancelOrderWithClientReset(object parameter)
        {
            if (parameter is int orderId)
            {
                CancelOrderAndResetClient(orderId);
            }
            else if (SelectedOrder != null)
            {
                CancelOrderAndResetClient(SelectedOrder.ID);
            }
        }

        // Метод для отмены заказа и обнуления Clientid
        public bool CancelOrderAndResetClient(int orderId)
        {
            try
            {
                var order = _context.Order.FirstOrDefault(o => o.ID == orderId);
                if (order == null)
                {
                    MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Проверяем, можно ли отменить заказ
                if (!CanCancelOrder(order))
                {
                    MessageBox.Show("Этот заказ нельзя отменить", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var result = MessageBox.Show("Вы уверены, что хотите отменить заказ?\n" +
                                           "Заказ будет удален из вашего списка.",
                                           "Отмена заказа",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Обнуляем Clientid
                    order.Clientid = null;

                    // Можно также установить статус "Отменен", если нужно
                    var cancelledStatus = _context.Status.FirstOrDefault(s => s.Name == "Отменен");
                    if (cancelledStatus != null)
                    {
                        order.Statusid = cancelledStatus.ID;
                    }

                    // Сохраняем изменения
                    _context.SaveChanges();

                    // Обновляем список заказов
                    LoadData();

                    MessageBox.Show("Заказ успешно отменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Обновленный метод проверки возможности отмены
        public bool CanCancelOrder(Order order)
        {
            if (order == null) return false;

            // Проверяем статусы, при которых можно отменить
            var allowedStatuses = new[] { "Новый", "В обработке", "Принят", "Готовится" };
            var currentStatus = order.Status?.Name ?? "";

            return allowedStatuses.Any(s => currentStatus.ToLower().Contains(s.ToLower()));
        }

        public RelayCommand RefreshCommand { get; }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        public ObservableCollection<Client> AvailableClients
        {
            get => _availableClients;
            set
            {
                _availableClients = value;
                OnPropertyChanged(nameof(AvailableClients));
            }
        }

        public ObservableCollection<Status> AvailableStatuses
        {
            get => _availableStatuses;
            set
            {
                _availableStatuses = value;
                OnPropertyChanged(nameof(AvailableStatuses));
            }
        }

        public ObservableCollection<Waiter> AvailableWaiters
        {
            get => _availableWaiters;
            set
            {
                _availableWaiters = value;
                OnPropertyChanged(nameof(AvailableWaiters));
            }
        }

        public ObservableCollection<Delivery> AvailableDeliveries
        {
            get => _availableDeliveries;
            set
            {
                _availableDeliveries = value;
                OnPropertyChanged(nameof(AvailableDeliveries));
            }
        }

        public ObservableCollection<Booking> AvailableBookings
        {
            get => _availableBookings;
            set
            {
                _availableBookings = value;
                OnPropertyChanged(nameof(AvailableBookings));
            }
        }

        // Команды для XAML
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        // Метод для загрузки заказов текущего пользователя
        public void LoadUserOrders()
        {
            Orders.Clear();

            if (_currentUser == null || _currentClient == null)
            {
                // Если пользователь не клиент или не найден
                return;
            }

            try
            {
                // Загружаем заказы только для текущего клиента
                var userOrders = _context.Order
                    .Where(o => o.Clientid == _currentClient.ID)
                    .Include(o => o.Client)
                    .Include(o => o.Client.User)
                    .Include(o => o.Status)
                    .Include(o => o.Waiter)
                    .Include(o => o.Waiter.User)
                    .Include(o => o.Delivery)
                    .Include(o => o.Delivery.User)
                    .Include(o => o.Booking)
                    .Include(o => o.Booking.Table)
                    .OrderByDescending(o => o.Time)
                    .ToList();

                foreach (var order in userOrders)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(Orders));
        }

        // Метод для загрузки всех заказов (для администраторов)
        public void LoadAllOrders()
        {
            Orders.Clear();

            try
            {
                var allOrders = _context.Order
                    .Include(o => o.Client)
                    .Include(o => o.Client.User)
                    .Include(o => o.Status)
                    .Include(o => o.Waiter)
                    .Include(o => o.Waiter.User)
                    .Include(o => o.Delivery)
                    .Include(o => o.Delivery.User)
                    .Include(o => o.Booking)
                    .Include(o => o.Booking.Table)
                    .OrderByDescending(o => o.Time)
                    .ToList();

                foreach (var order in allOrders)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки всех заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(Orders));
        }

        // Отмена заказа
        public void CancelOrder(Order order)
        {
            if (order == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите отменить заказ #{order.ID}?",
                "Подтверждение отмены", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Находим статус "Отменен"
                    var cancelledStatus = _context.Status.FirstOrDefault(s =>
                        s.Name.ToLower().Contains("отменен"));

                    if (cancelledStatus == null)
                    {
                        // Создаем статус "Отменен", если его нет
                        cancelledStatus = new Status
                        {
                            Name = "Отменен"
                        };
                        _context.Status.Add(cancelledStatus);
                        _context.SaveChanges();
                    }

                    order.Statusid = cancelledStatus.ID;
                    order.Status = cancelledStatus;
                    _context.SaveChanges();
                    LoadData();

                    MessageBox.Show("Заказ успешно отменен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Метод для очистки контекста (если нужно)
        public void Dispose()
        {
            _context?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}