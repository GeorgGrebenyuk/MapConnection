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
		
	}
}
