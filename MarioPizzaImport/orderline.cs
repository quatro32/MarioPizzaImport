//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MarioPizzaImport
{
    using System;
    using System.Collections.Generic;
    
    public partial class orderline
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public orderline()
        {
            this.productordersauces = new HashSet<productordersauce>();
            this.productorderingredients = new HashSet<productorderingredient>();
        }
    
        public int id { get; set; }
        public int productid { get; set; }
        public int orderid { get; set; }
        public Nullable<int> bottomid { get; set; }
        public int amount { get; set; }
        public decimal price { get; set; }
    
        public virtual bottom bottom { get; set; }
        public virtual order order { get; set; }
        public virtual product product { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<productordersauce> productordersauces { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<productorderingredient> productorderingredients { get; set; }
    }
}
