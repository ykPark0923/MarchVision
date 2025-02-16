using JidamVision.Grab;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Core
{
    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage
    {
        public static readonly int MAX_GRAB_BUF = 5;

        private HikRobotCam _hikRobotCam = null;
        private ImageSpace _imageSpace = null;

        public HikRobotCam MultiGrab
        {
            get => _hikRobotCam;
        }

        public ImageSpace ImageSpace
        {
            get => _imageSpace;
        }

        public InspStage() { }

        public bool Initialize()
        {
            _imageSpace = new ImageSpace();

            _hikRobotCam = new HikRobotCam();
            if (_hikRobotCam.InitGrab() == true)
            {
                _hikRobotCam.TransferCompleted += _multiGrab_TransferCompleted;

                InitModelGrab(MAX_GRAB_BUF);
            }

            return true;
        }


        public void InitModelGrab(int bufferCount)
        {
            if (_hikRobotCam == null)
                return;

            int pixelBpp = 8;
            _hikRobotCam.GetPixelBpp(out pixelBpp);

            int inspectionWidth;
            int inspectionHeight;
            int inspectionStride;
            _hikRobotCam.GetResolution(out inspectionWidth, out inspectionHeight, out inspectionStride);
            
            if (_imageSpace != null)
            {
                _imageSpace.SetImageInfo(pixelBpp, inspectionWidth, inspectionHeight, inspectionStride);
            }

            SetBuffer(bufferCount);

        }

        public void SetBuffer(int bufferCount)
        {
            if (_hikRobotCam == null)
                return;

            if (_imageSpace.BufferCount == bufferCount)
                return;

            _imageSpace.InitImageSpace(bufferCount);
            _hikRobotCam.InitBuffer(bufferCount);

            for (int i = 0; i < bufferCount; i++)
            {
                _hikRobotCam.SetBuffer(
                    _imageSpace.GetInspectionBuffer(i),
                    _imageSpace.GetnspectionBufferPtr(i),
                    _imageSpace.GetInspectionBufferHandle(i),
                    i);
            }
        }

        public void Grab(int bufferIndex)
        {
            if (_hikRobotCam == null)
                return;

            _hikRobotCam.Grab(bufferIndex, true);
        }

        private void _multiGrab_TransferCompleted(object sender, object e)
        {
            int bufferIndex = (int)e;
            Console.WriteLine($"_multiGrab_TransferCompleted {bufferIndex}");

            _imageSpace.Split(bufferIndex);
                        
            DisplayGrabImage(bufferIndex);
        }

        private void DisplayGrabImage(int bufferIndex)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDisplay();
            }
        }

    }
}
