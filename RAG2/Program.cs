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
double MIN_DATA_CONFIDENCE = .85;

string question = "";
string prompt = "";
string docType = "";
double docTypeSimilarity = 0;
string docTypeId = "";
double dataSimilarity = 0;
int itemsToReturn = 10;
int qNum = 1;

// Loop, prompting user to ask a question
while (true)
{
    // 1. Seeker asks a question
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n{qNum}. Please enter a question (blank to exit): ");
    Console.ForegroundColor = ConsoleColor.White;
    question = Console.ReadLine();

    // 1a. If blank entered, then exit program
    if (!string.IsNullOrEmpty(question))
    {
        // 2. Convert question to embedding
        var questionEmbedding = Utilities.GetEmbedding(question);

        // 3. Search vector database for most appropriate data source to answer the question 
        //var findResult = Utilities.Find("all", questionEmbedding, itemsToReturn);

        // 4. Confidence of found data source greater than minimum level?
        //if (findResult == null || findResult.data.documents[0].similarity < MIN_DATASOURCE_CONFIDENCE)
        //{
        //    // 4a. Ask seeker if their question relates to customers, products, sales reps, or none of these
        //    docType = Utilities.GetDataSource();

        //    // 4b. Question relates to customers, products, or sales reps?
        //    if (!string.IsNullOrEmpty(docType))
        //    {
        //        // 4c. If yes to question 4b, then add question to data source vector collection
        //        Utilities.AddIntent(docType, questionEmbedding);
        //    }

        //    // 4d. If answered none of these to 4b, then ask seeker if they want to search Internet
        //    else if (Utilities.GetYesNo("Ok, so we're agreed that the data does not exist in my knowledgebase. Would you like to search the Internet for your answer instead?"))
        //    {
        //        // 4e. If user wants to search Internet, then 9. Construct a GPT prompt to pass question to Internet
        //        prompt = question;

        //        // 10. Call GPT to get answer
        //        var answer = Utilities.CallGPT(prompt);

        //        // 11. Display the answer
        //        Utilities.DisplayMessage(answer);
        //    }
        //}
        //else
        //{
        // 5. If yes to question 4, search for match in data source collection
        //docTypeSimilarity = findResult.data.documents[0].similarity;
        //docTypeId = findResult.data.documents[0]._id;
        //docType = findResult.data.documents[0].name;
        docType = "all";
        var findResult = Utilities.Find(docType, questionEmbedding, itemsToReturn);
        dataSimilarity = findResult.data.documents[0].similarity;

        // 6. Was confidence level from searching target data source greater than minimum level?
        if (findResult == null || findResult.data.documents[0].similarity < MIN_DATA_CONFIDENCE)
        {
            // 6a. If no, ask seeker if they want to search the Internet, 6b. If answer to question about searching Internet is "yes" 
            //if (Utilities.GetYesNo($"Unfortunately, I could not find a good answer in my knowledgebase. Would you like to search the Internet for your answer instead?"))
            //{
                // 8. Construct prompt using seeker's question and name of the data source
                prompt = question;

                // 10. Call GPT to get the answer
                var answer = Utilities.CallGPT(prompt);

                // 11. Display the answer
                Utilities.DisplayMessage($"{answer}", ConsoleColor.Cyan);
            //}
        }
        else
        {
            // 7. If confidence level was > than minimum, then construct prompt using seeker's question and found data
            //string profileText = File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{docType}\\" + findResult.data.documents[0]._id.Split('.').Last() + ".txt");

            string profileText = "";
            foreach (var profile in findResult.data.documents)
            {
                //profileText += File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{docType}\\" + profile._id.Split('.').Last() + ".txt") + "|";
                if (profile.similarity >= MIN_DATA_CONFIDENCE)
                {
                    profileText += $"{profile.docType}: " + File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{profile.docType}\\" + profile._id.Split('.').Last() + ".txt") + "|";
                }
            }

            prompt = $"You are an AI-based chatbot. You work for Acme AI. Answer the following question: \"{question}\" using only the following data: \"{profileText}\"";

            // 10. Call GPT to get the answer
            var answer = Utilities.CallGPT(prompt, profileText);

            string[] keywords = new string[] { "unfortunately", "i'm sorry", "cannot" };

            foreach (var keyword in keywords)
            {
                if (answer.ToLower().Contains(keyword))
                {
                    // 10. Call GPT to get the answer
                    answer = Utilities.CallGPT(question);

                    //// 11. Display the answer
                    //Utilities.DisplayMessage($"{answer}", ConsoleColor.Cyan);

                    break;
                }
            }

            // 11. Display the answer
            //Utilities.DisplayMessage($"I found the answer in my {docType} database [{docTypeId}.{docTypeSimilarity}].  \n\nThe answer is: {answer} [{dataSimilarity}]. \n\nThe source data is: {profileText}");
            //Utilities.DisplayMessage($"I found the answer in my knowledgebase [{docTypeId}.{docTypeSimilarity}].  \n\nThe answer is: {answer} [{dataSimilarity}].");
            Utilities.DisplayMessage($"{answer}", ConsoleColor.Cyan);

            //// Display the source data
            //int cnt = 1;
            //foreach (var profile in findResult.data.documents)
            //{
            //    profileText = File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{profile.docType}\\" + profile._id.Split('.').Last() + ".txt") + "|";
            //    Utilities.DisplayMessage($"{cnt}. [{profile.similarity}] {profileText}\n");
            //    cnt++;
            //}

            // 12. Verify with seeker that this is a good answer
            //if (!Utilities.GetYesNo("Does this look correct? [Y/N]"))
            //    {
            //        // 13. Delete the intent entry that was used to find this answer

            //        Utilities.DeleteItem("doctype", docTypeId);
            //    }
            //}
        }

        qNum++;
        //Console.WriteLine("\n------");

    }
    else
    {
        // 1b. Exit program
        return;
    }
}
