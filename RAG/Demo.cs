using Newtonsoft.Json;
using RAG.DataStax;
using RAG.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RAG.Utilities
{
    public class Demo
    {
        public static async Task QuestionAndAnswerAsync(string collectionName)
        {
            // DataStax is used for the vector store, so need to get access
            // to its API and define a variable to hold the query response
            DataStax.API dsAPI = new DataStax.API();
            OpenAI.EmbeddingResult embeddingResponse = new OpenAI.EmbeddingResult();

            // OpenAI is used to generate the natural language responses to 
            // the user's questions, so need to get access to the API
            OpenAI.API openAiAPI = new OpenAI.API();
            
            // Let user keep asking more questions until they press [Enter]
            // without providing a question, then quit.
            string question = "";
            while (true)
            {
                // Get question from user, if blank, then exit
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nEnter a question (blank to exit): ");
                Console.ForegroundColor = ConsoleColor.White;
                question = Console.ReadLine();
                if (string.IsNullOrEmpty(question)) break;

                // Get vector for the question we want to ask
                embeddingResponse = await openAiAPI.EmbedAsync(question);
                string json = JsonConvert.SerializeObject(embeddingResponse.data[0].embedding);

                FindRequest fr = new FindRequest();
                fr.find.sort.vector = embeddingResponse.data[0].embedding;
                FindResult fres = await dsAPI.FindAsync(collectionName, fr);

                // Get the best matching document type based on it's ID
                string profileText = File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{fres.data.documents[0].docType}\\" + fres.data.documents[0]._id.Split('.').Last() + ".txt");
                Console.WriteLine($"doctype: {profileText}");

                // Use the previous query to lookup the data in the correct collection
                string docType = profileText.Split('|').First().ToLower();
                fr = new FindRequest();
                fr.find.sort.vector = embeddingResponse.data[0].embedding;
                fres = await dsAPI.FindAsync(docType, fr);
                profileText = File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{docType}\\" + fres.data.documents[0]._id.Split('.').Last() + ".txt");
                Console.WriteLine($"{docType}: {profileText}");

                // Get the ChatGPT response using this customer profile
                ChatRequest cr = new OpenAI.ChatRequest();
                cr.messages.Add(new ChatRequestMessage());
                cr.messages[0].content = $"Answer the following question: \"{question}\" using only the following data: \"{profileText}\"";
                string answer = openAiAPI.Chat(cr).Result.choices[0].message.content;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nAnswer: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(answer);
            }

        }

        public static async Task LoaderAsync(string collectionName, string docType)
        {
            OpenAI.API openAiAPI = new OpenAI.API();
            OpenAI.EmbeddingResult er = new OpenAI.EmbeddingResult();
            DataStax.API dsAPI = new DataStax.API();
            DataStax.Documents documents = new DataStax.Documents();

            // Get files containing customer profiles
            string[] profiles = Directory.GetFiles($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{docType}\\");

            bool collectionExists = false;

            // Loop through files
            foreach (string profile in profiles)
            {
                Console.WriteLine(profile);
                string id = profile.Split('\\').Last().Split('.')[0];
                string contents = File.ReadAllText(profile);
                string name = contents.Split('|').First();
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
                        Thread.Sleep(60000*(i+1));
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{ex.Message}, try again in {Convert.ToInt32(60*(i+1))} seconds");
                        Console.ForegroundColor = ConsoleColor.White;
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

                DataStax.InsertOne document = new DataStax.InsertOne();
                document.vector = er.data[0].embedding;
                document.name = name;
                document.docType = docType;
                document._id = $"{docType}.{id}";
                documents.insertMany.documents.Add(document);

            }

            // Add the embedding to the collection
            await dsAPI.WriteAsync(collectionName, documents);


        }
    }
}
