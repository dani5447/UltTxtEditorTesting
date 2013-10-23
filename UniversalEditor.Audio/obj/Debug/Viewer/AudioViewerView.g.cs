﻿#pragma checksum "..\..\..\Viewer\AudioViewerView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8EF51FF30F92D5DA4AC3CD47E95A123E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace UniversalEditor.Audio.Viewer {
    
    
    /// <summary>
    /// AudioViewerView
    /// </summary>
    public partial class AudioViewerView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 38 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid border;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image mediaBorder;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle tempRect;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider progress;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider volume;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label time;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\Viewer\AudioViewerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button volumeImage;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/UniversalEditor.Audio;component/viewer/audioviewerview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Viewer\AudioViewerView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\Viewer\AudioViewerView.xaml"
            ((UniversalEditor.Audio.Viewer.AudioViewerView)(target)).KeyDown += new System.Windows.Input.KeyEventHandler(this.OnKeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.border = ((System.Windows.Controls.Grid)(target));
            
            #line 38 "..\..\..\Viewer\AudioViewerView.xaml"
            this.border.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.OnMouseUp);
            
            #line default
            #line hidden
            return;
            case 3:
            this.mediaBorder = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.tempRect = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 5:
            this.progress = ((System.Windows.Controls.Slider)(target));
            
            #line 58 "..\..\..\Viewer\AudioViewerView.xaml"
            this.progress.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.OnProgressChanged);
            
            #line default
            #line hidden
            
            #line 59 "..\..\..\Viewer\AudioViewerView.xaml"
            this.progress.KeyDown += new System.Windows.Input.KeyEventHandler(this.OnKeyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.volume = ((System.Windows.Controls.Slider)(target));
            
            #line 61 "..\..\..\Viewer\AudioViewerView.xaml"
            this.volume.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.OnVolumeChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.time = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.volumeImage = ((System.Windows.Controls.Button)(target));
            
            #line 64 "..\..\..\Viewer\AudioViewerView.xaml"
            this.volumeImage.Click += new System.Windows.RoutedEventHandler(this.OnSoundClick);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 67 "..\..\..\Viewer\AudioViewerView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnPlayClick);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 74 "..\..\..\Viewer\AudioViewerView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnPauseClick);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 81 "..\..\..\Viewer\AudioViewerView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnStopClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

