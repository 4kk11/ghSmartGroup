using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel.Special;
using System.Linq;


namespace SmartGroup
{
	public class GOC_AssemblyPriority : GH_AssemblyPriority
	{
		public override GH_LoadingInstruction PriorityLoad()
		{
			Instances.CanvasCreated += Instances_CanvasCreated;
			return GH_LoadingInstruction.Proceed;
		}


		private void Instances_CanvasCreated(Grasshopper.GUI.Canvas.GH_Canvas canvas)
		{
			Instances.CanvasCreated -= Instances_CanvasCreated;

			RegisterNewMenuItems();

			GH_GroupReplacer.Init();
			WireDisplayChanger.Init();
		}

		private void RegisterNewMenuItems()
		{
			GH_DocumentEditor docEditor = Instances.DocumentEditor;

			docEditor.MainMenuStrip.SuspendLayout();

			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Name = "TestMenuItem";
			toolStripMenuItem.Text = "Test";
			toolStripMenuItem.DropDownItems.AddRange(GOC_MenuItems.menuItems.ToArray());

			docEditor.MainMenuStrip.ResumeLayout(false);
			docEditor.MainMenuStrip.PerformLayout();

			docEditor.MainMenuStrip.Items.Add(toolStripMenuItem);
		}

	}

	


	public class GroupObjectCleanerInfo : GH_AssemblyInfo
	{
		public override string Name => " GroupObjectCleaner";
		public override Bitmap Icon => null;
		public override string Description => "";
		public override Guid Id => new Guid("6d60552e-5179-4208-b24d-fa20a42edef7");
		public override string AuthorName => "";
		public override string AuthorContact => "";

	}
}
