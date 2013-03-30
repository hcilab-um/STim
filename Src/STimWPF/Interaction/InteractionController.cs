using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading;
using STimWPF.Interaction;
using STimWPF.Util;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace STimWPF.Interaction
{

	public class InteractionController : INotifyPropertyChanged
	{
		//private int currentFrame = 1;// totalFrames = 0;
		private Object processingLock = new Object();
		private EventWaitHandle processingWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
		private bool leftClick = false;
		private Point3D absoluteCursorLocation;
		private Point3D relativeCursorLocation;
		private SelectionMethod selectionMethod;
		private Zone zone;

		private Vector3D interactPlaneOrigin;
		private double armPlaneRadius = -1;
		private double armPlaneWidth = -1;
		private double armPlaneHeight = -1;
		private double boundaryCross = -1;
		
		//joints
		public JointType ShoulderRight { get; set; }
		public JointType ShoulderLeft { get; set; }
		public JointType Elbow { get; set; }
		public JointType Wrist { get; set; }
		public JointType Hand { get; set; }
		public JointType Head { get; set; }
		
		public GestureRecognizer Recognizer { get; set; }

		public Point3D AbsoluteCursorLocation
		{
			get { return absoluteCursorLocation; }
			set
			{
				absoluteCursorLocation = value;
				OnPropertyChanged("AbsoluteCursorLocation");
			}
		}

		public bool LeftClick
		{
			get { return leftClick; }
			set
			{
				leftClick = value;
				OnPropertyChanged("LeftClick");
			}
		}

		public Point3D RelativeCursorLocation
		{
			get { return relativeCursorLocation; }
			set
			{
				relativeCursorLocation = value;
				OnPropertyChanged("RelativeCursorLocation");
			}
		}
		
		public Rect MouseBoundaries { get; set; }

		public SelectionMethod SelectionMethod
		{
			get { return selectionMethod; }
			set
			{
				selectionMethod = value;
				OnPropertyChanged("SelectionMethod");
			}
		}

		public Zone Zone
		{
			get { return zone; }
			set
			{
				zone = value;
				OnPropertyChanged("Zone", zone);
			}
		}

		private bool hasUserClicked = false;
		private Object clickLock = new Object();
		public bool HasUserClicked
		{
			get
			{
				lock (clickLock)
				{
					if (hasUserClicked)
					{
						hasUserClicked = false;
						return true;
					}
					return false;
				}
			}
			set
			{
				lock (clickLock)
					hasUserClicked = value;
				OnPropertyChanged("HasUserClicked");
			}
		}

		public InteractionController()
		{
			armPlaneRadius = -1;
			Recognizer = new GestureRecognizer();
			PropertyChanged += new PropertyChangedEventHandler(Recognizer.InteractionCtrngine_PropertyChanged);
			RelativeCursorLocation = new Point3D(0, 0, 0);
			AbsoluteCursorLocation = new Point3D(0, 0, 0);
			SelectionMethod = STimWPF.Interaction.SelectionMethod.Click;
		}

		public bool ProcessNewSkeletonData(Skeleton skeleton, double deltaMilliseconds, Zone Zone)
		{
			if (skeleton == null)
				return false;
			//++totalFrames;
			//SkeletonCapture capture = new SkeletonCapture() { Delay = deltaMilliseconds, Skeleton = skeleton, FrameNro = totalFrames };
			//Thread backgroundThread = new Thread(ProcessInteractionData);
			//backgroundThread.Priority = ThreadPriority.AboveNormal;
			//backgroundThread.Start(capture);
			DoWork(skeleton, deltaMilliseconds, Zone);

			return true;
		}

		private void DoWork(Skeleton skeleton, double deltaMilliseconds, Zone Zone)
		{
			Size layoutSize = new Size(35, 35);
			LeftClick = false;
			if (Zone == Interaction.Zone.Interaction)
			{
				//1- find the position of the cursor on the layout plane
				RelativeCursorLocation = FindCursorPosition(skeleton, MouseBoundaries, selectionMethod);
				AbsoluteCursorLocation = ConvertToAbsoluteCursorLocation(RelativeCursorLocation, MouseBoundaries);
				//3- Looks for gestures [pressed, released]
        ICollection<InteractionGesture> gestures = Recognizer.ProcessGestures(skeleton, deltaMilliseconds, absoluteCursorLocation, selectionMethod, HasUserClicked);
				if (gestures != null && gestures.Count > 0 && gestures.ElementAt(0).Type == GestureType.Tap)
				{
					LeftClick = true;
				}
			}
		}

		private Point3D FindCursorPosition(Skeleton skeleton, Rect displayBoundary, SelectionMethod selectionM)
		{
			ShoulderRight = JointType.ShoulderRight;
			ShoulderLeft = JointType.ShoulderLeft;
			Elbow = JointType.ElbowRight;
			Wrist = JointType.WristRight;
			Hand = JointType.HandRight;
			Head = JointType.Head;
			Joint shoulderR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderRight);
			Joint shoulderL = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderLeft);
			Joint elbow = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Elbow);
			Joint wrist = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Wrist);
			Joint hand = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hand);
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);

			Vector3D shoulderRightP = new Vector3D(shoulderR.Position.X, shoulderR.Position.Y, shoulderR.Position.Z);
			Vector3D shoulderLeftP = new Vector3D(shoulderL.Position.X, shoulderL.Position.Y, shoulderL.Position.Z);
			Vector3D wristRightP = new Vector3D(wrist.Position.X, wrist.Position.Y, wrist.Position.Z);
			Vector3D elbowRightP = new Vector3D(elbow.Position.X, elbow.Position.Y, elbow.Position.Z);
			Vector3D handRightP = new Vector3D(hand.Position.X, hand.Position.Y, hand.Position.Z);
			Vector3D headP = new Vector3D(head.Position.X, head.Position.Y, head.Position.Z);

			if (armPlaneRadius == -1)
			{
				armPlaneRadius = ToolBox.GetDisplacementVector(shoulderRightP, elbowRightP).Length + ToolBox.GetDisplacementVector(elbowRightP, wristRightP).Length;
			}

			boundaryCross = Math.Sqrt(Math.Pow(displayBoundary.Width, 2) + Math.Pow(displayBoundary.Height, 2));
			armPlaneWidth = displayBoundary.Width / boundaryCross * armPlaneRadius;
			armPlaneHeight = displayBoundary.Height / boundaryCross * armPlaneRadius;

			Vector3D coordinateOriginP = ToolBox.GetMiddleVector(shoulderLeftP, shoulderRightP);
			
			Matrix3D transformMatrix = GetCoordinateTransformMatrix(shoulderLeftP, shoulderRightP, headP);
			
			interactPlaneOrigin = TransformVector(transformMatrix, coordinateOriginP, shoulderRightP);

			Vector3D relativePos = ToolBox.GetMiddleVector(handRightP, wristRightP);
			relativePos = TransformVector(transformMatrix, coordinateOriginP, relativePos);
			relativePos = ToolBox.GetDisplacementVector(interactPlaneOrigin, relativePos);

			if (relativePos.X < 0 || relativePos.Y < 0)
				return new Point3D(-1,-1,-1);
			if (relativePos.X > armPlaneWidth || relativePos.Y >armPlaneHeight)
				return new Point3D(-1, -1, -1);
			relativePos.X = relativePos.X / (armPlaneWidth) * displayBoundary.Width;
			relativePos.Y = relativePos.Y / (armPlaneHeight) * displayBoundary.Height;
			return (Point3D)relativePos;
		}

		private Vector3D TransformVector(Matrix3D transformMatrix, Vector3D origin, Vector3D rawVector)
		{
			return ToolBox.GetDisplacementVector(origin, rawVector) * transformMatrix;
		}

		private Matrix3D GetCoordinateTransformMatrix(Vector3D left, Vector3D right, Vector3D top)
		{
			//Build New coordinate system
			//Set Middle of Visitor shoulder as origin
			Vector3D coordinateOriginP = ToolBox.GetMiddleVector(left, right);
			//get Relative X, Y, Z direction
			Vector3D directionX = ToolBox.GetDisplacementVector(coordinateOriginP, right);
			Vector3D directionY = ToolBox.GetDisplacementVector(top, coordinateOriginP);
			Vector3D directionZ = Vector3D.CrossProduct(directionX, directionY);
			//get coordinate unit vector 
			directionX.Normalize();
			directionY.Normalize();
			directionZ.Normalize();
			return ToolBox.NewCoordinateMatrix(directionX, directionY, directionZ);
		}

		private Point3D ConvertToAbsoluteCursorLocation(Point3D relativePos, Rect windowRect)
		{
			return new Point3D(relativePos.X + windowRect.X, relativePos.Y + windowRect.Y, relativePos.Z);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		
		private void OnPropertyChanged(String name, object value = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs(name, value));
		}
	}

}
