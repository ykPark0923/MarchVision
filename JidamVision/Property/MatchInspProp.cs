using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JidamVision.Algorithm;
using JidamVision.Core;
using JidamVision.Teach;
using OpenCvSharp;

namespace JidamVision.Property
{
    public partial class MatchInspProp : UserControl
    {
        public MatchInspProp()
        {
            InitializeComponent();

            LoadInspParam();
        }

        private void LoadInspParam()
        {
            InspWindow inspWindow = Global.Inst.InspStage.InspWindow;
            if (inspWindow is null)
                return;

            OpenCvSharp.Size extendSize = inspWindow.MatchAlgorithm.ExtSize;
            int matchScore = inspWindow.MatchAlgorithm.MatchScore;

            txtExtendX.Text = extendSize.Width.ToString();
            txtExtendY.Text = extendSize.Height.ToString();
            txtScore.Text = matchScore.ToString();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            OpenCvSharp.Size extendSize = new OpenCvSharp.Size();
            extendSize.Width = int.Parse(txtExtendX.Text);
            extendSize.Height = int.Parse(txtExtendY.Text);
            int matchScore = int.Parse(txtScore.Text);

            InspWindow inspWindow = Global.Inst.InspStage.InspWindow;
            inspWindow.MatchAlgorithm.ExtSize = extendSize;
            inspWindow.MatchAlgorithm.MatchScore = matchScore;
            inspWindow.DoInpsect();
        }
    }
}
