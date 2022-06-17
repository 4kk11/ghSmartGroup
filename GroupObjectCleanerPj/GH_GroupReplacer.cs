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

namespace GroupObjectCleaner
{
	class GH_GroupReplacer
	{

		public static void Init()
		{
			//This only works once when grasshopper is started.
			Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
			Instances.DocumentServer.DocumentAdded += DocumentServer_DocumentAdded;

		}
		
		private static void DocumentServer_DocumentAdded(GH_DocumentServer sender, GH_Document doc)
		{
			List<IGH_DocumentObject> objs = doc.Objects.Where(x => x.Name != "GH_NewGroup" && (x as GH_Group) != null).ToList();

			foreach (IGH_DocumentObject obj in objs)
			{
				GH_Group gr = obj as GH_Group;
				GH_NewGroup newgr = new GH_NewGroup(gr);

				doc.RemoveObject(gr, false);
				doc.AddObject(newgr, false);
				
			}
			
		}

		private static void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
		{
			//This is done every time the document changes.
			if (e.OldDocument != null)
			{
				e.OldDocument.ObjectsAdded -= Document_ObjectsAdded;
				e.OldDocument.ObjectsAdded -= Document_ObjectsAdded_NewGroupParams;
				e.OldDocument.UndoStateChanged -= Document_UndoStateChanged;
				e.OldDocument.ObjectsDeleted -= Document_ObjectsDeleted;
			}

			if (e.NewDocument == null) return;

			e.NewDocument.ObjectsAdded += Document_ObjectsAdded;
			e.NewDocument.ObjectsAdded += Document_ObjectsAdded_NewGroupParams;
			e.NewDocument.UndoStateChanged += Document_UndoStateChanged;
			e.NewDocument.ObjectsDeleted += Document_ObjectsDeleted;
		}

		private static void Document_ObjectsAdded(object sender, GH_DocObjectEventArgs e)
		{
			foreach (IGH_DocumentObject item in e.Objects)
			{
				if (item.Name == "GH_NewGroup") continue;
				GH_Group gr = item as GH_Group;
				if (gr == null) continue;

				//Change GH_Group to GH_NewGroup
				gr.Colour = GetRandomColor(new Random());
				GH_NewGroup newgr = new GH_NewGroup(gr);
				
				GH_Document GrasshopperDocument = gr.OnPingDocument();
				
				GrasshopperDocument.RemoveObject(gr, false);
				GrasshopperDocument.AddObject(newgr, false);
				
				GH_UndoRecord undoRec = GrasshopperDocument.UndoUtil.CreateAddObjectEvent("Add NewGroup", newgr);
				newgr.RecordUndoEvent(undoRec);
			}
			//MessageBox.Show("Added");
		}

		private static void Document_ObjectsAdded_NewGroupParams(object sender, GH_DocObjectEventArgs e)
		{
			foreach (IGH_DocumentObject item in e.Objects)
			{
				if (item.Description == "NewGroupParams")
				{
					//MessageBox.Show("Added NewGroupParams!");
					WireDisplayChanger.paramList.Add(item as IGH_Param);
				}
			}
		}

		private static GH_UndoRecord deleteRecord ;
		private static void Document_UndoStateChanged(object sender, GH_DocUndoEventArgs e)
		{
			if (e.Record != null)
			{
				if (e.Record.Name == "Group")
				{
					e.Document.UndoServer.RemoveRecord(e.Record.Guid);
					return;
				}

				
				if (e.Record.Name == "Delete")
				{
					deleteRecord = e.Record;
					deleteRecord.Name = "NewDelete";
					e.Document.UndoServer.RemoveRecord(e.Record.Guid);
					return;
				}

			}

		}

		private static void Document_ObjectsDeleted(object sender, GH_DocObjectEventArgs e)
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;

			int mergeCount = 0;

			foreach (IGH_DocumentObject item in e.Objects)
			{
				if (item.Name == "Group") return;

				IGH_Param param = item as IGH_Param;
				if (param != null)
				{
					if (item.Description == "NewGroupParams")
					{
						WireDisplayChanger.paramList.Remove(param);
					}
				}
				
				Guid grGuid = item.getExProp("gname");
				if (grGuid != default(Guid))
				{						
					GH_Group gr = GrasshopperDocument.FindObject(grGuid, true) as GH_Group;
					if (gr == null) continue;
					
					gr.RecordUndoEvent("Info_NewGroup");
					mergeCount++;
					
					gr.RemoveObject(item.InstanceGuid);
				}

				
				
			}
			if (deleteRecord != null)
			{
				GrasshopperDocument.UndoServer.PushUndoRecord(deleteRecord);
				mergeCount++;
				deleteRecord = null;
			}		
			GrasshopperDocument.UndoServer.MergeRecords(mergeCount);
			
			//MessageBox.Show("Object Deleted");
		}

		private static Color GetRandomColor(Random r)
		{
			int alpha = 150;
			int red = r.Next(0,256);
			int green = r.Next(0,256);
			int blue = r.Next(0,256);

			return Color.FromArgb(alpha, red, green, blue);
		}

	}
}
