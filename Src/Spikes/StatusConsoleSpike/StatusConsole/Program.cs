using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using StatusConsole.Properties;
using Jayrock.Json.Conversion;
using Jayrock.Json;
using System.IO;
namespace StatusConsole
{

	class Program
	{

		public static readonly Uri AppStatus_URI = new Uri(Settings.Default.AppUriString + "/appStatus/save.json");

		static void Main(string[] args)
		{
			//need a string builder
			ExportContext exportCtx = new ExportContext();
			exportCtx.Register(new GrailsDateTimeExporter());
	
			WebClient client = new WebClient();
			AppStatus status;
			string command;
			while ((command = Console.ReadLine()) != "q")
			{
				status = new AppStatus() { LastUpdate = DateTime.Now, Message = "Hello .NET", RunningOK = true };

				StringBuilder jsonString = new StringBuilder();
				StringWriter writer = new StringWriter(jsonString);
				exportCtx.Export(status, new JsonTextWriter(writer));

				byte[] data = Encoding.ASCII.GetBytes(jsonString.ToString());				
				client.UploadData(AppStatus_URI, data);
				Console.WriteLine(">>>>>Response Received>>>>>>");
			}
		}
	}
}
