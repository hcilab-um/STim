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
using System.Windows.Media.Media3D;
using STimWPF.Util;
using STimWPF.Pointing;

namespace STimWPF.Interaction
{

	public delegate void KeySelectedEventHandler(object sender, KeySelectedEventArgs e);

	public class InteractionController : INotifyPropertyChanged
	{
		private int currentFrame = 1;// totalFrames = 0;
		private Object processingLock = new Object();
		private EventWaitHandle processingWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

		private Point3D cursorLocation;
		private Point3D secCursorLocation;
		private Key highlightedKey;
		private Key selectedKey;
		private SelectionMethod selectionMethod;

		private InteractionMethod mainInteractionMethod;

		private InteractionZone interactionZone;

		public GestureRecognizer Recognizer { get; set; }

		public Point3D CursorLocation
		{
			get { return cursorLocation; }
			set
			{
				cursorLocation = value;
				OnPropertyChanged("CursorLocation");
			}
		}


		public Key HighlightedKey
		{
			get { return highlightedKey; }
			set
			{
				if (highlightedKey == value)
					return;
				highlightedKey = value;
				OnPropertyChanged("HighlightedKey");
			}
		}

		public Key SelectedKey
		{
			get { return selectedKey; }
			set
			{
				if (selectedKey == value)
					return;
				selectedKey = value;
				OnPropertyChanged("SelectedKey");
			}
		}

		public SelectionMethod SelectionMethod
		{
			get { return selectionMethod; }
			set
			{
				selectionMethod = value;
				OnPropertyChanged("SelectionMethod");
			}
		}

		public InteractionMethod InteractionMethod
		{
			get { return mainInteractionMethod; }
			set
			{
				mainInteractionMethod = value;
				OnPropertyChanged("TypingMethod");
			}
		}

		public InteractionZone InteractionZone
		{
			get { return interactionZone; }
			set
			{
				interactionZone = value;
				OnPropertyChanged("InteractionZone", interactionZone);
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
			InteractionMethod = new BoxInteractionMethod();

			Recognizer = new GestureRecognizer();
			PropertyChanged += new PropertyChangedEventHandler(Recognizer.InteractionCtrngine_PropertyChanged);

			CursorLocation = new Point3D(0, 0, 0);
			secCursorLocation = new Point3D(0, 0, 0);
			SelectionMethod = STimWPF.Interaction.SelectionMethod.Click;
		}

		public bool ProcessNewSkeletonData(Skeleton skeleton, double deltaMilliseconds, InteractionZone interactionZone)
		{
			if (skeleton == null)
				return false;
			if (InteractionMethod == null)
				return false;

			//++totalFrames;
			//SkeletonCapture capture = new SkeletonCapture() { Delay = deltaMilliseconds, Skeleton = skeleton, FrameNro = totalFrames };
			//Thread backgroundThread = new Thread(ProcessTypingData);
			//backgroundThread.Priority = ThreadPriority.AboveNormal;
			//backgroundThread.Start(capture);
			DoWork(skeleton, deltaMilliseconds, interactionZone);

			return true;
		}

		public void ProcessTypingData(object data)
		{
			SkeletonCapture capture = (SkeletonCapture)data;
			double deltaTimeMilliseconds = capture.Delay;
			Skeleton skeleton = capture.Skeleton;
			int threadFrame = capture.FrameNro;
			int _currentFrame = 0;

			do
			{
				processingWaitHandle.WaitOne();
				lock (processingLock)
					_currentFrame = currentFrame;
				if (_currentFrame != threadFrame)
					processingWaitHandle.Set();
			} while (_currentFrame != threadFrame);

			DoWork(skeleton, deltaTimeMilliseconds, InteractionZone);

			lock (processingLock)
				currentFrame++;
			processingWaitHandle.Set();
		}

		private void DoWork(Skeleton skeleton, double deltaMilliseconds, InteractionZone interactionZone)
		{
			Size layoutSize = new Size(35, 35);

			//1- find the position of the cursor on the layout plane
			CursorLocation = InteractionMethod.FindCursorPosition(skeleton, layoutSize, selectionMethod, interactionZone);
			
			//3- Looks for gestures [pressed, released]
			ICollection<InteractionGesture> gestures = Recognizer.ProcessGestures(skeleton, deltaMilliseconds, cursorLocation, secCursorLocation, selectionMethod, highlightedKey, HasUserClicked);

			//4- Passes it on to the typing method itself
			InteractionStatus status = InteractionMethod.ProcessNewFrame(CursorLocation, gestures, skeleton, deltaMilliseconds);
			HighlightedKey = status.HighlightedKey;
			SelectedKey = status.SelectedKey;

		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name, object value = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs(name, value));
		}
	}

}
