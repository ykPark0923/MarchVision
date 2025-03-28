using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision.Algorithm
{
    public class DentAlgorithm : InspAlgorithm
    {
        //XML로 파라미터 셋팅 값 저장할수있도록 해야함**********************************************************
        public int _binaryMin { get; set; } = 30;
        public int _binaryMax { get; set; } = 255;
        public int _areaMin { get; set; } = 26;
        public int _areaMax { get; set; } = 2000;

        private List<Rect> _findArea;

        //#BINARY FILTER#4 이진화 영역 반환
        public override int GetResultRect(out List<Rect> resultArea)
        {
            resultArea = null;

            //#ABSTRACT ALGORITHM#7 검사가 완료되지 않았다면, 리턴
            if (!IsInspected)
                return -1;

            if (_findArea is null || _findArea.Count <= 0)
                return -1;

            resultArea = _findArea;
            return resultArea.Count;
        }

        public DentAlgorithm()
        {
            //#ABSTRACT ALGORITHM#5 각 함수마다 자신의 알고리즘 타입 설정
            InspectType = InspectType.InspDent;
        }

        public override bool DoInspect()
        {
            IsInspected = false;

            Mat aligned1 = new Mat();
            Mat aligned2 = new Mat();

            aligned1 = AlignImage(_srcImage, _diffSrc);
            aligned2 = AlignImage(_diffSrc, _diffSrc);


            if (aligned1 == null)
            {
                Console.WriteLine("input source aligned1 오류");
                return false;
            }

            if (aligned2 == null)
            {
                Console.WriteLine("diff source aligned 오류");
                return false;
            }



            Mat diffImage = new Mat();
            Cv2.Absdiff(aligned1, aligned2, diffImage);
            Cv2.ImShow("diffImage", diffImage);
            detectDent(diffImage);


            IsInspected = true;
            return true;
        }

        private void detectDent(Mat sourceImage)
        {
            Mat diffImage = sourceImage;

            // 그레이스케일 변환
            Mat grayDiff = new Mat();
            Cv2.CvtColor(diffImage, grayDiff, ColorConversionCodes.BGR2GRAY);

            // 이진화 (Dent 부분만 강조)
            Mat binaryDiff = new Mat();
            Cv2.Threshold(grayDiff, binaryDiff, _binaryMin, _binaryMax, ThresholdTypes.Binary); // Dent가 흰색으로 남음

            #region dent는 외곽에 안생김, 외곽지움
            // 외곽 영역 제거 (중심부만 남김)
            int roiWidth = diffImage.Cols * 86 / 100;   // 중심 너비 (86%)
            int roiHeight = diffImage.Rows * 79 / 100;  // 중심 높이 (79%)
            int x = (diffImage.Cols - roiWidth) / 2;
            int y = (diffImage.Rows - roiHeight) / 2;
            Rect innerROI = new Rect(x, y, roiWidth, roiHeight);

            // 외곽을 검정색으로 덮기 (중심만 남김)
            Mat mask = Mat.Zeros(binaryDiff.Size(), MatType.CV_8UC1);
            Cv2.Rectangle(mask, innerROI, new Scalar(255), -1); // 중심을 흰색으로 유지
            Cv2.BitwiseAnd(binaryDiff, mask, binaryDiff); // 중심 부분만 유지
            #endregion

            // 윤곽선 검출 → Dent 영역 찾기
            OpenCvSharp.Point[][] dentContours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binaryDiff, out dentContours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            bool dentDetected = false;
            Mat resultImage = _srcImage.Clone();

            foreach (var contour in dentContours)
            {
                double area = Cv2.ContourArea(contour);
                if (area >= _areaMin && area <= _areaMax)  // 일정 크기 이상의 Dent만 감지
                {
                    Rect boundingBox = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(resultImage, boundingBox, new Scalar(0, 255, 255), 2); // 노란색 박스 표시
                    dentDetected = true;
                }
            }

            if (dentDetected)
            {
                Console.WriteLine("NG: Dent Detected" );
            }
            else
            {
                Console.WriteLine("OK: Dent Not Detected");
            }
        }
    }
}