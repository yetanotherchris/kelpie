# kelpie
Scans NLog *text* log files and displays them in a filterable way in an MVC application, using MongoDB for storage.

## Getting started

- Make sure you have IIS installed
- Install MongoDB: https://www.mongodb.org/downloads
- Create `c:\mongodb\data\db`
- Run `start-mongodb.ps1`
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

### Performance, scaling and the ImportBufferSize settings
Kelpie has been test with duplicated data creating an 11gb MongoDB data directory. This was with 10+ apps per server with 1gb of data per server (around 5gb of log files in total.) 

The server was a VM dual core with 8gb of ram - the performance was ok, although indexing with lucene with this amount of data is very slow.

It's recommended you keep your log files rotating and small with Kelpie - over 250mb per log file will work but will be slow to index and search. With large log file sizes you have to tweak the 'ImportBufferSize' setting or the import tool will throw `OutOfMemoryExceptions` due to the 2gb limitations with the heap size in .NET. If your log files are only 5-10mb then you can set this to a high number, around 500-1000 which greatly speeds up imports.
