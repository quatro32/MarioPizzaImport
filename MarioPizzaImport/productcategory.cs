
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
    
public partial class productcategory
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public productcategory()
    {

        this.coupons = new HashSet<coupon>();

        this.products = new HashSet<product>();

        this.productcategory1 = new HashSet<productcategory>();

    }


    public int id { get; set; }

    public string name { get; set; }

    public Nullable<int> parentproductcategoryid { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<coupon> coupons { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<product> products { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<productcategory> productcategory1 { get; set; }

    public virtual productcategory productcategory2 { get; set; }

}

}
