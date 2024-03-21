using RAG.OpenAI;
using Newtonsoft.Json;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Retrieval Augmented Generation (RAG) Prototype");

// Get a test embedding
Embedder e = new Embedder();
EmbeddingResult er = await e.EmbedAsync("This is a test...");

// Display the returned json
Console.WriteLine(e.ToString());