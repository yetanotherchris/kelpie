# kelpie
Scans NLog *text* log files and displays them in a filterable way in an MVC application, using MongoDB for storage.

## Getting started

- Make sure you have IIS installed
- Install MongoDB: https://www.mongodb.org/downloads
- Create `c:\mongodb\data\db`
- Run `setupMongoDb.ps1`
  - Or install it as a service: https://docs.mongodb.org/manual/tutorial/install-mongodb-on-windows/#configure-a-windows-service-for-mongodb
- Run the setup.ps1 script.
- Update the app.config and web.config files with the servers and applications you want.
- Import your logs using the Kelpie.ConsoleApp program.

## Roadmap

### Iteration 1
1. Reads all files from directories (using async)
2. Table of logs for today
3. Table of logs for this week
4. Show error messages in detail
5. Rescan all logs

### Iteration 1.1
1. Remove RavenDB and put MongoDB there.

### Iteration 2
1. Count of common exceptions for today and this week
2. Drill down into common exceptions

### Iteration 3
1. Search messages

### Iteration 4
1. Paging

### Iteration 5
1. Smart re-scanning, journaling the last scan time for each app and server.
