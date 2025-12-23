namespace WpfApp1.classes_bd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Order")]
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            Ord_dish = new HashSet<Ord_dish>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int? Bookingid { get; set; }

        public int? Clientid { get; set; }

        public int? Statusid { get; set; }

        public DateTime? Time { get; set; }

        public string Adress { get; set; }

        [Column(TypeName = "money")]
        public decimal? Count { get; set; }

        public int? Waiterid { get; set; }

        public int? Deliveryid { get; set; }

        public virtual Booking Booking { get; set; }

        public virtual Client Client { get; set; }

        public virtual Delivery Delivery { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ord_dish> Ord_dish { get; set; }

        public virtual Status Status { get; set; }

        public virtual Waiter Waiter { get; set; }
    }
}
