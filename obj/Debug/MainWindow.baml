   M S B A M L   `   `   `  ����  ANTLRReasoningCounter&     ANTLRReasoningCounter.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$"  ARC: ANTLR Reasoning Counter��5   	   $	��718��6.   $
��1350��6;   ���	#FF323131.��5      � .+�5   
   	� -   5         	�Name$ displayModeDB��5      $&�� Use Data Retrieved from Database��6-   $
��Left=�6X   $��10,0,0,0q�6s   $	��Top=�6�    	� 	GroupName$ DisplayMode��6�    d� 	IsChecked$
 TrueY�6�   $
��Bold"�6�   $��16(�6�   6�   	� 5   
   -   5        $ displayModeScan��5      $!��Display Data from File Scan��6/   $
��Left=�6U   $��	10,20,0,0q�6p   $	��Top=�6�   $ DisplayMode��6�   $
��Bold"�6�   $��16(�6�   6�   �� 5   
   -   5         ��Name$ textBoxFilepath��5      $
��Left=�6+   $��23��6F   $��	30,45,0,0q�6R    �� TextWrapping$
 Wrap=�6e   $��Enter filepath��6y   $	��Top=�6�   $	��989��6�   ���	#FF959393
���   �6�   �� 5	   
   -   5         ��Name$ buttonBrowse��5	      $��Browse��6'   $
��Left=�68   $��1024,45,0,0q�6S   $	��Top=�6h   $��75��6�   6�   �� 5
   
   -   5        $ 
buttonScan��5
      $
��Scan��6%   $
��Left=�64   $��1109,45,0,0q�6O   $	��Top=�6d   $��75��6|   6�   �� 5   
   -   5        $ buttonAddToDB��5      $��	Add to DB��6(   $
��Left=�6<   $��1194,45,0,0q�6W   $	��Top=�6l   $��75��6�   6�   �� 5   
   ���	#FF474749$
��Left=�6%   $	��579��6@   $��	10,98,0,0q�6M   
���   �$	��Top=�6o   $
��1322��6�   �� 5   
   $��Source Code Information��6   $
��Left=�62   $��	10,65,0,0q�6M   $	��Top=�6`   $
��Bold"�6x   $��20(�6�   �� 5   
   $��Files��6   $
��Left=�6    $��	30,98,0,0q�6;   $	��Top=�6N   $
��Bold"�6f   $��16(�6x   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ dataGridFiles��5      $
��Left=�6*   $��
30,122,0,0q�6E   $	��Top=�6Y   $��100                   ��6q   $
��1302��5      
���   ����	#FF91919165   �� 5   
   $��Packages��6   $
��Left=�6#   $��
30,221,0,0q�6>   $	��Top=�6R   $
��Bold"�6j   $��16(�6|     5   
   -   5        $ dataGridPackages��5      $
��Left=�6-   $��
30,248,0,0q�6H   $	��Top=�6\   $	��100��6t   $
��1285��6�   
���   ����	#FF9191916�   �� 5   
   $��Classes��6   $
��Left=�6"   $��
30,353,0,0q�6=   $	��Top=�6Q   $
��Bold"�6i   $��16(�6{     5   
   -	   5        $ dataGridClasses��5      $
��Left=�6,   $��
30,379,0,0q�6G   $	��Top=�6[   $	��150��6s   $
��1285��6�   
���   ����	#FF9191916�   �� 5   
   -
   5        $ buttonClearDB��5      $��Clear DB��6(   $
��Left=�6;   $��	292,0,0,0q�6V   $	��Top=�6i   $��75��6�   6�     5   
   -   5        $ dataGridMethods��5      $
��Left=�6,   $��
30,560,0,0q�6G   $	��Top=�6[   $	��100��6s   $
��1285��6�   
���   ����	#FF9191916�   �� 5   
   $��Methods��6   $
��Left=�6"   $��
30,534,0,0q�6=   $	��Top=�6Q   $
��Bold"�6i   $��16(�6{   5      5      