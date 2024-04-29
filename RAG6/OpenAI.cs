using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RAG.OpenAI
{
    /// <summary>
    /// Methods in this class are used to work with the OpenAI API.
    /// </summary>
    public class API
    {
        // If empty constructor is used then the input text
        // must be set separately before the EmbedAsync method is called
        public API() { }

        // Prefill the input text
        public API(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Url to the OpenAI embedding service
        /// </summary>
        public string embeddingsUrl { get; } = "https://api.openai.com/v1/embeddings";

        /// <summary>
        /// The embedding model to use
        /// </summary>
        public string model { get; } = "text-embedding-ada-002";

        /// <summary>
        /// Text to convert to an embedding
        /// </summary>
        public string text { get; set; } = "";

        /// <summary>
        /// Holds JSON string returned from API
        /// </summary>
        private string _toString { get; set; } = "";
        
        /// <summary>
        /// Override the native ToString method to return JSON of response
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return _toString; }

        /// <summary>
        /// Object to hold the response from OpenAI API
        /// </summary>
        public EmbeddingResult Result { get; set; }

        /// <summary>
        /// Call ChatGPT and get the response.
        /// </summary>
        /// <param name="cr">Structure containing the chat request</param>
        /// <returns>Structure containing the chat response</returns>
        public async Task<ChatResponse> Chat(ChatRequest cr)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Bearer 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");
             var content = new StringContent(JsonConvert.SerializeObject(cr), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            ChatResponse chatResponse = JsonConvert.DeserializeObject<ChatResponse>(await response.Content.ReadAsStringAsync());
            return chatResponse;
        }

        /// <summary>
        /// If called without parameters, then just use the text property.
        /// </summary>
        /// <returns>A structure containing the vectorized text</returns>
        public async Task<EmbeddingResult> EmbedAsync()
        {
            return await EmbedAsync(text);
        }

        /// <summary>
        /// Otherwise use the provided text.
        /// </summary>
        /// <param name="_text">Text to convert to a vector representation</param>
        /// <returns>A structure containing the vectorized text</returns>
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

    // Classes used by OpenAI API chat request.

    public class ChatRequestMessage
    {
        public string role { get; set; } = "user";
        public string content { get; set; }
    }

    public class ChatRequest
    {
        public string model { get; set; } = "gpt-3.5-turbo";
        public List<ChatRequestMessage> messages { get; set; } = new List<ChatRequestMessage>();
        public double temperature { get; set; } = 0.0;
        public int top_p { get; set; } = 1;
        public int n { get; set; } = 1;
        public bool stream { get; set; } = false;
        public int max_tokens { get; set; } = 1000;
        public int presence_penalty { get; set; } = 0;
        public int frequency_penalty { get; set; } = 0;
    }

    // Classes used by OpenAI API chat response.

    public class Choice
    {
        public int index { get; set; }
        public ChatResponseMessage message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class ChatResponseMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ChatResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public ChatUsage usage { get; set; }
        public string system_fingerprint { get; set; }
    }

    public class ChatUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    // Classes used by OpenAI API Embedding service.

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
        public EmbedUsage usage { get; set; }
    }

    public class EmbedUsage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}
