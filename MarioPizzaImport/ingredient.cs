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
    
    public partial class ingredient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ingredient()
        {
            this.ingredientprices = new HashSet<ingredientprice>();
            this.productingredients = new HashSet<productingredient>();
            this.productorderingredients = new HashSet<productorderingredient>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public bool isvegetarian { get; set; }
        public bool isspicy { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ingredientprice> ingredientprices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<productingredient> productingredients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<productorderingredient> productorderingredients { get; set; }
    }
}
