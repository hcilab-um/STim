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
using System.Globalization;
using STim.Util;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace STim
{
  public class Core : INotifyPropertyChanged
  {

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

    private KinectSensor KinectSensor { get; set; }
    public bool IsInitialized { get; set; }
    private DepthPercentFilter DepthPercentF { get; set; }
    private VisitorController VisitorCtr { get; set; }
    public CalibrationControl Calibration { get; set; }
    public StatusController StatusCtr { get; set; }

    private Dictionary<int, WagSkeleton> currentVisitors = new Dictionary<int, WagSkeleton>();
    private List<WagFaceTracker> wagFaceTrackers = new List<WagFaceTracker>(Constants.FACE_TRACKER_CAPACITY);
    private List<VisitStatus> visitStatus = new List<VisitStatus>();

    private AttentionEstimatorSimple attentionerSimple = new AttentionEstimatorSimple();
    private AttentionEstimatorSocial attentionerSocial = new AttentionEstimatorSocial();

    private int currentFrame = 0;
    private SkeletonDrawer skeletonDrawer;

    private byte[] colorImage;
    private byte[] lastColorImage = new byte[0];
    private short[] depthImage;

    public event EventHandler<ColorImageReadyArgs> ColorImageReady;

    public WagSkeleton ClosestVisitor
    {
      get { return currentVisitors.Values.OrderBy<WagSkeleton, double>(skeleton => skeleton.TransformedPosition.Z).FirstOrDefault(); }
      set { } //this has to be there for binding purposes
    }

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

    private Point3D userHeadLocation;
    public Point3D UserHeadLocation
    {
      get { return userHeadLocation; }
      set
      {
        userHeadLocation = value;
        OnPropertyChanged("UserHeadLocation");
      }
    }

    private Core()
    {
      IsInitialized = false;
    }

    public void Initialize(Dispatcher uiDispatcher, log4net.ILog visitLogger, log4net.ILog statusLogger)
    {
      VisitorCtr = new VisitorController();
      StatusCtr = new StatusController(uiDispatcher, STimSettings.UploadPeriod, visitLogger, statusLogger) { VisitorContr = VisitorCtr };

      Calibration = new CalibrationControl(STimSettings.CalibrationFile);
      DepthPercentF = new DepthPercentFilter(STimSettings.BlockPercentBufferSize);

      //Gets the first head location
      UserHeadLocation = Calibration.UserHeadLocation;

      if (KinectSensor.KinectSensors.Count == 0)
      {
        throw new Exception("No Kinect found");
      }
      else
      {
        KinectSensor = KinectSensor.KinectSensors[0];
        if (KinectSensor == null || KinectSensor.Status == KinectStatus.NotPowered)
        {
          throw new Exception("Kinect Not Powered");
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

          for (int i = 0; i < Constants.FACE_TRACKER_CAPACITY; i++)
            wagFaceTrackers.Add(new WagFaceTracker(KinectSensor));
        }
      }

      IsInitialized = true;
    }

    public void Shutdown()
    {
      Calibration.Close();

      if (KinectSensor != null)
      {
        KinectSensor.Stop();
        KinectSensor.Dispose();
      }

      foreach (WagFaceTracker tracker in wagFaceTrackers)
        tracker.FaceTracker.Dispose();
      wagFaceTrackers.Clear();

      IsInitialized = false;
      StatusCtr.Stop();
    }

    private void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
    {
      currentFrame++;

      using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
      {
        if (depthFrame != null)
        {
          VisitorCtr.ClosePercent = CalculateBlockPercentage(depthFrame);
          depthImage = new short[depthFrame.PixelDataLength];
          depthFrame.CopyPixelDataTo(depthImage);
        }
      }

      DrawingImage imageCanvas = null;
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

      Skeleton[] rawSkeletons = new Skeleton[0];
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

          skeleton.InPeriphery = (skeleton.BodyOrientationAngle <= Constants.PERIPHERY_MAX_ANGLE);

          skeleton.AttentionSimple = attentionerSimple.CalculateAttention(skeleton);

          skeleton.AttentionSocial = attentionerSocial.CalculateAttention(skeleton, this.currentVisitors.Values.ToArray());
          WagFaceTracker wagTracker = wagFaceTrackers.Single(trk => trk.SkeletonId == skeleton.TrackingId);

          skeleton.FaceFrame = wagTracker.FaceTracker.Track(KinectSensor.ColorStream.Format, colorImage, KinectSensor.DepthStream.Format, depthImage, skeleton);

          if (skeleton.FaceFrame.TrackSuccessful)
          {
            skeleton.HeadOrientation = CalculateHeadOrientation(skeleton);
          }
        }

        if (Calibration.IsCalibrated && ClosestVisitor != null)
          UserHeadLocation = ClosestVisitor.HeadLocation;
      }

      StatusCtr.LoadNewSkeletonData(currentVisitors.Values.ToList(), ClosestVisitor, GetImageAsArray(imageCanvas, (currentFrame % 32) == 0));
      CleanOldSkeletons();
    }

    private byte[] GetImageAsArray(DrawingImage imageCanvas, bool performConvertion)
    {
      if (!performConvertion || currentVisitors.Count == 0 || imageCanvas == null)
        return lastColorImage;

      DrawingVisual drawingVisual = new DrawingVisual();
      DrawingContext drawingContext = drawingVisual.RenderOpen();
      drawingContext.DrawImage(imageCanvas, new System.Windows.Rect(0, 0, Constants.RENDER_WIDTH, Constants.RENDER_HEIGHT));
      drawingContext.Close();

      RenderTargetBitmap bmp = new RenderTargetBitmap((int)Constants.RENDER_WIDTH, (int)Constants.RENDER_HEIGHT, 96, 96, PixelFormats.Pbgra32);
      bmp.Render(drawingVisual);
      JpegBitmapEncoder encoder = new JpegBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(bmp));

      MemoryStream memory = new MemoryStream();
      encoder.Save(memory);
      lastColorImage = memory.ToArray();
      return lastColorImage;
    }

    private double CalculateBlockPercentage(DepthImageFrame depthFrame)
    {
      DepthImagePixel[] depthPixels;

      depthPixels = new DepthImagePixel[KinectSensor.DepthStream.FramePixelDataLength];
      depthFrame.CopyDepthImagePixelDataTo(depthPixels);
      int closePixel = 0;
      short constrain = (short)(STimSettings.CloseZoneConstrain);
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
            if (skeleton.FramesNotSeen > 0 && !(VisitorCtr.IsBlocked && skeleton.TrackingId == ClosestVisitor.TrackingId))
              continue;

            skeletonDrawer.DrawUpperSkeleton(skeleton, drawingContext);

            Joint head = skeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.Head);
            System.Windows.Point headP = skeletonDrawer.SkeletonPointToScreen(head.Position);
            headP.X -= 30; //These two hardcoded numbers are the displacements in X,Y of the white box content holder
            headP.Y -= 25;
            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.White, 1), new System.Windows.Rect(headP, new Size(100, 100)));

            //FormattedText
            drawingContext.DrawText(
                new FormattedText(
                  String.Format("ID: {0}", skeleton.TrackingId),
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight, new Typeface("Verdana"), 15, System.Windows.Media.Brushes.Red),
                headP);

            if (STimSettings.IncludeStatusRender)
            {
              System.Windows.Point socialDataPos = headP;
              socialDataPos.Y = headP.Y + 25;
              drawingContext.DrawText(
                  new FormattedText(
                    skeleton.AttentionSocial.ToString(),
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface("Verdana"), 15, System.Windows.Media.Brushes.Green),
                  socialDataPos);

              System.Windows.Point simpleDataPos = headP;
              simpleDataPos.Y = socialDataPos.Y + 30;
              drawingContext.DrawText(
                  new FormattedText(
                    skeleton.AttentionSimple.ToString(),
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface("Verdana"), 15, System.Windows.Media.Brushes.Green),
                  simpleDataPos);
            }
          }
        }
      }

      //Make sure the image remains within the defined width and height
      dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, Constants.RENDER_WIDTH, Constants.RENDER_HEIGHT));
      return drawingImage;
    }

    private static void InitializeDrawingImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
    {
      if (colorFrame != null)
      {
        byte[] cbyte = new byte[colorFrame.PixelDataLength];
        colorFrame.CopyPixelDataTo(cbyte);
        int stride = colorFrame.Width * 4;

        ImageSource imageBackground = BitmapSource.Create(
          (int)Constants.RENDER_WIDTH, 
          (int)Constants.RENDER_HEIGHT, 
          (int)Constants.PPI, (int)Constants.PPI,
          PixelFormats.Bgr32, null, cbyte, stride);
        drawingContext.DrawImage(imageBackground, new System.Windows.Rect(0.0, 0.0, Constants.RENDER_WIDTH, Constants.RENDER_HEIGHT));
      }
      else
      {
        // Draw a transparent background to set the render size
        drawingContext.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(0.0, 0.0, Constants.RENDER_WIDTH, Constants.RENDER_HEIGHT));
      }
    }

    private void ExtractValidSkeletons(Skeleton[] rawSkeletons)
    {
      var validSkeletons = rawSkeletons.Where(
        skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked &&
        skeleton.Joints.Count(joint => joint.TrackingState == JointTrackingState.Tracked) > Constants.MINIMUM_JOINT_THRESHOLD
      );

      foreach (Skeleton skeleton in validSkeletons)
      {
        if (currentVisitors.ContainsKey(skeleton.TrackingId))
          currentVisitors[skeleton.TrackingId].Update(skeleton);
        else
          currentVisitors.Add(skeleton.TrackingId, new WagSkeleton(skeleton));

        WagFaceTracker wagFaceTracker = wagFaceTrackers.SingleOrDefault(tracker => tracker.SkeletonId == skeleton.TrackingId);
        if (wagFaceTracker == null)
        {
          wagFaceTracker = wagFaceTrackers.FirstOrDefault(tracker => tracker.IsUsing == false);
          wagFaceTracker.SkeletonId = skeleton.TrackingId;
          wagFaceTracker.IsUsing = true;
        }

        currentVisitors[skeleton.TrackingId].LastFrameSeen = currentFrame;

        Calibration.ApplyTransformations(currentVisitors[skeleton.TrackingId]);
      }

      foreach (WagSkeleton wagSkeleton in currentVisitors.Values)
      {
        wagSkeleton.FramesNotSeen = currentFrame - wagSkeleton.LastFrameSeen;
      }

      OnPropertyChanged("ClosestVisitor");
    }

    private double CalculateBodyOrientationAngle(WagSkeleton userSkeleton)
    {
      Microsoft.Kinect.Joint shoulderLeft = userSkeleton.TransformedJoints[JointType.ShoulderLeft];
      Microsoft.Kinect.Joint shoulderRight = userSkeleton.TransformedJoints[JointType.ShoulderRight];

      Vector shoulderVector = new Vector(shoulderRight.Position.X - shoulderLeft.Position.X, shoulderRight.Position.Z - shoulderLeft.Position.Z);
      System.Windows.Media.Matrix matrix = new System.Windows.Media.Matrix();
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

      Vector3DF eyeLeft = FacePoints[Constants.LEFT_EYE];
      Vector3DF eyeRight = FacePoints[Constants.RIGHT_EYE];
      Vector3DF faceTop = FacePoints[Constants.FACE_TOP];
      Vector3DF faceBottom = FacePoints[Constants.FACE_BOTTOM];

      Vector3D faceVectorHorizontal = new Vector3D(eyeLeft.X - eyeRight.X, eyeLeft.Y - eyeRight.Y, eyeLeft.Z - eyeRight.Z);
      Vector3D faceVectorVertical = new Vector3D(faceTop.X - faceBottom.X, faceTop.Y - faceBottom.Y, faceTop.Z - faceBottom.Z);

      headOrientation = Vector3D.CrossProduct(faceVectorHorizontal, faceVectorVertical);
      headOrientation = Calibration.CalibrateTransform.Transform(headOrientation);
      headOrientation.Normalize();
      Matrix3D headPointsPointUpMatrix = new Matrix3D();
      headPointsPointUpMatrix.RotateAt(new System.Windows.Media.Media3D.Quaternion(new Vector3D(int.MaxValue, 0, 0), -20), skeleton.TransformedJoints[JointType.Head].Position.ToPoint3D());
      Vector3D lowered = headPointsPointUpMatrix.Transform(headOrientation);

      return lowered;
    }

    private void CleanOldSkeletons()
    {
      var oldSkeletons = currentVisitors.Values.Where(skeleton => skeleton.FramesNotSeen >= Constants.FACE_DELAY_THRESHOLD).ToArray();

      foreach (WagSkeleton skeleton in oldSkeletons)
      {
        if (VisitorCtr.IsBlocked && ClosestVisitor.TrackingId == skeleton.TrackingId)
          continue;
        currentVisitors.Remove(skeleton.TrackingId);
        WagFaceTracker wagFaceTracker = wagFaceTrackers.SingleOrDefault(wagTracker => wagTracker.SkeletonId == skeleton.TrackingId);
        wagFaceTracker.IsUsing = false;
        wagFaceTracker.SkeletonId = -1;
      }

      System.GC.Collect();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }
}