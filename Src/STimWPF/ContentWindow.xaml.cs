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
		private ContentState contentState;
		private DetailContentState detailContentState;
		
		public ContentState ContentState
		{
			get { return contentState; }
			set
			{
				contentState = value;
				OnPropertyChanged("ContentState");
			}
		}

		public DetailContentState DetailContentState
		{
			get { return detailContentState; }
			set
			{
				detailContentState = value;
				OnPropertyChanged("DetailContentState");
			}
		}

		public App AppInstance { get; set; }

		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public ContentWindow(App appInst)
		{
			AppInstance = appInst;
			ContentState = ContentState.Overview;
			InitializeComponent();
			CoreInstance.InteractionCtr.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(InteractionCtr_PropertyChanged);
			LocationChanged += new EventHandler(ContentWindow_LocationChanged);
			SizeChanged += new SizeChangedEventHandler(ContentWindow_SizeChanged);
		}

		void ContentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				Left = 0;
				Top = 0;
			}
			CoreInstance.InteractionCtr.MouseBoundaries = new Rect(Left, Top, ActualWidth, ActualHeight);
		}

		void ContentWindow_LocationChanged(object sender, EventArgs e)
		{
			CoreInstance.InteractionCtr.MouseBoundaries = new Rect(Left, Top, ActualWidth, ActualHeight);
		}

		private void contentW_Closed(object sender, EventArgs e)
		{
			AppInstance.CloseApp(this);
		}

		void InteractionCtr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}

		private void ContentWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CoreInstance.DepthImageReady += new EventHandler<DepthImageReadyArgs>(CoreInstance_DepthImageReady);
		}

		void CoreInstance_DepthImageReady(object sender, DepthImageReadyArgs e)
		{
			this.iKinectDepth.Source = e.Frame;
		}

		void Overview_Clicked(object sender, EventArgs e)
		{
			ContentState = ContentState.Overview;
		}

		private void Descrption_Clicked(object sender, RoutedEventArgs e)
		{
			ContentState = ContentState.Description;
		}

		private void Detail_Clicked(object sender, RoutedEventArgs e)
		{
			ContentState = ContentState.Detail;
		}

		private void PartA_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartA;
		}

		private void PartB_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartB;
		}

		private void PartC_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartC;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
