using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Retrieval Augmented Generation (RAG) Prototype");

// Get files containing customer profiles
string[] customerProfiles = Directory.GetFiles("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\");

string collectionName = "customer";
bool collectionExists = false;

// Writer object will save all the profiles to the vector store
OpenAI.Embedder e = new OpenAI.Embedder();
OpenAI.EmbeddingResult er = new OpenAI.EmbeddingResult();
DataStax.API dsAPI = new DataStax.API();
DataStax.Customers customers = new DataStax.Customers();

// Get vector for the question we want to ask
//er = await e.EmbedAsync("What does Minni do?");
//er = await e.EmbedAsync("Do any businesses operate in Bedrock?");
er = await e.EmbedAsync("Who can help me get stronger?");
string json = JsonConvert.SerializeObject(er.data[0].embedding);
Console.WriteLine("\nQuestion: Who can help me get stronger?");

FindRequest fr = new FindRequest();
fr.find.sort.vector = er.data[0].embedding;
FindResult fres = await dsAPI.FindAsync(fr);

Console.WriteLine($"\nResult: Company: {fres.data.documents[0].company}, Id: {fres.data.documents[0]._id}, Similarity: {fres.data.documents[0].similarity}");

// Get the document based on it's ID
string profileText = File.ReadAllText("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\" + fres.data.documents[0]._id + ".txt");
Console.WriteLine("\nProfile:\n"+profileText);

return;

// Loop through files
foreach (string profile in customerProfiles)
{
    Console.WriteLine(profile);
    string id = profile.Split('\\').Last().Split('.')[0];
    string contents = File.ReadAllText(profile);
    string company = contents.Split('|').First();
    string text = contents.Split('|').Last();

    // Get embedding for content
    for (int i = 0; i < 10; i++)
    {
        try
        {
            er = await e.EmbedAsync(text);
            break;
        }
        catch (Exception ex)
        {
            Thread.Sleep(10000);
        }
    }

    // Display the returned json
    Console.WriteLine(e.ToString());

    if (!collectionExists)
    {
        try
        {
            await dsAPI.CreateCollectionAsync(collectionName);
            collectionExists = true;
        }
        catch { }
        collectionExists = true;
    }

    DataStax.Document document = new DataStax.Document();
    document.vector = er.data[0].embedding;
    document.company = company;
    document._id = id;
    customers.insertMany.documents.Add(document);

}

// Add the embedding to the collection
await dsAPI.WriteAsync(collectionName, customers);

// Create a prompt for OpenAI ChatGPT to respond to

// Call OpenAI ChatGPT API to get response

// Display the ChatGPT response