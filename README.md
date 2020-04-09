# Atlas Taskmaster
 
 Taskmaster is a simple tool designed to quickly manage processes and applications in the background. Taskmaster can automatically restart programs that terminate unexpectedly, or be configured with existing code to automate starting, restarting, or stopping running programs.

Taskmaster works by defining "services" for the programs you need to manage. From there, you can configure Taskmaster to always keep certain applications open (and restart them if they crash) or manually via console commands and launch arguments.

## Setup

1. Download the [latest release here](https://github.com/AtlasAttack/Atlas-Taskmaster/releases).

2. Open the AtlasTaskmaster.exe config file. Under **"ServicePaths"**, change the **"path1"** text to the file path of the exe you want to manage. *(Example: If your file is located on your desktop, the file path would be Desktop\MyFile.exe).*

3. Under **"ServiceWindowNames"**, change **"applicationName1"** to a name that would appear in the main window of the application when it's run. *(If the application's window's name is "League of Legends", the name "League" would work).* If you're not sure what the window name of an application is, open Task Manager and locate your process in the list, then use that name.

4. (Optional) Under **"ServiceKeepRunning"**, if you'd like Taskmaster to restart the application in the event it closes, make sure the value in between the <string> tags is **true**. Otherwise set this to false.
 
 5. (Optional) If you'd like to manage multiple services, you'll need to add additional <string> tags to the sections outlined in steps 2-4. You can add as many services as you'd like.
 
 6. (Optional) You can update "ServiceUpdateRateSeconds" to change how often the application checks if a service has stopped. By default, this interval is every 10 seconds.

## Commands

Currently, taskmaster offers 3 commands:

**START** : Starts one of the configured services : *Usage: start 0 (Starts the first service configured in the "ServicePaths" setting in the config.*

**KILL** : Ends one of the configured services by killing the process, if it is running. : *Usage: kill 0*.

**RESTART** : Restarts one of the configured services by killing the process and restarting it. : *Usage restart 0*.

**Each command needs the index of the service you are referring to as a parameter, starting at 0.**

These commands can also be called using runtime arguments to the application. This is useful if you intend to use Taskmaster with other applications.
