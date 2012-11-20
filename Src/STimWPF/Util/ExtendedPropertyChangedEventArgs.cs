using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace STimWPF.Util
{

	public class ExtendedPropertyChangedEventArgs : PropertyChangedEventArgs
	{
		public object Value { get; set; }

		public ExtendedPropertyChangedEventArgs(String name, object value)
			: base(name)
		{
			Value = value;
		}
	}

}
