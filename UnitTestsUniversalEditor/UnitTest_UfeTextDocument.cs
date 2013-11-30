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
            Assert.AreEqual("C:/Users/User/Desktop/test.txt", document.GetFilePath());
        }

        [TestMethod]
        public void TestGetLineText()
        {
            Assert.AreEqual("abcdefg", document.GetLineText(0));
        }

        [TestMethod]
        public void TestInsertLine()
        {
            document.InsertLine(1);
            document.SetText(1, "hello");
            Assert.AreEqual(5, document.GetLineLength(1));
        }

        [TestMethod]
        public void TestRemoveLine()
        {
            document.InsertLine(1);
            document.SetText(1, "hello");
            document.RemoveLine(1);
            //the only remaining line is the first line with the "abcdefg"
            Assert.AreEqual(1, document.LineCount); 
        }

    }
}
