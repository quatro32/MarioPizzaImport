using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    public class OrderImporter : Importer<order>
    {
        public OrderImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string filePath)
        {
            //Winkelnaam;Klantnaam;TelefoonNr;Email;Adres;Woonplaats;Besteldatum;AfleverType;AfleverDatum;AfleverMoment;Product;PizzaBodem;PizzaSaus;Prijs;Bezorgkosten;Aantal;Extra Ingrediënten;Prijs Extra Ingrediënten;Regelprijs;Totaalprijs;Gebruikte Coupon;Coupon Korting;Te Betalen
            List<order> orders = new List<order>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                int row = 1;
                String line;
                order order = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (row > 6 && line != string.Empty)
                    {
                        string[] paths = line.Split(';');
                        if (paths[0] != string.Empty)
                        {
                            order = new order();

                            store store = database.stores.SingleOrDefault(i => i.name.ToUpper() == paths[0].ToUpper());
                            order.store = store ?? throw new Exception(string.Format("Store {0} does not exists!", paths[0]));

                            order.clientname = paths[1];
                            order.phonenumber = paths[2];
                            //add email field to order table, path[3]
                            //add address entity to order, get postalcode from database by using a lookup query, paths[4],paths[5]
                            order.datecreated = GetDateTimeFromLongDateString(paths[6]);

                            deliverytype deliverytype = database.deliverytypes.SingleOrDefault(i => i.name.ToUpper() == paths[7].ToUpper());
                            order.deliverytype = deliverytype ?? throw new Exception(string.Format("Deliverytype {0} does not exists!", paths[7]));

                            order.datedelivered = GetDateTimeFromLongDateString(paths[8], paths[9]);
                            //add deliverycosts to order table
                        }

                        orderline orderline = new orderline();
                        orderline.order = order;

                        //search if product exists
                        product product = database.products.SingleOrDefault(i => i.name.ToUpper() == paths[10].ToUpper());
                        orderline.product = product ?? throw new Exception(string.Format("Product {0} does not exists!", paths[10]));
                        //after we delete exception, create product and also create/select product type

                        orderline.amount = Convert.ToInt32(paths[15]);

                        string[] extraIngredients = paths[16].Split(',');
                        if (extraIngredients.Length > 0)
                        {
                            foreach (var ei in extraIngredients)
                            {
                                ingredient ingredient = database.ingredients.SingleOrDefault(i => i.name.ToUpper() == ei.Trim().ToUpper());
                                //TODO: look for mapping, else create new/exception
                                productorderingredient productorderingredient = new productorderingredient();
                                productorderingredient.ingredient = ingredient ?? throw new Exception(string.Format("Product {0} does not exists!", ei));
                                productorderingredient.orderline = orderline;
                            }
                        }

                    }
                    row++;
                }
            }

            //database.BulkInsert<order>(orders, );
            return orders.Count;
        }

        private int GetMonthNumberFromString(string monthString)
        {
            switch (monthString.ToLower())
            {
                case "januari":
                    return 1;
                case "februari":
                    return 2;
                case "maart":
                    return 3;
                case "april":
                    return 4;
                case "mei":
                    return 5;
                case "juni":
                    return 6;
                case "juli":
                    return 7;
                case "augustus":
                    return 8;
                case "september":
                    return 9;
                case "oktober":
                    return 10;
                case "november":
                    return 11;
                case "december":
                    return 12;
                default:
                    return 1;
            }
        }

        private DateTime GetDateTimeFromLongDateString(string dateString)
        {
            string[] paths = dateString.Split(' ');
            return new DateTime(Convert.ToInt32(paths[3]), GetMonthNumberFromString(paths[2]), Convert.ToInt32(paths[1]));
        }

        private DateTime GetDateTimeFromLongDateString(string dateString, string timeString)
        {
            return GetDateTimeFromLongDateString(dateString) + TimeSpan.Parse(timeString);
        }
    }
}
