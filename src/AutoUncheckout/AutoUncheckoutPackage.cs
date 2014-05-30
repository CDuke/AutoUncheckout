using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common.Internal;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell;
using Constants = EnvDTE.Constants;
using Task = System.Threading.Tasks.Task;

namespace KulikovDenis.AutoUncheckout
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidAutoUncheckoutPkgString)]
	[ProvideAutoLoad(Constants.vsContextNoSolution)]
	public sealed class AutoUncheckoutPackage : Package
	{
		private Events _events;
		private DocumentEvents _documentEvents;

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public AutoUncheckoutPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}



		/////////////////////////////////////////////////////////////////////////////
		// Overridden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();
			var dte = (DTE)GetService(typeof(DTE));
			_events = dte.Events;
			_documentEvents = _events.DocumentEvents;
			_documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

		}

		private void DocumentEvents_DocumentSaved(Document document)
		{
			try
			{
				var workspaces = Workstation.Current.GetAllLocalWorkspaceInfo();
				if (workspaces != null && workspaces.Length > 0)
				{
					var userLogin = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
					var currentWorkspace = workspaces.FirstOrDefault(w => w.Computer == Environment.MachineName && w.OwnerName == userLogin);
					if (currentWorkspace != null)
					{
						var tfs = new TfsTeamProjectCollection(currentWorkspace.ServerUri);
						var vcs = tfs.GetService<VersionControlServer>();
						var fileInfoItem = vcs.GetItem(document.FullName);

						if (fileInfoItem != null)
						{
							var md5 = new MD5CryptoServiceProvider();
							using (var fileStream = new FileStream(document.FullName, FileMode.Open, FileAccess.Read))
							{
								var currentHash = md5.ComputeHash(fileStream);
								var hashEquals = fileInfoItem.HashValue.SequenceEqual(currentHash);
								if (hashEquals)
								{
									var w = vcs.GetWorkspace(currentWorkspace);
									var fileInfo = new FileInfo(document.FullName);
									if (!fileInfo.IsReadOnly)
									{
										w.Undo(ItemSpec.FromStrings(new[] {document.FullName}, RecursionType.None), false);

//										var activeWindow = document.ActiveWindow;
//										document.DTE.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 222, null, null);
//										// Refresh source control
//										var sourceControlWindow = document.DTE.Windows.Item("{99B8FA2F-AB90-4F57-9C32-949F146F1914}");
//										sourceControlWindow. Activate();
//										document.DTE.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 222, null, null);
//
//										if (document.DTE.Solution != null && !string.IsNullOrEmpty(document.DTE.Solution.FullName))
//										{
//											var solutionWindow = document.DTE.Windows.Item(Constants.vsWindowKindSolutionExplorer);
//											solutionWindow.Activate();
//											document.DTE.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 222, null, null);
//										}
//
//										activeWindow.Activate();
									}
								}
							}
						}
					}
				}
			}
			catch
			{}
		}

		#endregion

	}
}
