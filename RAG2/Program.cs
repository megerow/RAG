using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using Newtonsoft.Json;
using RAG.DataStax;
using static System.Net.Mime.MediaTypeNames;
using RAG.OpenAI;
using RAG.Utilities;
using System.Runtime.CompilerServices;
using RAG2;

double MIN_DATASOURCE_CONFIDENCE = .9;
double MIN_DATA_CONFIDENCE = .9;

string question = "";
string prompt = "";
string docType = "";

// Loop, prompting user to ask a question
while (true)
{
    // 1. Seeker asks a question
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("\nEnter a question (blank to exit): ");
    Console.ForegroundColor = ConsoleColor.White;
    question = Console.ReadLine();

    // 1a. If blank entered, then exit program
    if (!string.IsNullOrEmpty(question))
    {
        // 2. Convert question to embedding
        var questionEmbedding = Utilities.GetEmbedding(question);

        // 3. Search vector database for most appropriate data source to answer the question 
        var findResult = Utilities.Find("doctype", questionEmbedding, 1);

        // 4. Confidence of found data source greater than minimum level?
        if (findResult.data == null || findResult.data.documents[0].similarity < MIN_DATASOURCE_CONFIDENCE)
        {
            // 4a. Ask seeker if their question relates to customers, products, sales reps, or none of these
            docType = Utilities.GetDataSource();

            // 4b. Question relates to customers, products, or sales reps?
            if (!string.IsNullOrEmpty(docType))
            {
                // 4c. If yes to question 4b, then add question to data source vector collection
                Utilities.AddIntent(docType, questionEmbedding);
            }

            // 4d. If answered none of these to 4b, then ask seeker if they want to search Internet
            else if (Utilities.GetYesNo("Would you like to search the Internet for your answer?"))
            {
                // 4e. If user wants to search Internet, then 9. Construct a GPT prompt to pass question to Internet
                prompt = question;

                // 10. Call GPT to get answer
                var answer = Utilities.CallGPT(prompt);

                // 11. Display the answer
                Utilities.DisplayMessage(answer);
            }
        }
        else
        {
            // 5. If yes to question 4, search for match in data source collection
            docType = findResult.data.documents[0].docType;
            findResult = Utilities.Find(docType, questionEmbedding, 1);

            // 6. Was confidence level from searching target data source greater than minimum level?
            if (findResult.data == null || findResult.data.documents[0].similarity < MIN_DATASOURCE_CONFIDENCE)
            {
                // 6a. If no, ask seeker if they want to search the Internet, 6b. If answer to question about searching Internet is "yes" 
                if (Utilities.GetYesNo("Would you like to search the Internet for your answer?"))
                {
                    // 8. Construct prompt using seeker's question and name of the data source
                    prompt = question;

                    // 10. Call GPT to get the answer
                    var answer = Utilities.CallGPT(prompt);

                    // 11. Display the answer
                    Utilities.DisplayMessage(answer);
                }
            }
            else
            {
                // 7. If confidence level was > than minimum, then construct prompt using seeker's question and found data
                string profileText = File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{docType}\\" + findResult.data.documents[0]._id.Split('.').Last() + ".txt");
                prompt = $"Answer the following question: \"{question}\" using only the following data: \"{profileText}\"";

                // 10. Call GPT to get the answer
                var answer = Utilities.CallGPT(prompt);

                // 11. Display the answer
                Utilities.DisplayMessage(answer);
            }
        }


    }
    else
    {
        // 1b. Exit program
        return;
    }
}
