using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Undo;
using System.Linq;

namespace SmartGroup
{
	class WireDisplayChanger
	{
		public static void Init()
		{
			Instances.ActiveCanvas.CanvasPrePaintWires += PrePaintWires;
			Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged_WireDisplayChanger;
		}

		public static HashSet<IGH_Param> paramList = new HashSet<IGH_Param>();
		public static bool NewPaintWiresToggle = true;
		private static void PrePaintWires(GH_Canvas canvas)
		{
			//毎回ドキュメントを走査するのは非効率なので、必要なIGH_Paramをリストで持ってくる。
			if (!NewPaintWiresToggle) return;
			foreach (IGH_Param param in paramList)
			{
				Guid gguid = param.getExProp("gname");
				if (gguid == default(Guid)) continue;
				GH_Group gr = canvas.Document.FindObject(gguid, true) as GH_Group;
				if (gr == null) continue;

				PointF input = param.Attributes.InputGrip;
				if (!param.Sources.Any()) continue;
				PointF output = param.Sources[0].Attributes.OutputGrip;

				var path = GH_Painter.ConnectionPath(input, output, GH_WireDirection.left, GH_WireDirection.right);

				Color col = Color.FromArgb(gr.Colour.R, gr.Colour.G, gr.Colour.B);
				
				Pen edge = new Pen(col, 5);				
				edge.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
				edge.DashPattern = new float[] { 1.5f, 2f };
				//edge.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

				canvas.Graphics.DrawPath(edge, path);

				edge.Dispose();
				path.Dispose();
			}
		}

		private static void ActiveCanvas_DocumentChanged_WireDisplayChanger(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
		{
			paramList.Clear();
			if (e.NewDocument == null) return;
			foreach (IGH_DocumentObject obj in e.NewDocument.Objects)
			{
				IGH_Param param = obj as IGH_Param;
				if (param == null) continue;
				if (param.Description == "NewGroupParams")
				{
					paramList.Add(param);
				}
			}
		}

	}
}
