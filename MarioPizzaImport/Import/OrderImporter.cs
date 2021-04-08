using EntityFramework.BulkInsert.Extensions;
using MarioPizzaImport.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarioPizzaImport
{
    public class OrderImporter : Importer<order>
    {
        private List<mapping> mappings = null;
        private List<store> stores = null;
        private List<deliverytype> deliveryTypes = null;
        private List<product> products = null;
        private List<sauce> sauces = null;
        private List<bottom> bottoms = null;
        private List<ingredient> ingredients = null;

        public OrderImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode)
        {
            database.Configuration.AutoDetectChangesEnabled = false;
            mappings = database.mappings.ToList();
            stores = database.stores.ToList();
            deliveryTypes = database.deliverytypes.ToList();
            products = database.products.ToList();
            sauces = database.sauces.ToList();
            bottoms = database.bottoms.ToList();
            ingredients = database.ingredients.ToList();
        }

        protected override int Import(string filePath)
        {
            //Winkelnaam;Klantnaam;TelefoonNr;Email;Adres;Woonplaats;Besteldatum;AfleverType;AfleverDatum;AfleverMoment;Product;PizzaBodem;PizzaSaus;Prijs;Bezorgkosten;Aantal;Extra Ingrediënten;Prijs Extra Ingrediënten;Regelprijs;Totaalprijs;Gebruikte Coupon;Coupon Korting;Te Betalen
            List<order> orders = new List<order>();
            List<coupon> coupons = database.coupons.ToList();
            using (StreamReader sr = new StreamReader(filePath))
            {
                int row = 1;
                int count = 0;
                String line;
                order order = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (row >= 6 && line != string.Empty)
                    {
                        string[] paths = line.Split(';');
                        if (paths[0] != string.Empty)
                        {
                            order = new order();

                            string storeName = paths[0];
                            string storeNameUppercased = storeName.ToUpper();
                            store store = stores.SingleOrDefault(i => i.name.ToUpper() == storeNameUppercased);
                            if (store == null)
                            {
                                Logger.Instance.LogError(filePath, string.Format("Store {0} does not exists on line {1}!", storeName, row));
                                continue;
                            }
                            order.store = store;

                            order.clientname = paths[1];
                            order.phonenumber = paths[2];
                            order.email = paths[3].ToLower();

                            order.address = new address()
                            {
                                countrycode = countrycode.code,
                                street = Regex.Replace(paths[4], "[^A-Z a-z]", ""),
                                number = Regex.Replace(paths[4], "[^0-9]", "")
                            };

                            try
                            {
                                order.datecreated = GetDateTimeFromLongDateString(paths[6]);
                            }
                            catch
                            {
                                Logger.Instance.LogError(filePath, string.Format("Unable to parse DateTime from datestring {0} on line {1}", paths[6], row));
                                continue;
                            }


                            string deliveryCostField = paths[14].Trim();
                            if (!string.IsNullOrEmpty(deliveryCostField))
                            {
                                order.deliverycost = Decimal.Parse(Regex.Replace(deliveryCostField, "[^0-9.]", "")) / 100;
                            }

                            string deliveryType = paths[7];
                            string deliveryTypeUppercased = deliveryType.ToUpper();
                            deliverytype deliverytype = deliveryTypes.SingleOrDefault(i => i.name.ToUpper() == deliveryTypeUppercased);
                            if (deliverytype == null)
                            {
                                Logger.Instance.LogError(filePath, string.Format("Deliverytype {0} does not exists on line {1}!", deliveryType, row));
                                continue;
                            }
                            order.deliverytype = deliverytype;

                            if (paths[9].Contains("soon"))
                            {
                                order.datedelivered = order.datecreated;
                            }
                            else
                            {
                                try
                                {
                                    order.datedelivered = GetDateTimeFromLongDateString(paths[8], paths[9]);
                                }
                                catch
                                {
                                    Logger.Instance.LogError(filePath, string.Format("Unable to parse DateTime from datestring {0} on line {1}", paths[6], row));
                                    continue;
                                }
                            }

                            //TODO: Proper
                            order.preferredtime = order.datecreated;

                            string orderPrice = paths[19];
                            if (!string.IsNullOrEmpty(orderPrice))
                            {
                                order.price = Decimal.Parse(Regex.Replace(orderPrice, "[^0-9.]", "")) / 100;
                            }

                            string couponField = Encoding.UTF8.GetString(Encoding.Default.GetBytes(paths[20]));
                            if (!string.IsNullOrEmpty(couponField))
                            {
                                // ToDo change to SingleOrDefault once duplicates are removed from the db.
                                coupon coupon = coupons.SingleOrDefault(i => i.description == couponField);
                                if (coupon == null)
                                {
                                    coupon = new coupon();
                                    coupon.description = couponField;
                                    coupon.startdate = DateTime.Now;
                                    coupon.enddate = DateTime.Now;
                                    coupon.code = "0000";
                                    coupons.Add(coupon);
                                }
                                order.coupon = coupon;
                            }

                            orders.Add(order);
                            count++;
                            Console.WriteLine("[INFO] Added order to orders. Current amount: {0}", count);
                        }

                        orderline orderline = new orderline();

                        //search if product exists
                        string mappedProductName = this.GetMappedValue(paths[10], false);
                        if (mappedProductName == string.Empty)
                        {
                            continue;
                        }
                        // ToDo change to SingleOrDefault once duplicates are removed from the db.
                        product product = products.SingleOrDefault(i => i.name == mappedProductName);
                        if (product == null)
                        {
                            Logger.Instance.LogError(filePath, string.Format("Product {0} does not exists on line {1}!", mappedProductName, row));
                            continue;
                        }
                        orderline.product = product;

                        string bottomName = paths[11];
                        if (string.IsNullOrEmpty(bottomName) == false)
                        {
                            string mappedBottomName = this.GetMappedValue(paths[11], false);
                            // ToDo change to SingleOrDefault once duplicates are removed from the db.
                            bottom bottom = bottoms.SingleOrDefault(i => i.name == mappedBottomName);
                            if (bottom == null)
                            {
                                Logger.Instance.LogError(filePath, string.Format("Bottom {0} does not exists on line {1}!", mappedBottomName, row));
                                continue;
                            }
                            orderline.bottom = bottom;
                        }

                        string sauceName = paths[12];
                        if (string.IsNullOrEmpty(sauceName) == false)
                        {
                            string mappedSauceName = this.GetMappedValue(paths[12], false);
                            // ToDo change to SingleOrDefault once duplicates are removed from the db.
                            sauce sauce = sauces.SingleOrDefault(i => i.name == mappedSauceName);
                            if (sauce == null)
                            {
                                Logger.Instance.LogError(filePath, string.Format("Sauce {0} does not exists on line {1}!", mappedSauceName, row));
                                continue;
                            }
                            // If the sauce is not the same as the default sauce, add it to productordersauce
                            if (sauce != product.sauce)
                            {
                                productordersauce productordersauce = new productordersauce();
                                productordersauce.orderline = orderline;
                                productordersauce.sauce = sauce;
                                productordersauce.amount = 1;
                                orderline.productordersauces.Add(productordersauce);
                            }
                        }

                        orderline.price = Decimal.Parse(Regex.Replace(paths[13], "[^0-9.]", "")) / 100;

                        orderline.amount = Convert.ToInt32(paths[15]);

                        string orderlinePrice = paths[18];
                        if (!string.IsNullOrEmpty(orderlinePrice))
                        {
                            orderline.price = Decimal.Parse(Regex.Replace(orderlinePrice, "[^0-9.]", "")) / 100;
                        }

                        string[] extraIngredients = paths[16].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (extraIngredients.Length > 0)
                        {
                            foreach (var ei in extraIngredients)
                            {
                                string mappedIngredientName = this.GetMappedValue(ei.Trim(), true);
                                ingredient ingredient = ingredients.SingleOrDefault(i => i.name == mappedIngredientName);
                                //TODO: look for mapping, else create new/exception
                                if (ingredient == null)
                                {
                                    Logger.Instance.LogError(filePath, string.Format("Ingredient {0} does not exists on line {1}!", mappedIngredientName, row));
                                    continue;
                                }

                                if (orderline.productorderingredients.Count > 0)
                                {
                                    IncrementOrCreateProductOrderIngredient(orderline, ingredient);
                                }
                                else
                                {
                                    CreateProductOrderIngredient(orderline, ingredient);
                                }
                            }
                        }
                        order.orderlines.Add(orderline);
                    }
                    row++;
                }
            }

            database.orders.AddRange(orders);

            Console.WriteLine("[INFO] Saving " + orders.Count + " records to database...");

            DateTime before = DateTime.Now;
            database.SaveChanges();

            DateTime after = DateTime.Now;
            TimeSpan ts = after.Subtract(before);
            Console.WriteLine(string.Format("Completed after {0}:{1}:{2}:{3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds));

            //database.BulkInsert<order>(orders, 5000);
            return orders.Count;
        }

        private void IncrementOrCreateProductOrderIngredient(orderline orderline, ingredient ingredient)
        {
            bool ingredientfound = false;
            foreach (productorderingredient orderIngredient in orderline.productorderingredients)
            {
                if (orderIngredient.ingredient == ingredient)
                {
                    orderIngredient.amount++;
                    ingredientfound = true;
                    break;
                }
            }

            if (ingredientfound == false)
            {
                CreateProductOrderIngredient(orderline, ingredient);
            }
        }

        private void CreateProductOrderIngredient(orderline orderline, ingredient ingredient)
        {
            productorderingredient productorderingredient = new productorderingredient();
            productorderingredient.amount = 1;
            productorderingredient.ingredient = ingredient;
            orderline.productorderingredients.Add(productorderingredient);
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
            return GetDateTimeFromLongDateString(dateString) + DateTime.Parse(timeString).TimeOfDay;
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
    }
}