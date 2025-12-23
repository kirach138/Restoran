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
    public class OrderDishViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Ord_dish _selectedOrderDish;
        private ObservableCollection<Client> _availableClients;
        private ObservableCollection<Order> _clientOrders;
        private ObservableCollection<Ord_dish> _orderDishes;
        private ObservableCollection<Dish> _availableDishes;
        private ObservableCollection<Discount> _availableDiscounts;
        private ObservableCollection<Cook> _availableCooks;

        private Client _selectedClient;
        private Order _selectedOrder;

        private void LoadAvailableCooks()
        {
            AvailableCooks.Clear();
            foreach (var cook in _context.Cook
                .Include(c => c.User)  // Включаем User для отображения имени
                .ToList())
            {
                AvailableCooks.Add(cook);
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

        public ObservableCollection<Order> ClientOrders
        {
            get => _clientOrders;
            set
            {
                _clientOrders = value;
                OnPropertyChanged(nameof(ClientOrders));
            }
        }

        public ObservableCollection<Ord_dish> OrderDishes
        {
            get => _orderDishes;
            set
            {
                _orderDishes = value;
                OnPropertyChanged(nameof(OrderDishes));
            }
        }

        public ObservableCollection<Dish> AvailableDishes
        {
            get => _availableDishes;
            set
            {
                _availableDishes = value;
                OnPropertyChanged(nameof(AvailableDishes));
            }
        }

        public ObservableCollection<Discount> AvailableDiscounts
        {
            get => _availableDiscounts;
            set
            {
                _availableDiscounts = value;
                OnPropertyChanged(nameof(AvailableDiscounts));
            }
        }

        public ObservableCollection<Cook> AvailableCooks
        {
            get => _availableCooks;
            set
            {
                _availableCooks = value;
                OnPropertyChanged(nameof(AvailableCooks));
            }
        }

        public Ord_dish SelectedOrderDish
        {
            get => _selectedOrderDish;
            set
            {
                _selectedOrderDish = value;
                OnPropertyChanged(nameof(SelectedOrderDish));
            }
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                LoadClientOrders();
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                LoadOrderDishes();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }

        // Свойства для статистики
        public decimal TotalCost => OrderDishes?.Sum(od => od.Cost ?? 0) ?? 0;
        public int TotalDishes => OrderDishes?.Sum(od => od.Count ?? 0) ?? 0;
        //public decimal AverageDiscount => OrderDishes?.Where(od => od.Discount != null).Average(od => od.Discount?.Value ?? 0) ?? 0;

        public OrderDishViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            AvailableClients = new ObservableCollection<Client>();
            ClientOrders = new ObservableCollection<Order>();
            OrderDishes = new ObservableCollection<Ord_dish>();
            AvailableDishes = new ObservableCollection<Dish>();
            AvailableDiscounts = new ObservableCollection<Discount>();
            AvailableCooks = new ObservableCollection<Cook>();

            AddCommand = new RelayCommand(AddOrderDish, CanAdd);
            DeleteCommand = new RelayCommand(DeleteOrderDish, CanDelete);
            SaveCommand = new RelayCommand(SaveChanges);
            RefreshCommand = new RelayCommand(RefreshData);

            LoadData();
        }

        private void LoadData()
        {
            LoadAvailableClients();
            LoadAvailableDishes();
            LoadAvailableDiscounts();
            LoadAvailableCooks();
        }

        private void LoadAvailableClients()
        {
            AvailableClients.Clear();
            foreach (var client in _context.Client.Include(c => c.User).ToList())
            {
                AvailableClients.Add(client);
            }
        }

        private void LoadAvailableDishes()
        {
            AvailableDishes.Clear();
            foreach (var dish in _context.Dish.ToList())
            {
                AvailableDishes.Add(dish);
            }
        }

        private void LoadAvailableDiscounts()
        {
            AvailableDiscounts.Clear();
            foreach (var discount in _context.Discount.ToList())
            {
                AvailableDiscounts.Add(discount);
            }
        }

        //private void LoadAvailableCooks()
        //{
        //    AvailableCooks.Clear();
        //    foreach (var cook in _context.Cook.Include(c => c.User).ToList())
        //    {
        //        AvailableCooks.Add(cook);
        //    }
        //}

        public void LoadClientOrders()
        {
            ClientOrders.Clear();
            OrderDishes.Clear();

            if (SelectedClient == null) return;

            var orders = _context.Order
                .Where(o => o.Clientid == SelectedClient.ID)
                .Include(o => o.Status)
                .OrderByDescending(o => o.Time)
                .ToList();

            foreach (var order in orders)
            {
                ClientOrders.Add(order);
            }

            OnPropertyChanged(nameof(ClientOrders));
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalDishes));
            //OnPropertyChanged(nameof(AverageDiscount));
        }

        public void LoadOrderDishes()
        {
            OrderDishes.Clear();

            if (SelectedOrder == null) return;

            var orderDishes = _context.Ord_dish
                .Where(od => od.Orderid == SelectedOrder.ID)
                .Include(od => od.Dish)
                .Include(od => od.Discount)
                .Include(od => od.Cook)
                .Include(od => od.Cook.User)  // Важно: включаем User для Cook
                .ToList();

            foreach (var orderDish in orderDishes)
            {
                OrderDishes.Add(orderDish);
            }

            OnPropertyChanged(nameof(OrderDishes));
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalDishes));
            //OnPropertyChanged(nameof(AverageDiscount));
        }

        //public void LoadOrderDishes()
        //{
        //    OrderDishes.Clear();

        //    if (SelectedOrder == null) return;

        //    var orderDishes = _context.Ord_dish
        //        .Where(od => od.Orderid == SelectedOrder.ID)
        //        .Include(od => od.Dish)
        //        .Include(od => od.Discount)
        //        .Include(od => od.Cook)
        //        .Include(od => od.Cook.User)
        //        .ToList();

        //    foreach (var orderDish in orderDishes)
        //    {
        //        OrderDishes.Add(orderDish);
        //    }

        //    OnPropertyChanged(nameof(OrderDishes));
        //    OnPropertyChanged(nameof(TotalCost));
        //    OnPropertyChanged(nameof(TotalDishes));
        //    //OnPropertyChanged(nameof(AverageDiscount));
        //}

        private void AddOrderDish(object parameter)
        {
            if (SelectedOrder == null || AvailableDishes.Count == 0) return;

            int newId = 1;
            if (_context.Ord_dish.Any())
            {
                newId = _context.Ord_dish.Max(od => od.ID) + 1;
            }

            var newOrderDish = new Ord_dish
            {
                ID = newId,
                Orderid = SelectedOrder.ID,
                Order = SelectedOrder,
                Dishid = AvailableDishes.First().ID,
                Dish = AvailableDishes.First(),
                Count = 1,
                Cost = AvailableDishes.First()?.Price ?? 0,
                Discounttype = AvailableDiscounts.FirstOrDefault()?.ID,
                Cookid = AvailableCooks.FirstOrDefault()?.ID
            };

            OrderDishes.Add(newOrderDish);
            SelectedOrderDish = newOrderDish;
            _context.Ord_dish.Add(newOrderDish);

            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalDishes));
        }

        private void DeleteOrderDish(object parameter)
        {
            if (SelectedOrderDish != null)
            {
                var dishToDelete = _context.Ord_dish.Find(SelectedOrderDish.ID);
                if (dishToDelete != null)
                {
                    _context.Ord_dish.Remove(dishToDelete);
                }
                OrderDishes.Remove(SelectedOrderDish);
                SelectedOrderDish = null;

                OnPropertyChanged(nameof(TotalCost));
                OnPropertyChanged(nameof(TotalDishes));
                //OnPropertyChanged(nameof(AverageDiscount));
            }
        }

        private void SaveChanges(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Изменения успешно сохранены.");

                // Пересчитываем общую сумму заказа
                if (SelectedOrder != null)
                {
                    SelectedOrder.Count = OrderDishes.Sum(od => od.Cost ?? 0);
                    _context.SaveChanges();
                }

                OnPropertyChanged(nameof(TotalCost));
                OnPropertyChanged(nameof(TotalDishes));
                //OnPropertyChanged(nameof(AverageDiscount));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        public void RefreshData(object parameter)
        {
            LoadData();
            LoadClientOrders();
            LoadOrderDishes();
        }

        private bool CanAdd(object parameter)
        {
            return SelectedOrder != null && AvailableDishes.Count > 0;
        }

        private bool CanDelete(object parameter)
        {
            return SelectedOrderDish != null;
        }

        // Метод для пересчета стоимости при изменении количества или выборе скидки
        public void RecalculateCost(Ord_dish orderDish)
        {
            if (orderDish == null || orderDish.Dish == null) return;

            decimal basePrice = orderDish.Dish.Price ?? 0;
            int count = orderDish.Count ?? 1;
            decimal discountValue = orderDish.Discount?.Value ?? 0;

            orderDish.Cost = basePrice * count * (1 - discountValue / 100);
            OnPropertyChanged(nameof(TotalCost));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}