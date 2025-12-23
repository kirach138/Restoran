namespace WpfApp1.classes_bd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Booking")]
    public partial class Booking
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Booking()
        {
            Order = new HashSet<Order>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public DateTime? Startb { get; set; }

        public DateTime? Endb { get; set; }

        public int Tableid { get; set; }

        public int? Clientid { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        [Column(TypeName = "money")]
        public decimal? Count { get; set; }

        public virtual Client Client { get; set; }

        public virtual Table Table { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
    }
}
