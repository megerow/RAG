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
        public Embedder(string input)
        {
            this.input = input;
        }

        public Embedder() { }

        public string embeddingsUrl { get; } = "https://api.openai.com/v1/embeddings";
        public string model { get; } = "text-embedding-ada-002";
        public string input { get; set; } = "";
        private string _toString { get; set; } = "";
        
        public string ToString() { return _toString; }

        public EmbeddingResult Result { get; set; }

        public async Task<EmbeddingResult> EmbedAsync(string text) 
        {
            // Define HTTP client to call OpenAI embeddings API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, embeddingsUrl);
            request.Headers.Add("Authorization", "Bearer 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");

            // Set the model and the contents
            var content = new StringContent("{\"model\": \"" + model + "\",\"input\": \"" + text + "\"}", null, "application/json");
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
