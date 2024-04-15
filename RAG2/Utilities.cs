using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;
using RAG.OpenAI;
using RAG.Utilities;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace RAG4
{
    public class Utilities
    {
        public static async void AddIntent(string docType, EmbeddingResult embedding)
        {
            DataStax.InsertOne document = new DataStax.InsertOne();
            document.vector = embedding.data[0].embedding;
            document.name = docType;
            document.docType = "doctype";
            var r = new Random(DateTime.Now.Second);
            document._id = r.NextInt64(1000000, 10000000).ToString();
            DataStax.Documents documents = new DataStax.Documents();
            documents.insertMany.documents.Add(document);
            DataStax.API dsAPI = new DataStax.API();
            await dsAPI.WriteAsync("doctype", documents);
        }

        public static string CallGPT(string prompt, string profileText = null, bool displayAnswer = false)
        {
            // Setup
            // ---------------------------------------------------------------
            // DataStax is used for the vector store, so need to get access
            // to its API and define a variable to hold the query response.
            DataStax.API dsAPI = new DataStax.API();
            OpenAI.EmbeddingResult embeddingResponse = new OpenAI.EmbeddingResult();
            // ---------------------------------------------------------------
            // OpenAI is used to generate the natural language responses to 
            // the user's questions, so need to get access to the API.
            OpenAI.API openAiAPI = new OpenAI.API();
            // ---------------------------------------------------------------

            ChatRequest cr = new OpenAI.ChatRequest();
            cr.messages.Add(new ChatRequestMessage());
            cr.messages[0].content = $"{prompt}";
            string answer = openAiAPI.Chat(cr).Result.choices[0].message.content;

            if (displayAnswer)
            { 
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nAnswer: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(answer);

                if (!string.IsNullOrEmpty(profileText))
                {
                    DisplayMessage(profileText);
                }
            }

            return answer;
        }

        public static async void DeleteItem(string collectionName, string id)
        {
            DataStax.API dsAPI = new DataStax.API();
            await dsAPI.DeleteItemAsync(collectionName, id);
        }

        public static string GetDataSource()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nI was unable to determine which data source to use to answer your question. Is your question related to [C]ustomers, [P]roducts, or [S]ales Reps?  (blank for none of those): ");
            Console.ForegroundColor = ConsoleColor.White;
            switch (Console.ReadKey().KeyChar.ToString().ToLower())
            {
                case "c": return "customer";
                case "p": return "product";
                case "s": return "salesrep";
                default: return "";
            }
        }

        public static bool GetYesNo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n{message}: ");
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadKey().KeyChar.ToString().ToLower() != "n";
        }

        public static void DisplayMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write($"\n{message}\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static FindResult Find (string collectionName, EmbeddingResult embeddingResult, int rowsToReturn)
        {
            try
            {
                DataStax.API dsAPI = new DataStax.API();
                FindRequest fr = new FindRequest();
                fr.find.sort.vector = embeddingResult.data[0].embedding;
                fr.find.options.limit = rowsToReturn;
                FindResult fres = dsAPI.FindAsync(collectionName, fr).Result;
                double similarity = fres.data.documents[0].similarity;
                return fres;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static EmbeddingResult GetEmbedding (string text)
        {
            // Setup
            // ---------------------------------------------------------------
            // DataStax is used for the vector store, so need to get access
            // to its API and define a variable to hold the query response.
            DataStax.API dsAPI = new DataStax.API();
            OpenAI.EmbeddingResult embeddingResult = new OpenAI.EmbeddingResult();
            // ---------------------------------------------------------------
            // OpenAI is used to generate the natural language responses to 
            // the user's questions, so need to get access to the API.
            OpenAI.API openAiAPI = new OpenAI.API();
            // ---------------------------------------------------------------

            // Get vector for the question we want to ask
            embeddingResult = openAiAPI.EmbedAsync(text).Result;
            return embeddingResult;
        }
    }
}
