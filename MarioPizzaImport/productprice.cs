
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
    
public partial class productprice
{

    public int id { get; set; }

    public int productid { get; set; }

    public decimal price { get; set; }

    public string currency { get; set; }

    public System.DateTime startdate { get; set; }

    public decimal vat { get; set; }

    public int countrycodeid { get; set; }



    public virtual countrycode countrycode { get; set; }

    public virtual product product { get; set; }

}

}
