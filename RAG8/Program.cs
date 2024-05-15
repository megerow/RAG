// -------------------------------------------------------------------------------
// Purpose: A basic example of a RAG implementation using OpenAI ChatGPT and
//          supporting services.
//
// By:      Mark Gerow
// Date:    4/29/2024
// --------------------------------------------------------------------------------
// Main routine for RAG6 prototype. Essentially just keeps asking seeker if they
// have a question, determines the appropriate data source to answer the question,
// gets the answer, and then repeats until the seeker presses [Enter] without
// providing a question.
// --------------------------------------------------------------------------------
using RAG;
using RAG.OpenAI;
using System.Text.RegularExpressions;

string? question = "";
int qNum = 1;
string dataSource = "";
string matchStringPattern = @"^\d$";

// Loop, prompting user to ask a question
while (true)
{

    // Seeker asks a question
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n{qNum}. Please enter a question or type the # of a follow-up question to the preceding answer (blank to exit): ");
    Console.ForegroundColor = ConsoleColor.White;
    question = Console.ReadLine();
    Utilities.LogIt($"\n{qNum}. Please enter a question (blank to exit): {question}");

    // If blank entered, then exit program
    if (!string.IsNullOrEmpty(question))
    {
        if (Regex.IsMatch(question, matchStringPattern) && Utilities.followOnQuestionList.Count() != 0)
        {
            question = Utilities.followOnQuestionList[int.Parse(question)-1];

            // Display the question
            Utilities.DisplayMessage(question);
        }
        
        try
        {
            // Get the data source
            dataSource = Utilities.GetDataSource(question);

            // Display the answer
            Utilities.DisplayMessage($"The {dataSource} data source will be used.");

            switch (dataSource.ToUpper())
            {
                case "SQL":
                    Utilities.QuerySQL(question);
                    break;

                case "VECTOR":
                    Utilities.QueryVector(question);
                    break;

                case "WEATHER":
                    Utilities.QueryWeather(question);
                    break;

                case "STOCK":
                    Utilities.QueryStock(question);
                    break;

                default:
                    Utilities.CallGPT(prompt: question, profileText: null, displayAnswer: true, addToHistory: true, followOnQuestions: 3, autoAskFollowOns: false);
                    break;
            }
        }
        catch (Exception ex)
        {
            // If an exception occurs, just ask GPT
            Utilities.CallGPT(prompt: question, profileText: null, displayAnswer: true, addToHistory: true);
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


