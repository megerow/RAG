// -------------------------------------------------------------------------------
// Purpose: Helper class to work with the DataStax vector database service.
//
// By:      Mark Gerow
// Date:    4/29/2024
// --------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace RAG.DataStax
{
    /// <summary>
    /// Methods for working with the DataStax vector database.
    /// </summary>
    internal class API
    {
        /// <summary>
        /// Create a new DataStax vector database collection.
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <returns></returns>
        public async Task CreateCollectionAsync(string  collectionName)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace");
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent("{\n  \"createCollection\": {\n    \"name\": \"" + collectionName + "\",\n    \"options\" : {\n      \"vector\" : {\n        \"dimension\" : 1536,\n        \"metric\" : \"cosine\"\n      }\n    }\n  }\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            RAG6.Utilities.DisplayMessage(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Delete an item in a collection.
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <param name="id">Unique ID of item to delete</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(string collectionName, string id)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/{collectionName}");
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent("{\n  \"deleteOne\": {\n    \"filter\": {\n      \"_id\": \""+id+"\"\n    }\n  }\n}\n", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Find match(es) in a collection.
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <param name="findRequest">Structure specifying search criteria</param>
        /// <returns>Structure containing search results</returns>
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
                RAG6.Utilities.DisplayMessage($"{ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Add a new item or update an existing item to/in a collection.
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <param name="id">Unique ID of item to be updated</param>
        /// <param name="name">Name of item</param>
        /// <param name="docType">Doc type of item</param>
        /// <param name="vector">Vector representation of item</param>
        /// <returns></returns>
        public async Task WriteAsync(string collectionName, string id, string name, string docType, List<double> vector)
        {
            Document document = new Document(id, name, docType, vector);

            string docJson = JsonConvert.SerializeObject(document);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/" + collectionName);
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent(docJson, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Add multiple documents to a collection at once.
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <param name="documents">Collection of documents to add</param>
        /// <returns></returns>
        public async Task WriteAsync(string collectionName, Documents documents)
        {
            string customersJson = JsonConvert.SerializeObject(documents);
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://953ccde9-fa5a-4634-a3a9-31d41d3bfb33-us-east1.apps.astra.datastax.com/api/json/v1/default_keyspace/" + collectionName);
            request.Headers.Add("Token", "AstraCS:jBqdQovblyPQlWIgBZtsGEws:aa14ebbbcd7f99d3f419f9ae56439cacd776bbc0bf20892e654a42a72a187879");
            var content = new StringContent(customersJson, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }

    // Classes used to work with DataStax vector store when adding or updating a single item.

    public class InsertOne
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string docType { get; set; }

        [JsonProperty("$vector")]
        public List<double> vector { get; set; }
    }

    public class InsertOneDocument
    {
        public InsertOne document { get; set; } = new InsertOne();
    }

    public class Document
    {
        public InsertOneDocument insertOne { get; set; } = new InsertOneDocument();

        public Document() { }

        public Document(string id, string name, string docType, List<double> vector)
        {
            this.insertOne.document._id = id;
            this.insertOne.document.name = name;
            this.insertOne.document.docType = docType;
            this.insertOne.document.vector = vector;
        }
    }

    // Classes used to work with DataStax vector store when adding or updating multiple items at once.

    public class InsertMany
    {
        public List<InsertOne> documents { get; set; } = new List<InsertOne>();
        public Options options { get; set; } = new Options();
    }

    public class Options
    {
        public bool ordered { get; set; } = false;
    }

    public class Documents
    {
        public InsertMany insertMany { get; set; } = new InsertMany();
    }

    // Classes used when searching the DataStax vector store.

    public class FindRequestBody
    {
        public Sort sort { get; set; } = new Sort();
        public FindOptions options { get; set; } = new FindOptions();
    }

    public class FindOptions
    {
        public int limit { get; set; } = 3;
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

    // Classes used to unpack the DataStax find results.

    public class FoundData
    {
        public List<FoundDocument> documents { get; set; }
        public object nextPageState { get; set; }
    }

    public class FoundDocument
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string docType { get; set; }

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
