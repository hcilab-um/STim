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
using System.ComponentModel;
using Microsoft.Win32;
using STimWPF.Properties;
using System.IO;
using Microsoft.Kinect;
using System.Windows.Threading;
using STimWPF.Interaction;

namespace STimWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ControlWindow : Window, INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		private App appInstance = null;
		private Dispatcher uiDispatcher = null;

		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public ControlWindow(App appInst)
		{
			InitializeComponent();
			appInstance = appInst;
			uiDispatcher = Dispatcher;
		}

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void ControlWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CoreInstance.ColorImageReady += new EventHandler<ColorImageReadyArgs>(CoreInstance_ColorImageReady);
		}

		private void ControlWindow_Closed(object sender, EventArgs e)
		{
			appInstance.CloseApp(this);
		}

		void CoreInstance_ColorImageReady(object sender, ColorImageReadyArgs e)
		{
			ImageSource colorFrame = e.Frame;
			if (colorFrame == null)
				return;

			iSource.Source = colorFrame;
		}
	}
}
