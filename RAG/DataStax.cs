using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG.DataStax
{
    internal class Writer
    {
        public async Task CreateCollectionAsync(string  collectionName)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace");
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent("{\n  \"createCollection\": {\n    \"name\": \"" + collectionName + "\",\n    \"options\" : {\n      \"vector\" : {\n        \"dimension\" : 1536,\n        \"metric\" : \"cosine\"\n      }\n    }\n  }\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }
        public async Task WriteAsync(string collectionName, string id, string company, List<double> vector)
        {
            Customer customer = new Customer(id, company, vector);

            string customerJson = JsonConvert.SerializeObject(customer);


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/" + collectionName);
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent(customerJson, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }

        public async Task WriteAsync(string collectionName, Customers customers)
        {

            string customersJson = JsonConvert.SerializeObject(customers);


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/" + collectionName);
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent(customersJson, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }
    }

    public class Document
    {
        public string _id { get; set; }
        public string company { get; set; }

        [JsonProperty("$vector")]
        public List<double> vector { get; set; }
    }

    public class InsertOne
    {
        public Document document { get; set; } = new Document();
    }

    public class Customer
    {
        public InsertOne insertOne { get; set; } = new InsertOne();

        public Customer() { }

        public Customer(string id, string company, List<double> vector)
        {
            this.insertOne.document._id = id;
            this.insertOne.document.company = company;
            this.insertOne.document.vector = vector;
        }
    }

    // --

    public class InsertMany
    {
        public List<Document> documents { get; set; } = new List<Document>();
        public Options options { get; set; } = new Options();
    }

    public class Options
    {
        public bool ordered { get; set; } = false;
    }

    public class Customers
    {
        public InsertMany insertMany { get; set; } = new InsertMany();
    }

}
