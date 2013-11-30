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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestsUniversalEditor
{
    [TestClass]
    public class UnitTest1
    {
        public static IEditorOwner owner;
        public static FileWrapper file;
        public UfeTextBoxInner innerTextBox;
        public UfeTextBox ufeTextBox;
        public UfeTextDocument document;
        public ScrollBar verticalScroll;
        public ScrollBar horizontalScroll;
        public readonly FindLineCtrl findLineCtrl;
        public readonly FindWordCtrl findWordCtrl;
        public readonly DocumentChangedCtrl documentChangedCtrl;
        public readonly DocumentDeletedCtrl documentDeletedCtrl;
        public readonly ReadonlyCtrl readonlyCtrl;
        public readonly UfePopup popup;
        public object x;

        [TestMethod]
        public void TestUfeTextBox_SaveStatus()
        {
            x = " ";
            file = new FileWrapper("C:/Users/User/Desktop/test.txt", true);
            document = new UfeTextDocument(file, x);
            ufeTextBox = new UfeTextBox(owner, document);

            //set up scroll bars
            verticalScroll = new ScrollBar();
            verticalScroll.Width = 17;
            verticalScroll.Visibility = Visibility.Collapsed;
            verticalScroll.Orientation = Orientation.Vertical;
            verticalScroll.Margin = new Thickness(0);
            verticalScroll.Padding = new Thickness(0);



            // innerTextBox = new UfeTextBoxInner(ufeTextBox, document, verticalScroll, horizontalScroll, findLineCtrl, findWordCtrl, documentChangedCtrl, documentDeletedCtrl, readonlyCtrl, popup);
            Assert.IsNotNull(ufeTextBox);
            // Assert.Equals("UfeTextBoxInner", innerTextBox.GetType());
        }

        [TestMethod]
        public void TestTextEditorBase_HasChanges()
        {
            Assert.IsTrue(true);
        }
    }
}
