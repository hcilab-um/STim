using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using STimWPF.Properties;
using System.Xml;
using System.Reflection;
using STim;

namespace STimWPF
{

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public ControlWindow MainW { get; set; }
		public ContentWindow contentW { get; set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();

			Core.Instance.Initialize
			(
				Dispatcher,
				Settings.Default.CloseZoneConstrain,
				Settings.Default.NotificationZoneConstrain,
				Settings.Default.BlockPercentBufferSize,
				Settings.Default.BlockDepthPercent,
				Settings.Default.UploadPeriod,
				Settings.Default.ImageFolder,
				Settings.Default.DateTimeFileNameFormat,
				Settings.Default.DateTimeLogFormat,
				Settings.Default.DisplayWidthInMeters,
				Settings.Default.DisplayHeightInMeters,
				Settings.Default.KinectDistanceZ,
				Settings.Default.KinectDistanceY,
				log4net.LogManager.GetLogger("VisitLogger"),
				log4net.LogManager.GetLogger("StatusLogger")
			);

			MainW = new ControlWindow(this);
			contentW = new ContentWindow(this);
			MainW.Show();
			contentW.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Core.Instance.Shutdown();
		}

		public void CloseApp(Window sender)
		{
			if (sender != MainW)
				MainW.Close();
			if (sender != contentW)
				contentW.Close();
		}

	}

}
