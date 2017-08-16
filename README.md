# Code Challenge Windows Client App

This C# application is designed to watch a folder and automatically process new website visits records for the Code Challenge app.
The application comes in two flavors: a console application and a Windows service

### The Console Application
In `dist\1.0.0\ConsoleApp.zip`
This console app is used mostly for testing the proper behavior of the programme. It will watch the folder specified in the configuration file (key: `DataFolder`) and exit automatically after 2 minutes.

To run it:
- Extract the ZIP file
- In the file `WindowsClientConsoleApp.exe.config` Replace the 'TODO' value with the key supplied separately:
`<add key="BackendEndpoint" value="TODO" />`
- Launch the programme by double-clicking the .exe

### The Windows Service
In `dist\1.0.0\WindowsService.zip`
This programme can be installed as a Windows service on the host machine and will continuously watch the folder specified in the configuration file (key: `DataFolder`).

To run it:
- Extract the ZIP file
- In the file `WindowsClientConsoleApp.exe.config` Replace the 'TODO' value with the key supplied separately:
`<add key="BackendEndpoint" value="TODO" />`
- Install the programme as a Windows service with this command: `sc create WebsiteVisitsFilesWatcher binPath= "<DIRECTORY>\WindowsClient.exe"`
- Start the service from the Windows services module
