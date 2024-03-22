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

                // Get the document based on it's ID
                string profileText = File.ReadAllText("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\" + fres.data.documents[0]._id + ".txt");

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

        public static async Task LoaderAsync(string collectionName)
        {
            OpenAI.API openAiAPI = new OpenAI.API();
            OpenAI.EmbeddingResult er = new OpenAI.EmbeddingResult();
            DataStax.API dsAPI = new DataStax.API();
            DataStax.Customers customers = new DataStax.Customers();

            // Get files containing customer profiles
            string[] customerProfiles = Directory.GetFiles("C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\Customers\\");

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


        }
    }
}
