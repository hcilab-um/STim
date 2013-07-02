using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Threading;
using STimWPF.Interaction;
using System.Windows.Media.Imaging;
using System.Windows;
using STimWPF.Properties;
using System.ComponentModel;
using STimWPF.Status;
using System.IO;
using STimWPF.Attention;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Globalization;
using STimWPF.Util;

namespace STimWPF
{
	public class Core : INotifyPropertyChanged
	{
		private const int VISITOR_COLOR_SHIFT = 50;
		private const int USER_COLOR_SHIFT = 40;
		private const byte MAX_INTENSITY = 255;
		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;

		private const int MINIMUM_JOINT_THRESHOLD = 7;
		
		private const float RENDER_WIDTH = 640.0f;
		private const float RENDER_HEIGHT = 480.0f;

		private const int FACE_TOP = 35;
		private const int LEFT_EYE = 19;
		private const int RIGHT_EYE = 54;
		private const int FACE_BOTTOM = 43;
	
		private const double KINECT_DISPLAY_CENTER_DISTACE_Z = 0.17;

		private static readonly double KinectDisplayCenterDistanceY = 0.58 / 2 + 0.275;
		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

		private long lastUpdate = -1;
		private int playerIndex = -1;
		private int currentFrame = 0;
		private bool showColorImage;

		private static Core instance = null;
		
		public static Core Instance
		{
			get
			{
				if (instance == null)
					instance = new Core();
				return instance;
			}
		}

		private SkeletonDrawer skeletonDrawer;

		private List<VisitStatus> visitStatus = new List<VisitStatus>();
		private byte[] colorImage;
		private short[] depthImage;

		private Dictionary<int, WagSkeleton> currentVisitors = new Dictionary<int, WagSkeleton>();
		private Dictionary<int, FaceTracker> faceTrackers = new Dictionary<int, FaceTracker>();

		private AttentionEstimatorSimple attentionerSimple = new AttentionEstimatorSimple();
		private AttentionEstimatorSocial attentionerSocial = new AttentionEstimatorSocial();

		private Matrix3D originTransform;

		public WagSkeleton ClosestVisitor
		{
			get { return currentVisitors.Values.OrderBy<WagSkeleton, double>(skeleton => skeleton.TransformedPosition.Z).FirstOrDefault(); }
			set { }
		}

		public KinectSensor KinectSensor { get; set; }
		public StatusController StatusCtr { get; set; }
		public VisitorController VisitorCtr { get; set; }

		public bool PlayBackFromFile { get; set; }
		public bool IsKinectConnected { get; set; }
		public DepthPercentFilter DepthPercentF { get; set; }
		public bool ShowColorImage
		{
			get { return showColorImage; }
			set
			{
				showColorImage = value;
				OnPropertyChanged("ShowColorImage");
			}
		}

		private Core() { }

		public void Initialize(int depthPercentBufferSize, int uploadPeriod)
		{
			IsKinectConnected = false;
			PlayBackFromFile = false;

			VisitorCtr = new VisitorController();
			DepthPercentF = new DepthPercentFilter(depthPercentBufferSize);

			ShowColorImage = false;
			
			if (KinectSensor.KinectSensors.Count == 0)
			{
				IsKinectConnected = false;
				Console.WriteLine("No Kinect found");
			}
			else
			{
				IsKinectConnected = true;
				KinectSensor = KinectSensor.KinectSensors[0];
				if (KinectSensor == null || KinectSensor.Status == KinectStatus.NotPowered)
				{
					IsKinectConnected = false;
					throw new Exception("Kinect Not Connected");
				}
				else
				{
					//need to update the kinect gear angle to figure out user distance
					TransformSmoothParameters smoothParameters = new TransformSmoothParameters()
					{
						Smoothing = 0.9f,
						Correction = 0.01f,
						Prediction = 0.1f,
						JitterRadius = 0.01f,
						MaxDeviationRadius = 0.01f
					};

					KinectSensor.DepthStream.Enable();
					KinectSensor.ColorStream.Enable();
					KinectSensor.SkeletonStream.Enable(smoothParameters);

					KinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
					
					KinectSensor.Start();
					
					KinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);
					VisitorCtr.StandardAngleInRadian = ToolBox.AngleToRadian(90 - KinectSensor.ElevationAngle);
					StatusCtr = new StatusController(uploadPeriod);
				}

				originTransform = SetTransformMatrix(0, KinectDisplayCenterDistanceY, KINECT_DISPLAY_CENTER_DISTACE_Z, KinectSensor.ElevationAngle);
			}
		
		}

		private Matrix3D SetTransformMatrix(double offsetX, double offsetY, double offsetZ, int rotateAroundX)
		{
			Matrix3D resultMatrix = new Matrix3D();
			resultMatrix.Rotate(new Quaternion(new Vector3D(10, 0, 0), -rotateAroundX));
			resultMatrix.Translate(new Vector3D(offsetX, offsetY, -offsetZ));
			return resultMatrix;
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			currentFrame++;
			if (PlayBackFromFile)
				return;

			Skeleton[] rawSkeletons = new Skeleton[0];

			DrawingImage imageCanvas = null;

			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null)
				{
					VisitorCtr.ClosePercent = CloseDepthPercentage(depthFrame);
					depthImage = new short[depthFrame.PixelDataLength];
					depthFrame.CopyPixelDataTo(depthImage);
					
				}
			}

			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					rawSkeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(rawSkeletons);
				}
			}

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					colorImage = new byte[colorFrame.PixelDataLength];
					colorFrame.CopyPixelDataTo(colorImage);
					imageCanvas = DrawImage(colorFrame, currentVisitors.Values.ToArray());
					if (ShowColorImage)
						ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
				}
			}

			ExtractValidSkeletons(rawSkeletons);
			VisitorCtr.DetectZone(ClosestVisitor);

			
			if (colorImage != null && depthImage != null)
			{

				//// Update the list of trackers and the trackers with the current frame information
				foreach (WagSkeleton skeleton in currentVisitors.Values)
				{
					LoadVisitorDataInOrder(skeleton);
				}

				StatusCtr.LoadNewSkeletonData(currentVisitors.Values.ToArray(), ClosestVisitor, imageCanvas);
			}

			CleanOldSkeletons();
		
		}

		private int CloseDepthPercentage(DepthImageFrame depthFrame)
		{
			DepthImagePixel[] depthPixels;

			depthPixels = new DepthImagePixel[KinectSensor.DepthStream.FramePixelDataLength];
			depthFrame.CopyDepthImagePixelDataTo(depthPixels);
			int closePixel = 0;
			short constrain = (short)(Settings.Default.InteractionZoneConstrain * 1000);
			for (int i = 0; i < depthPixels.Length; ++i)
			{
				closePixel += (depthPixels[i].Depth <= constrain ? 1 : 0);
			}
			int rawPercent = (int)((double)closePixel / depthPixels.Length * 100);
			return DepthPercentF.ProcessNewPercentageData(rawPercent);
		}

		//drawing color image and skeleton
		private DrawingImage DrawImage(ColorImageFrame colorFrame, WagSkeleton[] skeletons)
		{
			DrawingGroup dgColorImageAndSkeleton = new DrawingGroup();
			DrawingImage drawingImage = new DrawingImage(dgColorImageAndSkeleton);
			skeletonDrawer = new SkeletonDrawer(KinectSensor);

			using (DrawingContext drawingContext = dgColorImageAndSkeleton.Open())
			{
				InitializeDrawingImage(colorFrame, drawingContext);

				if (skeletons != null && skeletons.Count() > 0)
				{
					foreach (WagSkeleton skeleton in skeletons)
					{
						skeletonDrawer.DrawUpperSkeleton(skeleton, drawingContext);
						Joint head = skeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.Head);
						System.Windows.Point headP = skeletonDrawer.SkeletonPointToScreen(head.Position);
						System.Windows.Point socialDataP = headP;
						socialDataP.Y = headP.Y - 50;

						System.Windows.Point simpleDataP = headP;
						simpleDataP.Y = headP.Y - 90;

						//FormattedText
						drawingContext.DrawText(
							new FormattedText(skeleton.AttentionSocial.ToString(), CultureInfo.GetCultureInfo("en-us"),
										FlowDirection.LeftToRight, new Typeface("Verdana"), 20, System.Windows.Media.Brushes.Green),
							socialDataP);

						drawingContext.DrawText(
							new FormattedText(skeleton.AttentionSimple.ToString(), CultureInfo.GetCultureInfo("en-us"),
										FlowDirection.LeftToRight, new Typeface("Verdana"), 20, System.Windows.Media.Brushes.Green),
							simpleDataP);
					}
				}

			}

			//Make sure the image remains within the defined width and height
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			return drawingImage;
		}

		private static void InitializeDrawingImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
		{
			if (colorFrame != null)
			{
				byte[] cbyte = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(cbyte);
				int stride = colorFrame.Width * 4;

				ImageSource imageBackground = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Bgr32, null, cbyte, stride);
				drawingContext.DrawImage(imageBackground, new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			}
			else
			{
				// Draw a transparent background to set the render size
				drawingContext.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			}
		}

		private void ExtractValidSkeletons(Skeleton[] rawSkeletons)
		{
			var validSkeletons = rawSkeletons.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);
			validSkeletons = validSkeletons.Where(skeleton => skeleton.Joints.Count(joint => joint.TrackingState == JointTrackingState.Tracked) > MINIMUM_JOINT_THRESHOLD);

			foreach (Skeleton skeleton in validSkeletons)
			{
				if (currentVisitors.ContainsKey(skeleton.TrackingId))
					currentVisitors[skeleton.TrackingId].Update(skeleton);
				else
				{
					currentVisitors.Add(skeleton.TrackingId, new WagSkeleton(skeleton));
					faceTrackers.Add(skeleton.TrackingId, new FaceTracker(KinectSensor));
				}
				currentVisitors[skeleton.TrackingId].LastFrameSeen = currentFrame;
				ApplyTransformations(currentVisitors[skeleton.TrackingId]);
			}

			OnPropertyChanged("ClosestVisitor");
		}

		private void ApplyTransformations(WagSkeleton skeleton)
		{
			skeleton.TransformedPosition = originTransform.Transform(new Point3D()
			{
				X = skeleton.Position.X,
				Y = skeleton.Position.Y,
				Z = skeleton.Position.Z
			});

			foreach (JointType type in Enum.GetValues(typeof(JointType)))
			{
				Point3D transformpoint = originTransform.Transform(new Point3D()
				{
					X = skeleton.Joints[type].Position.X,
					Y = skeleton.Joints[type].Position.Y,
					Z = skeleton.Joints[type].Position.Z
				});

				SkeletonPoint jointPosition = new SkeletonPoint()
				{
					X = (float)transformpoint.X,
					Y = (float)transformpoint.Y,
					Z = (float)transformpoint.Z
				};

				skeleton.TransformedJoints[type] = new Joint()
				{
					TrackingState = skeleton.TransformedJoints[type].TrackingState,
					Position = jointPosition
				};
			}
		}

		private double CalculateBodyOrientationAngle(WagSkeleton userSkeleton)
		{
			Microsoft.Kinect.Joint shoulderLeft = userSkeleton.TransformedJoints[JointType.ShoulderLeft];
			Microsoft.Kinect.Joint shoulderRight = userSkeleton.TransformedJoints[JointType.ShoulderRight];

			System.Windows.Point shoulderRightP = new System.Windows.Point(shoulderRight.Position.X, shoulderRight.Position.Z);
			System.Windows.Point shoulderLeftP = new System.Windows.Point(shoulderLeft.Position.X, shoulderLeft.Position.Z);

			Vector shoulderVector = new Vector(shoulderRightP.X - shoulderLeftP.X, shoulderRightP.Y - shoulderLeftP.Y);
			Vector xAxis = new Vector(1, 0);

			double orientationAngle = Vector.AngleBetween(xAxis, shoulderVector);

			return Math.Abs(orientationAngle);
		}

		private Point3D CalculateHeadLocation(WagSkeleton skeleton)
		{
			Point3D headLocation = new Point3D(0, 0, 0);
			Joint head = skeleton.TransformedJoints[JointType.Head];
			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				headLocation = new Point3D(head.Position.X, head.Position.Y, head.Position.Z);
			return headLocation;
		}

		private Vector3D CalculateHeadOrientation(WagSkeleton skeleton)
		{
			Vector3D headOrientation = new Vector3D(0, 0, -1);

			FaceTrackFrame face = skeleton.FaceFrame;
			var FacePoints = face.Get3DShape();

			Vector3DF eyeLeft = FacePoints[LEFT_EYE];
			Vector3DF eyeRight = FacePoints[RIGHT_EYE];
			Vector3DF faceTop = FacePoints[FACE_TOP];
			Vector3DF faceBottom = FacePoints[FACE_BOTTOM];

			Vector3D faceVectorHorizontal = new Vector3D(eyeLeft.X - eyeRight.X, eyeLeft.Y - eyeRight.Y, eyeLeft.Z - eyeRight.Z);
			Vector3D faceVectorVertical = new Vector3D(faceTop.X - faceBottom.X, faceTop.Y - faceBottom.Y, faceTop.Z - faceBottom.Z);

			headOrientation = Vector3D.CrossProduct(faceVectorHorizontal, faceVectorVertical);
			headOrientation = originTransform.Transform(headOrientation);
			headOrientation.Normalize();

			Matrix3D headPointsPointUpMatrix = new Matrix3D();
			headPointsPointUpMatrix.RotateAt(new Quaternion(new Vector3D(1, 0, 0), -20), skeleton.TransformedJoints[JointType.Head].Position.ToPoint3D());
			Vector3D lowered = headPointsPointUpMatrix.Transform(headOrientation);

			if (headOrientation.Z > 0)
				throw new Exception("Right hand rule violation");

			return lowered;
		}

		private void LoadVisitorDataInOrder(WagSkeleton skeleton)
		{
			//The process below need to be in order
			FaceTracker tracker = faceTrackers[skeleton.TrackingId];
			skeleton.FaceFrame = tracker.Track(KinectSensor.ColorStream.Format, colorImage, KinectSensor.DepthStream.Format, depthImage, skeleton);
			skeleton.HeadLocation = CalculateHeadLocation(skeleton);
			skeleton.HeadOrientation = CalculateHeadOrientation(skeleton);
			skeleton.BodyOrientationAngle = CalculateBodyOrientationAngle(skeleton);
			skeleton.AttentionSimple = attentionerSimple.CalculateAttention(skeleton);
			skeleton.AttentionSocial = attentionerSocial.CalculateAttention(skeleton, this.currentVisitors.Values.ToArray());
		}

		private void CleanOldSkeletons()
		{
			var oldSkeletons = currentVisitors.Values.Where(skeleton => skeleton.LastFrameSeen <= (currentFrame - 5)).ToArray();
			foreach (WagSkeleton skeleton in oldSkeletons)
			{
				currentVisitors.Remove(skeleton.TrackingId);
				faceTrackers.Remove(skeleton.TrackingId);
			}
			System.GC.Collect();
		}

		public void Shutdown()
		{
			if (KinectSensor != null)
			{
				KinectSensor.Stop();
				KinectSensor.Dispose();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}