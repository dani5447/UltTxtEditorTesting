using System.Windows.Forms;

namespace WinForms.Utils.Utils
{
	public static class DialogHelper
	{
		public static string SelectFolder(string text)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.Description = text;
			dialog.ShowNewFolderButton = true;
			DialogResult result = dialog.ShowDialog();

			if (result != DialogResult.OK)
				return null;

			return dialog.SelectedPath;
		}
	}
}
