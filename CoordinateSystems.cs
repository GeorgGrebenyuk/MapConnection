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
		public static string GetCurrentCoordinateSystem()
		{
			Autodesk.Gis.Map.Platform.AcMapMap map = Autodesk.Gis.Map.Platform.AcMapMap.GetCurrentMap();
			string wkt = map.GetMapSRS();

			OSGeo.MapGuide.MgCoordinateSystemFactory factory = new OSGeo.MapGuide.MgCoordinateSystemFactory();
			string csCode = factory.ConvertWktToCoordinateSystemCode(wkt);

			return csCode;
		}

		[MultiReturn(new[] { "Console", "Full_WKT"})]
		public static Dictionary<string, object> GetFullWKTCodeOfCS(string Current_CS, string XML_MapCSLibrary_path)
		{
			string console = null;
			string Full_WKT = null;
			string PROJ_4 = null;
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
			int i1 = wkt.LastIndexOf("TOWGS84") + 8; //Кончается на квадратной скобке определения датума
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
			//Действия для проекции в формате PROJ.4
			PROJ_4 = null;


			return new Dictionary<string, object>
			{
				{"Console", console},
				{"Full_WKT", Full_WKT},
			};

		}
		[MultiReturn(new[] { "Ell_name", "Ell_a", "Ell_flat",  "Datum_X", "Datum_Y", "Datum_Z", "Datum_ωx", "Datum_ωy", 
	"Datum_ωz", "Datum_m", "CS_Name", "CS_CentrMerid", "CS_OrigLat", "CS_FN", "CS_FE", "CS_k", "Units", })]
		public static Dictionary<string, object> GetGeodedictDescrOfCS_TM (string Full_WKT_string)
        {
			var guid = Guid.NewGuid();
			//Запешем определение WKT во временный файл (ограничение в 255 символов для строки)
			//string writePath = $@"%TEMP%\{guid}_Dynamo.log";
			string writePath = $@"E:\Work\Temp\Temp\{guid}_Dynamo.log";
			using (StreamWriter WKT_file = new StreamWriter(writePath, true, Encoding.Default)) //Запись в итоговый файл первой строки PTS-файла
			{
				WKT_file.WriteLine(Full_WKT_string);
				WKT_file.Close();
				WKT_file.Dispose();
			}

			string[] data_strings = File.ReadAllLines(writePath);
			string Full_WKT = data_strings[0];
			//Определение параметров эллипсоида
			int i1 = Full_WKT.IndexOf("SPHEROID") + 10; //Начало строки [ для эллипсоида
			int i2 = Full_WKT.Substring(i1).IndexOf("]"); //Конец строки ] для эллипсоида

			string Ell = Full_WKT.Substring(i1, i2);
			string[] Ell_string = Ell.Split(new char[] { ',' });

			string Ell_name = Ell_string[0].Substring(0, Ell_string[0].Length - 2);
			double Ell_a = Convert.ToDouble(Ell_string[1]);
			double Ell_flat = Convert.ToDouble(Ell_string[2]);

			//Определение параметров датума
			int i3 = Full_WKT.IndexOf("TOWGS84") + 8; //Начало строки [ для датума
			int i4 = Full_WKT.Substring(i3).IndexOf("]"); //Конец строки ] для датума
			string Datum = Full_WKT.Substring(i3, i4);
			string[] Datum_string = Datum.Split(new char[] { ',' });

			double Datum_X = Convert.ToDouble(Datum_string[0]);
			double Datum_Y = Convert.ToDouble(Datum_string[1]);
			double Datum_Z = Convert.ToDouble(Datum_string[2]);
			double Datum_ωx = Convert.ToDouble(Datum_string[3]);
			double Datum_ωy = Convert.ToDouble(Datum_string[4]);
			double Datum_ωz = Convert.ToDouble(Datum_string[5]);
			double Datum_m = Convert.ToDouble(Datum_string[6]);

			//Определение параметров проекции (СК)
			//Определяем имя СК
			int i7 = Full_WKT.IndexOf("PROJECTION") + 12; //Начало строки [ для датума
			int i8 = Full_WKT.Substring(i7).IndexOf(",");
			string CS_Name = Full_WKT.Substring(i7, i8 - 2);

			//Численные параметры проекции
			int i5 = Full_WKT.IndexOf("PARAMETER") - 1; //Начало строки [ для датума
			int i6 = Full_WKT.Substring(i5).IndexOf("UNIT") - 1; //Конец перечисления параметров СК
			string CS_Descr = Full_WKT.Substring(i5, i6);
			string[] CSDescr_string = CS_Descr.Split(new char[] { ',' });

			double CS_FE = Convert.ToDouble(CSDescr_string[2].Substring(0, CSDescr_string[2].Length - 1));
			double CS_FN = Convert.ToDouble(CSDescr_string[4].Substring(0, CSDescr_string[4].Length - 1));
			double CS_CentrMerid = Convert.ToDouble(CSDescr_string[6].Substring(0, CSDescr_string[6].Length - 1));
			double CS_OrigLat = Convert.ToDouble(CSDescr_string[8].Substring(0, CSDescr_string[8].Length - 1));
			double CS_k = 1d;
			//Для проекций типа Гаусса-Крюгера k=1
			if (CS_Name == "Gauss_Kruger")
			{
				CS_k = 1d;

			}
			//else //Для проекций типа "Поперечная Меркатора"
			//{

			//}
			string Units_str = Full_WKT.Substring(1 + i6 + i5);
			string[] Units_string = Units_str.Split(new char[] { ',' });
			string Units = Units_string[0].Substring(6, Units_string[0].Length - 7);

			Console.WriteLine($"{Ell_name}, {Ell_a}, {Ell_flat}");
			Console.WriteLine($"{Datum_X}, {Datum_Y}, {Datum_Z},{Datum_ωx}, {Datum_ωy}, {Datum_ωz}, {Datum_m}");
			Console.WriteLine($"{CS_Name}, {CS_FE}, {CS_FN}, {CS_CentrMerid}, {CS_OrigLat}, {CS_k}");
			Console.WriteLine($"{Units}");

			return new Dictionary<string, object>
			{
				{"Ell_name", Ell_name},
				{"Ell_a", Ell_a},
				{"Ell_flat", Ell_flat},
				{"Datum_X", Datum_X},
				{"Datum_Y", Datum_Y},
				{"Datum_Z", Datum_Z},
				{"Datum_ωx", Datum_ωx},
				{"Datum_ωy", Datum_ωy},
				{"Datum_ωz", Datum_ωz},
				{"Datum_m", Datum_m},
				{"CS_Name", CS_Name},
				{"CS_CentrMerid", CS_CentrMerid},
				{"CS_OrigLat", CS_OrigLat},
				{"CS_FN", CS_FN},
				{"CS_FE", CS_FE},
				{"CS_k", CS_k},
				{"Units", Units},

			};


		}
	}
}
