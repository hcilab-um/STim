using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Serialization;

namespace STim.Util
{
  public class CalibrationControl
  {
    private readonly Point3D[] standardCalibrationPositions = new Point3D[] 
		{
			new Point3D(0, 0.5, 1.5), //top
			new Point3D(0.5, 0.5, 1.5), //right
			new Point3D(0, 0.5, 2), //front
			new Point3D(-0.5, 0.25, 1.5) //back
		};

    private Point3D[] captureCalibrationPositions;

    public Matrix3D CalibrateTransform { get; set; }

    public bool IsCalibrated { get; set; }

    private int CalibrationHeadIndex { get; set; }

    private OriginFinder OriginF { get; set; }

    private String FileName { get; set; }

    public Point3D UserHeadLocation
    {
      get
      {
        if (CalibrationHeadIndex >= 0 && CalibrationHeadIndex < standardCalibrationPositions.Length)
          return standardCalibrationPositions[CalibrationHeadIndex];
        return new Point3D();
      }
    }

    public CalibrationControl(String fileName)
    {
      IsCalibrated = true;
      CalibrationHeadIndex = 0;
      captureCalibrationPositions = new Point3D[standardCalibrationPositions.Length];
      CalibrateTransform = new Matrix3D();

      Load(fileName);
    }

    public void Load(String fileName)
    {
      if (!File.Exists(fileName))
      {
        CalibrateTransform = Matrix3D.Identity;
      }
      else
      {
        XmlRootAttribute xRoot = new XmlRootAttribute();
        xRoot.ElementName = typeof(Matrix3D).Name;
        XmlSerializer serializer = new XmlSerializer(typeof(Matrix3D), xRoot);
        XmlReader reader = XmlReader.Create(fileName);
        CalibrateTransform = (Matrix3D)serializer.Deserialize(reader);
      }
      FileName = fileName;
    }

    public void ResetCalibration()
    {
      IsCalibrated = false;
      CalibrationHeadIndex = 0;
    }

    public void Calibrate(WagSkeleton skeleton)
    {
      if (skeleton == null || skeleton.Joints[JointType.Head].TrackingState != JointTrackingState.Tracked)
        throw new Exception("Can Not Detect Head Position!");

      captureCalibrationPositions[CalibrationHeadIndex] = skeleton.Joints[JointType.Head].Position.ToPoint3D();

      if (++CalibrationHeadIndex >= standardCalibrationPositions.Length)
      {
        CalibrationHeadIndex = 0;

        Point3D estimatedOrigin = OriginF.BruteForceEstimateOrigin(standardCalibrationPositions, captureCalibrationPositions, Constants.KINECT_DETECT_RANGE);

        CalCulateTransformationMatrix(estimatedOrigin);

        IsCalibrated = true;
      }
    }

    private void CalCulateTransformationMatrix(Point3D estimateOrigin)
    {
      CalibrateTransform = Matrix3D.Identity;
      for (int i = 0; i < captureCalibrationPositions.Length; i++)
      {
        Vector3D vector = new Vector3D(captureCalibrationPositions[i].X - estimateOrigin.X, captureCalibrationPositions[i].Y - estimateOrigin.Y, captureCalibrationPositions[i].Z - estimateOrigin.Z);
        vector.Normalize();
        vector *= standardCalibrationPositions[i].ToVector3D().Length;
        captureCalibrationPositions[i] = new Point3D(estimateOrigin.X + vector.X, estimateOrigin.Y + vector.Y, estimateOrigin.Z + vector.Z);
      }

      Vector3D captureAxisX = new Vector3D()
      {
        X = captureCalibrationPositions[1].X - captureCalibrationPositions[0].X,
        Y = captureCalibrationPositions[1].Y - captureCalibrationPositions[0].Y,
        Z = captureCalibrationPositions[1].Z - captureCalibrationPositions[0].Z
      };

      Vector3D captureAxisZ = new Vector3D()
      {
        X = captureCalibrationPositions[0].X - captureCalibrationPositions[2].X,
        Y = captureCalibrationPositions[0].Y - captureCalibrationPositions[2].Y,
        Z = captureCalibrationPositions[0].Z - captureCalibrationPositions[2].Z
      };

      Vector3D captureAxisY = Vector3D.CrossProduct(captureAxisX, captureAxisZ);

      CalibrateTransform = Microsoft.Xna.Framework.Matrix.CreateWorld(estimateOrigin.ToVector3D().ToVector3(), captureAxisZ.ToVector3(), captureAxisY.ToVector3()).ToMatrix3D();

      CalibrateTransform.Invert();
    }

    public void Close()
    {
      using (StreamWriter streamWriter = new StreamWriter(FileName))
      {
        XmlSerializer serializer = new XmlSerializer(CalibrateTransform.GetType());
        serializer.Serialize(streamWriter, CalibrateTransform);
      }
    }

    public void ApplyTransformations(WagSkeleton skeleton)
    {

      skeleton.TransformedPosition = CalibrateTransform.Transform(new Point3D()
      {
        X = skeleton.Position.X,
        Y = skeleton.Position.Y,
        Z = skeleton.Position.Z
      });

      foreach (JointType type in Enum.GetValues(typeof(JointType)))
      {
        Point3D transformpoint = CalibrateTransform.Transform(new Point3D()
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

  }

}
