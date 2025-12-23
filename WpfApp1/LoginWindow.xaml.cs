using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.classes_bd;
using WpfApp1.models;

namespace WpfApp1
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Устанавливаем фокус на поле логина
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        // Свойство для получения информации о вошедшем пользователе
        public User LoggedInUser => _viewModel.CurrentUser;
        public string UserDisplayInfo => _viewModel.UserDisplayName;

        // Обработчики событий
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoginCommand.Execute(null);
        }

        private void LoginByBookingCode_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoginByBookingCodeCommand.Execute(null);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RegisterCommand.Execute(null);
        }

        private void InputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == nameof(UsernameTextBox))
                {
                    _viewModel.Username = textBox.Text;
                }
                else if (textBox.Name == nameof(BookingCodeTextBox))
                {
                    _viewModel.BookingCode = textBox.Text;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Background = Brushes.White;
                // Используйте один из следующих вариантов:

                // Вариант 1: Создать SolidColorBrush из hex цвета
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#402309"));

                // Или Вариант 2: Использовать готовый цвет
                // textBox.BorderBrush = Brushes.Black; // или любой другой стандартный цвет
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Background = Brushes.White;
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4C8A8"));
            }
        }

        private void ForgotPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("Функция восстановления пароля находится в разработке",
                "Восстановление пароля",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Обработчик нажатия Enter в полях ввода
        private void InputField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender == UsernameTextBox)
                {
                    PasswordBox.Focus();
                }
                else if (sender == PasswordBox)
                {
                    _viewModel.LoginCommand.Execute(null);
                }
                else if (sender == BookingCodeTextBox)
                {
                    _viewModel.LoginByBookingCodeCommand.Execute(null);
                }
            }
        }
    }
}