using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TextEditor;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Nsis
{
	internal class NsisTextDocument : UfeTextDocument
	{
		private readonly static string[] _commands = new[]
		{
			"Abort", "AddBrandingImage", "AddSize", "AllowRootDirInstall", "AllowSkipFiles", "AutoCloseWindow", "BGFont", "BGGradient", "BrandingText", "BringToFront", "Call", "CallInstDLL", "Caption", "ChangeUI", 
			"CheckBitmap", "ClearErrors", "CompletedText", "ComponentText", "CopyFiles", "CRCCheck", "CreateDirectory", "CreateFont", "CreateShortCut", "Delete", "DeleteINISec", "DeleteINIStr", "DeleteRegKey", "DeleteRegValue",
			"DetailPrint", "DetailsButtonText", "DirShow", "DirText", "DirVar", "DirVerify", "DisabledBitmap", "EnabledBitmap", "EnableWindow", "EnumRegKey", "EnumRegValue", "Exch", "Exec", "ExecShell", "ExecWait",
			"ExpandEnvStrings", "File", "FileBufSize", "FileClose", "FileErrorText", "FileOpen", "FileRead", "FileReadByte", "FileSeek", "FileWrite", "FileWriteByte", "FindClose", "FindFirst", "FindNext", "FindWindow",
			"FlushINI", "Function", "FunctionEnd", "GetCurInstType", "GetCurrentAddress", "GetDlgItem", "GetDLLVersion", "GetDLLVersionLocal", "GetErrorLevel", "GetFileTime", "GetFileTimeLocal", "GetFullPathName",
			"GetFunctionAddress", "GetInstDirError", "GetLabelAddress", "GetTempFileName", "Goto", "HideWindow", "Icon", "IfAbort", "IfErrors", "IfFileExists", "IfRebootFlag", "IfSilent", "InitPluginsDir", "InstallButtonText",
			"InstallColors", "InstallDir", "InstallDirRegKey", "InstProgressFlags", "InstType", "InstTypeGetText", "InstTypeSetText", "IntCmp", "IntCmpU", "IntFmt", "IntOp", "IsWindow", "LangString", "LangStringUP",
			"LicenseBkColor", "LicenseData", "LicenseForceSelection", "LicenseLangString", "LicenseText", "LoadLanguageFile", "LockWindow", "LogSet", "LogText", "MessageBox", "MiscButtonText", "Name", "OutFile", "Page",
			"PageEx", "PageExEnd", "PluginDir", "Pop", "Push", "Quit", "ReadEnvStr", "ReadINIStr", "ReadRegDWORD", "ReadRegStr", "Reboot", "RegDLL", "Rename", "RequestExecutionLevel", "ReserveFile", "Return", "RMDir",
			"SearchPath", "SectionDivider", "SectionEnd", "SectionGetFlags", "SectionGetInstTypes", "SectionGetSize", "SectionGetText", "SectionGroup", "SectionGroupEnd", "SectionIn", "SectionSetFlags",
			"SectionSetInstTypes", "SectionSetSize", "SectionSetText", "SendMessage", "SetAutoClose", "SetBrandingImage", "SetCompress", "SetCompressionLevel", "SetCompressor", "SetCompressorDictSize", "SetCtlColors",
			"SetCurInstType", "SetDatablockOptimize", "SetDateSave", "SetDetailsPrint", "SetDetailsView", "SetErrorLevel", "SetErrors", "SetFileAttributes", "SetFont", "SetOutPath", "SetOverwrite", "SetPluginUnload",
			"SetRebootFlag", "SetShellVarContext", "SetSilent", "SetStaticBkColor", "ShowInstDetails", "ShowUninstDetails", "ShowWindow", "SilentInstall", "SilentUnInstall", "Sleep", "SpaceTexts", "StrCmp", "StrCmpS",
			"StrCpy", "StrLen", "SubSection", "SubSectionEnd", "UninstallButtonText", "UninstallCaption", "UninstallEXEName", "UninstallIcon", "UninstallSubCaption", "UninstallText", "UninstPage", "UnRegDLL", "Var",
			"VIAddVersionKey", "VIProductVersion", "WindowIcon", "WriteINIStr", "WriteRegBin", "WriteRegDWORD", "WriteRegExpandStr", "WriteRegStr", "WriteUninstaller", "XPStyle", "!AddIncludeDir", "!AddPluginDir",
			"!appendfile", "!cd", "!define", "!delfile", "!echo", "!else", "!endif", "!error", "!execute", "!ifdef", "!ifmacrodef", "!ifmacrondef", "!ifndef", "!include", "!insertmacro", "!macro", "!macroend", "!packhdr",
			"!system", "!tempfile", "!undef", "!verbose", "!warning", "Section"
		};

		public NsisTextDocument(FileWrapper file)
			: base(file, null)
		{ }

		protected override UfeTextLineBase ParseLine(string text, object arg)
		{
			return new NsisTextLine(text);
		}

		private class NsisTextLine : UfeTextLineBase
		{
			private readonly List<UfeTextStyle> _parts = new List<UfeTextStyle>();
			private string _text;

			public NsisTextLine(string text)
			{
				_text = text;
			}

			public override void Initialize()
			{
				_parts.Clear();
				string temp = _text;


				if (temp.TrimStart().StartsWith(";", StringComparison.InvariantCultureIgnoreCase))
				{
					_parts.Add(new UfeTextStyle(temp, Brushes.Green));
				}
				else
				{
					string command = _commands.FirstOrDefault(x => temp.TrimStart().StartsWith(x, StringComparison.InvariantCultureIgnoreCase));

					if (!string.IsNullOrEmpty(command))
					{
						string content = temp.Substring(0, temp.IndexOf(command, StringComparison.InvariantCultureIgnoreCase) + command.Length);
						_parts.Add(new UfeTextStyle(content, Brushes.Blue));

						temp = temp.Substring(content.Length);
					}

					_parts.Add(new UfeTextStyle(temp, SystemColors.ControlTextBrush));
				}
			}

			public override void Remove(int start, int lenght)
			{
				_text = _text.Remove(start, lenght);
				Initialize();
			}

			public override UfeTextStyle[] Substring(int start, int lenght)
			{
				List<UfeTextStyle> result = new List<UfeTextStyle>();

				for (int index = 0; index < _parts.Count; index++)
				{
					UfeTextStyle part = _parts[index];

					if (part.Text.Length < start)
					{
						start -= part.Text.Length;
						continue;
					}

					if (part.Text.Length > start + lenght)
					{
						result.Add(new UfeTextStyle(part.Text.ToString().Substring(start, lenght), part.Brush));
						break;
					}

					int l = part.Text.Length - start;
					result.Add(new UfeTextStyle(part.Text.ToString().Substring(start, l), part.Brush));
					lenght -= l;
					start = 0;
				}

				return result.ToArray();
			}

			public override UfeTextStyle[] Substring(int start)
			{
				return Substring(start, Length - start);
			}

			public override UfeTextStyle[] Text
			{
				get { return _parts.ToArray(); }
			}

			public override int Length
			{
				get { return _text.Length; }
			}

			public override void Append(string text)
			{
				_text += text;
				Initialize();
			}

			public override void Insert(int start, string text)
			{
				_text = _text.Insert(start, text);
				Initialize();
			}

			public override HintItem[] GetHints(int position)
			{
				return new HintItem[0];
			}

			public override string TextAsString
			{
				get { return _text; }
			}
		}
	}
}
