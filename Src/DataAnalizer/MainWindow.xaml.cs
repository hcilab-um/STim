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
using GenericParsing;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Globalization;

namespace DataAnalizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private const string IMAGE_FOLDER = "Image";
		private const string STATUS_FOLDER = "StatusLogs";
		private const string VISIT_FOLDER = "VisitLogs";

		private const string IMAGE_FILE_COLUMN_NAME = "ImageFileName";

		private DataView visitDataView;

		private DataView statusDataView;

		private Dictionary<string, ImageSource> imageList;

		public DataView VisitDataView
		{
			get { return visitDataView; }
			set
			{
				visitDataView = value;
				OnPropertyChanged("VisitDataView");
			}
		}

		public DataView StatusDataView
		{
			get { return statusDataView; }
			set
			{
				statusDataView = value;
				OnPropertyChanged("StatusDataView");
			}
		}


		public MainWindow()
		{
			VisitDataView = new DataView();
			StatusDataView = new DataView();
			imageList = new Dictionary<string, ImageSource>();
			InitializeComponent();
		}

		private void BrowseFile_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			tbRootPath.Text = fbd.SelectedPath;

			StatusDataView = LoadCSVDataTables(tbRootPath.Text, STATUS_FOLDER);
			VisitDataView = LoadCSVDataTables(tbRootPath.Text, VISIT_FOLDER);

			FileInfo[] imageFiles = LoadFiles(tbRootPath.Text, IMAGE_FOLDER);

			//Load Image
			foreach (FileInfo fi in imageFiles)
			{
				imageList.Add(fi.Name, (new ImageSourceConverter()).ConvertFromString(fi.FullName) as ImageSource);
			}
		}

		private DataView LoadCSVDataTables(String rootPath, String dataFolder)
		{
			FileInfo[] visitFiles = LoadFiles(rootPath, dataFolder);
			DataView dataView = new DataView();
			foreach (FileInfo fi in visitFiles)
			{
				System.Data.DataSet dsResult;
				using (GenericParserAdapter parser = new GenericParserAdapter(fi.FullName))
				{
					parser.Load("VisitStatusFormat.xml");
					dsResult = parser.GetDataSet();
				}
				if (VisitDataView.Count == 0)
					dataView = dsResult.Tables[0].AsDataView();
				else
					dataView.Table.Merge(dsResult.Tables[0]);
			}
			return dataView;
		}

		private static FileInfo[] LoadFiles(String rootPath, String dataFolder)
		{
			String fullPath = string.Format("{0}\\{1}", rootPath, dataFolder);
			DirectoryInfo dataDirectory = new DirectoryInfo(fullPath);
			FileInfo[] visitFiles = dataDirectory.GetFiles("*", SearchOption.AllDirectories);
			visitFiles.OrderBy(files => files.CreationTime);
			return visitFiles;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (dgStatus.SelectedIndex >= StatusDataView.Table.Rows.Count)
				return;

			string imageFileName = StatusDataView.Table.Rows[dgStatus.SelectedIndex][IMAGE_FILE_COLUMN_NAME].ToString();
			
			if (imageList.ContainsKey(imageFileName))
				visitImage.Source = imageList[imageFileName];
			else
				visitImage.Source = null;

			visitDataView.RowFilter = string.Format("DateTime = {0}", StatusDataView.Table.Rows[dgStatus.SelectedIndex]["DateTime"]); 
		}
	}
}
