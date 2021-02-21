using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using CicekSepetiBotConsole.Models;
using HtmlAgilityPack;

namespace CicekSepetiBotConsole
{
    class Program
    {
        private static string product_card_query = "//div[@class='products__container-background']";
        private const string BASE_URI = "https://www.ciceksepeti.com";
        static void Main(string[] args)
        {
            Console.WriteLine("Çiçek sepeti adresini giriniz : ");
            string url = Console.ReadLine();

            List<Product> product_list = new List<Product>();
            
            HtmlWeb web = new HtmlWeb();
            web.PreRequest += PreRequest;
            int pageNumber = 50;

            while (true)
            {
                try
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc["page"] = pageNumber.ToString();
                    string preparedUri = url + ToQueryString(nvc);
                    var doc = web.Load(preparedUri);

                    var products = doc.DocumentNode.SelectNodes(product_card_query);

                    foreach (var productDiv in products)
                    {
                        Product new_product = new Product();

                        var product_url_node =
                            productDiv.SelectSingleNode(".//a[@class='products__item-link js-products__item-link']");
                        if (product_url_node != null)
                            new_product.productUri = BASE_URI + product_url_node.Attributes["href"].Value;
                        
                        var product_image_node = productDiv.SelectSingleNode(".//a[@class='products__item-link js-products__item-link']//div[contains(@class,'products__item-img-container')]//img");
                        string product_image_url = string.Empty;
                        if (product_image_node != null)
                            new_product.productImageUri = product_image_node.Attributes["data-src"].Value;
                        
                        var title_node = productDiv.SelectSingleNode(".//p[@class='products__item-title']");
                        if (title_node != null)
                            new_product.productTitle = title_node.InnerText;
                        
                        var price_node = productDiv.SelectSingleNode(".//a[@class='products__item-link js-products__item-link']//div[@class='products__item-info']//div[@class='products__item-details']//div[@class='products__item-price js-no-tax']//div[@class='price price--now']");
                        if (price_node != null)
                            new_product.productPrice = price_node.Attributes["data-price"].Value;
                        
                        product_list.Add(new_product);
                    }

                    pageNumber++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    break;
                }
            }

            foreach (var product in product_list)
            {
                Console.WriteLine(product.productUri);
            }
        }

        private static bool PreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;
            return true;
        }

        public static string ToQueryString(NameValueCollection nvc)
        {
            var array = (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format(
                    "{0}={1}",
                    HttpUtility.UrlEncode(key),
                    HttpUtility.UrlEncode(value))
            ).ToArray();
            return "?" + string.Join("&", array);
        }
    }
}