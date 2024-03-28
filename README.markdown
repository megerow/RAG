```mermaid
flowchart TD
    1(["1. Seeker asks <br>a question <span style="color: red; font-style: bold;">Q</span>"]) --> 2("2. Question <span style="color: red; font-style: bold;">Q</span> is <br>converted to embedding")
    2 --> 3[("3. Search vector database for <br>most appropriate data source: <span style="color: red; font-style: bold;">DS</span>, <br>to answer question: <span style="color: red; font-style: bold;">Q</span>")]
    3 --> 4{"4. Confidence of found<br> data source <span style="color: red; font-style: bold;">DS</span>, <br>greater than minimum <br>level"}
    4 -- Yes --> 4a[("5. Search data source  <span style="color: red; font-style: bold;">DS</span> in <br>vector database for data <span style="color: red; font-style: bold;">D</span>")]
    4 -- No --> 5a("9. Construct prompt using <br>seeker's question <span style="color: red; font-style: bold;">Q</span>")
    4a --> 4b{"6. Confidence of <br>found data  <span style="color: red; font-style: bold;">D</span><br>  greater than minimum <br>level"}
    4b -- Yes --> 4bi("7. Construct prompt using seeker's <br>question <span style="color: red; font-style: bold;">Q</span> and found data <span style="color: red; font-style: bold;">D</span>")
    4bi --> 4bii["10. Call ChatGPT API using prompt"]
    4bii --> 4biii("11. Display answer to seeker")
    4b -- No --> 4ci("8. Construct prompt using seeker's <br>question <span style="color: red; font-style: bold;">Q</span> and found data source <span style="color: red; font-style: bold;">DS</span>")
    4ci --> 4bii
    5a --> 4bii
    style 1 stroke:#00C853,fill:#C8E6C9
    style 4 fill:#FFF9C4
    style 4b stroke-width:1px,stroke-dasharray: 0,fill:#FFF9C4
    style 4biii stroke:#D50000,fill:#FFCDD2
```
