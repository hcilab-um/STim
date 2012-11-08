using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StatusConsole.Properties;

namespace StatusConsole
{
	public class AppStatus: INotifyPropertyChanged
	{
		DateTime _lastUpdate;
		bool _runningOK;
		string _message;

		public DateTime LastUpdate
		{
			get { return _lastUpdate; }
			set 
			{
				_lastUpdate = value;
				OnPropertyChanged("LastUpdate");
			}
		}

		public Boolean RunningOK
		{
			get { return _runningOK; }
			set
			{
				_runningOK = value;
				OnPropertyChanged("RunningOK");
			}
		}

		public string Message
		{
			get { return _message; }
			set
			{
				_message = value;
				OnPropertyChanged("Message");
			}
		}

		public AppStatus()
		{
			_lastUpdate = new DateTime(2012, 11, 06);
			_runningOK = true;
			_message = "";
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

}
