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

## Example config
Kelpie stores all its configuration in a JSON file, called kelpie.config. This should be in the webroot, or for the import tool in the bin folder.
An example config is below. Things to note:

- The "ConfigFile" property can point to another config file, if you want to commit in an empty config file.
- Set "ImportBufferSize" quite low - it's the number of log rows the import tool will store in memory until it saves to the MongoDB, however this is per thread. If you have lots of environments/servers you can get OutOfMemoryExceptions if this is set too high. For a single server/environment setup you can put this to about 100 however, speeding up the import.
- "MaxAgeDays" is the oldest age in das a log file can be for importing.
- As the file is JSON, remember to escape backslashes and quotes.

		{
			"ConfigFile": "",
			"ImportBufferSize": 10,
			"PageSize": 20,
			"MaxAgeDays": 7,
			"Applications": [
				"App1",
				"App2",
				"App3",
			],
			"Environments": [
				{
					"Name": "Development",
					"Servers": [
						{
							"Name": "ricecakes",
							"Path": "D:\\Errorlogs",
							"CopyFilesToLocal": false,
							"Username": null,
							"Password": null
						}
					]
				},
				{
					"Name": "Production",
					"Servers": [
						{
							"Name": "enterprise1",
							"Path": "D:\\ErrorLogsLive\\server1",
							"CopyFilesToLocal": false,
							"Username": "admin",
							"Password": "1234"
						},
						{
							"Name": "enterprise2",
							"Path": "\\shareddrive\\Errorlogs\\server2",
							"CopyFilesToLocal": false,
							"Username": "su",
							"Password": "guessme"
						}
					],

				}
			]
		}

## Roadmap

### Iteration 1 (Done)
1. Reads all files from directories (using async)
2. Table of logs for today
3. Table of logs for this week
4. Show error messages in detail
5. Rescan all logs

### Iteration 1.1 (Done)
1. Remove RavenDB and put MongoDB there.

### Iteration 2 (Done)
1. Count of common exceptions for today and this week
2. Drill down into common exceptions

### Iteration 2.1 (Done)
1. Decent configuration from a JSON .config file.

### Iteration 2.2 (Done)
1. Environments

### Iteration 3
1. Import tool improvements: --update, --copyfiles to only copy files.

### Iteration 4
2. Smart re-scanning, journaling the last scan time for each app and server.

### Iteration 5
1. Search messages

### Iteration 6
1. Improvements to the dashboard to show server with most errors, removing the average.
2. Totals for each day.

### Iteration 7
1. Paging
