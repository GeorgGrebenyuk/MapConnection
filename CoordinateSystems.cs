using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using OSGeo.MapGuide;
using Autodesk.Gis.Map.Platform;
using Autodesk.AutoCAD.Windows;
using Autodesk.DesignScript.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;

namespace MapConnection
{
	public class CoordinateSystems
	{
		public static string GetCurrentCoordinateSystem()
		{
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();

			OSGeo.MapGuide.MgCoordinateSystemFactory factory = new OSGeo.MapGuide.MgCoordinateSystemFactory();
			string csCode = factory.ConvertWktToCoordinateSystemCode(wkt);

			return csCode;
		}
		[MultiReturn(new[] { "csCode", "csCode_WKT" })]
		public static Dictionary<string, object> GetCoordinateSystem2()
		{
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();

			OSGeo.MapGuide.MgCoordinateSystemFactory factory = new OSGeo.MapGuide.MgCoordinateSystemFactory();
			string csCode = factory.ConvertWktToCoordinateSystemCode(wkt);

			return new Dictionary<string, object>
			{
				{"csCode", csCode},
				{"csCode_WKT", wkt},
			};
		}

		[MultiReturn(new[] { "Console", "Full_WKT", "PROJ_4" })]
		public static Dictionary<string, object> GetFullWKTCodeOfCS(string Current_CS, string XML_MapCSLibrary_path)
		{
			string console = null;
			string Full_WKT = null;
			string PROJ_4 = null;
			//Получение строки урезанного WKT (где нет параметров датума)
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();

			//Получение файла по данному пути
			if (System.IO.File.Exists(XML_MapCSLibrary_path))
			{

			}
			else console = "File don't exists";

				string CourceDatum = "RUSSIA-REF-SK95-2017";
			string TargetDatum = "WGS84";



			return new Dictionary<string, object>
			{
				{"Console", console},
				{"Full_WKT", wkt},
			};

		}
		
	}
}
