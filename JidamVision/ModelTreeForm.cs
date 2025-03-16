using JidamVision.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace JidamVision
{
    public partial class ModelTreeForm : DockContent
    {
        private ContextMenuStrip _contextMenu;

        public ModelTreeForm()
        {
            InitializeComponent();

            tvModelTree.Nodes.Add("Root");

            // 컨텍스트 메뉴 초기화
            _contextMenu = new ContextMenuStrip();
            ToolStripMenuItem addBaseRoiItem = new ToolStripMenuItem("Base ROI", null, AddNode_Click) { Tag = "Base" };
            ToolStripMenuItem addSubRoiItem = new ToolStripMenuItem("Sub ROI", null, AddNode_Click) { Tag = "Sub" };
            ToolStripMenuItem addIdRoiItem = new ToolStripMenuItem("ID ROI", null, AddNode_Click) { Tag = "ID" };

            _contextMenu.Items.Add(addBaseRoiItem);
            _contextMenu.Items.Add(addSubRoiItem);
            _contextMenu.Items.Add(addIdRoiItem);
        }

        private void tvModelTree_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode clickedNode = tvModelTree.GetNodeAt(e.X, e.Y);
                if (clickedNode != null && clickedNode.Text == "Root") ;
                {
                    tvModelTree.SelectedNode = clickedNode;
                    _contextMenu.Show(tvModelTree, e.Location);
                }
            }
        }

        private void AddNode_Click(object sender, EventArgs e)
        {
            if (tvModelTree.SelectedNode != null & sender is ToolStripMenuItem)
            {
                ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                string nodeType = menuItem.Tag?.ToString();
                if (nodeType == "Base")
                {
                    AddNewROI(InspWindowType.Base);
                    //tvModelTree.SelectedNode.Nodes.Add("Base01");
                }
                else if (nodeType == "Sub")
                {
                    AddNewROI(InspWindowType.Sub);
                    //tvModelTree.SelectedNode.Nodes.Add("Sub");
                }
                else if (nodeType == "ID")
                {
                    AddNewROI(InspWindowType.ID);
                    //tvModelTree.SelectedNode.Nodes.Add("ID");
                }
            }
        }

        private void AddNewROI(InspWindowType inspWindowType)
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.AddRoi(inspWindowType);
            }
        }
    }
}
