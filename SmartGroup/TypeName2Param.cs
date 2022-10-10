using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using System.Linq;
using GH_IO.Serialization;
namespace SmartGroup
{
	class TypeName2Param
	{
		public static IGH_Param change(string typeName)
		{
			IGH_Param param = null;

			switch (typeName)
			{
				case "GH_Point":
					param = new Param_Point();
					break;
				case "GH_Vector":
					param = new Param_Vector();
					break;
				case "GH_Circle":
					param = new Param_Circle();
					break;
				case "GH_Arc":
					param = new Param_Arc();
					break;
				case "GH_Curve":
					param = new Param_Curve();
					break;
				case "GH_Line":
					param = new Param_Line();
					break;
				case "GH_Plane":
					param = new Param_Plane();
					break;
				case "GH_Rectangle":
					param = new Param_Rectangle();
					break;
				case "GH_Box":
					param = new Param_Box();
					break;
				case "GH_Brep":
					param = new Param_Brep();
					break;
				case "GH_Mesh":
					param = new Param_Mesh();
					break;
				case "GH_MeshFace":
					param = new Param_MeshFace();
					break;
				case "GH_SubD":
					param = new Param_SubD();
					break;
				case "GH_Surface":
					param = new Param_Surface();
					break;
				case "GH_Field":
					param = new Param_Field();
					break;
				case "IGH_GeometricGoo":
					param = new Param_Geometry();
					break;
				case "GH_GeometryGroup":
					param = new Param_Group();
					break;
				case "GH_Transform":
					param = new Param_Transform();
					break;
				case "GH_Boolean":
					param = new Param_Boolean();
					break;
				case "GH_Integer":
					param = new Param_Integer();
					break;
				case "GH_Number":
					param = new Param_Number();
					break;
				case "GH_String":
					param = new Param_String();
					break;
				case "GH_Colour":
					param = new Param_Colour();
					break;
				case "GH_ComplexNumber":
					param = new Param_Complex();
					break;
				case "GH_Culture":
					param = new Param_Culture();
					break;
				case "GH_Interval":
					param = new Param_Interval();
					break;
				case "GH_Interval2D":
					param = new Param_Interval2D();
					break;
				case "GH_Guid":
					param = new Param_Guid();
					break;
				case "GH_Matrix":
					param = new Param_Matrix();
					break;
				case "GH_Time":
					param = new Param_Time();
					break;
				case "GH_Material":
					param = new Param_OGLShader();
					break;
				case "GH_StructurePath":
					param = new Param_StructurePath();
					break;
				case "IGH_Goo":
					param = new Param_GenericObject();
					break;
				default:
					param = new Param_GenericObject();
					break;
			}

			return param;
		}
	}
}
