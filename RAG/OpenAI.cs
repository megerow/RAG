using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RAG.OpenAI
{
    public class Embedder
    {
        // If empty constructor is used then the input text
        // must be set separately before the EmbedAsync method is called
        public Embedder() { }

        // Prefill the input text
        public Embedder(string text)
        {
            this.text = text;
        }

        // Url to the OpenAI embedding service
        public string embeddingsUrl { get; } = "https://api.openai.com/v1/embeddings";

        // The embedding model to use
        public string model { get; } = "text-embedding-ada-002";

        // Text to convert to an embedding
        public string text { get; set; } = "";

        // Holds JSON string returned from API
        private string _toString { get; set; } = "";
        
        // Override the native ToString method to return JSON of response
        public override string ToString() { return _toString; }

        // Object to hold the response from OpenAI API
        public EmbeddingResult Result { get; set; }

        // If called without parameters, then just use the text property
        public async Task<EmbeddingResult> EmbedAsync()
        {
            return await EmbedAsync(text);
        }

        // Otherwise use the provided text
        public async Task<EmbeddingResult> EmbedAsync(string _text) 
        {
            // Define HTTP client to call OpenAI embeddings API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, embeddingsUrl);
            request.Headers.Add("Authorization", "Bearer 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");

            // Set the model and the contents
            var content = new StringContent("{\"model\": \"" + model + "\",\"input\": \"" + _text + "\"}", null, "application/json");
            request.Content = content;

            // Call the API and return the result
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            // Save text version of the result for later use
            _toString = await response.Content.ReadAsStringAsync();

            // Convert result to object and return to calling program
            string json = await response.Content.ReadAsStringAsync();
            Result = JsonConvert.DeserializeObject<EmbeddingResult>(json);
            return Result;

        }
    }

    public class Datum
    {
        public string @object { get; set; }
        public int index { get; set; }
        public List<double> embedding { get; set; }
    }

    public class EmbeddingResult
    {
        public string @object { get; set; }
        public List<Datum> data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}
