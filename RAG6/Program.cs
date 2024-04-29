﻿// Main routine for RAG6 prototype. Essentially just keeps asking seeker if they
// have a question, determines the appropriate data source to answer the question,
// gets the answer, and then repeats until the seeker presses [Enter] without
// providing a question.
using RAG6;

string? question = "";
string prompt = "";
int qNum = 1;
string answer = "";
string sql = "";
string dataSource = "";

// Loop, prompting user to ask a question
while (true)
{
    answer = "";
    prompt = "";

    // Seeker asks a question
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n{qNum}. Please enter a question (blank to exit): ");
    Console.ForegroundColor = ConsoleColor.White;
    question = Console.ReadLine();
    Utilities.LogIt($"\n{qNum}. Please enter a question (blank to exit): {question}");

    // If blank entered, then exit program
    if (!string.IsNullOrEmpty(question))
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
                //Utilities.QueryVector(question);
                Utilities.CallGPT(question, null, true);
                break;
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


