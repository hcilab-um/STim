using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STimWPF.Status
{
	class GrailsDateTimeExporter : Jayrock.Json.Conversion.IExporter
	{
		public void Export(Jayrock.Json.Conversion.ExportContext context, object value, Jayrock.Json.JsonWriter writer)
		{
			DateTime target = (DateTime)value;
			writer.WriteString(target.ToString("yyyy-MM-ddThh:mm:ss" + DateTime.UtcNow.ToString("%K")));
		}

		public Type InputType
		{
			get { return typeof(DateTime); }
		}
	}
}
