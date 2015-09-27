# kelpie
Scans NLog *text* log files and displays them in a filterable way in an MVC application, using RavenDB for storage.

## Roadmap

### Iteration 1
1. Reads all files from directories (using async)
2. Table of logs for today
3. Table of logs for this week
4. Show error messages in detail
5. Rescan all logs

### Iteration 2
1. Count of common exceptions for today and this week
2. Drill down into common exceptions

### Iteration 3
1. Search messages

### Iteration 4
1. Smart re-scanning, journaling the last scan time for each app and server.