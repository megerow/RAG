using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;
using RAG.OpenAI;
using RAG.Utilities;
using System.Runtime.CompilerServices;

// Welcome the user
Console.WriteLine("Retrieval Augmented Generation (RAG) Prototype");

// Give the collection containing vector representations
// of customer profiles a name.
string collectionName = "customer3";

// Load the customer profiles into the vector database.
//await Demo.LoaderAsync(collectionName);

// Let the user ask questions and have ChatGPT
// respond using  information found in our customer profiles.
await Demo.QuestionAndAnswerAsync(collectionName);

return;

