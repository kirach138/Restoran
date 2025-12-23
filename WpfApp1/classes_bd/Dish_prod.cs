namespace WpfApp1.classes_bd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.CompilerServices;

    public partial class Dish_prod : INotifyPropertyChanged
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int? Dishid { get; set; }

        public int? Productid { get; set; }

        //public int? Count { get; set; }

        public virtual Dish Dish { get; set; }

        public virtual Product Product { get; set; }

        private int? _count;

        public new int? Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
