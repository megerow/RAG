using RAG4;

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
        Utilities.DisplayMessage($"\nThe {dataSource} database will be used.\n");

        if (dataSource.ToLower() == "vector")
        {
            Utilities.QueryVector(question);
        }
        else
        {
            Utilities.QuerySQL(question);
        }
    }
    else
    {
        // User pressed [Enter] rather than entering another question,
        // so exit
        return;
    }
}
