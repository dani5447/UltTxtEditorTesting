using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using TextEditor;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;
using UniversalEditor.Base.Mvvm;

namespace UnitTestsUniversalEditor
{
    [TestClass]
    public class UnitTest_UfeTextBox
    {
        public UfeTextBox textBox;
        public IEditorOwner owner;
        public UfeTextDocument doc;
        public FileWrapper file;
        public object arg;

        [TestInitialize]
        public void init()
        {
            arg = "1";
            //the file is defaulted to a txt file with "abcdefg" on the first line
            file = new FileWrapper("C:/Users/User/Desktop/test.txt", true);
            doc = new UfeTextDocument(file, arg);
            owner = null; //TODO: fix this
            //the owner is UniversalFileEditor.DomainModel, but it's an internal class that we
            //can't instantiate...
            textBox = new UfeTextBox(owner, doc);
        }

       // [TestMethod]
        //public void TestReadonly()
        //{
        //    bool readOnly = textBox.IsReadonly;
        //    Assert.IsFalse(readOnly);
        //}

       // [TestMethod]
       // public void TestIsEnabled()
       // {
            
       // }

    }
}
