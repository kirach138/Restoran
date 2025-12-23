using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Model_R _context; // Единый контекст для всего приложения
        private ProductViewModel _productVM;
        private DishViewModel _dishVM;
        private TablesViewModel _tablesVM;
        private DishProdViewModel _dishProdVM;
        private BookingViewModel _bookingVM;
        private UserTypeViewModel _userTypeVM;
        private UserViewModel _userVM;
        private ClientViewModel _clientVM;
        private AdminViewModel _adminVM;
        private CookViewModel _cookVM;
        private DeliveryViewModel _deliveryVM;
        private WaiterViewModel _waiterVM;
        private DiscountViewModel _discountVM;
        private OrderViewModel _orderVM;
        private OrderDishViewModel _orderDishVM;
        private User _currentUser;
        private CurrentOrder _currentOrder;
        private ReportViewModel _reportVM;


        public ProductViewModel ProductVM
        {
            get => _productVM;
            set { _productVM = value; OnPropertyChanged(nameof(ProductVM)); }
        }

        public DishViewModel DishVM
        {
            get => _dishVM;
            set { _dishVM = value; OnPropertyChanged(nameof(DishVM)); }
        }

        public TablesViewModel TableVM
        {
            get => _tablesVM;
            set { _tablesVM = value; OnPropertyChanged(nameof(TableVM)); }
        }

        public DishProdViewModel DishProdVM
        {
            get => _dishProdVM;
            set { _dishProdVM = value; OnPropertyChanged(nameof(DishProdVM)); }
        }

        public BookingViewModel BookingVM
        {
            get => _bookingVM;
            set { _bookingVM = value; OnPropertyChanged(nameof(BookingVM)); }
        }

        public UserTypeViewModel UserTypeVM
        {
            get => _userTypeVM;
            set { _userTypeVM = value; OnPropertyChanged(nameof(UserTypeVM)); }
        }

        public UserViewModel UserVM
        {
            get => _userVM;
            set { _userVM = value; OnPropertyChanged(nameof(UserVM)); }
        }

        public ClientViewModel ClientVM
        {
            get => _clientVM;
            set { _clientVM = value; OnPropertyChanged(nameof(ClientVM)); }
        }

        public AdminViewModel AdminVM
        {
            get => _adminVM;
            set { _adminVM = value; OnPropertyChanged(nameof(AdminVM)); }
        }

        public CookViewModel CookVM
        {
            get => _cookVM;
            set { _cookVM = value; OnPropertyChanged(nameof(CookVM)); }
        }

        public DeliveryViewModel DeliveryVM
        {
            get => _deliveryVM;
            set { _deliveryVM = value; OnPropertyChanged(nameof(DeliveryVM)); }
        }

        public WaiterViewModel WaiterVM
        {
            get => _waiterVM;
            set { _waiterVM = value; OnPropertyChanged(nameof(WaiterVM)); }
        }

        public DiscountViewModel DiscountVM
        {
            get => _discountVM;
            set { _discountVM = value; OnPropertyChanged(nameof(DiscountVM)); }
        }

        public OrderViewModel OrderVM
        {
            get => _orderVM;
            set { _orderVM = value; OnPropertyChanged(nameof(OrderVM)); }
        }

        public OrderDishViewModel OrderDishVM
        {
            get => _orderDishVM;
            set
            {
                _orderDishVM = value;
                OnPropertyChanged(nameof(OrderDishVM));
            }
        }

        private TableBookingViewModel _tableBookingVM;

        public TableBookingViewModel TableBookingVM
        {
            get => _tableBookingVM;
            set
            {
                _tableBookingVM = value;
                OnPropertyChanged(nameof(TableBookingVM));
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(IsClient));
                OnPropertyChanged(nameof(IsCook));
                OnPropertyChanged(nameof(IsWaiter));
                OnPropertyChanged(nameof(IsDelivery));
                if (OrderVM != null)
                {
                    OrderVM.CurrentUser = value;
                }
                if (TableBookingVM != null)
                {
                    TableBookingVM.CurrentUser = value;
                }
            }
        }

        public ReportViewModel ReportVM
        {
            get => _reportVM;
            set { _reportVM = value; OnPropertyChanged(nameof(ReportVM)); }
        }

        public decimal OrderTotalPrice => CurrentOrder?.TotalPrice ?? 0;
        public int OrderTotalItems => CurrentOrder?.TotalItems ?? 0;

        public CurrentOrder CurrentOrder
        {
            get => _currentOrder;
            set
            {
                _currentOrder = value;
                OnPropertyChanged(nameof(CurrentOrder));
                OnPropertyChanged(nameof(OrderTotalPrice));
                OnPropertyChanged(nameof(OrderTotalItems));
            }
        }

        public bool IsAdmin => CurrentUser?.Type == 1; // ID типа администратора
        public bool IsClient => CurrentUser?.Type == 0; // ID типа клиента
        public bool IsCook => CurrentUser?.Type == 2; // ID типа повара
        public bool IsWaiter => CurrentUser?.Type == 3; // ID типа официанта
        public bool IsDelivery => CurrentUser?.Type == 4; // ID типа доставки

        public ICommand RefreshAllCommand { get; }

        public MainViewModel()
        {
            // Создаем единый контекст для всего приложения
            _context = new Model_R();

            // Инициализируем все ViewModel с единым контекстом
            InitializeViewModels();

            CurrentOrder = new CurrentOrder();
            RefreshAllCommand = new RelayCommand(RefreshAll);
            ReportVM = new ReportViewModel(_context);
        }

        private void InitializeViewModels()
        {
            ProductVM = new ProductViewModel(_context);
            DishVM = new DishViewModel(_context);
            TableVM = new TablesViewModel(_context);
            DishProdVM = new DishProdViewModel(_context);
            BookingVM = new BookingViewModel(_context);
            UserTypeVM = new UserTypeViewModel(_context);
            UserVM = new UserViewModel(_context);
            ClientVM = new ClientViewModel(_context);
            AdminVM = new AdminViewModel(_context);
            CookVM = new CookViewModel(_context);
            DeliveryVM = new DeliveryViewModel(_context);
            WaiterVM = new WaiterViewModel(_context);
            DiscountVM = new DiscountViewModel(_context);
            OrderVM = new OrderViewModel(_context);
            OrderDishVM = new OrderDishViewModel(_context);
            TableBookingVM = new TableBookingViewModel(_context);
            ReportVM = new ReportViewModel(_context);
        }

        private void RefreshAll(object parameter)
        {
            // Закрываем текущий контекст
            _context.Dispose();

            // Создаем новый контекст
            _context = new Model_R();

            // Пересоздаем ViewModel с новым контекстом
            InitializeViewModels();

            // Обновляем данные
            RefreshAllData();
        }

        private void RefreshAllData()
        {
            ProductVM.LoadProducts();
            DishVM.LoadDishes();
            TableVM.LoadTables();
            DishProdVM.LoadDishes();
            DishProdVM.LoadAvailableProducts();
            BookingVM.LoadBookings();
            UserTypeVM.LoadUserTypes();
            UserVM.LoadData();
            ClientVM.LoadData();
            AdminVM.LoadData();
            CookVM.LoadData();
            DeliveryVM.LoadData();
            WaiterVM.LoadData();
            DiscountVM.LoadDiscounts();
            OrderVM.LoadData();
            OrderDishVM.RefreshData(null);
            ReportVM.GenerateReport(null);
        }

        // Метод для получения контекста для использования в других частях приложения
        public Model_R GetContext()
        {
            return _context;
        }

        // Метод для создания нового контекста (например, для фоновых операций)
        public Model_R CreateNewContext()
        {
            return new Model_R();
        }

        // Метод для сохранения всех изменений во всех ViewModel
        public bool SaveAllChanges()
        {
            try
            {
                // Сохраняем все изменения в контексте
                var changes = _context.SaveChanges();

                // Обновляем данные во всех ViewModel
                RefreshAllData();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Метод для отмены всех изменений
        public void DiscardAllChanges()
        {
            // Отменяем все изменения в контексте
            foreach (var entry in _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged))
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }

            // Обновляем данные во всех ViewModel
            RefreshAllData();
        }

        public ICommand CreateOrderCommand => new RelayCommand(CreateOrder, CanCreateOrder);

        private bool CanCreateOrder(object parameter)
        {
            return CurrentOrder?.HasItems == true && CurrentUser != null;
        }

        private void CreateOrder(object parameter)
        {
            try
            {
                if (!CurrentOrder.HasItems || CurrentUser == null)
                {
                    MessageBox.Show("Нет выбранных блюд или пользователь не авторизован",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var context = new Model_R())
                {
                    // Получаем ID для нового заказа
                    int newOrderId = GetNextOrderId(context);

                    // Создаем новый заказ
                    var newOrder = new Order
                    {
                        ID = newOrderId,
                        Time = DateTime.Now,
                        Count = CurrentOrder.TotalPrice,
                        Statusid = GetDefaultStatusId(context),
                        Clientid = GetClientId(context),
                        // Устанавливаем другие необходимые поля
                        Adress = "", // Можно добавить поле для ввода адреса
                        Bookingid = null, // Если нужно привязать к брони
                        Waiterid = null,
                        Deliveryid = null
                    };
                    int p = 0;
                    // Добавляем заказ в контекст
                    context.Order.Add(newOrder);
                    context.SaveChanges(); // Сохраняем заказ, чтобы получить ID

                    // Теперь добавляем блюда в заказ
                    foreach (var item in CurrentOrder.Items)
                    {
                        if (item.Dish == null) continue;

                        var orderDishId = GetNextOrderDishId(context) + p;

                        var orderDish = new Ord_dish
                        {
                            ID = orderDishId,
                            Orderid = newOrder.ID,
                            Dishid = item.Dish.ID,
                            Count = item.Quantity,
                            Cost = item.TotalPrice,
                            Discounttype = null, // Можно добавить логику скидок
                            Cookid = null
                        };

                        context.Ord_dish.Add(orderDish);
                        p++;
                    }

                    // Сохраняем все изменения
                    context.SaveChanges();

                    // Очищаем текущий заказ
                    CurrentOrder.Clear();

                    // Обновляем список заказов пользователя
                    OrderVM.LoadUserOrders();

                    MessageBox.Show($"Заказ №{newOrder.ID} успешно оформлен!\n" +
                                   $"Сумма: {newOrder.Count?.ToString("C")}",
                                   "Заказ оформлен",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNextOrderId(Model_R context)
        {
            // Используем максимальное значение из базы
            var maxId = context.Order.Max(o => (int?)o.ID) ?? 0;
            return maxId + 1;
        }

        private int GetNextOrderDishId(Model_R context)
        {
            // Используем максимальное значение из базы
            var maxId = context.Ord_dish.Max(od => (int?)od.ID) ?? 0;
            return maxId + 1;
        }

        private int? GetDefaultStatusId(Model_R context)
        {
            // Ищем статус "Новый" или "В обработке"
            var status = context.Status.FirstOrDefault(s =>
                s.Name.ToLower().Contains("новый") ||
                s.Name.ToLower().Contains("обработке"));

            return status?.ID ?? 1; // Возвращаем 1, если статус не найден
        }

        private int? GetClientId(Model_R context)
        {
            if (CurrentUser == null) return null;

            var client = context.Client.FirstOrDefault(c => c.ID == CurrentUser.ID);
            return client?.ID;
        }

        //private void CreateOrder(object parameter)
        //{
        //    try
        //    {
        //        if (!CurrentOrder.HasItems || CurrentUser == null)
        //        {
        //            MessageBox.Show("Нет выбранных блюд или пользователь не авторизован",
        //                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return;
        //        }

        //        // Используем единый контекст вместо создания нового
        //        using (var transaction = _context.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                // Получаем ID для нового заказа
        //                int newOrderId = GetNextOrderId();

        //                // Создаем новый заказ
        //                var newOrder = new Order
        //                {
        //                    ID = newOrderId,
        //                    Time = DateTime.Now,
        //                    Count = CurrentOrder.TotalPrice,
        //                    Statusid = GetDefaultStatusId(),
        //                    Clientid = GetClientId(),
        //                    Adress = "",
        //                    Bookingid = null,
        //                    Waiterid = null,
        //                    Deliveryid = null
        //                };

        //                int p = 0;
        //                // Добавляем заказ в контекст
        //                _context.Order.Add(newOrder);
        //                _context.SaveChanges(); // Сохраняем заказ, чтобы получить ID

        //                // Теперь добавляем блюда в заказ
        //                foreach (var item in CurrentOrder.Items)
        //                {
        //                    if (item.Dish == null) continue;

        //                    var orderDishId = GetNextOrderDishId() + p;

        //                    var orderDish = new Ord_dish
        //                    {
        //                        ID = orderDishId,
        //                        Orderid = newOrder.ID,
        //                        Dishid = item.Dish.ID,
        //                        Count = item.Quantity,
        //                        Cost = item.TotalPrice,
        //                        Discounttype = null,
        //                        Cookid = null
        //                    };

        //                    _context.Ord_dish.Add(orderDish);
        //                    p++;
        //                }

        //                // Сохраняем все изменения
        //                _context.SaveChanges();
        //                transaction.Commit();

        //                // Очищаем текущий заказ
        //                CurrentOrder.Clear();

        //                // Обновляем список заказов пользователя
        //                OrderVM.LoadData();

        //                MessageBox.Show($"Заказ №{newOrder.ID} успешно оформлен!\n" +
        //                               $"Сумма: {newOrder.Count?.ToString("C")}",
        //                               "Заказ оформлен",
        //                               MessageBoxButton.OK,
        //                               MessageBoxImage.Information);

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                throw;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}\n{ex.InnerException?.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private int GetNextOrderId()
        {
            var maxId = _context.Order.Max(o => (int?)o.ID) ?? 0;
            return maxId + 1;
        }

        private int GetNextOrderDishId()
        {
            var maxId = _context.Ord_dish.Max(od => (int?)od.ID) ?? 0;
            return maxId + 1;
        }

        private int? GetDefaultStatusId()
        {
            var status = _context.Status.FirstOrDefault(s =>
                s.Name.ToLower().Contains("новый") ||
                s.Name.ToLower().Contains("обработке"));

            return status?.ID ?? 1;
        }

        private int? GetClientId()
        {
            if (CurrentUser == null) return null;

            var client = _context.Client.FirstOrDefault(c => c.ID == CurrentUser.ID);
            return client?.ID;
        }

        // Команда для сброса заказа
        public ICommand ResetOrderCommand => new RelayCommand(ResetOrder);

        private void ResetOrder(object parameter)
        {
            var result = MessageBox.Show("Вы уверены, что хотите очистить текущий заказ?",
                "Сброс заказа", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentOrder.Clear();
                OnPropertyChanged(nameof(OrderTotalPrice));
                OnPropertyChanged(nameof(OrderTotalItems));
            }
        }

        public void AddDishToOrder(int dishId, int quantity = 1)
        {
            try
            {
                if (CurrentOrder == null)
                {
                    CurrentOrder = new CurrentOrder();
                }

                // Находим блюдо в DishVM
                var dish = DishVM.Dishes?.FirstOrDefault(d => d.ID == dishId);
                if (dish == null)
                {
                    // Если не найдено в DishVM, ищем в едином контексте
                    dish = _context.Dish.FirstOrDefault(d => d.ID == dishId);
                }

                if (dish != null)
                {
                    CurrentOrder.AddItem(dish, quantity);
                    OnPropertyChanged(nameof(OrderTotalPrice));
                    OnPropertyChanged(nameof(OrderTotalItems));
                }
                else
                {
                    MessageBox.Show("Блюдо не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении блюда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для добавления блюда (перегруженный вариант)
        public void AddDishToOrder(Dish dish, int quantity = 1)
        {
            try
            {
                if (CurrentOrder == null)
                {
                    CurrentOrder = new CurrentOrder();
                }

                if (dish != null)
                {
                    // Проверяем доступность блюда
                    var availableDish = _context.Dish.FirstOrDefault(d => d.ID == dish.ID);
                    if (availableDish == null || availableDish.Availability <= 0)
                    {
                        MessageBox.Show("Блюдо недоступно для заказа",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем, не превышаем ли доступное количество
                    var existingItem = CurrentOrder.Items?.FirstOrDefault(item => item.Dish.ID == dish.ID);
                    if (existingItem != null)
                    {
                        var availableCount = availableDish.Availability;
                        if (existingItem.Quantity + quantity > availableCount)
                        {
                            MessageBox.Show($"Доступно только {availableCount} шт.\n" +
                                           $"Уже в заказе: {existingItem.Quantity} шт.",
                                "Недостаточно товара", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    CurrentOrder.AddItem(dish, quantity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении блюда: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для освобождения ресурсов
        public void Dispose()
        {
            _context?.Dispose();

            // Освобождаем все ViewModel
            ProductVM?.Dispose();
            DishVM?.Dispose();
            TableVM?.Dispose();
            DishProdVM?.Dispose();
            BookingVM?.Dispose();
            //UserTypeVM?.Dispose();
            //UserVM?.Dispose();
            //ClientVM?.Dispose();
            //AdminVM?.Dispose();
            //CookVM?.Dispose();
            //DeliveryVM?.Dispose();
            //WaiterVM?.Dispose();
            //DiscountVM?.Dispose();
            //OrderVM?.Dispose();
            //OrderDishVM?.Dispose();
            //TableBookingVM?.Dispose();
        }

        //private void RefreshAll(object parameter)
        //{
        //    ProductVM.LoadProducts();
        //    DishVM.LoadDishes();
        //    TableVM.LoadTables();
        //    DishProdVM.LoadDishes();
        //    DishProdVM.LoadAvailableProducts();
        //    BookingVM.LoadBookings();
        //    UserTypeVM.LoadUserTypes();
        //    UserVM.LoadData();
        //    ClientVM.LoadData();
        //    AdminVM.LoadData();
        //    CookVM.LoadData();
        //    DeliveryVM.LoadData();
        //    WaiterVM.LoadData();
        //    DiscountVM.LoadDiscounts();
        //    OrderVM.LoadData();
        //    OrderDishVM.RefreshData(null);
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}