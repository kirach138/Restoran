using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class TableViewModel : INotifyPropertyChanged
    {
        private Table _table;
        private bool _isSelected;
        private bool _isAvailable;

        public Table Table
        {
            get => _table;
            set
            {
                _table = value;
                OnPropertyChanged(nameof(Table));
            }
        }

        public int ID => Table?.ID ?? 0;
        public int Number => Table?.Number ?? 0;
        public int Places => Table?.Places ?? 0;
        public decimal? Cost => Table?.Cost;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(StatusBackground));
                OnPropertyChanged(nameof(StatusBorderColor));
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                _isAvailable = value;
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(StatusBackground));
                OnPropertyChanged(nameof(StatusBorderColor));
            }
        }

        // Цвета для разных состояний столика
        public Brush StatusBackground
        {
            get
            {
                if (!IsAvailable)
                    return new SolidColorBrush(Color.FromRgb(150, 150, 150)); // Серый - недоступен
                if (IsSelected)
                    return new SolidColorBrush(Color.FromRgb(90, 52, 19)); // Темно-коричневый - выбран
                return new SolidColorBrush(Color.FromRgb(64, 35, 9)); // Основной коричневый - доступен
            }
        }

        public Brush StatusBorderColor
        {
            get
            {
                if (!IsAvailable)
                    return new SolidColorBrush(Color.FromRgb(120, 120, 120));
                if (IsSelected)
                    return new SolidColorBrush(Color.FromRgb(248, 184, 91)); // Золотой для выбранного
                return new SolidColorBrush(Color.FromRgb(90, 52, 19));
            }
        }

        public ICommand SelectTableCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}