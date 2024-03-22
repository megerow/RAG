using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;
using RAG.OpenAI;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Retrieval Augmented Generation (RAG) Prototype");

//string question = "What does Minni do?";
//string question = "Do any businesses operate in Bedrock?";
//string question = "Who can help me get stronger?";

// Writer object will save all the profiles to the vector store
OpenAI.API openAiAPI = new OpenAI.API();
OpenAI.EmbeddingResult er = new OpenAI.EmbeddingResult();
DataStax.API dsAPI = new DataStax.API();
DataStax.Customers customers = new DataStax.Customers();

string question = "";

while (true)
{
    // Get question from user, if blank, then exit
    Console.Write("\nEnter a question (blank to exit): ");
    question = Console.ReadLine();
    if (string.IsNullOrEmpty(question)) break;

    // Get vector for the question we want to ask
    er = await openAiAPI.EmbedAsync(question);
    string json = JsonConvert.SerializeObject(er.data[0].embedding);
    //Console.WriteLine("\nQuestion: " + question);

    FindRequest fr = new FindRequest();
    fr.find.sort.vector = er.data[0].embedding;
    FindResult fres = await dsAPI.FindAsync(fr);
    //Console.WriteLine($"\nResult: Company: {fres.data.documents[0].company}, Id: {fres.data.documents[0]._id}, Similarity: {fres.data.documents[0].similarity}");

    // Get the document based on it's ID
    string profileText = File.ReadAllText("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\" + fres.data.documents[0]._id + ".txt");
    //Console.WriteLine("\nProfile:\n" + profileText);

    // Get the ChatGPT response using this customer profile
    ChatRequest cr = new OpenAI.ChatRequest();
    cr.messages.Add(new ChatRequestMessage());
    cr.messages[0].content = $"Answer the following question: \"{question}\" using only the following data: \"{profileText}\"";
    string answer = openAiAPI.Chat(cr).Result.choices[0].message.content;
    Console.WriteLine("\nAnswer: " + answer);

}

return;

// Get files containing customer profiles
string[] customerProfiles = Directory.GetFiles("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\");

string collectionName = "customer";
bool collectionExists = false;

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
            er = await openAiAPI.EmbedAsync(text);
            break;
        }
        catch (Exception ex)
        {
            Thread.Sleep(10000);
        }
    }

    // Display the returned json
    Console.WriteLine(openAiAPI.ToString());

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