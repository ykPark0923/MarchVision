﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision.Algorithm
{
    public class SootAlgorithm : InspAlgorithm
    {
        public int _binaryMin { get; set; } = 10;
        public int _binaryMax { get; set; } = 255;
        public int _areaMin { get; set; } = 4;
        public int _areaMax { get; set; } = 1000;

        private List<Rect> _findArea;

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

        public SootAlgorithm()
        {
            InspectType = InspectType.InspSoot;
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
            detectSoot(diffImage);


            IsInspected = true;
            return true;
        }

        private void detectSoot(Mat sourceImage)
        {
            Mat diffImage = sourceImage;

            // 그레이스케일 변환
            Mat grayDiff = new Mat();
            Mat grayDiff2 = new Mat();
            Cv2.CvtColor(_srcImage, grayDiff, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(diffImage, grayDiff2, ColorConversionCodes.BGR2GRAY);

            // 밝기 증가된 부분을 강조 (Soot 검출을 위해)
            Mat sootMask = new Mat();
            Cv2.Subtract(grayDiff2, grayDiff, sootMask); // 밝아진 부분을 강조

            // 이진화
            Mat binaryDiff = new Mat();
            Cv2.Threshold(sootMask, binaryDiff, _binaryMin, _binaryMax, ThresholdTypes.Binary);

            // Soot와 배경의 경계를 분명히 하기 위해 Canny 엣지 검출 적용
            Mat edges = new Mat();
            Cv2.Canny(binaryDiff, edges, 100, 200); // 엣지 검출

            Mat bgMask = new Mat();
            Cv2.Threshold(grayDiff, bgMask, 37, 255, ThresholdTypes.BinaryInv); // 어두운 부분은 배경으로 설정

            #region dent는 외곽에 안생김, 외곽지움
            // 외곽 영역 제거 (중심부만 남김)
            int roiWidth = diffImage.Cols * 80 / 100;   // 중심 너비 (86%)
            int roiHeight = diffImage.Rows * 79 / 100;  // 중심 높이 (79%)
            int x = (diffImage.Cols - roiWidth) / 2;
            int y = (diffImage.Rows - roiHeight) / 2;
            Rect innerROI = new Rect(x, y, roiWidth, roiHeight);

            // 외곽을 제거하기 위한 마스크 생성
            Mat mask = Mat.Zeros(sootMask.Size(), MatType.CV_8UC1); // sootMask 크기의 제로 행렬 생성
            Cv2.Rectangle(mask, innerROI, new Scalar(255), -1); // 중심 영역만 흰색으로 유지

            // 8. 중심 부분만 남기고 외곽은 제거
            Cv2.BitwiseAnd(sootMask, mask, sootMask); // 외곽을 제거하여 sootMask를 업데이트
            #endregion

            // 최종 Soot 검출 결과
            Mat finalMask = new Mat();
            Cv2.BitwiseAnd(sootMask, bgMask, finalMask); // 배경 제거된 최종 sootMask

            // 윤곽선 검출
            OpenCvSharp.Point[][] sootContours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(finalMask, out sootContours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            bool sootDetected = false;

            List<Rect> boundingBoxes = new List<Rect>();
            foreach (var contour in sootContours)
            {
                double area = Cv2.ContourArea(contour);
                if (area >= _areaMin && area <= _areaMax) // 작은 노이즈는 무시
                {
                    boundingBoxes.Add(Cv2.BoundingRect(contour)); // 윤곽선을 감싸는 바운딩 박스를 추가
                }
            }

            // 가까운 영역 병합
            List<Rect> mergedBoxes = MergeBoundingBoxes(boundingBoxes, 100);

            // 결과 표시
            Mat resultImage = _srcImage.Clone(); // 원본 이미지를 복제
            foreach (var box in mergedBoxes)
            {
                Cv2.Rectangle(resultImage, box, new Scalar(0, 0, 255), 2); // 병합된 박스를 빨간색으로 표시
                sootDetected = true;
            }


            if (sootDetected)
            {
                Console.WriteLine("NG: sootDetected");
            }
            else
            {
                Console.WriteLine("OK: soot Not Detected");
            }

        }



        // 병합할 바운딩 박스가 가까운지 판단하는 함수 (중심점 거리 기준)
        public List<Rect> MergeBoundingBoxes(List<Rect> boxes, int threshold, int minArea = 50)
        {
            if (boxes.Count == 0)
                return new List<Rect>();

            List<Rect> mergedBoxes = new List<Rect>(boxes);
            bool merged;

            do
            {
                merged = false;
                List<Rect> newBoxes = new List<Rect>();

                while (mergedBoxes.Count > 0)
                {
                    Rect current = mergedBoxes[0];
                    mergedBoxes.RemoveAt(0);

                    for (int i = 0; i < mergedBoxes.Count; i++)
                    {
                        if (IsClose(current, mergedBoxes[i], threshold)) // 두 박스가 가까우면
                        {
                            // 두 박스를 병합
                            current = UnionRect(current, mergedBoxes[i]);
                            mergedBoxes.RemoveAt(i);
                            merged = true;
                            i--; // 리스트 크기 감소에 따른 인덱스 수정
                        }
                    }

                    // 최소 크기 기준을 만족하는 박스만 추가
                    if (current.Width * current.Height >= minArea)
                    {
                        newBoxes.Add(current);
                    }
                }

                mergedBoxes = newBoxes;
            } while (merged); // 더 이상 병합이 발생하지 않을 때까지 반복

            return mergedBoxes;
        }
        private bool IsClose(Rect box1, Rect box2, int threshold)
        {
            int centerX1 = box1.X + box1.Width / 2;
            int centerY1 = box1.Y + box1.Height / 2;
            int centerX2 = box2.X + box2.Width / 2;
            int centerY2 = box2.Y + box2.Height / 2;

            double distance = Math.Sqrt(Math.Pow(centerX1 - centerX2, 2) + Math.Pow(centerY1 - centerY2, 2));

            return distance < threshold; // 두 박스의 중심점 거리가 threshold 이하이면 가까운 것으로 판단
        }

        // 두 박스를 병합하는 함수
        private Rect UnionRect(Rect box1, Rect box2)
        {
            int x = Math.Min(box1.X, box2.X);
            int y = Math.Min(box1.Y, box2.Y);
            int width = Math.Max(box1.X + box1.Width, box2.X + box2.Width) - x;
            int height = Math.Max(box1.Y + box1.Height, box2.Y + box2.Height) - y;

            return new Rect(x, y, width, height); // 병합된 박스를 반환
        }
    }
}