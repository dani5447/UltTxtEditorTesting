   M S B A M L   `   `   `  ����L  HUniversalEditor, Version=0.4.39.17, Culture=neutral, PublicKeyToken=null1clr-namespace:UniversalEditorUniversalEditor  5   @   o<clr-namespace:UniversalEditor.Modules.OpenRelativeFileModule.UniversalEditor.Modules.OpenRelativeFileModule  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   	   a5clr-namespace:UniversalEditor.Modules.TabSwitchModule'UniversalEditor.Modules.TabSwitchModule  5   	        UniversalEditor.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   9universalEditorclr-namespace:UniversalEditor     6@   _openRelativeFileModule<clr-namespace:UniversalEditor.Modules.OpenRelativeFileModule     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	   QtabSwitchModule5clr-namespace:UniversalEditor.Modules.TabSwitchModule     5   	   -   5        $	��350��5   '   $	��525��64      UniversalEditor.App    DomainModel � DataContext# Z  6@    � Icon$ Resources/AppIcon.ico��6y   #��Z�6�    J� 	AllowDrop .  � Title 6	   �� *WindowTitle+ � 	Resources 5         LineButtonBorderBrush �� Color ! OpenedTabMenuItemDefaultStyle �� 
TargetType �� Property	 �� Value
 �� ElementName �� Path # OpenedTabMenuItemSeparatorStyle ! OpenedTabMenuItemCommandStyle �� Mode �� 	UriSource  TabMenuButtonStyle �� 
TargetType m� IsSubmenuOpen � AllowsTransparency J� SnapsToDevicePixels K� IsItemsHost-  'System.Windows.Input.KeyboardNavigation   DirectionalNavigation �� Triggers P� 
SourceName P� Property P� Value �� 
TargetName  TabMenuItem   TabNavigation � RowDefinitions.   (UniversalEditor.IntToVisibilityConverter   Default �� 	Converter � ColumnDefinitions ��Name %� IsVirtualizing  �� RelativeSource! �� AncestorType" �� Orientation#   TabIndex$ ��Name% �� CornerRadius& ��Name' �Name( � ToolTip+   %UniversalEditor.TabVisibilityDetector)   Default* `� 	Converter+ � ContextMenu, m� Command- m� Icon. m� CommandParameter��%�  &	        &	 #     &	 P    &	 �    &	     7   &	 �    '�� 7    7  7  '�� �    �� 5      $ 	#FF8693BD��62   �� 5      " m�60   .�5      ��  � ��	 6   �� *
FileName+�� 5       �@�ToolTip	 6   �� *DisplayFilePath+�� 5       �@m�Command	 6   �� $
 
tabControl��$% DataContext.SelectEditorCommand��� 5       �@m�CommandParameter	 6(   �� 5      �� 5      " m�62   .�5      ��  � ��	 5      �� 5      .R�5      �� 5      5      5      �� 5      " m�60   .�5       ��  � ��	 6   �� *Item1+$ OneTime=��� 5!       �@m�Command	 6   �� *Item2+$ OneTime=��� 5"       �@m�Icon	 5#      �� 5$      ��5%      �� 5&   	   $6 0/UniversalEditor;component/Resources/windows.png?�6   5(      5*      5+      �� 5-      " m�6%   .�5.      ��  � |�	 .  6!   �� 5/       � ��	 50      �� 51      " m�6   .R�52      �� $��ButtonBorder��6   $��1q�6#   ._�53   	   � .+�54   
   �� $��Marlett)�6   $��16(�6)   $��u��67   $��2q�6@   � 55   
   $��Popup��6   $��Bottom=�6   #��z 60    . |�.  $
��Fade=�6�   .��56      �� $��SubmenuBorder��6   $��1q�6'    . 8��  #��Z�57      $��1q�6>   ._�58      ��  . $ Cycle=�6*   59      5:      5;   
   5<   	    5=      P� 5>   	   $ ButtonBorder��6    �@J�IsMouseOver . . �5?   
   �� $ ButtonBorder��6    � ��	 �	#55FFFFFF6A   5@   
   5B      5D      5E      �� 5G      " m�6   .�5H      ��  � ��	 6   �� $ Header.FileName��� 5I       �@�ToolTip	 6   �� $ Header.FilePath��� 5J       � {�	 6!   �� $ 	IsEnabled��� 5K       �@m�IsCheckable	 6#   �� $ 	IsEnabled��� 5L       �@m�	IsChecked	 6!   �� $ 
IsSelected�$ TwoWay=�5M      �� 5O      " ��6   .�5P      ��  �@�OverridesDefaultStyle	 . 6-   �� 5Q       �@J�SnapsToDevicePixels	 . 6+   �� 5R       � ��	 5S      �� 5T      " ��6   .R�5U      � $
 None=�6   #��Z�65    5V   	   �� 5W   
   $
��Auto �6   �� 5X   
   $
��Auto �6   �� 5Y   
   $��* �6   .+�5[   	   �� $��TabPanelBorder��6   y�6&   �� *OpenedEditorsCount+# Z ._�5\   
   �� $
��svTP��62    . $��0��6>   ��.  $��Disabled=�6d   $��Disabled=�6�   ��5]      �� 5^      " ��6   .�5_      ��  � |�	 .  6)   �� 5`       � ��	 5a      �� 5b      .R�5c      �  .  5d      �� 5e      $��* �6#   �� 5f      $
��Auto �6#   .+�5h      �� $! PART_ScrollContentPresenter��6(    .   . $��0��6�   #��z��6�   o� 5i      $��1��6   .@�5j      m� 8��  8�� ��6G   ��   �� *FindAncestor+"! ��$ Items�5l      5m      5n      5o      5q      5r      .��5t      �� $��TabPanel��6   $" 
Horizontal=�6&    . $# 1��6R   5u      5v   
   �� 5w   	   $��1��6   
������$��2��62   �� 5x   	   $$ Border��6   $��2��6    $��0q�6-   $% 2��6A   $
 None=�6R   $ 	Contained=�6z   $# 2��6�   
�������._�5y   
   �� $& PART_SelectedContentHost��6   $��0q�6=   $��SelectedContent��6H   5z   
   5{   	   5|      5~      5      �� 5�      " ��6
   .�5�      ��  � ��#	 Z�6"   �� 5�       �@�FocusVisualStyle	 6(   Z� �� 5�       � ��	 5�      �� 5�      " ��6   .R�5�      � $
' Root��6   # z��6   ( 6B   �� *
FilePath+.+�5�   	   �� $$ Border��6   $��0q�6    $��0q�6+   #��z��6?   y�5�   
   `� 5�      #* Z) 6   .1�5�      ��   6   �� $! ScrollViewerL��� 5�        6   �� *Self+�� 5�      $ ViewportWidth�6P     6   �� $! ScrollViewerL�6P   �� 5�      $ ExtentWidth�6P     6   �� $! ScrollViewerL�6P   5�      ._�5�   
   �  5�      �� 5�      $��* �6   �� 5�      $
��Auto �6   .+�5�      �� $��Center=�6/   $��0��6L   $��Center=�6\   $��6,2,6,2q�6w   ��6   �� *
FileName++ 5�      �� 5�      .@�5�      m� $��Reload��6   , 6'   �� *RealoadCommand+- 5�      �� 5�      ��5�      �� 5�      $5 //UniversalEditor;component/Resources/reload.png?�6   5�      5�      �� 5�      m� 5�      $
��Save��6   , 6%   �� *SaveCommand+- 5�      �� 5�      ��5�      �� 5�      $3 -/UniversalEditor;component/Resources/save.png?�6   5�      5�      m� 5�      $��Save As��6   , 6(   �� *SaveAsCommand+m� 5�      $��Save Copy As��6   , 6-   �� *SaveCopyAsCommand+m� 5�      $��Save All��6   , 6)   �� *SaveAllCommand+�� 5�      m� 5�      $��Close��6   . 6&   �� *Editor+, 6J   �� *CloseCommand+- 5�      �� 5�      ��5�      �� 5�      $5 //UniversalEditor;component/Resources/delete.png?�6   5�      5�      m� 5�      $��	Close All��6   , 6*   �� *CloseAllCommand+m� 5�      $��Close All But This��6   , 63   �� *CloseAllButThisCommand+. 6^   �� *Editor+�� 5�      m� 5�      $��Copy File Name��6   , 6/   �� *CopyFileNameCommand+m� 5�      $��Copy File Path��6   , 6/   �� *CopyFilePathCommand+m� 5�      $��Copy File Directory��6   , 64   �� *CopyFileDirectoryCommand+�� 5�      m� 5�      $��Open File Location��6   , 63   �� *OpenFileLocationCommand+- 5�      �� 5�      ��5�      �� 5�      $5 //UniversalEditor;component/Resources/folder.png?�6   5�      5�      5�      5�      �� 5�      $��Center=�6W   $��Center=�6r   $��Center=�5�      $��Center=�6.   $��0,2,6,2q�6R   $��1��6c   $( Close editor��6s   ��5�      �� *CloseCommand+��63   �� *Editor+��5�      �� 5�      .R�5�      �� $$ closeBorder��6   
������ ._�5�      �� $��Marlett)�6   $��r��6.   #��Z�67   $��2q�6i   5�       5�      P� 5�      $ closeBorder��6    �@J�IsMouseOver . . �5�      �� $ closeBorder��6    � ��	 �	#55FFFFFF6F   5�      5�      5�      5�      5�   
   5�   	    5�      P� 5�   	   $
 Root��6    �@J�IsMouseOver . . �5�   
   ��  � ��
	 ��iA�6'   �� 5�   
    � ��
	 �����6'   5�   
   P� 5�   	    �@��
IsSelected . . �5�   
   ��  � ��
	 ����6'   �� 5�   
    � ��
	 �����6'   5�   
   5�      5�      5�      5�      / J� InputBindings/ 5�      �� 5�      0 �� Gesture$0 CTRL+N��6   1 �� Command1 6    �� *
NewCommand+�� 5�      $0 CTRL+O��6   1 6    �� *OpenCommand+�� 5�      $0 CTRL+S��6   1 6    �� *SaveCommand+�� 5�      $0 
CTRL+ALT+S��6   1 6$   �� *SaveAsCommand+�� 5�      $0 CTRL+F4��6   1 6!   �� *CloseActiveCommand+�� 5�      $0 CTRL+SHIFT+N��6   1 6&   �� *OpenRelativeFileCommand+�� 5�      $0 F1��6   1 6   �� *AboutCommand+.��5�      �  5�         booleanToVisibilityConverter��%   &	       �� 5      6"   5      5     �� 5     $
��Auto �6   �� 5     $
��Auto �6   �� 5     $��* �6   .+�5	     o�  .  .@�5
     m� $
��File��6   .@�5     m� $	��New��6   2 m� InputGestureText$2 Ctrl+N��6;   , 6   �� *
NewCommand+- 5     �� 5     ��5  	   �� 5  
   $2 ,/UniversalEditor;component/Resources/add.png?�6   5  	   5     m� 5     $��Open...��6   $2 Ctrl+O��6@   , 6    �� *OpenCommand+- 5     �� 5     ��5  	   �� 5  
   $3 -/UniversalEditor;component/Resources/open.png?�6   5  	   5     m� 5     $��Open Recent��6   ��6$   �� *RecentFiles+��5     �� 5     " m�6   .�5   	   ��  � ��	 6"   �� �� 5!  	    �@m�Command	 6#   �� $
 
tabControl��$* $DataContext.OpenSpecifiedFileCommand��� 5"  	    �@m�CommandParameter	 6,   �� 5#  	   5%     �� 5&     m� 5'     $
��Save��6   $2 Ctrl+S��6=   , 6   �� *SaveCommand+- 5(     �� 5)     ��5*  	   �� 5+  
   $3 -/UniversalEditor;component/Resources/save.png?�6   5-  	   5/     m� 50     $��
Save As...��6   $2 
Ctrl+Alt+S��6E   , 6#   �� *SaveAsCommand+6E   m� 51     $��Save Copy As...��6   , 6(   �� *SaveCopyAsCommand+m� 52     $��Save All��6   , 6!   �� *SaveAllCommand+�� 53     m� 54     $��Reload��6   , 6   �� *RealoadCommand+- 55     �� 56     ��57  	   �� 58  
   $5 //UniversalEditor;component/Resources/reload.png?�6   5:  	   5<     �� 5=     m� 5>     $��Close��6   $2 Ctrl+F4��6E   , 6   �� *CloseActiveCommand+- 5?     �� 5@     ��5A  	   �� 5B  
   $5 //UniversalEditor;component/Resources/delete.png?�6   5D  	   5F     m� 5G     $��	Close All��6   , 6"   �� *CloseAllCommand+m� 5H     $��Close All But This��6   , 6+   �� *CloseAllButActiveCommand+�� 5I     m� 5J     $
��Exit��6   $2 Alt+F4��6=   , 6   �� *ExitCommand+- 5K     �� 5L     ��5M  	   �� 5N  
   $3 -/UniversalEditor;component/Resources/exit.png?�6   5P  	   5R     5S     m� 5T     $
��Edit��6   .@�5U     m� $
��Undo��6   $2 Ctrl+Z��6L   , 6   �� *SelectedEditor.UndoCommand+- 5V     �� 5W     ��5X  	   �� 5Y  
   $3 -/UniversalEditor;component/Resources/Redo.png?�6   3 �� Rotation$3 	Rotate180=�6P   5[  	   5]     m� 5^     $
��Redo��6   $2 Ctrl+Y��6L   , 6   �� *SelectedEditor.RedoCommand+- 5_     �� 5`     ��5a  	   �� 5b  
   $3 -/UniversalEditor;component/Resources/Redo.png?�6   5d  	   5f     �� 5g     m� 5h     $	��Cut��6   $2 Ctrl+X��6J   , 6   �� *SelectedEditor.CutCommand+6J   m� 5i     $
��Copy��6   $2 Ctrl+C��6L   , 6   �� *SelectedEditor.CopyCommand+6L   m� 5j     $��Paste��6   $2 Ctrl+V��6N   , 6   �� *SelectedEditor.PasteCommand+6N   m� 5k     $��Delete��6   $	2 Del��6P   , 6   �� *SelectedEditor.DeleteCommand+6P   m� 5l     $��
Select All��6   $2 Ctrl+A��6W   , 6#   �� *!SelectedEditor.SelectAllCommand+6W   �� 5m     m� 5n     $��Copy File Name��6   , 6'   �� *CopyFileNameCommand+m� 5o     $��Copy File Path��6   , 6'   �� *CopyFilePathCommand+m� 5p     $��Copy File Directory��6   , 6,   �� *CopyFileDirectoryCommand+5q     m� 5r     $��Content Type��6   ��6$   �� *	Editors+y�6D   �� *HasOpenedEditor+# [ ��5s     �� 5t     " m�6   .�5u     ��  � ��	 6!   �� *DisplayName+�� 5v      �@m�Command	 6"   �� $
 
tabControl��$* $DataContext.ContentTypeChangeCommand��� 5w      �@m�	IsChecked	 6$   �� *	IsChecked+�� 5x      �@m�CommandParameter	 6+   �� *Format+5y     5{     m� 5|     $��Editor��6   ��6   �� *SelectedEditor.EditorCommands+y�6T   �� *" SelectedEditor.HasEditorCommands+# [ 4 �� FallbackValue$4 	Collapsed��m� 5}     $��Settings��6   .@�5~     m� $��
Options...��6   , 6#   �� *OptionsCommand+- 5     �� 5�     ��5�  	   �� 5�  
   $7 1/UniversalEditor;component/Resources/settings.png?�6   5�  	   5�     5�     m� 5�     $��Windows��6   ��6   �� *WindowsMenuItemContent+y�6N   �� *HasOpenedEditor+# [ ��5�     2   ,UniversalEditor.WindowsMenuItemStyleSelector  5�     5�     m� 5�     $
��Help��6   .@�5�     m� $��Documentation��6   , 6&   �� *DocumentationCommand+- 5�     �� 5�     ��5�  	   �� 5�  
   $3 -/UniversalEditor;component/Resources/info.png?�6   5�  	   5�     �� 5�     m� 5�     $��Check for update��6   , 6)   �� *UpdateCommand+- 5�     �� 5�     ��5�  	   �� 5�  
   $7 1/UniversalEditor;component/Resources/download.png?�6   5�  	   5�     m� 5�     $��Website��6   , 6    �� *WebsiteCommand+- 5�     �� 5�     ��5�  	   �� 5�  
   $2 ,/UniversalEditor;component/Resources/web.png?�6   5�  	   5�     m� 5�     $��About...��6   $2 F1��6B   , 6!   �� *AboutCommand+6B   5�     5�     �� 5�     -   5        5 ��Name$5 
tabControl��5�     $��2��6   $
 None=�6a   5�  3   I   CUniversalEditor.Modules.OpenRelativeFileModule.OpenRelativeFileView  5�     $��3��6b   $��Center=�6s   $��Center=�6�   $��Center=�6�    60   �� *OpenRelativeFileViewModel+6�   ;   5UniversalEditor.Modules.TabSwitchModule.TabSwitchView  5�     -   5        6  Name$6 tabSwitcher��5�  "   $��3��6b   $��Center=�6s   $��Center=�6�   $��Center=�6�    67   �� *TabSwitchViewModel+6�   5�     5�     