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
using System.ComponentModel;
using Microsoft.Kinect;
using STimWPF.Interaction;

namespace STimWPF
{
	/// <summary>
	/// Interaction logic for TextEntryWindow.xaml
	/// </summary>
	public partial class ContentWindow : Window, INotifyPropertyChanged
	{

		public App AppInstance { get; set; }

		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public ContentWindow(App appInst)
		{
			AppInstance = appInst;
			InitializeComponent();
		}

		private void contentW_Loaded(object sender, RoutedEventArgs e)
		{
			Core.Instance.StatusCtr.DisplayWidth = ActualWidth;
			Core.Instance.StatusCtr.DisplayHeight = ActualHeight;
		}

		private void contentW_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Core.Instance.StatusCtr.DisplayWidth = ActualWidth;
			Core.Instance.StatusCtr.DisplayHeight = ActualHeight;
		}

		private void contentW_Closed(object sender, EventArgs e)
		{
			AppInstance.CloseApp(this);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
