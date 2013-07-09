using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using STimWPF.Properties;
using System.Xml;
using System.Reflection;

namespace STimWPF
{

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public ControlWindow MainW { get; set; }
		public ContentWindow ContentW { get; set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();

			Core.Instance.Initialize
			(
					Settings.Default.BlockPercentBufferSize,
					Settings.Default.UploadPeriod
			);

			MainW = new ControlWindow(this);
			ContentW = new ContentWindow(this);
			MainW.Show();
			ContentW.Show();
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
			if (sender != ContentW)
				ContentW.Close();
		}

	}

}
