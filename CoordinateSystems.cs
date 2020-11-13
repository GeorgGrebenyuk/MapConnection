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
using System.Xml;
using System.Xml.Linq;

namespace MapConnection
{
	public class CoordinateSystems
	{
		/// <summary>
		/// Node GetCurrentCoordinateSystem return the name of current CS (assigned to drawing)
		/// </summary>
		/// <returns></returns>
		public static string GetCurrentCoordinateSystem()
		{
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();
			OSGeo.MapGuide.MgCoordinateSystemFactory factory = new OSGeo.MapGuide.MgCoordinateSystemFactory();
			string csCode = factory.ConvertWktToCoordinateSystemCode(wkt);

			return csCode;
		}
		/// <summary>
		/// Node GetFullWKTCodeOfCS return full WKT code of input CS's name (include geodetic datum)
		/// </summary>
		/// <param name="Current_CS">Name of current/needing CS</param>
		/// <param name="XML_MapCSLibrary_path">Absolute Path to XML-Definition of MapCSLibrary</param>
		/// <returns>Full-WKT code of CS and "Console" - text string with exeptions (if code doesn't finished)</returns>
		[MultiReturn(new[] { "Console", "Full_WKT", "Part_WKT"})]
		public static Dictionary<string, object> GetFullWKTCodeOfCS(string Current_CS, string XML_MapCSLibrary_path)
		{
			string console = null;
			string Full_WKT = null;

			//Получение строки урезанного WKT (где нет параметров датума)
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();

			//Определяем референц-эллипсоиды
			string SourceDatum = null;
			string TargetDatum = null;
			//Вытаскиваем определение начального реф-элл
			int i3 = wkt.IndexOf("DATUM") + 7; //Начало строки наименования реф-эллипсоида
			int i4 = wkt.Substring(i3).IndexOf(",") - 1; //Конец наименования реф-элл исключая кавычку
			SourceDatum = wkt.Substring(i3, i4);
			TargetDatum = "WGS84";
			//Формируем выражение WKT без нулевого определения датума (строки ДО и ПОСЛЕ)
			int i1 = wkt.IndexOf("TOWGS84") + 8; //Кончается на квадратной скобке определения датума
			int i2 = wkt.Substring(i1).IndexOf("]"); //На квадратной скобке конца определения датума начинается прочая неизменяемая часть кода WKT

			string WKT1 = wkt.Substring(0, i1);
			string WKT2 = wkt.Substring(i1 + i2, wkt.Length - (i1 + i2));


			List<string> datum_par = new List<string>();
			//Проверка на наличие файла и дальнейшая работа с XML
			if (System.IO.File.Exists(XML_MapCSLibrary_path))
			{
				XDocument CS_Lib = XDocument.Load(XML_MapCSLibrary_path);
				XNamespace xmlns = "http://www.osgeo.org/mapguide/coordinatesystem";

				var query = CS_Lib.Descendants().Where(m => m.Name.LocalName == "Transformation");

				var i = 0;
				foreach (var q in query)
				{
					var SourceDatumId = q.Element(xmlns + "SourceDatumId").Value;
					var TargetDatumId = q.Element(xmlns + "TargetDatumId").Value;

					//Console.WriteLine(++i);
					//Console.WriteLine("SourceDatumId:" + SourceDatumId);
					//Console.WriteLine("TargetDatumId:" + TargetDatumId);

					if (SourceDatumId == SourceDatum && TargetDatumId == TargetDatum)
					{
						foreach (var pm in q.Elements(xmlns + "OperationMethod").Elements(xmlns + "ParameterValue"))
						{
							datum_par.Add((pm.Element(xmlns + "Value").Value).ToString());
							console = "File was successfully treated";
						}
					}
				}
			}
			else console = "File don't exists";
			//Численные параметры датума
			string a1 = datum_par[0];
			string a2 = datum_par[1];
			string a3 = datum_par[2];
			double a4 = Convert.ToDouble(datum_par[3]) * 3600; //По умолчанию в градусах, и переводим в угловые секунды
			double a5 = Convert.ToDouble(datum_par[4]) * 3600;
			double a6 = Convert.ToDouble(datum_par[5]) * 3600;
			double a7 = Convert.ToDouble(datum_par[6]) * Math.Pow(10, 6);

			Full_WKT = $"{WKT1}{a1},{a2},{a3},{a4:f8},{a5:f8},{a6:f8},{a7}{WKT2}";


			return new Dictionary<string, object>
			{
				{"Console", console},
				{"Full_WKT", Full_WKT},
				{"Part_WKT", wkt},
			};

		}

		
	}
}
