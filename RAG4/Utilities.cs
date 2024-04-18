using OpenAI = RAG.OpenAI;
using DataStax = RAG.DataStax;
using RAG.DataStax;
using RAG.OpenAI;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RAG4
{
    public class Utilities
    {
        public static string logFilePath = $"c:\\temp\\RAG-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}.txt";
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

        public class Customer
        {
            public string Name { get; set; }
        }

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

        public class Product
        {
            public string Name { get; set; }
            public double Price { get; set; }
        }

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

        public class SalesRep
        {
            public string Name { get; set; }
        }

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
                    sqlSegments[i+1] = sqlSegments[i+1].Substring(0, p0+1)
                        + "%"
                        + sqlSegments[i+1].Substring(p0+1, p1-p0-1)
                        + "%"
                        + sqlSegments[i+1].Substring(p1);
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

        public static void LogIt(string s)
        {
            File.AppendAllText(logFilePath, s);
        }

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

        public static void QuerySQL(string question)
        {
            // Connect to the Azure RAG database
            //string? question = "";
            string prompt = "";
            int qNum = 1;
            string answer = "";
            string sql = "";

            try
            {
                // #1: Get the SQL to query the database

                prompt = $"Be sure to use correct Microsoft SQL Server syntax. You are a Microsoft SQL Server database developer.You have a Microsoft SQL Server database view named vwORDERS with the following structure:Date datetime, Quantity int, Product varchar(50), Price money, Customer varchar(50), SalesRep varchar(50), Amount money. Provide just the valid Microsoft SQL Server TSQL SELECT statement to answer the following question: {question}.";

                // Call GPT to get the answer
                sql = CallGPT(prompt);

                // In case GPT added ``` characters to format SQL as code, remove them
                sql = FixUp(sql);

                DisplayMessage($"{sql}", ConsoleColor.Gray);

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
                            prompt += $"{col.ColumnName}: {row[col.ColumnName]},";
                        }
                    }
                }

                answer = CallGPT(prompt);

                // Display the answer
                DisplayMessage($"{answer}", ConsoleColor.Cyan);

            }
            catch (Exception ex)
            {
                DisplayMessage($"Oops! An error occurred. {ex.Message}", ConsoleColor.Cyan);
            }

        }

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

        public static string CallGPT(string prompt, string profileText = null, bool displayAnswer = false, bool displayPrompt = false)
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

        public static string GetDataSource(string question)
        {
            OpenAI.API openAiAPI = new OpenAI.API();
            string prompt = $"You are an AI chatbot with access to the following data sources:\r\n\r\n1.  VECTOR: contains documents that profile customers, products, and sales reps and can be used to answer non-numerical questions about customers, products and sales reps.\r\n2. SQL: contains detailed data on orders placed by customers for products and which sales reps were involved and should be used when counting or summing data.\r\n\r\nWhich data source would you use to answer the following question: {question}. Only provide the one-word name of the data source.\r\n\r\n";
            string answer = CallGPT(prompt);
            return answer;
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
            LogIt($"\n{message}\n");
        }

        //public static FindResult Find(string collectionName, EmbeddingResult embeddingResult, int rowsToReturn)
        //{
        //    try
        //    {
        //        DataStax.API dsAPI = new DataStax.API();
        //        FindRequest fr = new FindRequest();
        //        fr.find.sort.vector = embeddingResult.data[0].embedding;
        //        fr.find.options.limit = rowsToReturn;
        //        FindResult fres = dsAPI.FindAsync(collectionName, fr).Result;
        //        double similarity = fres.data.documents[0].similarity;
        //        return fres;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //public static EmbeddingResult GetEmbedding(string text)
        //{
        //    // Setup
        //    // ---------------------------------------------------------------
        //    // DataStax is used for the vector store, so need to get access
        //    // to its API and define a variable to hold the query response.
        //    DataStax.API dsAPI = new DataStax.API();
        //    OpenAI.EmbeddingResult embeddingResult = new OpenAI.EmbeddingResult();
        //    // ---------------------------------------------------------------
        //    // OpenAI is used to generate the natural language responses to 
        //    // the user's questions, so need to get access to the API.
        //    OpenAI.API openAiAPI = new OpenAI.API();
        //    // ---------------------------------------------------------------

        //    // Get vector for the question we want to ask
        //    embeddingResult = openAiAPI.EmbedAsync(text).Result;
        //    return embeddingResult;
        //}
    }
}
