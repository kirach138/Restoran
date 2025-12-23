namespace WpfApp1.classes_bd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Dish")]
    public partial class Dish /*: INotifyPropertyChanged*/
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Dish()
        {
            Dish_prod = new HashSet<Dish_prod>();
            Ord_dish = new HashSet<Ord_dish>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Column(TypeName = "money")]
        public decimal? Price { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int? Availability { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Dish_prod> Dish_prod { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ord_dish> Ord_dish { get; set; }


        
    }
}
