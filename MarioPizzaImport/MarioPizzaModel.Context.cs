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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class dbi298845_prangersEntities : DbContext
    {
        public dbi298845_prangersEntities()
            : base("name=dbi298845_prangersEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<address> addresses { get; set; }
        public virtual DbSet<bottom> bottoms { get; set; }
        public virtual DbSet<bottomprice> bottomprices { get; set; }
        public virtual DbSet<countrycode> countrycodes { get; set; }
        public virtual DbSet<coupon> coupons { get; set; }
        public virtual DbSet<deliverytype> deliverytypes { get; set; }
        public virtual DbSet<ingredient> ingredients { get; set; }
        public virtual DbSet<ingredientprice> ingredientprices { get; set; }
        public virtual DbSet<order> orders { get; set; }
        public virtual DbSet<orderline> orderlines { get; set; }
        public virtual DbSet<postalcode> postalcodes { get; set; }
        public virtual DbSet<product> products { get; set; }
        public virtual DbSet<productcategory> productcategories { get; set; }
        public virtual DbSet<productingredient> productingredients { get; set; }
        public virtual DbSet<productorderingredient> productorderingredients { get; set; }
        public virtual DbSet<productordersauce> productordersauces { get; set; }
        public virtual DbSet<productprice> productprices { get; set; }
        public virtual DbSet<sauce> sauces { get; set; }
        public virtual DbSet<store> stores { get; set; }
        public virtual DbSet<township> townships { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<import_log> import_log { get; set; }
        public virtual DbSet<mapping> mappings { get; set; }
        public virtual DbSet<postalcode_import> postalcode_import { get; set; }
    
        public virtual int ImportPostalCode()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ImportPostalCode");
        }
    
        public virtual int SaveLog(string type, string fileName, string errorString)
        {
            var typeParameter = type != null ?
                new ObjectParameter("type", type) :
                new ObjectParameter("type", typeof(string));
    
            var fileNameParameter = fileName != null ?
                new ObjectParameter("fileName", fileName) :
                new ObjectParameter("fileName", typeof(string));
    
            var errorStringParameter = errorString != null ?
                new ObjectParameter("errorString", errorString) :
                new ObjectParameter("errorString", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SaveLog", typeParameter, fileNameParameter, errorStringParameter);
        }
    }
}
