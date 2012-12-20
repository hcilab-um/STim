﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Threading;
using STimWPF.Playback;
using STimWPF.Interaction;
using System.Windows.Media.Imaging;
using System.Windows;

namespace STimWPF
{
	public class Core
	{
		private static KinectSensor kinectSensor;

		public event EventHandler<ColorImageReadyArgs> ColorImageReady;
		public event EventHandler<DepthImageReadyArgs> DepthImageReady;

		public SkeletonFilter SkeletonF { get; set; }
		public InteractionController InteractionCtr { get; set; }
		public VisitorController VisitorCtr { get; set; }
		public SkeletonRecorder Recorder { get; set; }
		public SkeletonPlayer Player { get; set; }

		private SkeletonDrawer skeletonDrawer;
		public bool PlayBackFromFile { get; set; }
		private long lastUpdate = -1;

		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;

		private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
		private readonly Brush inferredJointBrush = Brushes.Yellow;
		private const double JointThickness = 3;

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

		public bool IsKinectConnected { get; set; }

		private Core()
		{
		}

		public void Initialize(Dispatcher uiDispatcher, int skeletonBufferSize, String destFolder, int playerBufferSize)
		{
			IsKinectConnected = false;
			PlayBackFromFile = false;
			SkeletonF = new SkeletonFilter(skeletonBufferSize);
			Recorder = new SkeletonRecorder(destFolder);
			Player = new SkeletonPlayer(playerBufferSize, uiDispatcher);
			Player.SkeletonFrameReady += new EventHandler<PlayerSkeletonFrameReadyEventArgs>(Player_SkeletonFrameReady);
			InteractionCtr = new InteractionController();
			VisitorCtr = new VisitorController();

			if (KinectSensor.KinectSensors.Count == 0)
			{
				IsKinectConnected = false;
				Console.WriteLine("No Kinect found");
			}
			else
			{
				IsKinectConnected = true;
				kinectSensor = KinectSensor.KinectSensors[0];
				if (kinectSensor == null)
					IsKinectConnected = false;
				else
				{
					kinectSensor.DepthStream.Enable();
					kinectSensor.ColorStream.Enable();
					kinectSensor.SkeletonStream.Enable();
					kinectSensor.Start();
					kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);

				}
			}
		}


		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			if (PlayBackFromFile)
				return;

			long currentTimeMilliseconds = -1;
			Skeleton[] skeletons = null;
			Skeleton rawSkeleton = null;
			Skeleton stableSkeleton = null;
			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(skeletons);
					currentTimeMilliseconds = skeletonFrame.Timestamp;

					foreach (Skeleton skeleton in skeletons)
					{
						if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
							//skip the rest below continue
							continue;
						rawSkeleton = skeleton;
						stableSkeleton = SkeletonF.ProcessNewSkeletonData(rawSkeleton);
						break;
					}
				}
			}

			if (stableSkeleton != null)
			{
				//Calculates the delta in time
				double deltaTime = (currentTimeMilliseconds - lastUpdate);
				if (lastUpdate == -1)
					deltaTime = 0;
				lastUpdate = currentTimeMilliseconds;

				//Processes the skeleton to find what interaction zone the user is
				VisitorCtr.DetectUserPosition(stableSkeleton);
				//Process the skeleton in to control the mouse
				InteractionCtr.ProcessNewSkeletonData(stableSkeleton, deltaTime, VisitorCtr.InteractionZone);
				//Sends the new skeleton into the recorder
				Recorder.ProcessNewSkeletonData(rawSkeleton, deltaTime);
			}

			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (DepthImageReady != null && depthFrame != null)
				{
					DepthImageReady(this, new DepthImageReadyArgs() { Frame = DrawDepthImage(depthFrame, stableSkeleton) });
				}
			}

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (ColorImageReady != null)
				{
					DrawingImage imageCanvas = DrawImage(colorFrame, stableSkeleton);
					ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
				}
			}
		}

		ImageSource DrawDepthImage(DepthImageFrame depthFrame, Skeleton stableSkeleton)
		{
			DepthImagePixel[] depthPixels;
			byte[] colorPixels;
			// Allocate space to put the depth pixels we'll receive
			depthPixels = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength];

			// Allocate space to put the color pixels we'll create
			colorPixels = new byte[kinectSensor.DepthStream.FramePixelDataLength * sizeof(int)];

			// This is the bitmap we'll display on-screen
			WriteableBitmap colorBitmap = new WriteableBitmap(kinectSensor.DepthStream.FrameWidth, kinectSensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
			depthFrame.CopyDepthImagePixelDataTo(depthPixels);

			// Get the min and max reliable depth for the current frame
			int minDepth = depthFrame.MinDepth;
			int maxDepth = depthFrame.MaxDepth;
			if (stableSkeleton != null)
			{
				maxDepth = (int)(stableSkeleton.Joints.Max<Joint>(j => j.Position.Z) * 1000);
				minDepth = (int)(stableSkeleton.Joints.Min<Joint>(j => j.Position.Z) * 1000);
			}

			// Convert the depth to RGB
			int colorPixelIndex = 0;
			
			for (int i = 0; i < depthPixels.Length; ++i)
			{
				// Get the depth for this pixel
				short depth = depthPixels[i].Depth;
				// To convert to a byte, we're discarding the most-significant
				// rather than least-significant bits.
				// We're preserving detail, although the intensity will "wrap."
				// Values outside the reliable depth range are mapped to 0 (black).

				// Note: Using conditionals in this loop could degrade performance.
				// Consider using a lookup table instead when writing production code.
				// See the KinectDepthViewer class used by the KinectExplorer sample
				// for a lookup table example.
				byte intensity = (byte)(stableSkeleton != null && depth >= minDepth && depth <= maxDepth ? depth : 255);
				// Write out blue byte
				colorPixels[colorPixelIndex++] = intensity;
				// Write out green byte
				colorPixels[colorPixelIndex++] = intensity;

				// Write out red byte                        
				colorPixels[colorPixelIndex++] = intensity;

				// We're outputting BGR, the last byte in the 32 bits is unused so skip it
				// If we were outputting BGRA, we would write alpha here.
				++colorPixelIndex;
			}
			// Write the pixel data into our bitmap
			colorBitmap.WritePixels(
					new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
					colorPixels,
					colorBitmap.PixelWidth * sizeof(int),
					0);
			return colorBitmap;
		}

		void Player_SkeletonFrameReady(object sender, PlayerSkeletonFrameReadyEventArgs e)
		{
			//We first stabilize it
			Skeleton stableSkeleton = SkeletonF.ProcessNewSkeletonData(e.FrameSkeleton);
			VisitorCtr.DetectUserPosition(stableSkeleton);
			//Process the new skeleton in the Interaction Controller
			InteractionCtr.ProcessNewSkeletonData(stableSkeleton, e.Delay, VisitorCtr.InteractionZone);

			//We paint the skeleton and send the image over to the UI
			if (ColorImageReady != null)
			{
				DrawingImage imageCanvas = DrawImage(null, stableSkeleton);
				ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
			}
		}

		private static void InitializeDrawingImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
		{
			if (colorFrame != null)
			{
				byte[] cbyte = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(cbyte);
				int stride = colorFrame.Width * 4;

				ImageSource imageBackground = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Bgr32, null, cbyte, stride);
				drawingContext.DrawImage(imageBackground, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
			}
			else
			{
				// Draw a transparent background to set the render size
				drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
			}
		}

		//drawing color image and skeleton
		private DrawingImage DrawImage(ColorImageFrame colorFrame, Skeleton skeleton)
		{
			DrawingGroup dgColorImageAndSkeleton = new DrawingGroup();
			DrawingImage drawingImage = new DrawingImage(dgColorImageAndSkeleton);
			skeletonDrawer = new SkeletonDrawer(kinectSensor);
			using (DrawingContext drawingContext = dgColorImageAndSkeleton.Open())
			{
				InitializeDrawingImage(colorFrame, drawingContext);
				skeletonDrawer.DrawSkeleton(skeleton, drawingContext);
			}

			//Make sure the image remains within the defined width and height
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

			return drawingImage;
		}

		private EventHandler playbackHandler;

		public void PlayBack(string fileName, EventHandler pbFinished, bool useDelay)
		{
			if (playbackHandler != null)
				Player.PlaybackFinished -= playbackHandler;
			playbackHandler = pbFinished;
			Player.UseDelay = useDelay;
			Player.PlaybackFinished += playbackHandler;
			Player.Load(fileName);
			Player.Start();

			PlayBackFromFile = true;
		}

		public void Shutdown()
		{
			if (kinectSensor != null)
			{
				kinectSensor.Stop();
				kinectSensor.Dispose();
			}
			Recorder.Stop(false, true);
		}
	}
}
