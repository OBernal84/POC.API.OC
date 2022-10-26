using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace POC.API.OC
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();

        static async Task Main(string[] args)
        {
            PrintHeader();

            var jsonText = await GetResultFromAPIText();
            
            StoreData(JObject.Parse(jsonText));

            //GetData();

        }

        /// <summary>
        /// Prints a header starting the application
        /// </summary>
        private static void PrintHeader()
        {
            Console.WriteLine("##########################################################################");
            Console.WriteLine("#");
            Console.WriteLine("#");
            Console.WriteLine("#                            Testing POC API");
            Console.WriteLine("#");
            Console.WriteLine("#");
            Console.WriteLine("##########################################################################");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        /// <summary>
        /// Calls the contentapi.geappliances.io API
        /// </summary>
        /// <returns> A string containing the json result</returns>
        private static Task<string> GetResultFromAPIText()
        {
            Console.WriteLine("Getting JSON from API ...");
            Console.WriteLine("");
            Console.WriteLine("");

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("x-api-key", "cgS1hi0UOL86JevockEQk4VYuTScGzCu5ahk7kyL");
            return Client.GetStringAsync("https://contentapi.geappliances.io/search/b2b/results?N=4294966683"); ;
        }

        /// <summary>
        /// From a JObject it parses, gets the tokens, formats data and stores it into a database
        /// </summary>
        /// <param name="jObject">The Json returned by the API</param>
        private static void StoreData(JObject jObject)
        {
            Console.WriteLine("Saving JSON into database...");

            //Database connection, quick and dirty way
            const string connectionString = @"Data Source=azsqldev1;Initial Catalog=PurchasingTest;Integrated Security = True;Application Name=Construction.UI;";
            var cnn = new SqlConnection(connectionString);
            cnn.Open();

            ////////////////////////////////////////////////////////////////////
            //Here we start playing with the JSON file using Json Path
            ////////////////////////////////////////////////////////////////////

            //Gets single values from records node
            var totalNumRecs = (string)jObject.SelectToken("results[2].records.totalNumRecs");
            var firstRecNum = (string)jObject.SelectToken("results[2].records.firstRecNum");
            var lastRecNum = (string)jObject.SelectToken("results[2].records.lastRecNum");

            //Gets arrays from records node
            var sortOptions = jObject.SelectToken("results[2].records.sortOptions").Select(s => s).ToArray();
            var sortOptionString = string.Empty;
            foreach (var sortOption in sortOptions) sortOptionString += sortOption;

            //Gets all products objects from records node
            var products = jObject.SelectToken("results[2].records.products").Select(s => s).ToArray();

            foreach (var product in products)
            {
                //Gets arrays from product node

                var productId = Guid.NewGuid();
                var productName = ((JProperty)product).Name;
                var searcheables = product.SelectToken("$..Searchable").Select(s => s).ToArray();
                var brandImageNames = product.SelectToken("$..['Brand.ImageName']").Select(s => s).ToArray();
                var upcs = product.SelectToken("$..UPC").Select(s => s).ToArray();
                var benefitCopies = product.SelectToken("$...BenefitCopy").Select(s => s).ToArray();
                var attributes = product.SelectToken("$....Attributes").Select(s => s).ToArray();
                var images = product.SelectTokens("$.....Images").Select(s => s).ToArray();
                var documents = product.SelectTokens("$......Documents").Select(s => s).ToArray();
                var services = product.SelectTokens("$.......Services").Select(s => s).ToArray();
                var relationships = product.SelectTokens("$........Relationships").Select(s => s).ToArray();
                var binaryObjectDetails = product.SelectTokens("$.........BinaryObjectDetails").Select(s => s).ToArray();
                var featuredParts = product.SelectTokens("$..........FeaturedPart").Select(s => s).ToArray();

                ////////////////////////////////////////////////////////////////////
                //Here we finish playing with the JSON file using Json Path
                ////////////////////////////////////////////////////////////////////

                //The following needs to be improved I didn't find a way to convert Jtoken arrays in a concatenate string in a clean way

                var searchableString = string.Empty;
                foreach (var searcheable in searcheables) searchableString += searcheable;

                var brandImageNameString = string.Empty;
                foreach (var brandImageName in brandImageNames) brandImageNameString += brandImageName;

                var upcString = string.Empty;
                foreach (var upc in upcs) upcString += upc;


                //Executing an insert per product

                const string query = "INSERT INTO result_products " +
                                     "(id, totalNumRecs, firstRecNum, lastRecNum, sortOptions, productName, searcheables, brandImageName, upcs) " +
                                     "VALUES " +
                                     "(@id, @totalNumRecs, @firstRecNum, @lastRecNum, @sortOptions, @productName, @searcheables, @brandImageName, @upcs) ";

                var command = new SqlCommand(query, cnn);
                command.Parameters.AddWithValue("@id", productId);
                command.Parameters.AddWithValue("@totalNumRecs", totalNumRecs);
                command.Parameters.AddWithValue("@firstRecNum", firstRecNum);
                command.Parameters.AddWithValue("@lastRecNum", lastRecNum);
                command.Parameters.AddWithValue("@sortOptions", sortOptionString);
                command.Parameters.AddWithValue("@productName", productName);
                command.Parameters.AddWithValue("@searcheables", searchableString);
                command.Parameters.AddWithValue("@brandImageName", brandImageNameString);
                command.Parameters.AddWithValue("@upcs", upcString);

                command.ExecuteNonQuery();

                StoreBenefitCopies(cnn, benefitCopies, productId);

                StoreAttributes(cnn, attributes, productId);

                StoreImages(cnn, images, productId);

                StoreDocuments(cnn, documents, productId);

                StoreServices(cnn, services, productId);

                StoreRelationships(cnn, relationships, productId);

                StoreBinaryObjectDetails(cnn, binaryObjectDetails, productId);

                StoreFeaturedParts(cnn, featuredParts, productId);
            }

            cnn.Close();
        }

        private static void StoreBenefitCopies(SqlConnection cnn, JToken[] benefitCopies, Guid productId)
        {
            foreach (var benefitCopy in benefitCopies)
            {
                var tempObjects = benefitCopy.SelectToken("$");
                var id = Guid.NewGuid();
                var info = string.Empty;

                foreach (var tempObject in tempObjects) info += tempObject.SelectToken("$");

                const string query2 = "INSERT INTO result_benefitCopies " +
                                      "(id, product_id, info) " +
                                      "VALUES " +
                                      "(@id, @product_id, @info) ";

                var command = new SqlCommand(query2, cnn);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@product_id", productId);
                command.Parameters.AddWithValue("@info", info);

                command.ExecuteNonQuery();
            }
        }

        private static void StoreAttributes(SqlConnection cnn, JToken[] attributes, Guid productId)
        {
            var info = string.Empty;
            foreach (var attribute in attributes)
            {
                info += attribute.SelectToken("$");
                var id = Guid.NewGuid();

                const string query2 = "INSERT INTO result_attributes " +
                                      "(id, product_id, info) " +
                                      "VALUES " +
                                      "(@id, @product_id, @info) ";

                var command = new SqlCommand(query2, cnn);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@product_id", productId);
                command.Parameters.AddWithValue("@info", info);

                command.ExecuteNonQuery();
            }
        }

        private static void StoreImages(SqlConnection cnn, IEnumerable<JToken> images, Guid productId)
        {
            foreach (var image in images)
            {
                var tempObjects = image.SelectToken("$");
                var id = Guid.NewGuid();

                if (tempObjects != null && tempObjects.HasValues)
                {
                    foreach (var tempObject in tempObjects)
                    {
                        if (tempObject != null)
                        {
                            var info = tempObject.SelectToken("$").ToString();
                            const string query = "INSERT INTO result_images " +
                                                 "(id, product_id, info) " +
                                                 "VALUES " +
                                                 "(@id, @product_id, @info) ";

                            var command = new SqlCommand(query, cnn);
                            command.Parameters.AddWithValue("@id", id);
                            command.Parameters.AddWithValue("@product_id", productId);
                            command.Parameters.AddWithValue("@info", info);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static void StoreDocuments(SqlConnection cnn, JToken[] documents, Guid productId)
        {
            foreach (var document in documents)
            {
                var tempObjects = document.SelectToken("$");
                var id = Guid.NewGuid();
                foreach (var tempObject in tempObjects)
                {
                    var info = tempObject.SelectToken("$").ToString();
                    const string query2 = "INSERT INTO result_documents " +
                                          "(id, product_id, info) " +
                                          "VALUES " +
                                          "(@id, @product_id, @info) ";

                    var command = new SqlCommand(query2, cnn);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@info", info);

                    command.ExecuteNonQuery();
                }


            }
        }

        private static void StoreServices(SqlConnection cnn, JToken[] services, Guid productId)
        {
            foreach (var service in services)
            {
                var tempObjects = service.SelectToken("$");
                var id = Guid.NewGuid();
                var info = string.Empty;

                foreach (var tempObject in tempObjects) info += tempObject.SelectToken("$");

                if (!string.IsNullOrEmpty(info))
                {
                    const string query2 = "INSERT INTO result_services " +
                                          "(id, product_id, info) " +
                                          "VALUES " +
                                          "(@id, @product_id, @info) ";

                    var command = new SqlCommand(query2, cnn);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@info", info);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void StoreRelationships(SqlConnection cnn, JToken[] relationShips, Guid productId)
        {
            foreach (var relationShip in relationShips)
            {
                var tempObjects = relationShip.SelectToken("$");
                var id = Guid.NewGuid();
                var info = string.Empty;

                foreach (var tempObject in tempObjects) info += tempObject.SelectToken("$");

                if (!string.IsNullOrEmpty(info))
                {
                    const string query2 = "INSERT INTO result_relationShips " +
                                          "(id, product_id, info) " +
                                          "VALUES " +
                                          "(@id, @product_id, @info) ";

                    var command = new SqlCommand(query2, cnn);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@info", info);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void StoreBinaryObjectDetails(SqlConnection cnn, JToken[] binaryObjectDetails, Guid productId)
        {
            foreach (var binaryObjectDetail in binaryObjectDetails)
            {
                var tempObjects = binaryObjectDetail.SelectToken("$");
                var id = Guid.NewGuid();
                var info = string.Empty;

                foreach (var tempObject in tempObjects) info += tempObject.SelectToken("$");

                if (!string.IsNullOrEmpty(info))
                {
                    const string query2 = "INSERT INTO result_binaryObjectDetails " +
                                          "(id, product_id, info) " +
                                          "VALUES " +
                                          "(@id, @product_id, @info) ";

                    var command = new SqlCommand(query2, cnn);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@info", info);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void StoreFeaturedParts(SqlConnection cnn, JToken[] featuredParts, Guid productId)
        {
            foreach (var featuredPart in featuredParts)
            {
                var tempObjects = featuredPart.SelectToken("$");
                var id = Guid.NewGuid();
                var info = string.Empty;

                foreach (var tempObject in tempObjects) info += tempObject.SelectToken("$");

                if (!string.IsNullOrEmpty(info))
                {
                    const string query2 = "INSERT INTO result_featuredParts " +
                                          "(id, product_id, info) " +
                                          "VALUES " +
                                          "(@id, @product_id, @info) ";

                    var command = new SqlCommand(query2, cnn);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@info", info);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void GetData()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Reading JSON from database...");

            const string connectionString = @"Data Source=azsqldev1;Initial Catalog=PurchasingTest;Integrated Security = True;Application Name=Construction.UI;";
            var cnn = new SqlConnection(connectionString);
            cnn.Open();

            const string query = "SELECT totalNumRecs, firstRecNum, lastRecNum, sortOptions, productName, searcheables, brandImageName, upcs, benefitCopies, attributes, images, documents, services, relationships, binaryObjectDetails, featuredParts FROM Results WHERE id = 1";

            var command = new SqlCommand(query, cnn);

            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("The total number of records is");
                    Console.WriteLine(reader["totalNumRecs"]);

                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("The first number of record is");
                    Console.WriteLine(reader["firstRecNum"]);

                    Console.WriteLine("Writing The first product name");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine(reader["productName"]);
                }
            }
            finally
            {

                reader.Close();
            }

            cnn.Close();
        }
    }
}
