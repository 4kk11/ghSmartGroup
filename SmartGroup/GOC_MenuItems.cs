using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel.Undo;
using Grasshopper.Kernel.Special;
using System.Linq;

namespace SmartGroup
{
	class GOC_MenuItems
	{
		public static List<ToolStripMenuItem> menuItems
		{
			get
			{
				//Init items;
				List<ToolStripMenuItem> list = new List<ToolStripMenuItem>();
				ToolStripMenuItem debugItem1 = new ToolStripMenuItem();
				ToolStripMenuItem debugItem2 = new ToolStripMenuItem();
				ToolStripMenuItem mainItem1 = new ToolStripMenuItem();
				ToolStripButton toolbarItem = new ToolStripButton(Properties.Resources.WireColorIcon_24);

				//Set item properties
				////debugItems
				debugItem1.Name = "Debug_ShowAll_NewGroupParamsName";
				debugItem1.Text = "Debug_ShowAll_NewGroupParamsName";
				debugItem1.Click += (sender, e) =>
				{
					MessageBox.Show(String.Join(",", WireDisplayChanger.paramList.Select(x => x.NickName)));
					//GetInstanceGuidsinGroup();
				};
				
				debugItem2.Name = "Debug_Show_GroupGuidFromComp";
				debugItem2.Text = "Debug_Show_GroupGuidFromComp";
				debugItem2.Click += (sender, e) =>
				{
					string message = GetGroupGuidFromComp();
					MessageBox.Show(message);
				};
				////mainItems	
				mainItem1.Name = "GroupInputWires";
				mainItem1.Text = "GroupInputWires";
				mainItem1.Checked = true;
				mainItem1.Image = Properties.Resources.WireColorIcon_24;
				mainItem1.Click += (sender, e) =>
				{
					WireDisplayChanger.NewPaintWiresToggle = !WireDisplayChanger.NewPaintWiresToggle;
					mainItem1.Checked = !mainItem1.Checked;
					toolbarItem.Checked = !toolbarItem.Checked;
					Instances.ActiveCanvas.Refresh();
				};
							
				toolbarItem.ToolTipText = "Switch GroupInputWires";
				toolbarItem.Checked = true;
				toolbarItem.Click += (sender, e) =>
				{
					mainItem1.PerformClick();		
				};

				//Add items;
				list.Add(debugItem1);
				list.Add(debugItem2);
				list.Add(mainItem1);

				ToolStrip canvasToolbar = Instances.DocumentEditor.Controls[0].Controls[1] as ToolStrip;
				canvasToolbar.Items.Add(new ToolStripSeparator());
				canvasToolbar.Items.Add(toolbarItem);


				return list;

			}
		}


		public static void SetGroupGuidToComp()
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (!GrasshopperDocument.SelectedObjects().Any()) return;
			GH_Group gr = GrasshopperDocument.SelectedObjects()[0] as GH_Group;
			if (gr == null) return;

			foreach (IGH_DocumentObject obj in gr.Objects())
			{
				obj.setExProp("gname", gr.InstanceGuid);

			}

		}

		public static string GetGroupGuidFromComp()
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (!GrasshopperDocument.SelectedObjects().Any()) return "Not Found";

			string message = GrasshopperDocument.SelectedObjects()[0].getExProp("gname").ToString();

			return message;
		}

		private static void GetInstanceGuid()   ////////////////
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (GrasshopperDocument.SelectedObjects().Any())
			{
				foreach (var obj in GrasshopperDocument.SelectedObjects())
				{
					MessageBox.Show(obj.InstanceGuid.ToString());
				}
			}
		}

		private static void GetInstanceGuidsinGroup()   ////////////////
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (!GrasshopperDocument.SelectedObjects().Any()) return;
			GH_Group gr = GrasshopperDocument.SelectedObjects()[0] as GH_Group;
			if (gr == null) return;

			MessageBox.Show(String.Join("\n", gr.Objects().Select(x => x.InstanceGuid.ToString())));
		}

		private static void Test_GetRecipient()
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (!GrasshopperDocument.SelectedObjects().Any()) return;
			IGH_Param param = GrasshopperDocument.SelectedObjects()[0] as IGH_Param;
			if (param == null) return;

			MessageBox.Show(String.Join("\n", param.Recipients.Select(x => x.NickName)));
		}

		private static void Test_SetRecordUndoEvent()   ////////////////
		{
			GH_Document GrasshopperDocument = Instances.ActiveCanvas.Document;
			if (GrasshopperDocument.SelectedObjects().Any())
			{
				foreach (var obj in GrasshopperDocument.SelectedObjects())
				{
					
					Guid gguid = obj.getExProp("gname");
					if (gguid == default(Guid)) continue;
					GH_Group gr = GrasshopperDocument.FindObject(gguid, true) as GH_Group;
					gr.RecordUndoEvent("ddd");
					gr.RemoveObject(obj.InstanceGuid);
					obj.clearExProp();
					
				}
			}
		}

		public static void CreateNewGroup()
		{
			GH_Document GrasshopperDocument = Instances.DocumentServer[0];
			if (!GrasshopperDocument.SelectedObjects().Any()) return;

			GH_NewGroup gr = new GH_NewGroup(GrasshopperDocument.SelectedObjects().ToList());
			GrasshopperDocument.AddObject(gr, false);
			GH_UndoRecord undoRec = GrasshopperDocument.UndoUtil.CreateAddObjectEvent("Add NewGroup", gr);
			gr.RecordUndoEvent(undoRec);

			Instances.ActiveCanvas.Refresh();
		}
	}
}
