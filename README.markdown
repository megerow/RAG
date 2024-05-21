The following flowchart depicts the logic used by the RAG8 project to process seeker questions and display answers using OpenAI GPT 3.5.  This project is for demonstration purposes only.

```mermaid
flowchart TD
    ng(["<span style='color: black'>1. Prompt seeker to<br> ask a question</span>"]) --> nz{"2. Did the seeker <br>ask a question?"}
    nz -- Yes, Seeker entered<br> a question --> nb{"3. Call GPT to classify<br>question by<br>data source"}
    nd[/"4. Call GPT to convert <br>question into a valid <br>SQL SELECT statement"/] -- SQL returned --> n3[("5. Query SQL <br>database for results")]
    n3 -- Data returned --> nf("6. Construct GPT prompt<br> using original question and <br>query results from database")
    nf --> nr[/"7. Call ChatGPT to get<br>the final answer<br> using prompt"/]
    nr --> nk[/"8. Display answer<br> from GPT"/]
    nk --> ng
    nz -- No, <br>Seeker pressed [Enter]<br> without a question --> na(["<span style='color: black'>14. Exit program</span>"])
    nb -- SQL<br>(Structured) --> nd
    nb -- Vector<br>(Unstructured) --> n8["9. Calculate question <br>embedding"]
    n8 --> nj["10. Search vector<br>database for<br>matching documents"]
    nj --> n6{"11. Matches found<br>and confidence<br>level &gt;= minimum?"}
    n6 -- Yes --> nq("12. Construct prompt<br>with found docs")
    n6 -- No --> ns("13. Prompt using <br>only question")
    nq --> nr
    ns --> nr
    nb -- Weather<br>related --> nc[/"15. Call GPT to extract<br>city and state<br>from question"/]
    nb -- Stock<br>related --> n5[/"19. Call GPT to extract<br>company name<br>and lookup ticker symbol"/]
    nc --> ni[/"16. Call OpenWeatherMap API <br>to get the latitude and<br> longitude of the specified<br> city and state"/]
    ni --> nl[/"17. Call OpenWeatherMap API<br>to get the current weather<br>for the specified<br>latitude and longitude"/]
    n5 --> no[/"20. Call the FinnHub API to<br>return the stock quote"/]
    nl --> n2("18. Construct prompt using data<br>from OpenWeatherMap<br>service")
    n2 --> nr
    no --> nu("21. Construct prompt using<br>data from FinnHub<br>stock quote service")
    nu --> nr
    style ng stroke:#00C853,fill:#C8E6C9
    style na stroke:#D50000,fill:#FFCDD2


```
