   M S B A M L   `   `   `  ����  UniversalEditor.Csv(    "UniversalEditor.Csv.TableContainer   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5      � .+�5	      &   System.Windows.Controls.DataGrid  -   5            HeadersVisibility$	  All=�5	   %      ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$ 75 6W      HorizontalGridLinesBrush
 �����   VerticalGridLinesBrush
 �����   EnableRowVirtualization . !   EnableColumnVirtualization .    SelectionMode$ Extended=�5
   H      CanUserReorderColumns .     CanUserSortColumns .  	   SelectionUnit$	 CellOrRowHeader=�5      
   CanUserResizeRows
 .  ��5	      ��    RowHeaderStyle 5      �� 5       �� 
TargetType:  4System.Windows.Controls.Primitives.DataGridRowHeader"  6   .�5      ��  �� Property � �� �� Value$ 0,0,1,1q�6)   �� 5       � �� 5      �� 5   	    �� 
TargetType"  6   .R�5   
   �� 
�������#��z��6(   #��z��6\   ._�5      �� #��z��6    �� TextAlignment$ Center=�66   $��30��6M   #��z��6[   5      5   
   5       �� Triggers 5      P� 5       P� Property �@ IsRowSelected P� Value . . �5   	   ��  � ��$
 Bold"�6&   �� 5   	    � ��
 ����6&   5   	   5         	CellStyle 5       �� 5!      *  $System.Windows.Controls.DataGridCell"  6    5"      P� 5#       �@ 
IsSelected . . �5$   	   ��  � ��
 �����6&   �� 5%   	    � ��
 �����6'   �� 5&   	    � ��
 �   �6&   5'   	   P� 5(       �@J�	IsFocused . . �5)   	   ��  � ��
 �����6&   �� 5*   	    � ��
 ����6'   �� 5+   	    � ��
 �   �6&   5,   	   P� 5-       �@ 	IsEditing . . �5.   	   ��  � ��
 ���� 6&   �� 5/   	    � ��
 ����6'   �� 50   	    � ��
 �   �6&   51   	   53      55      56      57      