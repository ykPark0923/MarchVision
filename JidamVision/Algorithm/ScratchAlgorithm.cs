﻿using System;
using System.Collections.Generic;
using JidamVision.Algorithm;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace JidamVision.Algorithm
{
    // ScratchAlgorithm은 InspAlgorithm을 상속받아 스크래치 탐지 기능을 구현합니다.
    public class ScratchAlgorithm : InspAlgorithm
    {
        public int _binaryMin { get; set; } = 50;
        public int _binaryMax { get; set; } = 255;
        public int _areaMin { get; set; } = 10;
        public int _areaMax { get; set; } = 600;
        public int _ratioMin { get; set; } = 2;
        public int _ratioMax { get; set; } = 25;

        private List<Rect> _findArea;

        public ScratchAlgorithm()
        {
            InspectType = InspectType.InspScratch;
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
            detectScratch(diffImage);

            IsInspected = true;
            return true;
        }

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

        private void detectScratch(Mat sourceImage)
        {
            Mat diffImage = sourceImage;

            #region Scratch는 외곽에 안생김, 외곽지움
            // 외곽 영역 제거 (중심부만 남김)
            int roiWidth = diffImage.Cols * 80 / 100;   // 중심 너비 (86%)
            int roiHeight = diffImage.Rows * 80 / 100;  // 중심 높이 (79%)
            int x = (diffImage.Cols - roiWidth) / 2;
            int y = (diffImage.Rows - roiHeight) / 2;
            Rect innerROI = new Rect(x, y, roiWidth, roiHeight);

            // 해당 영역만큼 자르기
            Mat roiImage = new Mat(diffImage, innerROI);
            #endregion

            // 그레이스케일 변환
            Mat grayDiff = new Mat();
            Cv2.CvtColor(diffImage, grayDiff, ColorConversionCodes.BGR2GRAY);

            // 이진화 (Threshold 적용)
            Mat binaryDiff = new Mat();
            Cv2.Threshold(grayDiff, binaryDiff, _binaryMin, _binaryMax, ThresholdTypes.Binary);

            // 커널을 사용한 형태학적 연산 (닫기 연산)
            Mat kernel = new Mat(5, 5, MatType.CV_8U, Scalar.All(30));
            Mat closedDiff = new Mat();
            Cv2.MorphologyEx(binaryDiff, closedDiff, MorphTypes.Close, kernel);

            // 윤곽선 검출
            OpenCvSharp.Point[][] scratchContours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(closedDiff, out scratchContours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // 스크래치 발견 여부를 기록
            bool scratchDetected = false;
            // 결과 이미지를 복사 (원본 이미지를 수정하지 않음)
            Mat resultImage = _srcImage.Clone();

            foreach (var contour in scratchContours)
            {
                // 윤곽선의 면적 계산
                double area = Cv2.ContourArea(contour);
                if (area >= _areaMin && area <= _areaMax)  // 면적 조건 확인
                {
                    // 윤곽선의 최소 면적 직사각형 찾기
                    RotatedRect box = Cv2.MinAreaRect(contour);
                    float aspectRatio = Math.Max(box.Size.Width, box.Size.Height) / Math.Min(box.Size.Width, box.Size.Height);

                    if (aspectRatio >= _ratioMin && aspectRatio <= _ratioMax)  // 비율 조건 확인
                    {
                        // 해당 윤곽선의 바운딩 박스를 그리기
                        Rect boundingBox = Cv2.BoundingRect(contour);
                        boundingBox.X += x;
                        boundingBox.Y += y;

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

                        // 결과 이미지에 파란색 사각형 그리기
                        Cv2.Rectangle(resultImage, boundingBox, new Scalar(255, 0, 0), 2);
                        scratchDetected = true;
                    }
                }


                if (scratchDetected)
                {
                    Console.WriteLine("NG: scratchDetected");
                }
                else
                {
                    Console.WriteLine("OK: scratch Not Detected");
                }
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