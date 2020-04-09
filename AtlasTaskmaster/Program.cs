using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasTaskmaster
{
	class Program
	{
		static List<SoftwareService> services = new List<SoftwareService>();
		/// <summary>
		/// RUNTIME ARGS:
		///	 arg[0] --> The operation to execute. This is a string, and can be RESTART, KILL, or START.
		///	 arg[1] --> The path to perform the operation on. This is an integer representing the index of the path/service name, as defined in the .settings file.
		/// </summary>
		/// <param name="args"></param>
		static void Main( string[] args ) {
			Console.Title = "Atlas Taskmaster";
			InitializeServices();
			bool isConfigured = Taskmaster.Default.ServicePaths[0] != "path1";
			bool continueAfterFailedConfig = false;
			int argCount = args.Length;
			Console.WriteLine( $"[STARTUP] - Taskmaster started. Got runtime arg count: {argCount}" );
			if ( !isConfigured ) {
				Console.WriteLine( $"[ERROR] - Taskmaster is not configured correctly. Please designate a proper file path.\nPress [E] to ignore this message." );
				string response = Console.ReadLine();
				if ( response.ToUpper() == "E" ) {
					continueAfterFailedConfig = true;
					Console.WriteLine( $"Taskmaster will continue. Type QUIT to close the program." );
				}
			}

			if ( continueAfterFailedConfig || isConfigured ) {
				OnStart( args );
				string responseString = "";
				while ( responseString != "QUIT" ) {
					responseString = Console.ReadLine().ToUpper();
					ParseCommand( responseString );
				}
			}


		}

		public static void ParseCommand( string rawInput ) {
			string input = rawInput.ToUpper();
			string[] inputParams = rawInput.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
			Print( $"Parsing command: {rawInput}" );
			if ( inputParams[0].ToUpper() == "RESTART") {
				if ( inputParams.Length > 1 ) {
					int targetServiceID = Convert.ToInt32( inputParams[1] );
					RestartService( targetServiceID );
				} else {
					PrintError( "Invalid parameters. Usage: RESTART [ServiceID] to terminate a program and restart it. Services must be defined in the config file." );
				}
				return;
			} else if ( inputParams[0].ToUpper() == "KILL") {
				if ( inputParams.Length > 1 ) {
					int targetServiceID = Convert.ToInt32( inputParams[1] );
					KillService( targetServiceID );
				} else {
					PrintError( "Invalid parameters. Usage: KILL [ServiceID] to terminate a program. Services must be defined in the config file." );
				}
				
				
				return;
			} else if ( inputParams[0].ToUpper() == "START" && inputParams.Length > 1 ) {
				if ( inputParams.Length > 1 ) {
					int targetServiceID = Convert.ToInt32( inputParams[1] );
					StartService( targetServiceID );
				} else {
					PrintError( "Invalid parameters. Usage: START [ServiceID] to start a program. Services must be defined in the config file." );
				}
				
				return;
			} else if ( inputParams[0].ToUpper() == "SCOUT") {
				if ( inputParams.Length > 1 ) {
					string searchString = inputParams[1];
					foreach ( Process p in Process.GetProcesses() ) {
						if ( p.ProcessName.ToUpper().Contains( searchString.ToUpper() ) || p.MainWindowTitle.ToUpper().Contains( searchString.ToUpper() ) ) {
							Print( $"Found process: {p.ProcessName} [Window: {p.MainWindowTitle} | Handle:{p.MainWindowHandle}] matching search query: {searchString}" );
						}
					}
				} else {
					PrintError( "Invalid parameters. Usage: Scout [Term] | Prints out the data for all processes which match the given [Term]." );
				}

			}
		}

		/// <summary>
		/// Called periodically to manage services on the machine
		/// </summary>
		private static async void OnTick() {
			ManageServices();
			await Task.Delay( Taskmaster.Default.ServiceUpdateRateSeconds * 1000 );
			OnTick();
		}

		/// <summary>
		/// Registers the service paths and window names, and forms SoftwareServices classes with them. With this, we can more easily keep track of which services need to be managed, and when.
		/// </summary>
		private static void InitializeServices() {
			int serviceCount = Taskmaster.Default.ServicePaths.Count;
			for ( int index = 0; index < serviceCount; index++ ) {
				string name = Taskmaster.Default.ServiceWindowNames[index];
				string path = Taskmaster.Default.ServicePaths[index];
				services.Add( new SoftwareService( Taskmaster.Default.ServicePaths[index], Taskmaster.Default.ServiceWindowNames[index], Convert.ToBoolean( Taskmaster.Default.ServiceKeepRunning[index] ) ) );
				Print( $" Registered service: {name} - Path ({path})" );
			}
			LinkServices();
			OnTick();
			Print( $" Initialization complete. Ready to receive commands!" );
		}

		/// <summary>
		/// Searches all processes on the local machine, and links those processes to known watched services if possible.
		/// </summary>
		private static void LinkServices() {
			int index = 0;
			foreach ( SoftwareService service in services ) {
				if ( service.link != null ) continue;
				Process searchProcess = GetProcessByWindowName( service.windowName );
				if ( searchProcess == null ) {
					//No running process could be found with the name.
				} else {
					Print( $"Watched service [{index}] { service.windowName}  is already running! ({service.path})" );
					service.link = searchProcess;
				}
				index++;
			}
		}

		private static void ManageServices() {
			foreach ( SoftwareService service in services ) {
				if ( service.keepRunning ) {
					if ( service.link == null || service.link.HasExited ) {
						Print( $"Restarting watched service: { service}" );
						RestartService( services.IndexOf( service ) );
					}
				}
			}
		}

		private static void OnStart( string[] args ) {
			int targetServiceID = -1;
			if ( args.Length >= 2 ) {
				try {
					targetServiceID = Convert.ToInt32( args[1] );
				}
				catch ( FormatException ) {
					PrintError( "Runtime argument error: the serviceID argument supplied was not a valid integer!" );
					return;
				}

			}
			if ( args.Length > 0 ) {
				if ( args[0].ToUpper() == "RESTART" ) {
					RestartService( targetServiceID );
				} else if ( args[0].ToUpper() == "START" ) {
					StartService( targetServiceID );
				} else if ( args[0].ToUpper() == "KILL" || args[0].ToUpper() == "STOP" ) {
					KillService( targetServiceID );
				}
			}

		}

		static Process GetProcessByWindowName( string windowName ) {
			Process[] processes = Process.GetProcesses();
			foreach ( Process p in processes ) {
				if ( p.MainWindowTitle.Contains( windowName ) && p.HasExited == false ) {
					return p;
				}
			}
			return null;
		}

		public static bool IsValidServiceID( int serviceID ) {
			return (serviceID <= services.Count - 1);
		}

		public static void RestartService( int serviceID ) {
			string path = Taskmaster.Default.ServicePaths[serviceID];
			string windowName = Taskmaster.Default.ServiceWindowNames[serviceID];
			Process existingProcess = GetProcessByWindowName( windowName );
			if ( existingProcess != null ) existingProcess.Kill();
			services[serviceID].OnStarted( StartProcess( path ) );

		}

		public static void StartService( int serviceID ) {
			string path = Taskmaster.Default.ServicePaths[serviceID];
			services[serviceID].OnStarted( StartProcess( path ) );
		}

		public static void KillService( int serviceID ) {
			string windowName = Taskmaster.Default.ServiceWindowNames[serviceID];
			Process existingProcess = GetProcessByWindowName( windowName );
			if ( existingProcess != null ) existingProcess.CloseMainWindow();
		}

		private static void OnRelevantServiceStopped( object sender, System.EventArgs e ) {
			Print( "OnRelevantServiceStop() Called. Managing active services." );
			ManageServices();
		}

		public static Process StartProcess( string processPath ) {
			try {
				Process p = new Process();
				p.EnableRaisingEvents = true;
				p.Exited += new EventHandler( OnRelevantServiceStopped );
				string path = Environment.ExpandEnvironmentVariables( processPath );
				Print( "*****************************" );
				ProcessStartInfo startInfo = new ProcessStartInfo( path );
				startInfo.WorkingDirectory = Path.GetDirectoryName( path );
				startInfo.UseShellExecute = true;
				startInfo.WindowStyle = ProcessWindowStyle.Normal;
				p.StartInfo = startInfo;
				p.Start();
				Print( "Finished launching process." );
				return p;
			}
			catch ( Exception e ) {
				PrintError( "Got exception trying to start process name:" + processPath + "\nDetails:" + e.ToString() );
			}
			return null;
		}

		public static void Print( string input ) {
			Console.WriteLine( input );
		}

		static void PrintError( string errorInput ) {
			Console.WriteLine( "************ ERROR ************" );
			Console.WriteLine( errorInput );
			Console.WriteLine( "************ END ERROR ************" );
		}

	}
}
