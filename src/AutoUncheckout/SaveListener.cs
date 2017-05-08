using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace KulikovDenis.AutoUncheckout
{
	internal class SaveListener : IVsRunningDocTableEvents3
	{
		private readonly AutoUncheckoutPackage _package;

		public SaveListener(AutoUncheckoutPackage package)
		{
			_package = package;
		}

		public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
			string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
			uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterSave(uint docCookie)
		{
			var info = _package.Rdt.GetDocumentInfo(docCookie);
			var fileName = info.Moniker;
			_package.Uncheckout(fileName);
			return VSConstants.S_OK;
		}

		public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
			uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeSave(uint docCookie)
		{
			return VSConstants.S_OK;
		}
	}
}