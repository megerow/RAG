// -------------------------------------------------------------------------------
// Purpose: High-level tilities to use when working with any of the services 
//          or data sources.
//
// By:      Mark Gerow
// Date:    4/29/2024
// --------------------------------------------------------------------------------

using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using RAG.DataStax;
using RAG.OpenAI;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics.SymbolStore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RAG6
{
    public class Utilities
    {
        public static string logFilePath = $"c:\\temp\\RAG-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}.txt";

        /// <summary>
        /// OBSOLETE: Add an intent to the DocType collection in the vector database.
        /// </summary>
        /// <param name="docType">Doc type to assign</param>
        /// <param name="embedding">Embedding</param>
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

        /// <summary>
        /// Call ChatGPT API to get answer to question.
        /// </summary>
        /// <param name="prompt">The formatted question (aka prompt), including any RAG data to send to ChatGPT</param>
        /// <param name="profileText">If provided, display the vector profile used to answer the question</param>
        /// <param name="displayAnswer">If TRUE, display the answer in this method</param>
        /// <param name="displayPrompt">If TRUE, display the prompt passed into the method</param>
        /// <returns></returns>
        public static string CallGPT(string prompt, string? profileText = null, bool displayAnswer = false, bool displayPrompt = false)
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

            if (displayPrompt) DisplayMessage($"PROMPT: {prompt}\"");

            ChatRequest cr = new OpenAI.ChatRequest();
            cr.messages.Add(new ChatRequestMessage());
            cr.messages[0].content = $"{prompt}";
            string answer = openAiAPI.Chat(cr).Result.choices[0].message.content;

            if (displayAnswer)
            {
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.Write("\nAnswer: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n{answer}");

                if (!string.IsNullOrEmpty(profileText))
                {
                    DisplayMessage(profileText);
                }
            }

            return answer;
        }

        /// <summary>
        /// Delete an item (row) from a vector collection.
        /// </summary>
        /// <param name="collectionName">Name of collection containing the item to be deleted</param>
        /// <param name="id">The unique ID of the item to be deleted</param>
        public static async void DeleteItem(string collectionName, string id)
        {
            DataStax.API dsAPI = new DataStax.API();
            await dsAPI.DeleteItemAsync(collectionName, id);
        }

        /// <summary>
        /// Helper method to display text in the console window.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="color">Color to use</param>
        public static void DisplayMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write($"\n{message}\n");
            Console.ForegroundColor = ConsoleColor.White;
            LogIt($"\n{message}\n");
        }

        /// <summary>
        /// Call the DataStax API to find the best match(es) for the provided embedding.
        /// </summary>
        /// <param name="collectionName">Name of collection (aka table) to search in the vector database</param>
        /// <param name="embeddingResult">The embedding to look for</param>
        /// <param name="rowsToReturn">Number of rows to return</param>
        /// <returns>Result object containing best matches from the vector database</returns>
        public static FindResult Find(string collectionName, EmbeddingResult embeddingResult, int rowsToReturn)
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

        /// <summary>
        /// Clean up SQL statement to use "LIKE" rather than "=" for certain columns.
        /// </summary>
        /// <param name="sql">Source SQL SELECT statement to clean up.</param>
        /// <returns>Reformatted SQL SELECT statement</returns>
        public static string FixUp(string sql)
        {
            sql = sql.Replace("```", "");
            if (!sql.ToLower().StartsWith("select"))
            {
                sql = sql.Substring(sql.ToLower().IndexOf("select"));
            }

            string[] sqlSegments = sql.Split('=');
            for (int i = 0; i < sqlSegments.Length; i++)
            {
                if (sqlSegments[i].ToLower().EndsWith("salesrep ")
                    || sqlSegments[i].ToLower().EndsWith("customer ")
                    || sqlSegments[i].ToLower().EndsWith("product "))
                {
                    int p0 = GetNthIndex(sqlSegments[i + 1], '\'', 1);
                    int p1 = GetNthIndex(sqlSegments[i + 1], '\'', 2);
                    sqlSegments[i + 1] = sqlSegments[i + 1].Substring(0, p0 + 1)
                        + "%"
                        + sqlSegments[i + 1].Substring(p0 + 1, p1 - p0 - 1)
                        + "%"
                        + sqlSegments[i + 1].Substring(p1);
                }
            }
            sql = "";
            for (int i = 0; i < sqlSegments.Length - 1; i++)
            {
                if (sqlSegments[i].ToLower().EndsWith("salesrep ")
                    || sqlSegments[i].ToLower().EndsWith("customer ")
                    || sqlSegments[i].ToLower().EndsWith("product "))
                {
                    sql += sqlSegments[i] + "LIKE";
                }
                else
                {
                    sql += sqlSegments[i] + "=";
                }
            }
            sql += sqlSegments.Last();

            return sql;
        }

        /// <summary>
        /// Create 1,000 random orders in the vector database by randomly combining customers, products, and sales reps.
        /// </summary>
        public static void GenerateOrderData()
        {
            // Will use this to generate random index into lists to create rows 
            // to add to the ORDERS table.
            Random rand = new Random();

            List<Customer> customers = GetCustomers();
            List<Product> products = GetProducts();
            List<SalesRep> salesReps = GetSalesReps();

            using (SqlConnection con = new SqlConnection("Server=tcp:megerow.database.windows.net,1433;Initial Catalog=RAG;Persist Security Info=False;User ID=megerow;Password=Zippy2024;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                con.Open();
                for (int i = 0; i < 1000; i++)
                {
                    Customer customer = customers[rand.Next(0, customers.Count)];
                    Product product = products[rand.Next(0, products.Count)];
                    SalesRep salesRep = salesReps[rand.Next(0, salesReps.Count)];
                    int qty = rand.Next(1, 10);
                    int daysBack = rand.Next(1, 365);
                    string sql = $"INSERT INTO ORDERS (Date,Quantity,Product,Price,Customer,SalesRep) values ('{DateTime.Now.AddDays(-daysBack).Date}',{qty},'{product.Name}',{product.Price},'{customer.Name.Replace("'", "''")}','{salesRep.Name}');";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(i);
                }
            }
        }

        /// <summary>
        /// Provide a list of customer names used to generate random orders.
        /// </summary>
        /// <returns>Return a list of customers</returns>
        public static List<Customer> GetCustomers()
        {
            List<Customer> customers = new List<Customer>();
            customers.Add(new Customer() { Name = "Scooby Snack Solutions" });
            customers.Add(new Customer() { Name = "Wile E. Coyote Engineering" });
            customers.Add(new Customer() { Name = "Daffy Duck Financial Consultants" });
            customers.Add(new Customer() { Name = "Popeye's Muscle Supplement" });
            customers.Add(new Customer() { Name = "Taz's Tornado Tours" });
            customers.Add(new Customer() { Name = "Mickey & Minnie's Magical Events" });
            customers.Add(new Customer() { Name = "SpongeBob's Seafood Shack" });
            customers.Add(new Customer() { Name = "Dexter's Lab Innovations" });
            customers.Add(new Customer() { Name = "The Flintstones Stone-Age Construction" });
            customers.Add(new Customer() { Name = "Garfield's Gourmet Catering" });
            return customers;
        }

        /// <summary>
        /// Call ChatGPT API to determine the type of question being asked and the data source that
        /// should be used to answer it.
        /// </summary>
        /// <param name="question">Seeker's question</param>
        /// <returns>Name of data source to use</returns>
        public static string GetDataSource(string question)
        {
            OpenAI.API openAiAPI = new OpenAI.API();
            string prompt = $"You are an AI chatbot with access to the following data sources:\r\n\r\n1.  VECTOR: contains documents that profile customers, products, and sales reps and can be used to answer non-numerical questions about customers, products and sales reps.\r\n2. SQL: contains detailed data on orders placed by customers for products and which sales reps were involved and should be used when counting or summing data. WEATHER: provides detailed data for questions related to the weather or temperature. STOCK: provides data about stock prices or market conditions. \r\n\r\nWhich data source would you use to answer the following question: {question}. Only provide the one-word name of the data source. If not of these sources seem to fit, reply \"OTHER\".\r\n\r\n";
            string answer = CallGPT(prompt);
            return answer;
        }

        /// <summary>
        /// Get embedding for a given string so that it can be used for vector search.
        /// </summary>
        /// <param name="text">Text to get embedding for</param>
        /// <returns>Embedding</returns>
        public static EmbeddingResult GetEmbedding(string text)
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

        /// <summary>
        /// Provide a list of product names and prices used to generate random orders.
        /// </summary>
        /// <returns>Return a list of products</returns>
        public static List<Product> GetProducts()
        {
            List<Product> products = new List<Product>();
            products.Add(new Product() { Name = "AI Data Analytics Suit", Price = 2999 });
            products.Add(new Product() { Name = "AI Chatbot Platform", Price = 1499 });
            products.Add(new Product() { Name = "AI Image Recognition Software", Price = 3499 });
            products.Add(new Product() { Name = "AI Virtual Assistant", Price = 1999 });
            products.Add(new Product() { Name = "AI Speech Recognition System", Price = 2299 });
            products.Add(new Product() { Name = "AI Predictive Maintenance Software", Price = 3999 });
            products.Add(new Product() { Name = "AI Marketing Automation Platform", Price = 2799 });
            products.Add(new Product() { Name = "AI Fraud Detection System", Price = 4299 });
            products.Add(new Product() { Name = "AI Recommendation Engine", Price = 2499 });
            products.Add(new Product() { Name = "AI Sentiment Analysis Tool", Price = 2199 });
            return products;
        }

        /// <summary>
        /// Helper method to return the position of the nth occurance of a character
        /// withing a string.
        /// </summary>
        /// <param name="s">String to search</param>
        /// <param name="t">Character to find</param>
        /// <param name="n">The instance of the character for which to return the position</param>
        /// <returns>Position of the nth occurence of the specified character within the provided string</returns>
        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Get list of sales rep names used to generate random orders.
        /// </summary>
        /// <returns>List of sales reps</returns>
        public static List<SalesRep> GetSalesReps()
        {
            List<SalesRep> salesReps = new List<SalesRep>();
            salesReps.Add(new SalesRep() { Name = "Lance Rivers" });
            salesReps.Add(new SalesRep() { Name = "Natalie Cruz" });
            salesReps.Add(new SalesRep() { Name = "Max Stone" });
            salesReps.Add(new SalesRep() { Name = "Samantha Fox" });
            salesReps.Add(new SalesRep() { Name = "Ryan Cooper" });
            salesReps.Add(new SalesRep() { Name = "Emily Hayes" });
            salesReps.Add(new SalesRep() { Name = "Tyler Morgan" });
            salesReps.Add(new SalesRep() { Name = "Olivia Lane" });
            salesReps.Add(new SalesRep() { Name = "Cameron Brooks" });
            salesReps.Add(new SalesRep() { Name = "Ella Patel" });
            return salesReps;
        }

        /// <summary>
        /// Prompt user for yes/no response and return TRUE if yes, otherwise FALSE.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <returns>TRUE if user answered yes to prompt</returns>
        public static bool GetYesNo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n{message}: ");
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadKey().KeyChar.ToString().ToLower() != "n";
        }

        /// <summary>
        /// Write text to the log file.
        /// </summary>
        /// <param name="s">Text to write</param>
        public static void LogIt(string s)
        {
            File.AppendAllText(logFilePath, s);
        }

        /// <summary>
        /// Process a question that uses data from the SQL database
        /// </summary>
        /// <param name="question">Question asked by seeker</param>
        public static void QuerySQL(string question)
        {
            // Connect to the Azure RAG database
            //string? question = "";
            string prompt = "";
            string answer = "";
            string sql = "";

            try
            {
                // #1: Get the SQL to query the database

                prompt = $"Be sure to use correct Microsoft SQL Server syntax. You are a Microsoft SQL Server database developer.You have a Microsoft SQL Server database view named vwORDERS with the following structure:Date datetime, Quantity int, Product varchar(50), Price money, Customer varchar(50), SalesRep varchar(50), Amount money. Provide just the valid Microsoft SQL Server TSQL SELECT statement to answer the following question: {question}. For units sold use whole integers with commas between thousands. For amount sold use currency format.";

                // Call GPT to get the answer
                sql = CallGPT(prompt);

                // In case GPT added ``` characters to format SQL as code, remove them
                sql = FixUp(sql);

                //DisplayMessage($"{sql}", ConsoleColor.Gray);

                // #2: Query the database and construct the prompt
                prompt = $"You are an AI chatbot. Answer the question: \"{question}\" using the following data: ";
                using (SqlConnection con = new SqlConnection("Server=tcp:megerow.database.windows.net,1433;Initial Catalog=RAG;Persist Security Info=False;User ID=megerow;Password=Zippy2024;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    DataTable dataTable = new DataTable();
                    da.Fill(dataTable);

                    // If only one column and row returned, change its name to "Answer" to 
                    // help GPT understand what to do.
                    if (dataTable.Columns.Count == 1 && dataTable.Rows.Count == 1)
                    {
                        dataTable.Columns[0].ColumnName = "Answer";
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        prompt += "\n";
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            prompt += $"{col.ColumnName}: {row[col.ColumnName]}";
                            if (col.ColumnName != dataTable.Columns[dataTable.Columns.Count - 1].ColumnName) prompt += ", ";
                        }
                    }
                }

                answer = CallGPT(prompt, null, false, false);

                // Display the answer
                DisplayMessage($"{answer}", ConsoleColor.Cyan);

            }
            catch (Exception ex)
            {
                DisplayMessage($"Oops! An error occurred. {ex.Message}", ConsoleColor.Cyan);
            }

        }

        /// <summary>
        /// Process a question that uses data from the FinnHub stock API
        /// </summary>
        /// <param name="question">Question asked by seeker</param>
        public static void QueryStock(string question)
        {
            try
            {
                // Make sure the list of stock symbols is in memory, and if not load it.
                if (symbols == null)
                {
                    string json = File.ReadAllText("C:\\Users\\meger\\source\\repos\\RAG\\RAG6\\Symbol.json");
                    symbols = JsonConvert.DeserializeObject<List<Symbol>>(json);
                }

                // 2. Extract the name or symble from the question.
                string prompt = $"Extract the company name in the following question: {question}. Answer should just be the company name and its stock ticker symbol with no other text. The format should be: Company:Symbol. If you cannot determine the ticker symbol answer \"Unknown\".";

                // #2: Call GPT to get the answer
                string company = CallGPT(prompt);

                // 3. Search the symbols list for the name or symbol to use.
                string symbol = company.Split(':')[1];

                // 4. Call the FinnHub API to get it's current information
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://finnhub.io/api/v1/quote?symbol={symbol}&token=conuvo9r01qm6hd181fgconuvo9r01qm6hd181g0");
                var response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                Quote quote = JsonConvert.DeserializeObject<Quote>(response.Content.ReadAsStringAsync().Result);

                // 5. Call ChatGPT to answer the question
                prompt = $"Answer the question: {question}, using only the following data: {JsonConvert.SerializeObject(quote)}. Display amounts in dollar format.";

                string answer = CallGPT(prompt);

                DisplayMessage(answer, ConsoleColor.Cyan);

            }
            catch (Exception ex)
            {
                DisplayMessage($"Oops! An error occurred. {ex.Message}", ConsoleColor.Cyan);
            }
        }

        /// <summary>
        /// Process a question that uses data from the vector database.
        /// </summary>
        /// <param name="question">Question asked by seeker</param>
        public static void QueryVector(string question)
        {
            double MIN_DATA_CONFIDENCE = .85;

            // "Embed" question, creating a vector
            var questionEmbedding = Utilities.GetEmbedding(question);

            // Search for best match in vector database containing documents for
            // customers, products, and salesreps
            string docType = "all";
            int itemsToReturn = 10;
            double dataSimilarity = 0;
            var findResult = Utilities.Find(docType, questionEmbedding, itemsToReturn);
            dataSimilarity = findResult.data.documents[0].similarity;
            string prompt = "";

            // If no match, or confidence level too low, then ask GPT to search its public knowledgebase
            if (findResult == null || findResult.data.documents[0].similarity < MIN_DATA_CONFIDENCE)
            {
                // In this case, prompt will just be the original question
                prompt = $"You are an AI-based chatbot. You work for Acme AI. Answer the following question: \"{question}\"";

                // Call GPT to get the answer
                var answer = CallGPT(prompt);

                // Display the answer
                DisplayMessage($"{answer}", ConsoleColor.Cyan);
            }
            else
            {
                // If confidence level was > than minimum, then construct prompt using seeker's question and found data
                string profileText = "";
                foreach (var profile in findResult.data.documents)
                {
                    if (profile.similarity >= MIN_DATA_CONFIDENCE)
                    {
                        profileText += $"Type: {profile.docType}, Name: {profile.name}, Profile: " + File.ReadAllText($"C:\\Users\\meger\\source\\repos\\RAG\\RAG\\Documents\\{profile.docType}\\" + profile._id.Split('.').Last() + ".txt") + "\n\n";
                    }
                }

                prompt = $"You are an AI-based chatbot. You work for Acme AI. \n\nAnswer the following question: \"{question}\". \n\nUsing the following data: \"{profileText}\"";

                // Call GPT to get the answer
                var answer = CallGPT(prompt);

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
                        answer = CallGPT(question);

                        break;
                    }
                }

                // Display the answer
                DisplayMessage($"{answer}", ConsoleColor.Cyan);

            }
        }

        /// <summary>
        /// Process a question that uses data from the weather service
        /// </summary>
        /// <param name="question">Question asked by the seeker</param>
        public static void QueryWeather(string question)
        {
            string prompt = "";
            string answer = "";
            string loc = "";

            try
            {
                // #1: Get the city and state specified by the end user

                prompt = $"Extract the city and state in the united states for the following question: {question}. Answer should be in format: City;State. The state should be in 2-character form. If you can determine the city, but not the state, use \"CA\" for the state. If you cannot determine neither the city or state answer \"Unknown\".";

                // #2: Call GPT to get the answer
                loc = CallGPT(prompt);
                string city = loc.Split(';').First();
                string state = loc.Split(';').Last();
                string country = "usa";

                //DisplayMessage($"{loc}", ConsoleColor.Gray);

                // #3: Now use the OpenWeatherMaps API to get the lat/long for the city and state
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://api.openweathermap.org/geo/1.0/direct?q={city},{state},{country}&appid=d87f3cd5eec403ef0737c3b4131709a9");
                var response = client.Send(request);
                response.EnsureSuccessStatusCode();
                Geo[] geo = JsonConvert.DeserializeObject<Geo[]>(response.Content.ReadAsStringAsync().Result);

                //DisplayMessage($"Latitude: {geo.First().lat}, Longitude: {geo.First().lon}", ConsoleColor.Gray);

                // #4: Use the latitude/longitude to get the current weather
                client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Get, $"https://api.openweathermap.org/data/2.5/weather?lat={geo.First().lat}&lon={geo.First().lon}&appid=d87f3cd5eec403ef0737c3b4131709a9&units=imperial");
                response = client.Send(request);
                response.EnsureSuccessStatusCode();
                string json = response.Content.ReadAsStringAsync().Result;
                WeatherRoot weather = JsonConvert.DeserializeObject<WeatherRoot>(json);

                string data = $"The weather in {city}, {state} is: {weather.weather.First().description}, Temperature: {weather.main.feels_like} degrees Fahrenheit, Wind speed: {weather.wind.speed} MPH";

                //DisplayMessage(data, ConsoleColor.Gray);

                // #5: Call ChatGPT with the data to answer the question.
                prompt = $"Answer the question: {question}, using only the following data: {data}.";

                answer = CallGPT(prompt);

                DisplayMessage(answer, ConsoleColor.Cyan);

                return;

            }
            catch (Exception ex)
            {
                DisplayMessage($"Oops! An error occurred. {ex.Message}", ConsoleColor.Cyan);
            }
        }

        /// <summary>
        /// A list of stock ticker symbols.
        /// </summary>
        public static List<Symbol> symbols { get; set; } = null;

    }
}
