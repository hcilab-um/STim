using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Threading;
using STim.Interaction;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using STim.Status;
using System.IO;
using STim.Attention;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Globalization;
using STim.Util;

namespace STim
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

		private const int PERIPHERY_MAX_ANGLE = 110;
		//height should be 0.58

		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

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

		private int currentFrame = 0;
		private Matrix3D originTransform;

		private List<VisitStatus> visitStatus = new List<VisitStatus>();
		private byte[] colorImage;
		private short[] depthImage;

		private Dictionary<int, WagSkeleton> currentVisitors = new Dictionary<int, WagSkeleton>();
		private Dictionary<int, FaceTracker> faceTrackers = new Dictionary<int, FaceTracker>();

		private AttentionEstimatorSimple attentionerSimple = new AttentionEstimatorSimple();
		private AttentionEstimatorSocial attentionerSocial = new AttentionEstimatorSocial();

		public WagSkeleton ClosestVisitor
		{
			get { return currentVisitors.Values.OrderBy<WagSkeleton, double>(skeleton => skeleton.TransformedPosition.Z).FirstOrDefault(); }
			set { }
		}

		public KinectSensor KinectSensor { get; set; }
		public DepthPercentFilter DepthPercentF { get; set; }
		public StatusController StatusCtr { get; set; }
		public VisitorController VisitorCtr { get; set; }

		private bool showColorImage = false;
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

		public void Initialize(Dispatcher uiDispatcher, double closeZoneConstrain, double notificationZoneConstrain, int blockPercentBufferSize, double blockDepthPercent, int uploadPeriod, string imageFolder, string dateTimeFileNameFormat,
			string dateTimeLogFormat, double displayWidthInMeters, double displayHeightInMeters, double kinectDistanceZ, double kinectDistanceY, int screenGridRows, int screenGridColumns, bool includeStatusRender, log4net.ILog visitLogger, log4net.ILog statusLogger)
		{
			STimSettings.CloseZoneConstrain = closeZoneConstrain;
			STimSettings.NotificationZoneConstrain = notificationZoneConstrain;

			STimSettings.BlockPercentBufferSize = blockPercentBufferSize;
			STimSettings.BlockDepthPercent = blockDepthPercent;
			
			STimSettings.UploadPeriod = uploadPeriod;
			
			STimSettings.ImageFolder = imageFolder;
			STimSettings.DateTimeFileNameFormat = dateTimeFileNameFormat;
			STimSettings.DateTimeLogFormat = dateTimeLogFormat;
			
			STimSettings.DisplayWidthInMeters = displayWidthInMeters;
			STimSettings.DisplayHeightInMeters = displayHeightInMeters;
			
			STimSettings.KinectDistanceZ = kinectDistanceZ;
			STimSettings.KinectDistanceY = kinectDistanceY;
			
			STimSettings.ScreenGridRows = screenGridRows;
			STimSettings.ScreenGridColumns = screenGridColumns;

			STimSettings.IncludeStatusRender = includeStatusRender;

			VisitorCtr = new VisitorController();
			DepthPercentF = new DepthPercentFilter(STimSettings.BlockPercentBufferSize);
			StatusCtr = new StatusController(uiDispatcher, STimSettings.UploadPeriod, visitLogger, statusLogger) { VisitorContr = VisitorCtr };

			if (KinectSensor.KinectSensors.Count == 0)
			{
				Console.WriteLine("No Kinect found");
			}
			else
			{
				KinectSensor = KinectSensor.KinectSensors[0];
				if (KinectSensor == null || KinectSensor.Status == KinectStatus.NotPowered)
				{
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

					originTransform = CreateOriginMatrix(0, STimSettings.DisplayHeightInMeters / 2 + STimSettings.KinectDistanceY, STimSettings.KinectDistanceZ, KinectSensor.ElevationAngle);
				}
			}
		}

		private Matrix3D CreateOriginMatrix(double offsetX, double offsetY, double offsetZ, int rotateAroundX)
		{
			Matrix3D resultMatrix = new Matrix3D();
			resultMatrix.Rotate(new Quaternion(new Vector3D(10, 0, 0), -rotateAroundX));
			resultMatrix.Translate(new Vector3D(offsetX, offsetY, -offsetZ));
			return resultMatrix;
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			currentFrame++;

			Skeleton[] rawSkeletons = new Skeleton[0];

			DrawingImage imageCanvas = null;

			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null)
				{
					VisitorCtr.ClosePercent = CalculateBlockPercentage(depthFrame);
					depthImage = new short[depthFrame.PixelDataLength];
					depthFrame.CopyPixelDataTo(depthImage);
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

			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					rawSkeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(rawSkeletons);
				}
			}

			ExtractValidSkeletons(rawSkeletons);
			VisitorCtr.Zone = VisitorCtr.DetectZone(ClosestVisitor);

			if (colorImage != null && depthImage != null)
			{
				//// Update the list of trackers and the trackers with the current frame information
				foreach (WagSkeleton skeleton in currentVisitors.Values)
				{
					//The process below need to be in order
					skeleton.HeadLocation = CalculateHeadLocation(skeleton);
					skeleton.BodyOrientationAngle = CalculateBodyOrientationAngle(skeleton);
					skeleton.InPeriphery = (skeleton.BodyOrientationAngle <= PERIPHERY_MAX_ANGLE);

					skeleton.AttentionSimple = attentionerSimple.CalculateAttention(skeleton);
					skeleton.AttentionSocial = attentionerSocial.CalculateAttention(skeleton, this.currentVisitors.Values.ToArray());

					FaceTracker tracker = faceTrackers[skeleton.TrackingId];
					if (tracker == null)
						continue;
					skeleton.FaceFrame = tracker.Track(KinectSensor.ColorStream.Format, colorImage, KinectSensor.DepthStream.Format, depthImage, skeleton);
					if (skeleton.FaceFrame.TrackSuccessful)
						skeleton.HeadOrientation = CalculateHeadOrientation(skeleton);
				}
			}

			StatusCtr.LoadNewSkeletonData(currentVisitors.Values.ToList(), ClosestVisitor, GetImageAsArray(imageCanvas, (currentFrame % 32) == 0));
			CleanOldSkeletons();
		}

		private byte[] lastImage = new byte[0];
		private byte[] GetImageAsArray(DrawingImage imageCanvas, bool performConvertion)
		{
			if (!performConvertion || currentVisitors.Count == 0 || imageCanvas == null)
				return lastImage;

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawImage(imageCanvas, new System.Windows.Rect(0, 0, RenderWidth, RenderHeight));
			drawingContext.Close();

			RenderTargetBitmap bmp = new RenderTargetBitmap((int)RenderWidth, (int)RenderHeight, 96, 96, PixelFormats.Pbgra32);
			bmp.Render(drawingVisual);
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bmp));

			MemoryStream memory = new MemoryStream();
			encoder.Save(memory);
			lastImage = memory.ToArray();
			return lastImage;
		}

		private double CalculateBlockPercentage(DepthImageFrame depthFrame)
		{
			DepthImagePixel[] depthPixels;

			depthPixels = new DepthImagePixel[KinectSensor.DepthStream.FramePixelDataLength];
			depthFrame.CopyDepthImagePixelDataTo(depthPixels);
			int closePixel = 0;
			short constrain = (short)(STimSettings.CloseZoneConstrain + STimSettings.KinectDistanceZ * 1000);
			for (int i = 0; i < depthPixels.Length; ++i)
			{
				closePixel += (depthPixels[i].Depth <= constrain ? 1 : 0);
			}
			double rawPercent = (double)(closePixel * 100) / (double)depthPixels.Length;
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
						Joint shoulderCenter = skeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderCenter);
						System.Windows.Point headP = skeletonDrawer.SkeletonPointToScreen(head.Position);
						System.Windows.Point shoulderP = skeletonDrawer.SkeletonPointToScreen(shoulderCenter.Position);

						double distance = new Vector(headP.X - shoulderP.X, headP.Y - shoulderP.Y).Length / 2;

						System.Windows.Point skeletonIdPos = headP;
						System.Windows.Point socialDataPos = headP;
						System.Windows.Point simpleDataPos = headP;


						socialDataPos.Y = headP.Y - distance;
						simpleDataPos.Y = socialDataPos.Y - 30;

						skeletonIdPos.X = headP.X - distance;
						skeletonIdPos.Y = socialDataPos.Y;

						//FormattedText
						drawingContext.DrawText(
							new FormattedText(skeleton.TrackingId.ToString(), CultureInfo.GetCultureInfo("en-us"),
										FlowDirection.LeftToRight, new Typeface("Verdana"), 20, System.Windows.Media.Brushes.Red),
							skeletonIdPos);
						if (STimSettings.IncludeStatusRender)
						{
							//FormattedText
							drawingContext.DrawText(
								new FormattedText(skeleton.AttentionSocial.ToString(), CultureInfo.GetCultureInfo("en-us"),
											FlowDirection.LeftToRight, new Typeface("Verdana"), 15, System.Windows.Media.Brushes.Green),
								socialDataPos);

							drawingContext.DrawText(
								new FormattedText(skeleton.AttentionSimple.ToString(), CultureInfo.GetCultureInfo("en-us"),
											FlowDirection.LeftToRight, new Typeface("Verdana"), 15, System.Windows.Media.Brushes.Green),
								simpleDataPos);
						}
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
					currentVisitors.Add(skeleton.TrackingId, new WagSkeleton(skeleton));

				try
				{
					if (!faceTrackers.ContainsKey(skeleton.TrackingId))
						faceTrackers.Add(skeleton.TrackingId, new FaceTracker(KinectSensor));
				}
				catch (InvalidOperationException)
				{
					//Try/Catch code and comment taken from the SDK project 
					// During some shutdown scenarios the FaceTracker is unable to be instantiated.  Catch that exception
					// and don't track a face.
					Console.WriteLine("ExtractValidSkeletons - creating a new FaceTracker threw an InvalidOperationException");
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

			Vector shoulderVector = new Vector(shoulderRight.Position.X - shoulderLeft.Position.X, shoulderRight.Position.Z - shoulderLeft.Position.Z);
      Matrix matrix = new Matrix();
      matrix.Rotate(-90);
      Vector bodyFacingDirection = matrix.Transform(shoulderVector);
      Vector displayLocation = -new Vector(userSkeleton.HeadLocation.X, userSkeleton.HeadLocation.Z);
      double orientationAngle = Math.Abs(Vector.AngleBetween(displayLocation, bodyFacingDirection));
			return Math.Abs(orientationAngle);
		}

		private Point3D CalculateHeadLocation(WagSkeleton skeleton)
		{
			Point3D headLocation = new Point3D(0, 0, 0);
			Joint head = skeleton.TransformedJoints[JointType.Head];
			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				headLocation = head.Position.ToPoint3D();
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
			headPointsPointUpMatrix.RotateAt(new Quaternion(new Vector3D(int.MaxValue, 0, 0), -20), skeleton.TransformedJoints[JointType.Head].Position.ToPoint3D());
			Vector3D lowered = headPointsPointUpMatrix.Transform(headOrientation);

			if (headOrientation.Z > 0)
				throw new Exception("Right hand rule violation");
			return lowered;
		}

		private void CleanOldSkeletons()
		{
			var oldSkeletons = currentVisitors.Values.Where(skeleton => skeleton.LastFrameSeen <= (currentFrame - 100)).ToArray();
			foreach (WagSkeleton skeleton in oldSkeletons)
			{
				if (!(VisitorCtr.IsBlocked && ClosestVisitor.TrackingId == skeleton.TrackingId))
				{
					currentVisitors.Remove(skeleton.TrackingId);
					faceTrackers.Remove(skeleton.TrackingId);
				}
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