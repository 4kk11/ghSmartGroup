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
using Grasshopper.Kernel.Undo;
using Grasshopper.Kernel.Types;
using System.Linq;
using GH_IO.Serialization;

namespace SmartGroup
{
	public class GH_NewGroup : GH_Group
	{

		public GH_NewGroup() : base()
		{
			Init(Grasshopper.GUI.Canvas.GH_Skin.group_back, "");
		}

		public GH_NewGroup(GH_Group baseGroup) : base()
		{
			foreach (IGH_DocumentObject obj in baseGroup.Objects())
			{
				AddObject(obj.InstanceGuid);
				obj.setExProp("gname", this.InstanceGuid);
			}
			Init(baseGroup.Colour, baseGroup.NickName);
			
		}

		public GH_NewGroup(List<IGH_DocumentObject> objects) : base()
		{
			foreach (IGH_DocumentObject obj in objects)
			{
				AddObject(obj.InstanceGuid);
				obj.setExProp("gname", this.InstanceGuid);
			}
			Init(Grasshopper.GUI.Canvas.GH_Skin.group_back, "");
		}

		private void Init(Color col, string nickName)
		{
			Colour = col;
			NickName = nickName;
			Name = "GH_NewGroup";
		}

		public override IEnumerable<string> Keywords => new List<string>() {"GH_NewGroup"};
		
		public override bool AppendMenuItems(ToolStripDropDown menu)
		{
			
			bool b = base.AppendMenuItems(menu);
			menu.Items[10].Click -= RemoveFromGroup_DoCllick;
			menu.Items[10].Click += RemoveFromGroup_DoCllick;
			menu.Items[9].Click -= AddToGroup_DoClick;
			menu.Items[9].Click += AddToGroup_DoClick;

			Menu_AppendItem(menu, "Add_InputGroupParams", Menu_DoClick_Add_InputGroupParams,Properties.Resources.AddNewGroupParamsIcon);
			Menu_AppendItem(menu, "Delete_InputGroupParams", Menu_DoClick_Delete_InputGroupParams, Properties.Resources.DeleteNewGroupParamsIcon);
			return b;
		}

		private void Menu_DoClick_Add_InputGroupParams(object sender, EventArgs e)
		{
			Dictionary<Guid, IGH_Param> dic = new Dictionary<Guid, IGH_Param>();
			GH_Document GrasshopperDocument = OnPingDocument();
			int undoCount = 0;

			foreach (IGH_DocumentObject obj in Objects())
			{
				GH_Component comp = obj as GH_Component;
				if (comp == null) continue;
				List<IGH_Param> inputs = comp.Params.Input;
				if (!inputs.Any()) continue;

				foreach (IGH_Param input in inputs)
				{
					if (input.SourceCount == 0) continue;
					IGH_Param srcOutput = input.Sources[0];
					if (srcOutput.Attributes.GetTopLevel.DocObject.getExProp("gname") != this.InstanceGuid)
					{
						Guid srcGuid = srcOutput.InstanceGuid;
						GrasshopperDocument.UndoUtil.RecordWireEvent("Add_InputGroupParams", input);
						undoCount++;
						if (!dic.Keys.Contains(srcGuid))
						{
							IGH_Param pn = AddNumberComp(input, comp, GrasshopperDocument);
							GrasshopperDocument.UndoUtil.RecordAddObjectEvent("Add_InputGroupParams", pn);						
							undoCount++;
							dic[srcGuid] = pn;
						}
						else
						{
							IGH_Param pn = dic[srcGuid];
							input.RemoveSource(srcGuid);
							input.AddSource(pn);
						}
					}
				}
			}
			dic.Values.ToList().ForEach(x => this.AddObject(x.InstanceGuid));
			GrasshopperDocument.UndoUtil.MergeRecords(undoCount);
			Instances.ActiveCanvas.Refresh();
			this.ExpireSolution(true);
		}

		private void Menu_DoClick_Delete_InputGroupParams(object sender, EventArgs e)
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			List<IGH_Param> removeParams = new List<IGH_Param>();
			List<IGH_UndoAction> undoActions = new List<IGH_UndoAction>();
			bool existNewParam = false;
			foreach (IGH_DocumentObject obj in Objects())
			{
				IGH_Param param = obj as IGH_Param;
				if (param == null) continue;
				if (param.GetType().Namespace != "Grasshopper.Kernel.Parameters") continue;
				//if (param.Description == "NewGroupParams") continue;
				existNewParam = true;
				var recipients = param.Recipients;
				var sources = param.Sources;
				if (sources.Count == 0 || recipients.Count == 0) continue;
				foreach (IGH_Param src in sources)
				{
					foreach (IGH_Param rec in recipients)
					{
						undoActions.Add( new Grasshopper.Kernel.Undo.Actions.GH_WireAction(rec));
						rec.AddSource(src);
					}
				}
				removeParams.Add(param);
				undoActions.Add(new Grasshopper.Kernel.Undo.Actions.GH_RemoveObjectAction(param));
			}
			if (existNewParam)
			{
				GrasshopperDocument.UndoServer.PushUndoRecord(new GH_UndoRecord("Delete", undoActions));
				GrasshopperDocument.RemoveObjects(removeParams, true);
			}
		}

		private IGH_Param AddNumberComp(IGH_Param receiver, GH_Component comp, GH_Document doc)
		{
			IGH_Param sender = receiver.Sources[0];

			IGH_Param pn = TypeName2Param.change(sender.Type.Name);
			
			pn.CreateAttributes();
			pn.Attributes.Pivot = new System.Drawing.PointF((float)Attributes.Bounds.Left - pn.Attributes.Bounds.Width, (float)receiver.Attributes.Bounds.Y + 5);
			pn.NickName = sender.NickName;
			pn.WireDisplay = GH_ParamWireDisplay.hidden;
			pn.setExProp("gname", this.InstanceGuid);
			pn.Description = "NewGroupParams";
				
			doc.AddObject(pn, false);
			
			
			receiver.RemoveSource(sender);
			receiver.AddSource(pn);
			pn.AddSource(sender);

			return pn;
		}

		private void RemoveFromGroup_DoCllick(object sender, EventArgs e)
		{
			OnPingDocument().SelectedObjects().ForEach(x => x.clearExProp());
		}

		private void AddToGroup_DoClick(object sender, EventArgs e)
		{
			OnPingDocument().SelectedObjects().ForEach(x => x.setExProp("gname", this.InstanceGuid));
		}

		public override void RemovedFromDocument(GH_Document document)
		{
			Objects().ForEach(x => x.clearExProp());
			base.RemovedFromDocument(document);
			
		}
		
		

		public override Guid ComponentGuid
		{
			get
			{
				return new Guid("{8d3FAD0D-4534-4AD3-961A-37A210182F70}");
			}
		}
		
		public override bool Write(GH_IWriter writer)
		{
			bool b = base.Write(writer);
			if (writer.Archive != null) WriteNewGuid(writer);
			return b;		
		}

		public override bool Read(GH_IReader reader)
		{
			
			if (this.OnPingDocument() != null)
			{
				Objects().ForEach(x => x.clearExProp());
			}

			int idCount = reader.GetInt32("ID_Count");			
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			for (int i = 0; i < idCount; i++)
			{
				//if (GrasshopperDocument == null) break;
				Guid instanaceGuid = reader.GetGuid("ID", i);
				IGH_DocumentObject obj = GrasshopperDocument.FindObject(instanaceGuid, true);
				obj.setExProp("gname", reader.GetGuid("InstanceGuid"));
			}
			
			return base.Read(reader);
			
		}

		private void WriteNewGuid(GH_IWriter writer)
		{
			var root = writer.Archive.GetRootNode;
			var definitions = root.Chunks.Where(x => x.Name == "Definition");
			if (!definitions.Any()) return;
			var definition = definitions.ElementAt(0);
			var definitionObjects = definition.Chunks.Where(x => x.Name == "DefinitionObjects").ElementAt(0);
			var objectChunks = definitionObjects.Chunks.Where(x => x.Name == "Object");

			foreach (GH_IChunk ch in objectChunks)
			{
				GH_Chunk oc = ch as GH_Chunk;
				if (oc.FindItem("Name")._string == "GH_NewGroup")
				{
					oc.RemoveItem("GUID");
					oc.SetGuid("GUID", new Guid("{c552a431-af5b-46a9-a8a4-0fcbc27ef596}"));
					oc.RemoveItem("Name");
					oc.SetString("Name", "Group");
				}

			}

			writer.RemoveItem("Name");
			writer.SetString("Name", "Group");
			
		}
		
		

	}
}
