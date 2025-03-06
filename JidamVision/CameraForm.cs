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
using OpenCvSharp.Extensions;
using System.Web;

namespace JidamVision
{
    public partial class CameraForm : DockContent
    {
        eImageChannel _currentImageChannel = eImageChannel.Color;

        public CameraForm()
        {
            InitializeComponent();
        }

        private eImageChannel GetCurrentChannel()
        {
            if (rbtnRedChannel.Checked)
            {
                return eImageChannel.Red;
            }
            else if (rbtnBlueChannel.Checked)
            {
                return eImageChannel.Blue;
            }
            else if (rbtnGreenChannel.Checked)
            {
                return eImageChannel.Green;
            }
            else if (rbtnGrayChannel.Checked)
            {
                return eImageChannel.Gray;
            }

            return eImageChannel.Color;
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (bitmap == null)
            {
                _currentImageChannel = GetCurrentChannel();
                bitmap = Global.Inst.InspStage.ImageSpace.GetBitmap(0, _currentImageChannel);
                if (bitmap == null)
                    return;
            }

            imageViewer.LoadBitmap(bitmap);
        }

        public OpenCvSharp.Mat GetDisplayImage()
        {
            return Global.Inst.InspStage.ImageSpace.GetMat(0, _currentImageChannel);
        }

        private void CameraForm_Resize(object sender, EventArgs e)
        {
            int margin = 10;

            int xPos = Location.X + this.Width - btnGrab.Width - margin;

            btnGrab.Location = new Point(xPos, btnGrab.Location.Y);
            btnLive.Location = new Point(xPos, btnLive.Location.Y);
            btnSetRoi.Location = new Point(xPos, btnSetRoi.Location.Y);
            groupBox1.Location = new Point(xPos, groupBox1.Location.Y);

            imageViewer.Width = this.Width - btnGrab.Width - margin * 2;
            imageViewer.Height = this.Height - margin * 2;

            imageViewer.Location = new Point(margin, margin);
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.Grab(0);
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.LiveMode = !Global.Inst.InspStage.LiveMode;

            if (Global.Inst.InspStage.LiveMode)
                Global.Inst.InspStage.Grab(0);
        }

        private void btnSetRoi_Click(object sender, EventArgs e)
        {
            imageViewer.RoiMode = !imageViewer.RoiMode;
            imageViewer.Invalidate();
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {

        }

        private void rbtnRedChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnBlueChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnGreenChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnGrayChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }
    }
}
