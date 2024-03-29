using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RAG.OpenAI
{
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

        public async Task<ChatResponse> Chat(ChatRequest cr)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Bearer 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");
            request.Headers.Add("Cookie", "__cf_bm=YBscyoX0CDVPiKUubXMM9Y5ntwYm0iNZj8XtfqAuuNQ-1711129536-1.0.1.1-NR.rh7geDNIxATDkG81BOqmgT5HPrt7neqw1VzCruGC33l3aPYsMHWkuD31yqRJSDqv9RvFWltHEmQey_dN4cw; _cfuvid=mM.NfIqjutvFJHLmJivLE255VI9vr9ra15B8.nHFi7I-1710962548377-0.0.1.1-604800000");
            //var content = new StringContent("{\n    \"model\": \"gpt-3.5-turbo\",\n    \"messages\": [\n        {\n            \"role\": \"user\",\n            \"content\": \"Answer the following question: Who can help me get stronger? using only the following data: Popeye's Muscle Supplements|Founded by the spinach-fueled Popeye himself, Popeye's Muscle Supplements is a rising star in the health and wellness industry. Despite its modest size, this company has made waves with its range of all-natural supplements designed to boost strength and endurance. With endorsements from athletes and fitness enthusiasts alike, Popeye's Muscle Supplements is gaining traction as a trusted name in the competitive world of nutritional supplements.\"\n        }\n    ],\n    \"temperature\": 0,\n    \"top_p\": 1,\n    \"n\": 1,\n    \"stream\": false,\n    \"max_tokens\": 250,\n    \"presence_penalty\": 0,\n    \"frequency_penalty\": 0\n}", null, "application/json");
            var content = new StringContent(JsonConvert.SerializeObject(cr), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
            ChatResponse chatResponse = JsonConvert.DeserializeObject<ChatResponse>(await response.Content.ReadAsStringAsync());
            return chatResponse;
        }

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

    // -- CHAT REQUEST

    public class ChatRequestMessage
    {
        public string role { get; set; } = "user";
        public string content { get; set; }
    }

    public class ChatRequest
    {
        public string model { get; set; } = "gpt-3.5-turbo";
        public List<ChatRequestMessage> messages { get; set; } = new List<ChatRequestMessage>();
        public int temperature { get; set; } = 0;
        public int top_p { get; set; } = 1;
        public int n { get; set; } = 1;
        public bool stream { get; set; } = false;
        public int max_tokens { get; set; } = 250;
        public int presence_penalty { get; set; } = 0;
        public int frequency_penalty { get; set; } = 0;
    }

    // -- CHAT RESPONSE

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

    // -- EMBED

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
