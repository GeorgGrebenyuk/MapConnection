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
using Autodesk.AutoCAD.Windows;
using Autodesk.DesignScript.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace MapConnection
{
	public class MapCSLibrary
	{
		/// <summary>
		/// CS_Language_RUS return system values for creation LSP file, if program language is Russian
		/// </summary>
		/// <returns>CS_value, CS_Agree - give them to GetPartOfMAPCSLIBRARY or GetAllOfMAPCSLIBRARY</returns>
		[MultiReturn(new[] { "CS_value", "CS_Agree" })]
		public static Dictionary<string, object> CS_Language_RUS()
		{
			string CS_value = "СистемаКоординат";
			string CS_Agree = "Д";
			return new Dictionary<string, object>
			{
				{"CS_value", CS_value},
				{"CS_Agree", CS_Agree},
			};
		}
		/// <summary>
		/// CS_Language_ENG return system values for creation LSP file, if program language is English
		/// </summary>
		/// <returns>CS_value, CS_Agree - give them to GetPartOfMAPCSLIBRARY or GetAllOfMAPCSLIBRARY</returns>
		[MultiReturn(new[] { "CS_value", "CS_Agree" })]
		public static Dictionary<string, object> CS_Language_ENG()
		{
			string CS_value = "C";
			string CS_Agree = "Y";
			return new Dictionary<string, object>
			{
				{"CS_value", CS_value},
				{"CS_Agree", CS_Agree},
			};
		}

		/// <summary>
		/// Node GetListOfMAPCSLIBRAR for export to LSP file naming of coordinate systems (user or system)
		/// </summary>
		/// <param name="CS_value">Name of type "CoordinateSystem"</param>
		/// <param name="selection">If false - exporting only User's definitions; if true - exporting only system definitions</param>
		/// <param name="CS_Agree">Permit to add other associated definitions</param>
		/// <param name="Folder_Path">Directory path to save LSP file</param>
		async public static void GetPartOfMAPCSLIBRARY(string Folder_Path, string CS_value, string CS_Agree, bool selection)
		{
			var guid = Guid.NewGuid();
			string writePath = $@"{Folder_Path}\{guid}.lsp";

			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();
			MgCoordinateSystemDictionary csDict = csCatalog.GetCoordinateSystemDictionary();

			MgCoordinateSystemEnum csDictEnum = csDict.GetEnum();
			int csCount = csDict.GetSize();
			using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
			{
				export_file.WriteLine(@"(command ""MAPCSLIBRARYEXPORT""");
				export_file.Close();
				export_file.Dispose();
			}

			MgStringCollection csNames = csDictEnum.NextName(csCount);
			string csName = null;

			MgCoordinateSystem cs = null;
			bool csProtect;

			for (int i = 0; i < csCount; i++)
			{
				csName = csNames.GetItem(i);
				cs = csDict.GetCoordinateSystem(csName);
				csProtect = cs.IsProtected();

					if (csProtect == selection)
					{
						using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
						{
							string csNameStr = csName.ToString();
							await export_file.WriteLineAsync($@"""{csNameStr}""" + " " + $"\"{CS_value}\"");
						}
					}
			}
			string space = " ";
			using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
			{
				export_file.Write(" " + $@"""{space}""" + " " + $@"""{CS_Agree}""" +" " +  @"""""" + ")");
				export_file.Close();
				export_file.Dispose();
			}

		}
		/// <summary>
		/// Node GetListOfMAPCSLIBRAR for export to LSP file naming of coordinate systems (all library)
		/// </summary>
		/// <param name="CS_value">Name of type "CoordinateSystem"</param>
		/// <param name="CS_Agree">Permit to add other associated definitions</param>
		/// <param name="Folder_Path">Directory path to save LSP file</param>
		async public static void GetAllOfMAPCSLIBRARY(string Folder_Path, string CS_value, string CS_Agree)
		{
			var guid = Guid.NewGuid();
			string writePath = $@"{Folder_Path}\{guid}.lsp";

			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();
			MgCoordinateSystemDictionary csDict = csCatalog.GetCoordinateSystemDictionary();

			MgCoordinateSystemEnum csDictEnum = csDict.GetEnum();
			int csCount = csDict.GetSize();
			using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
			{
				export_file.WriteLine(@"(command ""MAPCSLIBRARYEXPORT""");
				export_file.Close();
				export_file.Dispose();
			}

			MgStringCollection csNames = csDictEnum.NextName(csCount);
			string csName = null;

			for (int i = 0; i < csCount; i++)
			{
				csName = csNames.GetItem(i);
				using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
				{
					string csNameStr = csName.ToString();
					await export_file.WriteLineAsync($@"""{csNameStr}""" + " " + $"\"{CS_value}\"");
				}
			}
			string space = " ";
			using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.UTF8))
			{
				export_file.Write(" " + $@"""{space}""" + " " + $@"""{CS_Agree}""" + " " + @"""""" + ")");
				export_file.Close();
				export_file.Dispose();
			}

		}
		/// <summary>
		/// Node GetListAndCountOfCategories returns a list of all Categories in MapCSLibrary and count of them
		/// </summary>
		/// <returns>Count of Categories and result List</returns>
		[MultiReturn(new[] { "CatCount", "CatList" })]
		public static Dictionary<string, object> GetListAndCountOfCategories() //Работает!
		{
			var guid = Guid.NewGuid();
			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();
			MgCoordinateSystemCategoryDictionary csDictCat = csCatalog.GetCategoryDictionary();

			MgCoordinateSystemEnum csDictCatEnum = csDictCat.GetEnum();
			int csCatCount = csDictCat.GetSize();
			MgStringCollection csCatNames = csDictCatEnum.NextName(csCatCount);
			string csCategoryName = null;

			List<string> csCatNamesL = new List<string>();
			for (int i3 = 0; i3 < csCatCount; i3++)
			{
				csCategoryName = csCatNames.GetItem(i3);
				csCatNamesL.Add(csCategoryName);
			}
			return new Dictionary<string, object>
			{
				{"CatCount", csCatCount},
				{"CatList", csCatNamesL},
			};
		}
		
		/// <summary>
		/// Node RenameCurrentCategory rename Category form MapCSLibrary
		/// </summary>
		/// <param name="new_cat_name"></param>
		/// <param name="old_cat_name"></param>
		public static void RenameCurrentCategory(string new_cat_name, string old_cat_name) //Работает!
		{
			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();
			MgCoordinateSystemCategoryDictionary csDictCat = csCatalog.GetCategoryDictionary();
			csDictCat.Rename(old_cat_name, new_cat_name);

		}

		/// <summary>
		/// Node GetFullWKTCodeOfAllCS create a List and external txt file with FullWKT codes of coordinate systems in MapCSLibrary
		/// </summary>
		/// <param name="XML_MapCSLibrary_path_Full">Path to XML-defonotion of MapCSLibrary</param>
		/// <param name="selection">Bool parameter </param>
		/// <param name="WriteFolder"></param>
		/// <param name="all_data"></param>
		/// <returns></returns>
		[MultiReturn(new[] { "Console", "Full_WKT_List" })]
		public static Dictionary<string, object> GetFullWKTCodeOfAllCS(string XML_MapCSLibrary_path_Full, bool selection, string WriteFolder, bool all_data = false)
		{
			string console = null;
			var guid = Guid.NewGuid();

			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();
			MgCoordinateSystemDictionary csDict = csCatalog.GetCoordinateSystemDictionary();

			MgCoordinateSystemEnum csDictEnum = csDict.GetEnum();
			int csCount = csDict.GetSize();
			MgStringCollection csNames = csDictEnum.NextName(csCount);

			MgCoordinateSystem cs = null;
			string csName = null;
			//string csType = null;
			bool csProtect;
			List<string> CS_List = new List<string>();
			string writePath2 = $@"{WriteFolder}\{guid}_CS.log";


			for (int i = 0; i < csCount; i++)
			{
				csName = csNames.GetItem(i);
				cs = csDict.GetCoordinateSystem(csName);
				csProtect = cs.IsProtected();

				if (csProtect == selection && all_data == false) //Если selection = false, то выбираем только пользовательскую группу, если true - то для системной библиотеки только
				{
					CS_List.Add(csName);
					using (StreamWriter export_file2 = new StreamWriter(writePath2, true, Encoding.Default))
					{
						export_file2.WriteLine(csName);
						export_file2.Close();
						export_file2.Dispose();
					}
				}
				else if (all_data == true) //Экспорт библиотеки целиком
				{
					CS_List.Add(csName);
					using (StreamWriter export_file2 = new StreamWriter(writePath2, true, Encoding.Default))
					{
						export_file2.WriteLine(csName);
						export_file2.Close();
						export_file2.Dispose();
					}
				}
			}
			//Создание списка для WKT отталкивайсь от функции перевода наименования СК в WKT
			List<string> WKT_List = new List<string>();
			string writePath3 = $@"{WriteFolder}\{guid}_CS-WKT.log";

			for (int i10 = 0; i10 < CS_List.Count; i10++)
			{
				WKT_List.Add(coordSysFactory.ConvertCoordinateSystemCodeToWkt(CS_List[i10]));
				using (StreamWriter export_file3 = new StreamWriter(writePath3, true, Encoding.Default))
				{
					export_file3.WriteLine(coordSysFactory.ConvertCoordinateSystemCodeToWkt(CS_List[i10]));
					export_file3.Close();
					export_file3.Dispose();
				}
			}
			//Создание пустого списка с полными кодами WKT
			List<string> WKT_Full_List = new List<string>();
			//Запись в файл (на всякий случай)
			string writePath = $@"{WriteFolder}\{guid}_FullWKT.log";
			//Перебор списка с урезанными WKT
			int i11 = 0;

			do
			{
				string wkt = WKT_List[i11];
				if (wkt.Length > 10 && wkt.Contains("PROJCS") != false)  //Условие для афф. определений где не задается WKT
				{
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
					if (System.IO.File.Exists(XML_MapCSLibrary_path_Full))
					{
						XDocument CS_Lib = XDocument.Load(XML_MapCSLibrary_path_Full);
						XNamespace xmlns = "http://www.osgeo.org/mapguide/coordinatesystem";

						var query = CS_Lib.Descendants().Where(m => m.Name.LocalName == "Transformation");

						var i = 0;
						foreach (var q in query)
						{
							var SourceDatumId = q.Element(xmlns + "SourceDatumId").Value;
							var TargetDatumId = q.Element(xmlns + "TargetDatumId").Value;

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

					string Full_WKT = null;
					Full_WKT = $"{WKT1}{a1},{a2},{a3},{a4:f8},{a5:f8},{a6:f8},{a7}{WKT2}";

					using (StreamWriter export_file = new StreamWriter(writePath, true, Encoding.Default))
					{
						export_file.WriteLine(Full_WKT);
						export_file.Close();
						export_file.Dispose();
					}

					WKT_Full_List.Add(Full_WKT);
				}
				i11++;
			} while (i11 < WKT_List.Count);

			return new Dictionary<string, object>
			{
				{"Console", console},
				{"Full_WKT_List", WKT_Full_List},
			};

		}
		public static string CStoCategory() //string Category_name
		{
			MgCoordinateSystemFactory coordSysFactory = new MgCoordinateSystemFactory();
			MgCoordinateSystemCatalog csCatalog = coordSysFactory.GetCatalog();

			MgCoordinateSystemDictionary csDict = csCatalog.GetCoordinateSystemDictionary();

			MgCoordinateSystemEnum csDictEnum = csDict.GetEnum();
			int csCount = csDict.GetSize();
			MgStringCollection csNames = csDictEnum.NextName(csCount);

			MgCoordinateSystem cs = null;
			string csName = null;
			bool csProtect;

			var cDict = csCatalog.GetCategoryDictionary().GetCategory("NewCategory");

			//Перебираем словарь СК
			int i1 = 0;
			for (int i2 = 0; i2 < csCount; i2++)
			{
				csName = csNames.GetItem(i2);
				cs = csDict.GetCoordinateSystem(csName);
				csProtect = cs.IsProtected();

				//Условие - если данная СК пользовательская (импортированная из XML)
				if (csProtect == false)
				{   //Пусть для примера для созданной категории с одной внесеной ск по имени "NewCategory"
					cDict.AddCoordinateSystem(csName);
					i1++;
				}
			}
			return i1.ToString();
		}
	}

}

