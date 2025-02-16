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
using JidamVision.Core;

namespace JidamVision
{
    public partial class CameraForm : DockContent
    {
        //private DockPanel _dockPanel;

        public CameraForm()
        {
            InitializeComponent();

            //_dockPanel = new DockPanel
            //{
            //    Dock = DockStyle.Fill
            //};
            //Controls.Add(_dockPanel);

            //// Visual Studio 2015 테마 적용
            //_dockPanel.Theme = new VS2015BlueTheme();

            //LoadDockingWindows();
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.Grab(0);
        }

        public void UpdateDisplay()
        {
            var bitmap = Global.Inst.InspStage.ImageSpace.GetBitmap(0);
            if (bitmap == null)
                return;

            picMainView.Image = bitmap;
        }

        //private void LoadDockingWindows()
        //{
        //    var resultWindow = new ResultForm();
        //    resultWindow.Show(_dockPanel, DockState.DockBottom);
        //}
    }
}
