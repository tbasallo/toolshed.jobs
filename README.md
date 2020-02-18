# toolshed.jobs
This project is a simple logging library focused on the tracking of a job's start, updates and completion.

### Why Not...
The reason I rolled my own and did not use one of the many excellent options (Seri, log4net, etc.) is that I 
needed something that I could easily drop into Azure and quickly query. And this does just that. I wanted to track specifics for a "job" - 
automated type jobs that included some additional meta level tracking - started, completed, aborted, etc.
