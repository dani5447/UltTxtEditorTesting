   M S B A M L   `   `   `  ����Q  MUniversalEditor.Base, Version=0.4.39.17, Culture=neutral, PublicKeyToken=nulla5clr-namespace:UniversalEditor.Base.Utils.LoggerModule'UniversalEditor.Base.Utils.LoggerModule  5   	   4    .UniversalEditor.Base.Utils.LoggerModule.Logger   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      NloggerModule5clr-namespace:UniversalEditor.Base.Utils.LoggerModule     5   	     � ShowInTaskbar  .   � WindowStyle$ 
ToolWindow=�5   0   $	��300��5   #    � SizeToContent$ WidthAndHeight=�62    � 
ResizeMode$ NoResize=�6Q    � Title$ 	Exception��6g   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� -   5         ��Name$ image��5      $��40��6   $��40��6*   $
��Fill=�66   �� 5   
   $��1��6-   $��5q�6=   ��6   �� *	Message+6=   �� 5   
   $��1��6   $��Right=�6   $��Close��6:   $��75��6J   $��0,2,2,2q�6U    �� IsCancel . $��1��6v   9� 5   
   $��2��6   $��2��6    $��
Details...��64   .��5      �� 	 �� 
IsReadOnly	 . ��6   �� *	Details+
 �� Mode$
 OneTime=�6=   5      5      5      