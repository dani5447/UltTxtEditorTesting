   M S B A M L   `   `   `  ����L  HUniversalEditor, Version=0.4.39.17, Culture=neutral, PublicKeyToken=nulla5clr-namespace:UniversalEditor.Modules.TabSwitchModule'UniversalEditor.Modules.TabSwitchModule  5      ;    5UniversalEditor.Modules.TabSwitchModule.TabSwitchView   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      QtabSwitchModule5clr-namespace:UniversalEditor.Modules.TabSwitchModule     5        � AllowsTransparency  . ��5   7   �� *IsOpen+ �� Mode$ TwoWay=�.��5	      �� #��Z�6
   $��1q�6B   #��Z�6V   ��6�   �� *
MaxWidth+._�5
      �  � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5      �� $��0��6   
������$��2��6.   �� 5      $��16(�6M   $
��Bold"�6[   $��CharacterEllipsis=�6m   $��1��6�   $��5,0,5,0q�6�   ��6   ��  �� ElementName$
 list�� �� Path$ SelectedItem.FileName�6�   �� 5      $��Active Files:��6   $��2��6$   $
��Bold"�61   $��
5, 0, 5, 0q�6C   �� 5      $��4��6T   $��5, 0, 5, 10q�6a   $��CharacterEllipsis=�6v   ��6   �� $
 list��$" SelectedItem.DisplayFilePath�6v   �� 5      -   5         ��Name$
 list��5      $��3��6   #��Z�6(    �� SelectionMode$ Single=�6�   $��5, 10, 5, 10q�5      ��5   Z   �� *OpenedEditors+ �� SelectedIndex 5      �� *SelectionIndex+$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�	 �� SelectedItem	 6c   �� *SelectedEditor+$ OneWay=�$ PropertyChanged=�
 � 	Resources
 5       �� 
TargetType �� Property �� Value �� 
TargetType ��Name"  System.Windows.Input.Cursors   Hand � Cursor �� Padding
 � Tag �� Triggers P� Property P� Value �� 
TargetName��%�  '��       �� 5      " ��6   .�5      ��  � �� 5    	   �� 5!   
   " ��6   .R�5"      �� $ Bd��6   # Z 6   
������ $	��170��6X   ._�5#      �� -   5        $ 5,0,5,0q�5#   g   ��6   �� *
FileName+ 60   �� 6g   5$       5%      P� 5&       �@��
IsSelected . . �5'      ��  � ��
 �����6*   �� 5(      $ Bd��6    � ��
 ����6:   5)      5+      5-   	   5.      5/      ��50      �� 51      .R�52      �  � Orientation$ Vertical=�6    K� IsItemsHost . $	��200��6<   53      ��55      5  -System.Windows.Controls.ItemContainerTemplate�  56      58      5:      5<      5=      5>      