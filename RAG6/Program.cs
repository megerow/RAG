// See https://aka.ms/new-console-template for more information
using RAG6;

//// Unix timestamp
//long unixTimestamp = 1713582000;

//// Convert Unix timestamp to DateTimeOffset
//DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

//// Convert DateTimeOffset to DateTime
//DateTime dateTime = dateTimeOffset.DateTime;

//// Display the DateTime value
//Console.WriteLine("Unix Timestamp: " + unixTimestamp);
//Console.WriteLine("DateTime: " + dateTime.ToString());

string? question = "";
string prompt = "";
int qNum = 1;
string answer = "";
string sql = "";
string dataSource = "";

//Utilities.QueryWeather("what is the weather in Menlo Park?");

//Console.WriteLine();
//Console.WriteLine("Press any key to continue...");
//Console.Read();

//return;

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


