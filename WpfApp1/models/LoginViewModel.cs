using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _bookingCode;
        private bool _rememberMe;
        private string _errorMessage;
        private bool _isLoginEnabled;
        private User _currentUser;
        private string _userTypeName;
        private string _userDisplayName;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                UpdateLoginButtonState();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                UpdateLoginButtonState();
            }
        }

        public string BookingCode
        {
            get => _bookingCode;
            set
            {
                _bookingCode = value;
                OnPropertyChanged(nameof(BookingCode));
                UpdateLoginButtonState();
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set
            {
                _rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set
            {
                _isLoginEnabled = value;
                OnPropertyChanged(nameof(IsLoginEnabled));
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(UserTypeName));
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        public bool IsLoggedIn => CurrentUser != null;

        public string UserTypeName
        {
            get => _userTypeName;
            private set
            {
                _userTypeName = value;
                OnPropertyChanged(nameof(UserTypeName));
            }
        }

        public string UserDisplayName
        {
            get => _userDisplayName;
            private set
            {
                _userDisplayName = value;
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand LoginByBookingCodeCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
            LoginByBookingCodeCommand = new RelayCommand(LoginByBookingCode, CanLoginByBookingCode);
            RegisterCommand = new RelayCommand(Register);

            // Загружаем сохраненные данные, если есть
            //LoadSavedCredentials();
        }

        private void UpdateLoginButtonState()
        {
            IsLoginEnabled = !string.IsNullOrWhiteSpace(Username) &&
                            !string.IsNullOrWhiteSpace(Password);
        }

        private bool CanLogin(object parameter)
        {
            return IsLoginEnabled;
        }

        private void Login(object parameter)
        {
            try
            {
                ErrorMessage = string.Empty;

                using (var context = new Model_R())
                {
                    // Ищем пользователя по логину
                    var user = context.User
                        .Include("User_type")
                        .FirstOrDefault(u => u.Login == Username);

                    if (user == null)
                    {
                        ErrorMessage = "Пользователь с таким логином не найден";
                        return;
                    }

                    // Проверяем пароль (в реальном приложении нужно использовать хеширование)
                    if (user.Password != Password)
                    {
                        ErrorMessage = "Неверный пароль";
                        return;
                    }

                    //// Сохраняем данные для входа, если выбрано "Запомнить меня"
                    //if (RememberMe)
                    //{
                    //    SaveCredentials();
                    //}
                    //else
                    //{
                    //    ClearSavedCredentials();
                    //}

                    // Получаем тип пользователя и дополнительную информацию
                    SetUserInfo(user, context);

                    // Устанавливаем текущего пользователя
                    CurrentUser = user;

                    // Закрываем окно входа
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var loginWindow = Application.Current.Windows
                            .OfType<LoginWindow>()
                            .FirstOrDefault();

                        if (loginWindow != null)
                        {
                            loginWindow.DialogResult = true;
                            loginWindow.Close();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при входе: {ex.Message}";
            }
        }

        private void SetUserInfo(User user, Model_R context)
        {
            // Получаем тип пользователя из User_type
            var userType = context.User_type.FirstOrDefault(ut => ut.ID == user.Type);
            UserTypeName = userType?.Name ?? "Неизвестный тип";

            // Получаем конкретную информацию о пользователе
            string specificName = user.Name;

            // Ищем в конкретных таблицах
            if (user.Type == 0) // Клиент
            {
                var client = context.Client.FirstOrDefault(c => c.ID == user.ID);
                //if (client != null)
                //{
                //    specificName = client.Name ?? user.Name;
                //}
            }
            else if (user.Type == 1) // Админ
            {
                var admin = context.Admin.FirstOrDefault(a => a.ID == user.ID);
                //if (admin != null)
                //{
                //    specificName = admin.Name ?? user.Name;
                //}
            }
            else if (user.Type == 2) // Повар
            {
                var cook = context.Cook.FirstOrDefault(c => c.ID == user.ID);
                //if (cook != null)
                //{
                //    specificName = cook.Name ?? user.Name;
                //}
            }
            else if (user.Type == 3) // Официант
            {
                var waiter = context.Waiter.FirstOrDefault(w => w.ID == user.ID);
                //if (waiter != null)
                //{
                //    specificName = waiter.Name ?? user.Name;
                //}
            }
            else if (user.Type == 4) // Доставка
            {
                var delivery = context.Delivery.FirstOrDefault(d => d.ID == user.ID);
                //if (delivery != null)
                //{
                //    specificName = delivery.Name ?? user.Name;
                //}
            }
            specificName = user.Name;
            UserDisplayName = $"{UserTypeName}: {specificName}";
        }

        private bool CanLoginByBookingCode(object parameter)
        {
            return !string.IsNullOrWhiteSpace(BookingCode);
        }

        private void LoginByBookingCode(object parameter)
        {
            try
            {
                ErrorMessage = string.Empty;

                using (var context = new Model_R())
                {
                    // Ищем бронирование по коду
                    var booking = context.Booking
                        .Include("Client.User")
                        .FirstOrDefault(b => b.Code == BookingCode);

                    if (booking == null)
                    {
                        ErrorMessage = "Бронирование с таким кодом не найдено";
                        return;
                    }

                    if (booking.Client?.User == null)
                    {
                        ErrorMessage = "Для этого бронирования не найден пользователь";
                        return;
                    }

                    // Устанавливаем пользователя как клиента
                    SetUserInfo(booking.Client.User, context);
                    CurrentUser = booking.Client.User;

                    // Закрываем окно входа
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var loginWindow = Application.Current.Windows
                            .OfType<LoginWindow>()
                            .FirstOrDefault();

                        if (loginWindow != null)
                        {
                            loginWindow.DialogResult = true;
                            loginWindow.Close();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при входе по коду: {ex.Message}";
            }
        }

        private void Register(object parameter)
        {
            // TODO: Реализовать окно регистрации
            MessageBox.Show("Функция регистрации находится в разработке",
                "Регистрация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        //private void SaveCredentials()
        //{
        //    try
        //    {
        //        // Сохраняем только логин и флаг "запомнить"
        //        if (Properties.Settings.Default != null)
        //        {
        //            Properties.Settings.Default.Username = Username;
        //            Properties.Settings.Default.RememberMe = RememberMe;
        //            Properties.Settings.Default.Save();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении учетных данных: {ex.Message}");
        //    }
        //}

        //private void LoadSavedCredentials()
        //{
        //    try
        //    {
        //        if (Properties.Settings.Default != null && Properties.Settings.Default.RememberMe)
        //        {
        //            Username = Properties.Settings.Default.Username ?? "";
        //            RememberMe = Properties.Settings.Default.RememberMe;
        //            // Пароль не загружаем - пользователь должен ввести его заново
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке учетных данных: {ex.Message}");
        //    }
        //}

        //private void ClearSavedCredentials()
        //{
        //    try
        //    {
        //        if (Properties.Settings.Default != null)
        //        {
        //            Properties.Settings.Default.Username = string.Empty;
        //            Properties.Settings.Default.RememberMe = false;
        //            Properties.Settings.Default.Save();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Ошибка при очистке учетных данных: {ex.Message}");
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}