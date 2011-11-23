using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Forms = System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Coding4Fun.Kinect.Wpf.Controls;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Research.Kinect.Nui;
using Nui = Microsoft.Research.Kinect.Nui;

namespace DTWGestureRecognition
{
	public partial class MainWindow : Window
	{
		readonly Runtime _runtime = Runtime.Kinects[0];
        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.

        /// <summary>
        /// The red index
        /// </summary>
        private const int RedIdx = 2;

        /// <summary>
        /// The green index
        /// </summary>
        private const int GreenIdx = 1;

        /// <summary>
        /// The blue index
        /// </summary>
        private const int BlueIdx = 0;

        /// <summary>
        /// How many skeleton frames to ignore (_flipFlop)
        /// 1 = capture every frame, 2 = capture every second frame etc.
        /// </summary>
        private const int Ignore = 2;

        /// <summary>
        /// How many skeleton frames to store in the _video buffer
        /// </summary>
        private const int BufferSize = 32;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        private const int MinimumFrames = 6;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        private const int CaptureCountdownSeconds = 3;

        /// <summary>
        /// Where we will save our gestures to. The app will append a data/time and .txt to this string
        /// </summary>
        private const string GestureSaveFileLocation = @"C:\Users\andre.h\Documents\Kinect\kinectdtw - Exp 03d\trunk\DTWGestureRecognition\";

        /// <summary>
        /// Where we will save our gestures to. The app will append a data/time and .txt to this string
        /// </summary>
        private const string GestureSaveFileNamePrefix = @"RecordedGestures";

        /// <summary>
        /// Dictionary of all the joints Kinect SDK is capable of tracking. You might not want always to use them all but they are included here for thouroughness.
        /// </summary>
        private readonly Dictionary<JointID, Brush> _jointColors = new Dictionary<JointID, Brush>
        { 
            {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0, 0))},
            {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79, 84, 33))},
            {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84, 33, 42))},
            {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215, 86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33, 79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33, 33, 84))},
            {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77, 109, 243))},
            {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,  69, 243))},
            {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77, 109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69, 33, 84))},
            {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222, 76))},
            {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77, 109, 243))}
        };

        /// <summary>
        /// The depth frame byte array. Only supports 320 * 240 at this time
        /// </summary>
        private readonly byte[] _depthFrame32 = new byte[320 * 240 * 4];

        /// <summary>
        /// Flag to show whether or not the gesture recogniser is capturing a new pose
        /// </summary>
        private bool _capturing;

        /// <summary>
        /// Dynamic Time Warping object
        /// </summary>
        private DtwGestureRecognizer _dtw;

        /// <summary>
        /// How many frames occurred 'last time'. Used for calculating frames per second
        /// </summary>
        private int _lastFrames;

        /// <summary>
        /// The 'last time' DateTime. Used for calculating frames per second
        /// </summary>
        private DateTime _lastTime = DateTime.MaxValue;

        /// <summary>
        /// Total number of framed that have occurred. Used for calculating frames per second
        /// </summary>
        private int _totalFrames;

        /// <summary>
        /// Switch used to ignore certain skeleton frames
        /// </summary>
        private int _flipFlop;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private ArrayList _video;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private DateTime _captureCountdown = DateTime.Now;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private Forms.Timer _captureCountdownTimer;

        /// <summary>
        /// Variables for the mouse movement scaling
        /// </summary>
        private const float ClickThreshold = 0.33f;
        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;


        // variables from xbox demo
        private static double _handLeft;
        private static double _handTop;

        private List<Button> buttons;
        private List<Button> gestureButtons;

        private static bool _isClosing = false;
        private static Button _selectedButton = null;

        private static bool _controlsOpen = false;

		public MainWindow()
		{
			InitializeComponent();

			Loaded += new RoutedEventHandler(MainWindow_Loaded);
			Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
			kinectButton.Click += new RoutedEventHandler(kinectButton_Clicked);
			_runtime.VideoFrameReady += runtime_VideoFrameReady;
			_runtime.SkeletonFrameReady += SkeletonFrameReady;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			InitializeButtons();
			//Since only a color video stream is needed, RuntimeOptions.UseColor is used.
			_runtime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | Microsoft.Research.Kinect.Nui.RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
			_runtime.SkeletonEngine.TransformSmooth = true;

			//Use to transform and reduce jitter
			_runtime.SkeletonEngine.SmoothParameters = new TransformSmoothParameters
			{
				Smoothing = 0.5f,
				Correction = 0.3f,
				Prediction = 0.4f,
				JitterRadius = 0.05f,
				MaxDeviationRadius = 0.04f
			};

            try
            {
                _runtime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                _runtime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show(
                    "Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }
            _lastTime = DateTime.Now;

            _dtw = new DtwGestureRecognizer(18, 0.6, 2, 2, 10);
            _video = new ArrayList();

            //// If you want to see the depth image and frames per second then include this
            //// I'mma turn this off 'cos my 'puter is proper slow
            _runtime.DepthFrameReady += NuiDepthFrameReady;

            _runtime.SkeletonFrameReady += NuiSkeletonFrameReady;
            _runtime.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;

            //// If you want to see the RGB stream then include this
            //_runtime.VideoFrameReady += NuiColorFrameReady;

            Skeleton3DDataExtract.Skeleton3DdataCoordReady += NuiSkeleton3DdataCoordReady;

		}
		void MainWindow_Unloaded(object sender, RoutedEventArgs e)
		{
			_isClosing = true;
			_runtime.Uninitialize();
            Environment.Exit(0);
		}
		private void InitializeButtons()
		{
			buttons = new List<Button>
			    {
			        button1, 
					button2, 
					button3, 
					button4, 
					button5,
                    buttonExit,
                    toggleControls
			    };
            gestureButtons = new List<Button>
                {
                    dtwRead,
                    dtwCapture,
                    dtwLoadFile,
                    dtwSaveToFile,
                    dtwShowGestureTest
                };
		}

        /// <summary>
        /// Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame that displays different players in different colors
        /// </summary>
        /// <param name="depthFrame16">The depth frame byte array</param>
        /// <returns>A depth frame byte array containing a player image</returns>
        private byte[] ConvertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < _depthFrame32.Length; i16 += 2, i32 += 4) {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                var intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                _depthFrame32[i32 + RedIdx] = 0;
                _depthFrame32[i32 + GreenIdx] = 0;
                _depthFrame32[i32 + BlueIdx] = 0;

                // choose different display colors based on player
                switch (player) {
                    case 0:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 2);
                        break;
                    case 1:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        break;
                    case 2:
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        break;
                    case 3:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 4:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 4);
                        break;
                    case 5:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 6:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 7:
                        _depthFrame32[i32 + RedIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + GreenIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + BlueIdx] = (byte)(255 - intensity);
                        break;
                }
            }

            return _depthFrame32;
        }


        /// <summary>
        /// Called when each depth frame is ready
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Image Frame Ready Event Args</param>
        private void NuiDepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            byte[] convertedDepthFrame = ConvertDepthFrame(image.Bits);

            depthImage.Source = BitmapSource.Create(
                image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, convertedDepthFrame, image.Width * 4);

            ++_totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(_lastTime) > TimeSpan.FromSeconds(1)) {
                int frameDiff = _totalFrames - _lastFrames;
                _lastFrames = _totalFrames;
                _lastTime = cur;
                frameRate.Text = frameDiff + " fps";
            }
        }

        /// <summary>
        /// Gets the display position (i.e. where in the display image) of a Joint
        /// </summary>
        /// <param name="joint">Kinect NUI Joint</param>
        /// <returns>Point mapped location of sent joint</returns>
        private Point GetDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            _runtime.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320)); // convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240)); // convert to 320, 240 space
            int colorX, colorY;
            var iv = new ImageViewArea();

            // Only ImageResolution.Resolution640x480 is supported at this point
            _runtime.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, 0, out colorX, out colorY);

            // Map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeletonCanvas.Width * colorX / 640.0), (int)(skeletonCanvas.Height * colorY / 480));
        }

        /// <summary>
        /// Works out how to draw a line ('bone') for sent Joints
        /// </summary>
        /// <param name="joints">Kinect NUI Joints</param>
        /// <param name="brush">The brush we'll use to colour the joints</param>
        /// <param name="ids">The JointsIDs we're interested in</param>
        /// <returns>A line or lines</returns>
        private Polyline GetBodySegment(JointsCollection joints, Brush brush, params JointID[] ids)
        {
            var points = new PointCollection(ids.Length);
            foreach (JointID t in ids)
            {
                points.Add(GetDisplayPosition(joints[t]));
            }

            var polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        /// <summary>
        /// Runds every time a skeleton frame is ready. Updates the skeleton canvas with new joint and polyline locations.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Event Args</param>
        private void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
            var brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeletonCanvas.Children.Clear();
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {

                        Point jointPos = GetDisplayPosition(joint);
                        var jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = _jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeletonCanvas.Children.Add(jointLine);
                    }
                }

                iSkeleton++;

                // set the cursor position analogous to the right hand position
                //MouseOperations.SetCursorPos(, Convert.ToInt16(data.Position.Y));
                //if (data.Joints[JointID.HandRight].TrackingState == JointTrackingState.Tracked)
                //{
                //    int cursorX, cursorY;
                //    Joint rightHand = data.Joints[JointID.HandRight];
                //    Joint leftHand = data.Joints[JointID.HandLeft];

                //    //find which hand is being used for cursor - is the user right handed or left handed? by seeing which hand is up
                //    var jointCursorHand = (rightHand.Position.Y > leftHand.Position.Y)
                //                    ? rightHand
                //                    : leftHand;
                //    Joint scaledCursorHand = jointCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);
                //    cursorX = (int)scaledCursorHand.Position.X;
                //    cursorY = (int)scaledCursorHand.Position.Y;
                //    MouseOperations.SetCursorPos(cursorX, cursorY);

                //    OnHoverButtonMoved(kinectButton, cursorX, cursorY);

                //}

            } // for each skeleton

        }


        /// <summary>
        /// Called each time a skeleton frame is ready. Passes skeletal data to the DTW processor
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Ready Event Args</param>
        private static void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            foreach (SkeletonData data in skeletonFrame.Skeletons) {
                Skeleton3DDataExtract.ProcessData(data);
            }
        }


		void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			if (e.SkeletonFrame.Skeletons.Count() == 0) return;

			SkeletonFrame skeletonSet = e.SkeletonFrame;

			SkeletonData firstPerson = (from s in skeletonSet.Skeletons
										where s.TrackingState == SkeletonTrackingState.Tracked
										orderby s.UserIndex descending
										select s).FirstOrDefault();
			if (firstPerson==null) return;

			JointsCollection joints = firstPerson.Joints;

			Joint rightHand = joints[JointID.HandRight];
			Joint leftHand = joints[JointID.HandLeft];

			//find which hand is being used for cursor - is the user right handed or left handed?
			var joinCursorHand = (rightHand.Position.Y > leftHand.Position.Y)
							? rightHand
							: leftHand;

			float posX = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.X;
			float posY = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.Y;

			Joint scaledCursorJoint = new Joint
									{
										TrackingState = JointTrackingState.Tracked,
										Position = new Microsoft.Research.Kinect.Nui.Vector
													{
														X = posX,
														Y = posY,
														Z = joinCursorHand.Position.Z
													}
									};
			OnButtonLocationChanged(kinectButton, buttons, gestureButtons, (int)scaledCursorJoint.Position.X, (int)scaledCursorJoint.Position.Y);
		}
		private static void OnButtonLocationChanged(HoverButton hand, List<Button> buttons, List<Button> gestureButtons, int X, int Y)
		{
			if (IsButtonOverObject(hand, buttons, gestureButtons)) hand.Hovering(); else hand.Release();

			Canvas.SetLeft(hand, X - (hand.ActualWidth / 2));
			Canvas.SetTop(hand, Y - (hand.ActualHeight / 2));
		}
		void kinectButton_Clicked(object sender, RoutedEventArgs e)
		{
			//call the click event of the selected button
            _selectedButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, _selectedButton));

		}
		public static bool IsButtonOverObject(FrameworkElement hand, List<Button> buttons, List<Button> gestureButtons)
		{
			if (_isClosing || !Window.GetWindow(hand).IsActive) return false;

			// get the location of the top left of the hand and then use it to find the middle of the hand
			var handTopLeft = new Point(Canvas.GetTop(hand), Canvas.GetLeft(hand));
			_handLeft = handTopLeft.X + (hand.ActualWidth / 2);
			_handTop = handTopLeft.Y + (hand.ActualHeight / 2);

            if (_controlsOpen)
            {
                foreach (Button target in gestureButtons)
                {
                    Point targetTopLeft = target.PointToScreen(new Point());
                    if (_handTop > targetTopLeft.X
                        && _handTop < targetTopLeft.X + target.ActualWidth
                        && _handLeft > targetTopLeft.Y
                        && _handLeft < targetTopLeft.Y + target.ActualHeight)
                    {
                        _selectedButton = target;
                        return true;
                    }
                }
            }

			foreach (Button target in buttons)
			{
                Point targetTopLeft = target.PointToScreen(new Point());
                if (_handTop > targetTopLeft.X
                    && _handTop < targetTopLeft.X + target.ActualWidth
                    && _handLeft > targetTopLeft.Y
                    && _handLeft < targetTopLeft.Y + target.ActualHeight)
                {
                    _selectedButton = target;
                    return true;
                }
			}
			return false;
		}

		void runtime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
		{
			//pull out the video frame from the eventargs and load it into our image object
			PlanarImage image = e.ImageFrame.Image;
			BitmapSource source = BitmapSource.Create(image.Width, image.Height, 96, 96,
				PixelFormats.Bgr32, null, image.Bits, image.Width * image.BytesPerPixel);
			videoImage.Source = source;
		}

        //private MediaElement mediaElement1 = new MediaElement();
        //mediaElement1.LoadedBehavior = MediaState.Manual;
        //mediaElement1.Source = new Uri(@"C:\Music\MySong.mp3", UriKind.RelativeOrAbsolute);
        //mediaElement1.Play();


		private void button1_Click(object sender, RoutedEventArgs e)
		{

		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Button 2 Clicked");
		}

		private void button3_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Button 3 Clicked");
		}

		private void button4_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Button 4 Clicked");
		}

		private void button5_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Button 5 Clicked");
		}


        /// <summary>
        /// Runs every time our 2D coordinates are ready.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 2Ddata Coord Event Args</param>
        private void NuiSkeleton3DdataCoordReady(object sender, Skeleton3DdataCoordEventArgs a)
        {
            currentBufferFrame.Text = _video.Count.ToString();

            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            if (_video.Count > MinimumFrames && _capturing == false) {
                ////Debug.WriteLine("Reading and video.Count=" + video.Count);
                string s = _dtw.Recognize(_video);

                if (s.Contains("@Right hand swipe left")) {
                    WindowState = WindowState.Maximized;
                } else if (s.Contains("@Left hand swipe right")) {
                    WindowState = WindowState.Normal;
                } else if (s.Contains("@Right hand pull down")) {
                    WindowState = WindowState.Minimized;
                } else if (s.Contains("@Right hand push up")) {
                    WindowState = WindowState.Normal;
                }


                results.Text = "Recognised as: " + s;
                if (!s.Contains("__UNKNOWN")) {
                    // There was no match so reset the buffer
                    _video = new ArrayList();
                }
            }

            // Ensures that we remember only the last x frames
            if (_video.Count > BufferSize) {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing) {
                    DtwStoreClick(null, null);
                } else {
                    // Remove the first frame in the buffer
                    _video.RemoveAt(0);
                }
            }

            // Decide which skeleton frames to capture. Only do so if the frames actually returned a number. 
            // For some reason my Kinect/PC setup didn't always return a double in range (i.e. infinity) even when standing completely within the frame.
            // TODO Weird. Need to investigate this
            if (!double.IsNaN(a.GetPoint(0).X)) {
                // Optionally register only 1 frame out of every n
                _flipFlop = (_flipFlop + 1) % Ignore;
                if (_flipFlop == 0) {
                    _video.Add(a.GetCoords());
                }
            }

            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }



        /// <summary>
        /// Opens the sent text file and creates a _dtw recorded gesture sequence
        /// Currently not very flexible and totally intolerant of errors.
        /// </summary>
        /// <param name="fileLocation">Full path to the gesture file</param>
        public void LoadGesturesFromFile(string fileLocation)
        {
            int itemCount = 0;
            string line;
            string gestureName = String.Empty;

            // TODO I'm defaulting this to 18 here for now as it meets my current need but I need to cater for variable lengths in the future
            ArrayList frames = new ArrayList();
            double[] items = new double[18];

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
            while ((line = file.ReadLine()) != null) {
                if (line.StartsWith("@")) {
                    gestureName = line;
                    continue;
                }

                if (line.StartsWith("~")) {
                    frames.Add(items);
                    itemCount = 0;
                    items = new double[18];
                    continue;
                }

                if (!line.StartsWith("----")) {
                    items[itemCount] = Double.Parse(line);
                }

                itemCount++;

                if (line.StartsWith("----")) {
                    _dtw.AddOrUpdate(frames, gestureName);
                    frames = new ArrayList();
                    gestureName = String.Empty;
                    itemCount = 0;
                }
            }

            file.Close();
        }

        /// <summary>
        /// Read mode. Sets our control variables and button enabled states
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwReadClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;

            // Set the capturing? flag
            _capturing = false;

            // Update the status display
            status.Text = "Reading";
        }


        /// <summary>
        /// Starts a countdown timer to enable the player to get in position to record gestures
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwCaptureClick(object sender, RoutedEventArgs e)
        {
            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);

            _captureCountdownTimer = new Forms.Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown;
        }

        /// <summary>
        /// The method fired by the countdown timer. Either updates the countdown or fires the StartCapture method if the timer expires
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event Args</param>
        private void CaptureCountdown(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer) {
                if (DateTime.Now < _captureCountdown) {
                    status.Text = "Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                } else {
                    _captureCountdownTimer.Stop();
                    status.Text = "Recording gesture";
                    StartCapture();
                }
            }
        }

        /// <summary>
        /// Capture mode. Sets our control variables and button enabled states
        /// </summary>
        private void StartCapture()
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = false;
            dtwStore.IsEnabled = true;

            // Set the capturing? flag
            _capturing = true;

            ////_captureCountdownTimer.Dispose();

            status.Text = "Recording gesture" + gestureList.Text;

            // Clear the _video buffer and start from the beginning
            _video = new ArrayList();
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwStoreClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;

            // Set the capturing? flag
            _capturing = false;

            status.Text = "Remembering " + gestureList.Text;

            // Add the current video buffer to the dtw sequences list
            _dtw.AddOrUpdate(_video, gestureList.Text);
            results.Text = "Gesture " + gestureList.Text + "added";

            // Scratch the _video buffer
            _video = new ArrayList();

            // Switch back to Read mode
            DtwReadClick(null, null);
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwSaveToFile(object sender, RoutedEventArgs e)
        {
            string fileName = GestureSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
            System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw.RetrieveText());
            status.Text = "Saved to " + fileName;
        }

        /// <summary>
        /// Loads the user's selected gesture file
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwLoadFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            dlg.InitialDirectory = GestureSaveFileLocation;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true) {
                // Open document
                LoadGesturesFromFile(dlg.FileName);
                //dtwTextOutput.Text = _dtw.RetrieveText();
                status.Text = "Gestures loaded!";
            }
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwShowGestureText(object sender, RoutedEventArgs e)
        {
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            _isClosing = true;
            _runtime.Uninitialize();
            Environment.Exit(0);
            this.Close();
        }


        private void toggleControls_Click(object sender, RoutedEventArgs e)
        {
            if (_controlsOpen)
            {
                gestureControls.IsExpanded = false;
                _controlsOpen = false;
                toggleControls.Background = new SolidColorBrush(Color.FromRgb(75,86,87));
            }
            else
            {
                gestureControls.IsExpanded = true;
                _controlsOpen = true;
                toggleControls.Background = new SolidColorBrush(Color.FromRgb(151, 205, 237));
            }

        }


	}

}
