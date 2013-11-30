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
//using System.Windows.Threading;
using System.Xml.Linq;
using TextEditor;
using TextEditor.ScriptHighlight;
using TextEditor.Utils;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen; //System.Windows.Media is in the PresentationCore.dll
//using Point = System.Windows.Point;

namespace UnitTestsUniversalEditor
{
    [TestClass]
    public class UfeTextBoxInnerUnitTest
    {
        public static FileWrapper file;

        [TestInitialize]
        public void init()
        {
            //the file is defaulted to a txt file with "abcdefg" on the first line
            file = new FileWrapper("C:/Users/User/Desktop/test.txt", true);
        }

        [TestMethod]
        public void TestFileNotNull()
        {
            Assert.IsNotNull(file);
            Assert.AreEqual("abcdefg", file.GetLine(0));
        }

        [TestMethod]
        public void TestGetNumLines()
        {
            Assert.AreEqual(1, file.GetLinesCount());
        }

        [TestMethod]
        public void TestClearLinesThenCount()
        {
            file.ClearLines();
            Assert.AreEqual(0, file.GetLinesCount());
        }

        [TestMethod]
        public void TestChangeLine()
        {
            file.SetLine(2, "hi");
            Assert.IsTrue(file.HasChanges);
            Assert.AreEqual("hi", file.GetLine(2));
        }
        
    }
}
