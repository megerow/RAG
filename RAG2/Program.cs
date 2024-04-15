using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;
using RAG.OpenAI;
using RAG.Utilities;
using System.Runtime.CompilerServices;
using RAG4;

double MIN_DATA_CONFIDENCE = .85;

string? question = "";
string prompt = "";
string docType = "";
double dataSimilarity = 0;
int itemsToReturn = 10;
int qNum = 1;

// Loop, prompting user to ask a question
while (true)
{
    // Seeker asks a question
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n{qNum}. Please enter a question (blank to exit): ");
    Console.ForegroundColor = ConsoleColor.White;
    question = Console.ReadLine();

    // If blank entered, then exit program
    if (!string.IsNullOrEmpty(question))
    {

        // "Embed" question, creating a vector
        var questionEmbedding = Utilities.GetEmbedding(question);

        // Search for best match in vector database containing documents for
        // customers, products, and salesreps
        docType = "all";
        var findResult = Utilities.Find(docType, questionEmbedding, itemsToReturn);
        dataSimilarity = findResult.data.documents[0].similarity;

        // If no match, or confidence level too low, then ask GPT to search its public knowledgebase
        if (findResult == null || findResult.data.documents[0].similarity < MIN_DATA_CONFIDENCE)
        {
            // In this case, prompt will just be the original question
            prompt = $"You are an AI-based chatbot. You work for Acme AI. Answer the following question: \"{question}\"";

            // Call GPT to get the answer
            var answer = Utilities.CallGPT(prompt);

            // Display the answer
            Utilities.DisplayMessage($"{answer}", ConsoleColor.Cyan);
        }
        else
        {
            // If confidence level was > than minimum, then construct prompt using seeker's question and found data
            string profileText = "";
            foreach (var profile in findResult.data.documents)
            {
                if (profile.similarity >= MIN_DATA_CONFIDENCE)
                {
                    profileText += $"Type: {profile.docType}, Name: {profile.name}, Profile: " + File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{profile.docType}\\" + profile._id.Split('.').Last() + ".txt")+ "\n\n";
                }
            }

            prompt = $"You are an AI-based chatbot. You work for Acme AI. \n\nAnswer the following question: \"{question}\". \n\nUsing the following data: \"{profileText}\"";

            // Call GPT to get the answer
            var answer = Utilities.CallGPT(prompt);

            // If ChatGPT couldn't answer the question with the retrieved internal data,
            // try again with just the question.
            string[] keywords = new string[] { "unfortunately", "i'm sorry", "cannot" };
            foreach (var keyword in keywords)
            {
                // First answer contained one of the phrases that indicates answer
                // was not found.
                if (answer.ToLower().Contains(keyword))
                {
                    // Call GPT to get the answer
                    answer = Utilities.CallGPT(question);

                    break;
                }
            }

            // Display the answer
            Utilities.DisplayMessage($"{answer}", ConsoleColor.Cyan);

        }

        qNum++;

    }
    else
    {
        // User pressed [Enter] rather than entering another question,
        // so exit
        return;
    }
}
