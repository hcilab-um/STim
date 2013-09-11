using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STim
{
	public class STimSettings
	{
		public static double CloseZoneConstrain { get; set; }
		public static double InteractionZoneConstrain { get; set; }
		public static double NotificationZoneConstrain { get; set; }

		public static int BlockPercentBufferSize { get; set; }
		public static int UploadPeriod { get; set; }

		public static string ImageFolder { get; set; }

		public static string DateTimeFileNameFormat { get; set; }
		public static string DateTimeLogFormat { get; set; }

		public static double DisplayWidthInMeters { get; set; }
		public static double DisplayHeightInMeters { get; set; }
		
		public static double BlockDepthPercent { get; set; }

		public static int ScreenGridRows { get; set; }
		public static int ScreenGridColumns { get; set; }

		public static bool IncludeStatusRender { get; set; }
		public static string CalibrationFile { get; set; }

	}
}
