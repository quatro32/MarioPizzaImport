using MarioPizzaImport.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    public class OrderImporter : Importer<order>
    {
        private IEnumerable<mapping> mappings;

        public OrderImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode)
        {
            //get all mapping, so we don't have to do a select-query easch itteration.
            mappings = database.mappings.AsEnumerable();
        }

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
                            if (order != null)
                            {
                                // TODO: range.
                                this.database.orders.Add(order);

                                // Save to database
                                this.database.SaveChanges();
                            }

                            order = new order();

                            string storeNameUppercased = paths[0].ToUpper();
                            store store = database.stores.SingleOrDefault(i => i.name.ToUpper() == storeNameUppercased);
                            order.store = store ?? throw new Exception(string.Format("Store {0} does not exists!", paths[0]));

                            order.clientname = paths[1];
                            order.phonenumber = paths[2];
                            order.email = paths[3];
                            //add email field to order table, path[3]
                            //add address entity to order, get postalcode from database by using a lookup query, paths[4],paths[5]

                            order.address = new address()
                            {
                                countrycode = countrycode.code,
                                street = Regex.Replace(paths[4], "[^A-Z a-z]", ""),
                                number = Regex.Replace(paths[4], "[^0-9]", "")
                            };

                            order.datecreated = GetDateTimeFromLongDateString(paths[6]);

                            string deliveryTypeUppercased = paths[7].ToUpper();
                            deliverytype deliverytype = database.deliverytypes.SingleOrDefault(i => i.name.ToUpper() == deliveryTypeUppercased);
                            order.deliverytype = deliverytype ?? throw new Exception(string.Format("Deliverytype {0} does not exists!", paths[7]));

                            if (paths[9].Contains("soon"))
                            {
                                order.datedelivered = order.datecreated;
                            }
                            else
                            {
                                order.datedelivered = GetDateTimeFromLongDateString(paths[8], paths[9]);
                            }

                            //TODO: Proper
                            order.preferredtime = order.datecreated;
                            //add deliverycosts to order table
                        }

                        orderline orderline = new orderline();
                        orderline.order = order;

                        //search if product exists
                        string mappedProductName = this.GetMappedValue(paths[10], false);
                        product product = database.products.SingleOrDefault(i => i.name == mappedProductName);
                        orderline.product = product ?? throw new Exception(string.Format("Product {0} does not exists!", paths[10]));
                        //after we delete exception, create product and also create/select product type

                        orderline.amount = Convert.ToInt32(paths[15]);

                        string[] extraIngredients = paths[16].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (extraIngredients.Length > 0)
                        {
                            foreach (var ei in extraIngredients)
                            {
                                string mappedIngredientName = this.GetMappedValue(ei.Trim(), true);
                                ingredient ingredient = database.ingredients.SingleOrDefault(i => i.name == mappedIngredientName);
                                
                                productorderingredient productorderingredient = new productorderingredient();
                                productorderingredient.ingredient = ingredient ?? throw new Exception(string.Format("Ingredient {0} does not exists!", ei));
                                productorderingredient.orderline = orderline;
                            }
                        }

                        if (order != null)
                        {
                            order.orderlines.Add(orderline);
                        }
                    }
                    row++;
                }
            }

            //database.BulkInsert<order>(orders, );
            return orders.Count;
        }

        private string GetMappedValue(string value, bool isIngredient)
        {
            mapping mapping = mappings.SingleOrDefault(i => i.originalname.ToLower() == value.Trim().ToLower() && i.mappedto != string.Empty && i.isingredient == isIngredient);
            if (mapping != null && mapping.mappedto != "" && mapping.mappedto != null)
            {
                return mapping.mappedto;
            }
            return value;
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
