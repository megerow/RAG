﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG.DataStax
{
    internal class API
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

        public async Task<FindResult> FindAsync(string collectionName, FindRequest findRequest)
        {
            string findJson = JsonConvert.SerializeObject(findRequest);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/" + collectionName);
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent(findJson, null, "application/json");
            request.Content = content;
            try
            {
                var response = client.Send(request);
                response.EnsureSuccessStatusCode();
                FindResult fr = JsonConvert.DeserializeObject<FindResult>(await response.Content.ReadAsStringAsync());
                return fr;
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            return null;
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

    // -- INSERT ONE

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

    // -- INSERT MANY

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

    // -- FIND REQUEST

    public class FindRequestBody
    {
        public Sort sort { get; set; } = new Sort();
        public FindOptions options { get; set; } = new FindOptions();
    }

    public class FindOptions
    {
        public int limit { get; set; } = 1;
        public bool includeSimilarity { get; set; } = true;
    }

    public class FindRequest
    {
        public FindRequestBody find { get; set; } = new FindRequestBody();
    }

    public class Sort
    {
        [JsonProperty("$vector")]
        public List<double> vector { get; set; } = new List<double>();
    }

    // -- FIND RESULT

    public class FoundData
    {
        public List<FoundDocument> documents { get; set; }
        public object nextPageState { get; set; }
    }

    public class FoundDocument
    {
        public string _id { get; set; }
        public string company { get; set; }

        [JsonProperty("$vector")]
        public List<double> vector { get; set; }

        [JsonProperty("$similarity")]
        public double similarity { get; set; }
    }

    public class FindResult
    {
        public FoundData data { get; set; }
    }

}
