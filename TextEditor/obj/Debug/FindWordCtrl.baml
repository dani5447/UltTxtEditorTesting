   M S B A M L   `   `   `  ����Q  MUniversalEditor.Base, Version=0.4.39.17, Culture=neutral, PublicKeyToken=nulliHclr-namespace:UniversalEditor.Base.Options;assembly=UniversalEditor.BaseUniversalEditor.Base.Options  5       
TextEditor   TextEditor.FindWordCtrl   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      \optionsHclr-namespace:UniversalEditor.Base.Options;assembly=UniversalEditor.Base     5      $	��250��5      #��Z�6+   $��1,0,0,0q�6b   .��5      � #��Z�6     � ColumnDefinitions  5	   
   �� 5
      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � RowDefinitions 5      �� 5      �� 5      �� 5      �� 5      .+�5   	   �� -   5        $��1��5      $��2q�6    ��.  "  System.Windows.Input.Cursors   Hand � Cursor# Z 6Q    � ToolTip$
 Find��6r   ��5      �� 5      .R�5      � .+�5      �� 
������ 6   �� 5      $��16��6   ��5   	   �� 5   
    �� 	UriSource$6 0/TextEditor;component/Resources/step-forward.png?�6   5   	   5      5       5"      �� 5#      -   5        $��2��5#      $��X��6   $��15��66   $��15��6A   ��.  $��4,0,4,0q�6_   ��5$      �� 5%      .R�5&      � .+�5'      �� 
������ $��Stretch=�67   $��Stretch=�6S   F� 5(       �� Stretch$
 Fill=�6   
���   ���������    � �U     �U����    �?� �U ����� ����?� �� ���?� ������ �� �U�?����pJt�z�GyP@+�yP@���x�R@р���ˁѐjJtQ ��+�yP@�t�������j�xn ��р��t��z�GyP@ ������x�R@�j��ˁ���QJt�z�GyP@	# Z 5)      $ 
Hide panel��6@   5*      5+      5-      �� 5.   
   -   5         ��Name$ txtWord��5.      $��23��6#   $��2q�6/   $��Center=�6:   $��Center=�6�   $��0��6�   ��. |�.  +� IsFocusScope . 5/   3   �� 51   
   -   5        $ 
txtReplace��51      $��1��6&   $��23��63   $��2q�6?   $��Center=�6J   $��Center=�6�   $��0��6�   ��. |�. 6�   �� 52      -   5        	 ��Name$	 
btnReplace��52      $��1��6   $��1��6/   $��2q�6<   ��.  $ Replace��6p   # Z 6�   ��53      �� 54      .R�55      � .+�56      �� 
������ 6   �� 57      $��16��6   ��58   	   �� 59   
   $6 0/TextEditor;component/Resources/step-forward.png?�6   5;   	   5<      5=      5?      �� 5@      -   5        $	 btnReplaceAll��5@      $��2��6"   $��1��62   $��2q�6?   ��.  $ Replace All��6v   # Z 6�   ��5A      �� 5B      .R�5C      � .+�5D      �� 
������ 6   �� 5E      $��16��6   ��5F   	   �� 5G   
   $6 0/TextEditor;component/Resources/fast-forward.png?�6   5I   	   5J      5K      5M      �� 5O      -   5        $��Strong search��5O      $��2q�6%   $��2��60   $��3��6=   *   $UniversalEditor.Base.Options.Options
   Instance � DataContext# Z
 6Q    d� 	IsChecked 6�   �� *IsStrongSearch+ �� Mode$ OneTime=�6�   �� 5Q      $��3��6   $��3��6"   
������$��2��6A   5S      5T      