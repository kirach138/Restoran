namespace WpfApp1.classes_bd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Ord_dish
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int? Orderid { get; set; }

        public int? Dishid { get; set; }

        public int? Count { get; set; }

        [Column(TypeName = "money")]
        public decimal? Cost { get; set; }

        public int? Discounttype { get; set; }

        public int? Cookid { get; set; }

        public virtual Cook Cook { get; set; }

        public virtual Discount Discount { get; set; }

        public virtual Dish Dish { get; set; }

        public virtual Order Order { get; set; }
    }
}
