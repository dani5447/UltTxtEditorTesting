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
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TextEditor;
using TextEditor.Utils;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;
using UniversalEditor.Base.Options;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;


namespace UnitTestsUniversalEditor
{
    [TestClass]
    public class UnitTest_UfeTextBoxInner
    {
        public UfeTextBox textBox;
        public IEditorOwner owner;
        public UfeTextDocument doc;
        public static FileWrapper file;
        public object arg;
        public UfeTextBoxInner innerTextBox;
        public ScrollBar verticalScroll, horizontalScroll;
        public FindLineCtrl findLineCtrl;
        public FindWordCtrl findWordCtrl;
        public DocumentChangedCtrl documentChangedCtrl;
        public DocumentDeletedCtrl documentDeletedCtrl;
        public ReadonlyCtrl readonlyCtrl;
        public UfePopup popup;

        [TestInitialize]
        public void init()
        {
            arg = "1";
            //the file is defaulted to a txt file with "abcdefg" on the first line
            file = new FileWrapper("C:/Users/User/Desktop/test.txt", true);
            doc = new UfeTextDocument(file, arg);
            textBox = new UfeTextBox(owner, doc);
            //owner = 
            //the owner is UniversalFileEditor.DomainModel, but it's an internal class that we
            //can't instantiate...
            //innerTextBox = new UfeTextBoxInner(textBox, doc, verticalScroll, horizontalScroll, findLineCtrl, findWordCtrl, documentChangedCtrl, documentDeletedCtrl, readonlyCtrl, popup);

        }

       // [TestMethod]
       // public void TestInsert()
       // {
            //innerTextBox.Insert(0,0,"z");
            
       // }

       // [TestMethod]
       // public void TestRemoveRow()
       // {
            //innerTextBox.RemoveRow(0);
            
       // }

      //  [TestMethod]
       // public void TestInsertLine()
       // {
           // int prefixLength = innerTextBox.InsertLine(1,"test", true);

       // }

       // [TestMethod]
       // public void TestFindText()
      //  {
            //innerTextBox.FindText("bc", true);

      //  }

    }
}
