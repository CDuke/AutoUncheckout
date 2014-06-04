using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using EnvDTE;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = EnvDTE.Constants;

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
	public sealed class AutoUncheckoutPackage : Package, IVsShellPropertyEvents
	{
		private uint _cookie;
		private Events _events;
		private DocumentEvents _documentEvents;
		private MD5CryptoServiceProvider _md5Provider;

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

			_md5Provider = new MD5CryptoServiceProvider();
			var shellService = GetService<SVsShell,IVsShell>();

			if (shellService != null)
				ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _cookie));
		}

		#endregion

		private void DocumentEvents_DocumentSaved(Document document)
		{
			try
			{
				var tfsContext = GetService<ITeamFoundationContextManager>();
				if (tfsContext == null)
					return;

				if (tfsContext.CurrentContext == null || tfsContext.CurrentContext.TeamProjectCollection == null)
					return;
				
				var tfs = tfsContext.CurrentContext.TeamProjectCollection;
				var vcs = tfs.GetService<VersionControlServer>();
				if (vcs == null)
					return;

				var workspace = vcs.TryGetWorkspace(document.FullName);
//					vcs.QueryWorkspaces(null, tfs.AuthorizedIdentity.UniqueName, Environment.MachineName).FirstOrDefault();
				if (workspace == null)
					return;
				var fileInfoItem = vcs.GetItem(document.FullName);

				if (fileInfoItem != null)
				{
					using (var fileStream = new FileStream(document.FullName, FileMode.Open, FileAccess.Read))
					{
						var currentHash = _md5Provider.ComputeHash(fileStream);
						var hashEquals = fileInfoItem.HashValue.SequenceEqual(currentHash);
						if (hashEquals)
						{
							var fileInfo = new FileInfo(document.FullName);
							if (!fileInfo.IsReadOnly)
							{
								// This is from
								// Assembly: Microsoft.VisualStudio.TeamFoundation.VersionControl, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
								// Type: Microsoft.VisualStudio.TeamFoundation.VersionControl.ClientHelperVS
								// Method: internal static void Undo(Workspace workspace, PendingChange[] changes)
								//using (new WorkspaceSuppressAsynchronousScanner(workspace))
								//{
//									using (new WorkspacePersistedMetadataTables(workspace))
//									{
										workspace.Undo(ItemSpec.FromStrings(new[] {document.FullName}, RecursionType.None), false);
										var vsFileChangeEx = GetService<SVsFileChangeEx, IVsFileChangeEx>();
										vsFileChangeEx.SyncFile(document.FullName);
//									}
								//}
							}
						}
					}
				}
			}
			// Yes, Supress all exception
			catch
			{}
		}

		public int OnShellPropertyChange(int propid, object var)
		{
			// when zombie state changes to false, finish package initialization
			if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
			{
				if (!(bool)var)
				{
					// zombie state dependent code
					var dte = GetService<SDTE, DTE>();
					if (dte != null)
					{
						_events = dte.Events;
						_documentEvents = _events.DocumentEvents;
						_documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

						// eventlistener no longer needed
						var shellService = GetService<SVsShell, IVsShell>();

						if (shellService != null)

							ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(_cookie));

						_cookie = 0;
					}
				}

			}

			return VSConstants.S_OK;
		}

		private T GetService<T>() where T : class
		{
			return GetService(typeof(T)) as T;
		}

		private TInterface GetService<TType, TInterface>()
			where TInterface : class
		{
			return GetService(typeof(TType)) as TInterface;
		}
	}
}
