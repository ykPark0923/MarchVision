using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision.Algorithm
{

    public class CrackAlgorithm : InspAlgorithm
    {
        //XML로 파라미터 셋팅 값 저장할수있도록 해야함**********************************************************
        public int _binaryMin { get; set; } = 50;
        public int _binaryMax { get; set; } = 255;
        public int _areaMin { get; set; } = 100;
        public int _areaMax { get; set; } = 5000;

        private List<Rect> _findArea;



        public CrackAlgorithm()
        {
            //#ABSTRACT ALGORITHM#5 각 함수마다 자신의 알고리즘 타입 설정
            InspectType = InspectType.InspCrack;
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
            detectCrack(diffImage);


            IsInspected = true;
            return true;
        }

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

        private void detectCrack(Mat sourceImage)
        {
            Mat diffImage = sourceImage;


            #region crack은 외곽에만 생김, 내부영역 지워버림
            // 이미지 크기 계산
            int roiWidth = diffImage.Cols * 90 / 100;   // 전체 너비의 80%
            int roiHeight = diffImage.Rows * 90 / 100;  // 전체 높이의 80%

            // 중심을 기준으로 ROI 좌표 설정
            int x = (diffImage.Cols - roiWidth) / 2;  // 중앙 정렬 X 좌표
            int y = (diffImage.Rows - roiHeight) / 2; // 중앙 정렬 Y 좌표

            // ROI 생성
            Rect innerROI = new Rect(x, y, roiWidth, roiHeight);

            // 안쪽 영역을 검정색으로 채우기
            Cv2.Rectangle(diffImage, innerROI, new Scalar(0), -1);
            #endregion

            // 그레이스케일 변환
            Mat grayDiff = new Mat();
            Cv2.CvtColor(diffImage, grayDiff, ColorConversionCodes.BGR2GRAY);

            // 이진화 (Threshold 적용)
            Mat binaryDiff = new Mat();
            Cv2.Threshold(grayDiff, binaryDiff, _binaryMin, _binaryMax, ThresholdTypes.Binary);
            //Cv2.ImShow("binaryDiff", binaryDiff);

            // 윤곽선 검출
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binaryDiff, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            bool crackDetected = false;
            Mat resultImage = _srcImage.Clone();

            if (_findArea is null)
                _findArea = new List<Rect>();

            _findArea.Clear();

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area >= _areaMin && area <= _areaMax)  // 일정 크기 이상만 감지
                {
                    Rect boundingBox = Cv2.BoundingRect(contour);

                    // boundingBox의 좌상단 좌표 (시작 좌표)
                    Point2f topLeft = new Point2f(boundingBox.X, boundingBox.Y);

                    // 역변환하여 원본 좌표를 계산
                    Point2f originalTopLeft = perspectiveInverseTransform(topLeft, inversePerspectiveMatrix);

                    // boundingBox의 좌상단 좌표를 계산된 원본 좌표로 보정
                    Rect boundingBoxWithOffset = new Rect(
                        (int)(boundingBox.X + (originalTopLeft.X - topLeft.X)),
                        (int)(boundingBox.Y + (originalTopLeft.Y - topLeft.Y)),
                        boundingBox.Width,
                        boundingBox.Height
                    );

                    _findArea.Add(boundingBoxWithOffset);

                    // 원본 이미지에 사각형 그리기
                    //Cv2.Rectangle(resultImage, boundingBoxWithOffset, new Scalar(0, 100, 255), 2);  // 주황 박스 그리기
                    crackDetected = true;
                }
            }
            if (crackDetected)
            {
                Console.WriteLine("NG: crackDetected");
            }
            else
            {
                Console.WriteLine("OK: crack Not Detected");
            }
        }
        private Point2f perspectiveInverseTransform(Point2f point, Mat inverseMatrix)
        {
            // Homogeneous 좌표로 변환 (3x1 크기의 행렬로 설정)
            Mat homogenousPoint = new Mat(3, 1, MatType.CV_32F);
            homogenousPoint.Set<float>(0, 0, point.X);
            homogenousPoint.Set<float>(1, 0, point.Y);
            homogenousPoint.Set<float>(2, 0, 1); // 동차 좌표로 변환

            // inverseMatrix와 homogenousPoint의 데이터 타입을 맞추기 위해 변환 (CV_32F)
            if (inverseMatrix.Type() != MatType.CV_32F)
            {
                inverseMatrix.ConvertTo(inverseMatrix, MatType.CV_32F);
            }

            // 행렬 곱셈: 역변환 행렬을 적용
            Mat transformedPoint = inverseMatrix * homogenousPoint; // 행렬 곱셈

            // 역변환 후 좌표
            float x = transformedPoint.Get<float>(0, 0) / transformedPoint.Get<float>(2, 0);
            float y = transformedPoint.Get<float>(1, 0) / transformedPoint.Get<float>(2, 0);

            return new Point2f(x, y);
        }
    }
}
