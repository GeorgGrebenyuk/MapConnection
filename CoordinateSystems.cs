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
using System.Net.NetworkInformation;

namespace MapConnection
{
	public class CoordinateSystems
	{
		public static string GetCoordinateSystem()
		{
			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;

			string Current_CS = Units_Window.UnitZoneSettings.CoordinateSystemCode;
			return Current_CS;
		}
	}
}
