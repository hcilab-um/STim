using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _3DTools;
using System.Windows.Media.Media3D;
using STimWPF.Properties;
using System.ComponentModel;

namespace STimWPF.AttentionControls
{
	/// <summary>
	/// Interaction logic for SpatialAlignControl.xaml
	/// </summary>
	public partial class SpatialAlignControl : UserControl, INotifyPropertyChanged
	{
		private static readonly Point3D LeftTopFront = new Point3D(-Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, 0);
		private static readonly Point3D RightTopFront = new Point3D(Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, 0);
		private static readonly Point3D LeftBottomFront = new Point3D(-Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, -Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, 0);
		private static readonly Point3D RightBottomFront = new Point3D(Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, -Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, 0);

		private static readonly Point3D LeftTopBack = new Point3D(-Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, -Settings.Default.DisplayDepthtInMeters);
		private static readonly Point3D RightTopBack = new Point3D(Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, -Settings.Default.DisplayDepthtInMeters);
		private static readonly Point3D LeftBottomBack = new Point3D(-Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, -Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, -Settings.Default.DisplayDepthtInMeters);
		private static readonly Point3D RightBottomBack = new Point3D(Settings.Default.DisplayWidthInMeters / 2 - Settings.Default.CenterOffsetX, -Settings.Default.DisplayHeightInMeters / 2 - Settings.Default.CenterOffsetY, -Settings.Default.DisplayDepthtInMeters);

		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(SpatialAlignControl));
		public static readonly DependencyProperty HeadLocationProperty = DependencyProperty.Register("HeadLocation", typeof(Point3D), typeof(SpatialAlignControl));

		public Point3D ObjectPosition { get; set; }
		
		public double ObjectDiameter { get; set; }

		private Point3D calibrateHeadLocation;

		private int calibrateStepIndex = 0;

		private readonly Point3D[] calibrationHeadPositions = new Point3D[] { new Point3D(-Settings.Default.DisplayWidthInMeters / 2, 0.5, 1),  new Point3D(0, 0.5, 1),  new Point3D(Settings.Default.DisplayWidthInMeters / 2, 0.5, 1),
 																																					new Point3D(-Settings.Default.DisplayWidthInMeters / 2, 0.25, 1), new Point3D(0, 0.25, 1), new Point3D(Settings.Default.DisplayWidthInMeters / 2, 0.25, 1),
																																					new Point3D(-Settings.Default.DisplayWidthInMeters / 2, 0, 1),    new Point3D(0, 0, 1),    new Point3D(Settings.Default.DisplayWidthInMeters / 2, 0, 1),};

		public double Distance
		{
			get { return (double)GetValue(DistanceProperty); }
			set { SetValue(DistanceProperty, value); }
		}

		public Point3D HeadLocation
		{
			get { return (Point3D)GetValue(HeadLocationProperty); }
			set { SetValue(HeadLocationProperty, value); }
		}

		public Point3D CalibrateHeadLocation
		{
			get { return calibrateHeadLocation; }
			set
			{
				calibrateHeadLocation = value;
				OnPropertyChanged("CalibrateHeadLocation");
			}
		}

		public SpatialAlignControl()
		{
			ObjectPosition = new Point3D(-Settings.Default.CenterOffsetX, -Settings.Default.CenterOffsetY, -Settings.Default.DisplayDepthtInMeters / 2);
			ObjectDiameter = 0.011;
			CalibrateHeadLocation = calibrationHeadPositions[calibrateStepIndex];
			HeadLocation = calibrationHeadPositions[calibrateStepIndex];
			InitializeComponent();
			drawBox();
		}

		private void drawBox()
		{

			ScreenSpaceLines3D lineCollection = new ScreenSpaceLines3D();
			int width = 2;
			lineCollection.Thickness = width;
			lineCollection.Color = Colors.Red;

			DrawLine(lineCollection, LeftTopBack, RightTopBack);
			DrawLine(lineCollection, LeftBottomBack, RightBottomBack);
			DrawLine(lineCollection, LeftTopBack, LeftBottomBack);
			DrawLine(lineCollection, RightTopBack, RightBottomBack);

			DrawLine(lineCollection, LeftTopBack, LeftTopFront);
			DrawLine(lineCollection, RightTopBack, RightTopFront);
			DrawLine(lineCollection, LeftBottomBack, LeftBottomFront);
			DrawLine(lineCollection, RightBottomBack, RightBottomFront);
			
			viewport.Children.Add(lineCollection);

		}

		private void DrawLine(ScreenSpaceLines3D lineCollection, Point3D start, Point3D end)
		{
			lineCollection.Points.Add(start);
			lineCollection.Points.Add(end);
		}

		private void CenterCalibration_Click(object sender, RoutedEventArgs e)
		{
			if (++calibrateStepIndex >= calibrationHeadPositions.Length)
			{
				calibrateStepIndex = 0;
				MessageBox.Show("CalibrationFinished");
			}	
			CalibrateHeadLocation = calibrationHeadPositions[calibrateStepIndex];
			HeadLocation = calibrationHeadPositions[calibrateStepIndex];
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
