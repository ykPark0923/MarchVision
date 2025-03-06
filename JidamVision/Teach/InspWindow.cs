using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JidamVision.Algorithm;
using OpenCvSharp;
using JidamVision.Core;
using System.Security.Policy;

namespace JidamVision.Teach
{
    public class InspWindow
    {
        System.Drawing.Rectangle _rect;
        Mat _teachingImage;

        MatchAlgorithm _matchAlgorithm;

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
            List<Point> points = new List<Point>();

            //Cv2.ImWrite("d:\\temp\\abc.jpg", _teachingImage);

            _matchAlgorithm.SetTemplateImage(_teachingImage);

            int matchCount = _matchAlgorithm.MatchTemplateMultiple(srcImage, out points);
            if (matchCount <= 0)
                return false;

            foreach (var point in points)
            {
                Console.WriteLine($"매칭된 위치: {point}");
                //Cv2.Rectangle(srcImage, new Rect(point.X, point.Y, _teachingImage.Width, _teachingImage.Height), Scalar.Green, 2);
            }

            return true;
        }

    }
}
