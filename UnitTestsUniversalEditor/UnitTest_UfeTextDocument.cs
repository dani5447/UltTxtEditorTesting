using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; //System.Windows.Controls is in PreseantationFramework.dll
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using TextEditor;
using TextEditor.ScriptHighlight;
using TextEditor.Utils;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;

namespace UnitTestsUniversalEditor
{
    [TestClass]
    public class UnitTest_UfeTextDocument
    {
        public static IEditorOwner owner;
        public static FileWrapper file;
        public UfeTextDocument document;
        public object arg;

        [TestInitialize]
        public void init()
        {
            arg = "1";
            //the file is defaulted to a txt file with "abcdefg" on the first line
            file = new FileWrapper("C:/Users/User/Desktop/test.txt", true);
            document = new UfeTextDocument(file, arg);
        }

        [TestMethod]
        public void TestDocumentSetToFile()
        {
            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsNewFile);
            Assert.AreEqual("abcdefg", document.GetLineText(0));
        }

        [TestMethod]
        public void Test()
        {
            Assert.IsTrue(true);
        }
    }
}
