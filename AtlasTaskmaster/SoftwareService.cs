using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasTaskmaster
{
	/// <summary>
	/// Defines a service set by the user in the app's .Settings file.
	/// This is populated automatically
	/// </summary>
	public class SoftwareService
	{
		internal string path = "";
		internal string windowName = "";
		internal bool keepRunning = false;
		internal Process link;

		public SoftwareService(string servicePath, string serviceWindowName, bool keepServiceRunning = false) {
			path = servicePath;
			windowName = serviceWindowName;
			keepRunning = keepServiceRunning;
		}

		public override string ToString() {
			return $"{windowName} | {path}";
		}

		public void OnStarted(Process processLink) {
			link = processLink;
		}

	}
}
