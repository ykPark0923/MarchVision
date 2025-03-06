using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JidamVision.Algorithm;
using OpenCvSharp;
using JidamVision.Core;
using System.Security.Policy;
using System.Drawing;

namespace JidamVision.Teach
{
    public class InspWindow
    {
        System.Drawing.Rectangle _rect;
        Mat _teachingImage;

        MatchAlgorithm _matchAlgorithm;
        List<OpenCvSharp.Point> _matchpoints;

        public MatchAlgorithm MatchAlgorithm => _matchAlgorithm;

        public InspWindow()
        {
            _matchAlgorithm = new MatchAlgorithm();
        }

        public bool SetTeachingImage(Mat image, System.Drawing.Rectangle rect)
        {
            _rect = rect;
            _teachingImage = new Mat(image, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
            return true;
        }

        public bool DoInpsect()
        {
            if (_teachingImage is null)
                return false;

            if (_matchAlgorithm is null)
                _matchAlgorithm = new MatchAlgorithm();

            Mat srcImage = Global.Inst.InspStage.GetMat();
            
            //Cv2.ImWrite("d:\\temp\\abc.jpg", _teachingImage);

            _matchAlgorithm.SetTemplateImage(_teachingImage);

            int matchCount = _matchAlgorithm.MatchTemplateMultiple(srcImage, out _matchpoints);
            if (matchCount <= 0)
                return false;

            return true;
        }

        public int GetMatchRect(out List<Rectangle> rectangles)
        {
            rectangles = new List<Rectangle> ();

            int halfWidth = _teachingImage.Width;
            int halfHeight = _teachingImage.Height;

            foreach (var point in _matchpoints)
            {
                Console.WriteLine($"매칭된 위치: {_matchpoints}");
                rectangles.Add(new Rectangle(point.X - halfWidth, point.Y - halfHeight, _teachingImage.Width, _teachingImage.Height));
            }

            return rectangles.Count;
        }

    }
}
