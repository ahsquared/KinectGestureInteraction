   M S B A M L   `   `   `  ���� P  LCoding4Fun.Kinect.Wpf, Version=1.1.0.0, Culture=neutral, PublicKeyToken=nullnKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.WpfCoding4Fun.Kinect.Wpf.Controls   KinectUserInterfaceDemo&    DTWGestureRecognition.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     `ControlsKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.Wpf       � WindowState$  	Maximized=� � WindowStyle$ SingleBorderWindow=����#1E1C37 � Title$ Kinect Demo��$
��1045��$
��1920��.��� -    �Name$ theGrid��.+��� ���#141717$
��1900��.+�0   *Coding4Fun.Kinect.Wpf.Controls.HoverButton  -     Name$ kinectButton��$��0q�$��0q�   	ImageSize$ 64Q�   ImageSource$ /Resources/Hand_Basic.png��   ActiveImageSource$ /Resources/Hand_Basic.png��   TimeInterval$
 1000��	 �� Left$		 253��

 �� Top$	
 910�� K� ZIndex$
 1000���� -   $��toggleControls��$	 30��$
 20��$��90��$	��180�����#4B5657$��Right=�$��Bottom=�.���� $��36(����#221E17$��64��$��Controls��$��0,0,10,0q��� -   $��button1��$	 30��$	
 132��$	��150��$	��180�����#4B5657$��Right=�$��Bottom=�.���� $��36(����#221E17$��64��$��	Afro Jams��$��0,0,10,0q��� -   $��button2��$	 30��$	
 303��$	��150��$	��180�����#969481$��Right=�$��Bottom=�.���� $��36(����#332F28$��64��$��Berimbau��$��0,0,10,0q��� -   $��button3��$	 30��$	
 480�����#D2C9B0$	��150��$	��180��$��Right=�$��Bottom=�.���� $��36(����#332F28$��64��$
��Jazz��$��0,0,10,0q��� -   $��button4��$	 30��$	
 653�����#F4E3C1$	��150��$	��180��$��Right=�$��Bottom=�.���� $��28(����#332F28$��64��$��Play Wildlife��$��0,0,10,0q��� -   $��button5��$	 30��$	
 827�����#59676E$	��150��$	��180��$��Right=�$��Bottom=�.���� $��28(����#221E17$��64��$��Stop Wildlife��$��0,0,10,0q��� -	   $��
buttonExit��$
	 1680��$
 48�����#731311$	��150��$	��180��$��Right=�$��Right=�$��Bottom=�.���� $��36(����#ffffff$��64��$
��Quit��$��95���� -
   $��
videoImage��$	��240��$	��320��$��Right=�$��Bottom=�$
	 1579��$	
 765���� -    ��Name$ videoTextTip��$��24(�
�������$��35��$��Video��$	��289��$��Right=�$��Bottom=�$��
0,0,20,240q�$
	 1586��$	
 725���� -   $��
depthImage��$	��240��$	��320��$��Right=�$��Bottom=�$��	0,0,330,0q�$
	 1259��$	
 766���� -   $ depthTextTip��$��24(�
�������$��35��$��Depth��$	��289��$��Right=�$��Bottom=�$��0,0,350,240q�$
	 1266��$	
 725���� -   $��skeletonCanvas��$	��320��$	��240��
���   �}�. $��Right=�$��Bottom=�$y�Visible=�$		 939��$	
 766���� -   $ skeletonTextTip��$��24(�
�������$��35��$��Skeleton��$	��289��$��Right=�$��Bottom=�$��0,0,680,240q�$		 946��$	
 725��9� -   $��gestureControls��$��24(�$��Gesture Controls�����#ffffff$	��394��$	��820��$		 250��$
 25������  �� 
TargetType" 9� �� Triggers ��  �� Binding ��  �� ElementName$ toggleControls�� �� Path$ 	IsChecked� �� Value true.a���  �� Property �@9�
IsExpanded �� Value . .��� $
	 1099��$	
 124��.+��� -   $��dtwRead��$
��Read��{�.  ���#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��	10,10,0,0q�$
��Left=�$	��Top=��� -   $��
dtwCapture��$��Capture�����#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��
10,128,0,0q�$
��Left=�$	��Top=��� -   $��dtwStore��$��Store��{�.  ���#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��
10,248,0,0q�$
��Left=�$	��Top=��� -   $��dtwLoadFile��$��Load Gestures��{�. ���#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��
204,10,0,0q�$
��Left=�$	��Top=��� -   $��dtwSaveToFile��$��Save Gestures��{�. ���#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��204,128,0,0q�$
��Left=�$	��Top=��� -   $��dtwShowGestureTest��$��Gesture Text��{�. ���#B6B835$��24(�$	��108��$	��180��$��Right=�$��Bottom=�$��204,248,0,0q�$
��Left=�$	��Top=��� -   $��gestureList��$��50��$	��400��$��
400,10,0,0q�$
��Left=�$	��Top=����#B6B835$��18(�$��Center=�.@���  �� 
IsSelected . .��@Right hand swipe left�� .��@Right hand swipe right�� .��@Left hand swipe left�� .��@Left hand swipe right�� .��@Two hands zoom in�� .��@Two hands zoom out�� .��@Right hand wave�� .��@Left hand wave�� .��@Right hand pull down�� .��@Left hand pull down�� .��@Right hand push up�� .��@Left hand push up�� .��@Both hands pull down�� .��@Both hands push up�� .��@Get on down at the disco!�� -   $��results��$��18(�|�.  $��Center=�$��0q�$��Results�� �� TextAlignment$
 Left=�$
��Left=�$	��Top=�$	��400��$��40��$��
400,79,0,0q��� -   $��currentBufferFrame��$��18(�|�.  $��Center=�$��0q�$��Buffer��$
 Left=�$
��Left=�$	��Top=�$	��400��$��40��$��400,179,0,0q��� -   $��	frameRate��$��18(�|�.  $��Center=�$��0q�$��	frameRate��$
 Left=�$
��Left=�$	��Top=�$	��400��$��40��$��400,229,0,0q��� -   $��dtwTextOutput��$��18(�|�.  $��Center=�$��0q�$��DTW Text Output��$
 Left=�$
��Left=�$	��Top=�$	��400��$��40��$��400,279,0,0q��� -   $��status��$��18(�|�.  $��0q�$��Status��$��Center=�$
 Left=�$	��400��$��40��$		 653��$	
 199��$��400,128,18,190q�r� -   $��
videoMedia�� r� LoadedBehavior$ Manual=� r� Source$ Resources/Wildlife.wmv?�$
��1280��$	��720��$		 250��$
 25��$y�	Collapsed=�