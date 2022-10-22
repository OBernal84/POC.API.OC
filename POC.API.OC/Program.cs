using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

            GetData();

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
            const string connectionString = @"Data Source=SERVER;Initial Catalog=Test;User ID=sa;Password=test";
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

                var productName = ((JProperty)product).Name;
                var searcheables = product.SelectToken("$..Searchable").Select(s => s).ToArray();
                var brandImageNames = product.SelectToken("$..['Brand.ImageName']").Select(s => s).ToArray();
                var upcs = product.SelectToken("$..UPC").Select(s => s).ToArray();
                var benefitCopies = product.SelectToken("$...BenefitCopy").Select(s => s).ToArray();
                var attributes = product.SelectToken("$....Attributes").Select(s => s).ToArray();
                var images = product.SelectTokens("$.....Images");
                var documents = product.SelectTokens("$......Documents").Select(s => s).ToArray();
                var services = product.SelectTokens("$.......Services").Select(s => s).ToArray();
                var relationships = product.SelectTokens("$........Relationships").Select(s => s).ToArray();
                var binaryObjectDetails = product.SelectTokens("$.........BinaryObjectDetails").Select(s => s).ToArray();
                var featuredParts = product.SelectTokens("$..........FeaturedPart");

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

                var benefitCopyString = string.Empty;
                foreach (var benefitCopy in benefitCopies) benefitCopyString += benefitCopy;

                var attributeString = string.Empty;
                foreach (var attribute in attributes) attributeString += attribute;

                var imageString = string.Empty;
                foreach (var image in images) imageString += image;

                var documentString = string.Empty;
                foreach (var document in documents) documentString += document;

                var serviceString = string.Empty;
                foreach (var service in services) serviceString += service;

                var relationshipString = string.Empty;
                foreach (var relationship in relationships) relationshipString += relationship;

                var binaryObjectDetailsString = string.Empty;
                foreach (var binaryObjectDetail in binaryObjectDetails) binaryObjectDetailsString += binaryObjectDetail;

                var featuredPartString = string.Empty;
                foreach (var featuredPart in featuredParts) featuredPartString += featuredPart;


                //Finally executing an insert per product

                const string query = "INSERT INTO Results " +
                                     "(totalNumRecs, firstRecNum, lastRecNum, sortOptions, productName, searcheables, brandImageName, upcs, benefitCopies, attributes, images, documents, services, relationships, binaryObjectDetails, featuredParts) " +
                                     "VALUES " +
                                     "(@totalNumRecs, @firstRecNum, @lastRecNum, @sortOptions, @productName, @searcheables, @brandImageName, @upcs, @benefitCopies, @attributes, @images, @documents, @services, @relationships, @binaryObjectDetails, @featuredParts) ";

                var command = new SqlCommand(query, cnn);
                command.Parameters.AddWithValue("@totalNumRecs", totalNumRecs);
                command.Parameters.AddWithValue("@firstRecNum", firstRecNum);
                command.Parameters.AddWithValue("@lastRecNum", lastRecNum);
                command.Parameters.AddWithValue("@sortOptions", sortOptionString);
                command.Parameters.AddWithValue("@productName", productName);
                command.Parameters.AddWithValue("@searcheables", searchableString);
                command.Parameters.AddWithValue("@brandImageName", brandImageNameString);
                command.Parameters.AddWithValue("@upcs", upcString);
                command.Parameters.AddWithValue("@benefitCopies", benefitCopyString);
                command.Parameters.AddWithValue("@attributes", attributeString);
                command.Parameters.AddWithValue("@images", imageString);
                command.Parameters.AddWithValue("@documents", documentString);
                command.Parameters.AddWithValue("@services", serviceString);
                command.Parameters.AddWithValue("@relationships", relationshipString);
                command.Parameters.AddWithValue("@binaryObjectDetails", binaryObjectDetailsString);
                command.Parameters.AddWithValue("@featuredParts", featuredPartString);

                command.ExecuteNonQuery();
            }

            cnn.Close();
        }

        private static void GetData()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Reading JSON from database...");

            const string connectionString = @"Data Source=SERVER;Initial Catalog=Test;User ID=sa;Password=test";
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
