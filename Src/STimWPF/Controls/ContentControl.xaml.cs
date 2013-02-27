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
using STimWPF.Interaction;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for ContentControl.xaml
	/// </summary>
	public partial class ContentControl : UserControl, INotifyPropertyChanged, IUIInformer
	{
		
		private static readonly DependencyProperty LeftClickProperty = DependencyProperty.Register("LeftClick", typeof(bool), typeof(ContentControl));
		private static readonly DependencyProperty RelativeCursorLocationProperty = DependencyProperty.Register("RelativeCursorLocation", typeof(Point3D), typeof(ContentControl));
		private static readonly DependencyProperty TimerStateProperty = DependencyProperty.Register("TimerState", typeof(TimerState), typeof(ContentControl));
		private MainPage mainPageWF;
		private MainPage mainPageLA;
		private DetailWFPage detailWFPage;
		private DetailLAPage detailLAPage;
		public MainPage MainPageWF
		{
			get { return mainPageWF; }
			set 
			{
				mainPageWF = value;
				OnPropertyChanged("MainPageWF");
			}
		}

		public MainPage MainPageLA
		{
			get { return mainPageLA; }
			set
			{
				mainPageLA = value;
				OnPropertyChanged("MainPageLA");
			}
		}

		public DetailLAPage DetailLAPage
		{
			get { return detailLAPage; }
			set
			{
				detailLAPage = value;
				OnPropertyChanged("DetailLAPage");
			}
		}

		public DetailWFPage DetailWFPage
		{
			get { return detailWFPage; }
			set
			{
				detailWFPage = value;
				OnPropertyChanged("DetailWFPage");
			}
		}

		public bool	LeftClick
		{
			get { return (bool)GetValue(LeftClickProperty); }
			set { SetValue(LeftClickProperty, value); }
		}

		public Point3D RelativeCursorLocation
		{
			get { return (Point3D)GetValue(RelativeCursorLocationProperty); }
			set { SetValue(RelativeCursorLocationProperty, value); }
		}

		public TimerState TimerState
		{
			get { return (TimerState)GetValue(TimerStateProperty); }
			set { SetValue(TimerStateProperty, value); }
		}

		public ContentControl()
		{
			InitializeComponent();
		}

		void onMainPageClick(object sender, EventArgs e)
		{
			Button bt = (Button)sender;
			string[] nameInfo = bt.Name.Split('_');
			foreach (MainPage m in Enum.GetValues(typeof(MainPage)))
			{
				if (m.ToString().Equals(nameInfo[0]))
				{
					if(nameInfo[1].Equals("WF"))
						MainPageWF = m;
					else if(nameInfo[1].Equals("LA"))
						MainPageLA = m;
					else
						throw new Exception("Invalid Name: "+nameInfo[1]);
				}
			}

			if (MainPageWF == MainPage.Detail)
			{
				if (nameInfo[1].Equals("WF"))
					DetailWFPage = DetailWFPage.DetailMenu_WF;
				else
					DetailLAPage = DetailLAPage.DetailMenu_LA;
			}
		}

		void onWFDetailClick(object sender, EventArgs e)
		{
			Button bt = (Button)sender;
			foreach (DetailWFPage d in Enum.GetValues(typeof(DetailWFPage)))
			{
				if (d.ToString().Equals(bt.Name))
					DetailWFPage = d;
			}
		}

		void onLADetailClick(object sender, EventArgs e)
		{
			Button bt = (Button)sender;
			foreach (DetailLAPage d in Enum.GetValues(typeof(DetailLAPage)))
			{
				if (d.ToString().Equals(bt.Name))
					DetailLAPage = d;
			}
		}

		void onClickDetailMenu(object sender, EventArgs e)
		{
			Button bt = (Button)sender;
			string[] nameInfo = bt.Name.Split('_');
			if(nameInfo[1].Equals("WF"))
				DetailWFPage = DetailWFPage.DetailMenu_WF;
			else
				DetailLAPage = DetailLAPage.DetailMenu_LA;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public string CurrentPage()
		{
			return MainPageLA.ToString() + "-" + Detail_LA.ToString() + "|" + MainPageWF.ToString() + "-" + Detail_WF.ToString();
		}
	}
}
