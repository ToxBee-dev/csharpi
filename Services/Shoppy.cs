using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace csharpi.Services
{
    //Created By Plague
    //Copyright Reserved
    //Do Not Remove This Notice
    public class Shoppy
    {
        #region Serializable Classes
        public class Product
        {
            public string id;
            public object attachment_id;
            public string title;
            public string description;
            public object image;
            public bool unlisted;
            public string type;
            public int price;
            public string currency;
            public Email email;
            public int stock_warning;
            public Quantity quantity;
            public int confirmations;
            public Custom_Field[] custom_fields;
            public string[] gateways;
            public string[] webhook_urls;
            public string dynamic_url;
            public object position;
            public object created_at;
            public object updated_at;
            public object stock; // Unknown as the number is over int.MaxValue but no decimal points, maybe long?
            public object[] accounts;
        }

        public class Email
        {
            public bool enabled;
            public string value;
        }

        public class Quantity
        {
            public int min;
            public int max;
        }

        public class Order
        {
            public string id;
            public string pay_id;
            public string product_id;
            public string coupon_id;
            public int price;
            public string currency;
            public string exchange_rate;
            public string email;
            public int delivered;
            public int confirmations;
            public int required_confirmations;
            public string transaction_id;
            public string crypto_address;
            public string crypto_amount;
            public string shipping_address; // Fuckin Useless Mate
            public int quantity;
            public string gateway; // PayPal, Etc
            public Custom_Field[] custom_fields;
            public string refund_id;
            public bool is_replacement;
            public string replacement_id;
            public object paid_at; // Idk The Type For This Atm
            public object disputed_at; // Same ^
            public object created_at;
            public object is_partial;
            public object crypto_received;
            public string return_url;
            public object[] coupon;
            public Product product;
            public Agent agent;
            public string hash;
        }

        public class Geo
        {
            public string ip;
            public string iso_code;
            public string country;
            public string city;
            public string state;
            public string state_name;
            public string postal_code;
            public double lat;
            public double lon;
            public string timezone;
            public string continent;
            public string currency;
            public bool @default;
            public bool cached;
        }

        public class Data
        {
            public bool is_mobile;
            public bool is_tablet;
            public bool is_desktop;
            public string platform;
            public NameAndVersion browser;
            public NameAndVersion device;
        }

        public class NameAndVersion
        {
            public string name;
            public object version; // Why Is This A Bool? Can Also Be String It Seems
        }

        public class Agent
        {
            public Geo geo;
            public Data data;
        }

        public class Custom_Field
        {
            public string name;
            public string type;
            public bool required;
            public string value;
        }

        public class Feedback
        {
            public string id;
            public string order_id;
            public string comment;
            public int stars;
            public int rating;
            public object response;
            public object created_at;
            public object updated_at;
            public ProductMinimalInfo product;
        }

        public class ProductMinimalInfo
        {
            public string id;
            public string title;
        }

        public class Query
        {
            public string id;
            public int status;
            public string subject;
            public string email;
            public string message;
            public object created_at;
            public object updated_at;
            public object[] replies;
            public Agent agent;
        }
        #endregion

        private string APIKey;

        public Shoppy(string APIKey)
        {
            this.APIKey = APIKey;
        }

        #region Methods - Deserialize Them Into The Classes Above Yourself!
        public string GetAllOrders()
        {
            return SendRequest("orders", "GET");
        }

        public string GetAllOrdersPage(int page)
        {
            return SendRequest($"orders/?page={page}", "GET");
        }

        public string GetAllProducts()
        {
            return SendRequest("products", "GET");
        }

        public string GetAllFeedbacks()
        {
            return SendRequest("feedbacks", "GET");
        }

        public string GetAllQueries()
        {
            return SendRequest("queries", "GET");
        }

        public string GetOrder(string id)
        {
            return SendRequest($"orders/{id}", "GET");
        }

        public string GetProduct(string id)
        {
            return SendRequest($"products/{id}", "GET");
        }

        public string GetFeedback(string id)
        {
            return SendRequest($"feedback/{id}", "GET");
        }

        public string GetQuery(string id)
        {
            return SendRequest($"queries/{id}", "GET");
        }

        public string UpdateProduct(string id, Product NewProductData)
        {
            return SendRequest($"products/{id}", "POST", JsonConvert.SerializeObject(NewProductData));
        }

        public string UpdateQuery(string id, bool status)
        {
            return SendRequest($"products/{id}/" + (status ? "reopen" : "close"), "POST");
        }

        public string DeleteProduct(string id)
        {
            return SendRequest($"products/{id}", "DELETE");
        }
        #endregion

        #region Private Methods
        private string SendRequest(string url, string type, string jsonData = "")
        {
            var request = (HttpWebRequest)WebRequest.Create($"https://shoppy.gg/api/v1/{url}");

            switch (request.Method)
            {
                case "GET":
                    break;
                case "PUT":
                    break;
                case "POST":
                    request.ContentType = "application/json";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(jsonData);
                    }
                    break;
                case "DELETE":
                    break;
            }

            request.UserAgent = "SomeApplication/1.0";

            request.Headers.Add("Authorization", $"{APIKey}");

            var response = request.GetResponse();

            string responseFromServer;

            using (var dataStream = response.GetResponseStream())
            {
                var reader = new StreamReader(dataStream ?? throw new InvalidOperationException());
                responseFromServer = reader.ReadToEnd();
            }

            response.Close();

            return responseFromServer;
        }
        #endregion
    }
}
