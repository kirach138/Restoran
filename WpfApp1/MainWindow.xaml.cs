using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.classes_bd;
using WpfApp1.models;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    //Добавить отчёт для админа, починить круд,
    //добавить вход для официантов, поваров, доставщиков,
    //добавить ввод адреса для заказов, залить проект на гит, сделать презентацию
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private MainViewModel _mainViewModel;
        private User _currentUser;
        private string _userDisplayInfo;

        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            DataContext = _mainViewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;

            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.LoggedInUser;
                _userDisplayInfo = loginWindow.UserDisplayInfo;

                // Передаем пользователя в MainViewModel
                _mainViewModel.CurrentUser = _currentUser;

                UpdateUserInterface();
                LoadData();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void UpdateUserInterface()
        {
            // Обновляем кнопку входа на информацию о пользователе
            if (ButtonUser != null)
            {
                ButtonUser.Content = _userDisplayInfo ?? "Войти";

                // Меняем обработчик - теперь это будет меню пользователя
                ButtonUser.Click -= ButtonUser_Click;
                ButtonUser.Click += UserMenuButton_Click;
            }
        }

        private void LoadData()
        {
            try
            {
                // Загружаем блюда из базы данных
                _mainViewModel.DishVM.LoadDishes();

                // Инициализируем CurrentOrder если он null
                if (_mainViewModel.CurrentOrder == null)
                {
                    _mainViewModel.CurrentOrder = new CurrentOrder();
                }

                // Загружаем заказы текущего пользователя
                _mainViewModel.OrderVM.LoadUserOrders();

                // Передаем текущего пользователя в ViewModels
                _mainViewModel.CurrentUser = _currentUser;
                _mainViewModel.TableBookingVM.CurrentUser = _currentUser;

                // Обновляем UI
                OnPropertyChanged(nameof(IsUserLoggedIn));

                // Для отладки
                Console.WriteLine($"Загружено блюд: {_mainViewModel.DishVM.Dishes?.Count ?? 0}");
                Console.WriteLine($"Загружено заказов: {_mainViewModel.OrderVM.Orders?.Count ?? 0}");
                Console.WriteLine($"Текущий пользователь: {_currentUser?.ID}");
                Console.WriteLine($"CurrentOrder инициализирован: {_mainViewModel.CurrentOrder != null}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int orderId))
            {
                var result = MessageBox.Show("Вы уверены, что хотите отменить заказ?\n" +
                                           "Заказ будет удален из вашего списка.",
                                           "Отмена заказа",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CancelOrderAndResetClient(orderId);
                }
            }
        }

        private void CancelOrderAndResetClient(int orderId)
        {
            try
            {
                using (var context = new Model_R())
                {
                    var order = context.Order.FirstOrDefault(o => o.ID == orderId);
                    if (order == null)
                    {
                        MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обнуляем Clientid
                    order.Clientid = null;

                    // Устанавливаем статус "Отменен"
                    var cancelledStatus = context.Status.FirstOrDefault(s => s.Name == "Отменен");
                    if (cancelledStatus != null)
                    {
                        order.Statusid = cancelledStatus.ID;
                    }

                    context.SaveChanges();

                    // Обновляем список заказов в ViewModel
                    _mainViewModel.OrderVM.LoadUserOrders();

                    MessageBox.Show("Заказ успешно отменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCreateOrder(object parameter)
        {
            if (_mainViewModel.CurrentUser == null || _mainViewModel.CurrentOrder == null)
                return false;

            if (!_mainViewModel.CurrentOrder.HasItems)
                return false;

            // Проверяем, что все блюда еще доступны
            try
            {
                using (var context = new Model_R())
                {
                    foreach (var item in _mainViewModel.CurrentOrder.Items)
                    {
                        var dish = context.Dish.FirstOrDefault(d => d.ID == item.Dish.ID);
                        if (dish == null || dish.Availability < item.Quantity)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем контекстное меню для пользователя
            var contextMenu = new ContextMenu();

            // Информация о пользователе
            var userInfoItem = new MenuItem
            {
                Header = _userDisplayInfo,
                IsEnabled = false
            };
            contextMenu.Items.Add(userInfoItem);

            // Разделитель
            contextMenu.Items.Add(new Separator());

            // Выход
            var logoutItem = new MenuItem
            {
                Header = "Выйти",
                
            };
            logoutItem.Click += LogoutMenuItem_Click;
            contextMenu.Items.Add(logoutItem);

            // Настройки профиля
            var profileItem = new MenuItem
            {
                Header = "Настройки профиля"
            };
            profileItem.Click += ProfileMenuItem_Click;
            contextMenu.Items.Add(profileItem);

            // Показываем меню
            contextMenu.IsOpen = true;
        }

        private void OpenWindowDishButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int dishId))
            {
                // Создаем и показываем окно с информацией о блюде
                var dishWindow = new W_Dish(_mainViewModel);
                dishWindow.LoadDishInfo(dishId);
                dishWindow.Owner = this;

                if (dishWindow.ShowDialog() == true)
                {
                    // Получаем количество из окна состав
                    int quantity = dishWindow.SelectedQuantity;
                    var dish = dishWindow.SelectedDish;

                    // Добавляем блюдо в текущий заказ
                    _mainViewModel.AddDishToOrder(dish, quantity);

                    MessageBox.Show($"Добавлено {quantity} шт. \"{dish.Name}\" в заказ\n" +
                                   $"На сумму: {(dish.Price * quantity)?.ToString("C")}",
                        "Добавлено в заказ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private void AddToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int dishId))
            {
                // Находим блюдо в списке
                var dish = _mainViewModel.DishVM.Dishes.FirstOrDefault(d => d.ID == dishId);

                if (dish != null)
                {
                    // Добавляем блюдо в текущий заказ (по умолчанию 1 шт.)
                    _mainViewModel.AddDishToOrder(dish, 1);

                    MessageBox.Show($"Добавлено 1 шт. \"{dish.Name}\" в заказ",
                        "Добавлено в заказ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        // В классе MainWindow добавьте свойство для проверки авторизации
        public bool IsUserLoggedIn => _currentUser != null;

        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки профиля находятся в разработке",
                "Профиль",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?",
                "Выход",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Сбрасываем информацию о пользователе
                _currentUser = null;
                _userDisplayInfo = null;
                _mainViewModel.CurrentUser = null;

                // Восстанавливаем кнопку входа
                ButtonUser.Content = "Войти";
                ButtonUser.Click -= UserMenuButton_Click;
                ButtonUser.Click += ButtonUser_Click;

                // Показываем окно входа
                ShowLoginWindow();
            }
        }

        // Обработчик для кнопки входа (когда пользователь не авторизован)
        private void ButtonUser_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginWindow();
        }


        // Свойство для привязки в XAML
        public MainViewModel MainVM
        {
            get => _mainViewModel;
            set
            {
                _mainViewModel = value;
                // Здесь можно добавить уведомление об изменении, если нужно
            }
        }

        // Обновите обработчик для кнопки "Столики":
        private void ButtonTables_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
            // Можно добавить обновление данных при переходе на вкладку
            _mainViewModel.TableBookingVM.SearchAvailableTables(null);
        }

        
        // Остальные обработчики событий (оставляем как были)
        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
        }

        //private void OpenWindowAdminButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Ваша реализация
        //}

        private void ButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }

        //private void ButtonTables_Click(object sender, RoutedEventArgs e)
        //{
        //    MainTabControl.SelectedIndex = 2;
        //}

        private void ButtonOrderAll_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
        }

        

        private void OpenWindowAdminButton_Click(object sender, RoutedEventArgs e)
        {
            W_Admin secondWindow = new W_Admin();

            

            // Подписка на событие закрытия окна
            secondWindow.Closed += (s, args) =>
            {
                
            };

            secondWindow.Show();
        }

        private void OpenWindowTableButton_Click(object sender, RoutedEventArgs e)
        {
            W_Table secondWindow = new W_Table();

            

            // Подписка на событие закрытия окна
            secondWindow.Closed += (s, args) =>
            {
                
            };

            secondWindow.Show();
        }

        private void OpenWindowBookingButton_Click(object sender, RoutedEventArgs e)
        {
            W_Booking secondWindow = new W_Booking();

            // Подписка на событие закрытия окна
            secondWindow.Closed += (s, args) =>
            {
                // Действия после закрытия окна
                MessageBox.Show("Второе окно закрыто");
            };

            secondWindow.Show();
        }

        private void OpenWindowOrderButton_Click(object sender, RoutedEventArgs e)
        {
            W_Order secondWindow = new W_Order();

            
            secondWindow.Closed += (s, args) =>
            {
                
            };

            secondWindow.Show();
        }

        
        private void ButtonOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Заказ оформлен!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonOrderC_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Заказ отменен", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открывается окно для оформления бронирования", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonBookC_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Бронь отменена", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

       

        private void ButtonUserEnter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вход успешно выполнен!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonUserReg_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открывается окно регистрации", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonDish_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выбрано блюдо", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a WPF layout demonstration application.", "Help", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void Action1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Action 1 executed!", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Action2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Action 2 executed!", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Action3_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Action 3 executed!", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Action4_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Action 4 executed!", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Footer settings applied!", "Apply", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}

