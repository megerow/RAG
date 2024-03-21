using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Retrieval Augmented Generation (RAG) Prototype");

// Get a test embedding
OpenAI.Embedder e = new OpenAI.Embedder();
OpenAI.EmbeddingResult er = await e.EmbedAsync("**Scooby Snack Solutions**: This quirky company, founded by Scooby-Doo and the gang, specializes in providing innovative snacks and solutions for pet owners. With a team comprising experts in pet nutrition and food science, Scooby Snack Solutions has quickly become a household name in the pet industry. Despite its relatively small size, the company's dedication to quality and creativity has earned them a loyal customer base and positive reviews from pet owners worldwide.");

// Display the returned json
Console.WriteLine(e.ToString());

// Save the embedding, along with metadata to the Datastax vector database
DataStax.Writer writer = new DataStax.Writer();
await writer.CreateCollectionAsync("customer");
await writer.WriteAsync("customer", "1", "Scooby Snack Solutions", er.data[0].embedding);

// Query the Datastax vector database

// Create a prompt for OpenAI ChatGPT to respond to

// Call OpenAI ChatGPT API to get response

// Display the ChatGPT response