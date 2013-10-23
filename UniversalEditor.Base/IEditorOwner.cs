using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEditor.Base
{
	public interface IEditorOwner
	{
		void CloseEditor(EditorBase target);
		void OpenEditor(string filePath);
		void CloseActiveEditor();
		void RefreshActiveTitle();
	}
}
