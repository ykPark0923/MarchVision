﻿using JidamVision.Setting;
using JidamVision.Util;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using static MessagingLibrary.Message;

namespace JidamVision.Sequence
{
    public enum SeqCmd
    {
        None = 0,
        OpenRecipe,
        InspReady,
        VisionReady,
        InspStart,
        InspDone,
        InspEnd,
    }

    public enum Vision2Mmi
    {
        None = 0,
        InspStart,
        InspReady,
        InspDone,
        InspEnd,
        ModeLoaded,
        Error
    }

    public enum MmiSeq
    {
        None = 0,
        InspReady,
        InspStart,
        InspDone,
        Error

    }

    public class VisionSequence : IDisposable
    {
        private static VisionSequence _sequence = null;

        public static VisionSequence Inst
        {
            get
            {
                if (_sequence == null)
                {
                    _sequence = new VisionSequence();
                }
                return _sequence;
            }
        }

        #region Event
        public delegate void XEventHandler<T1, T2>(object sender, T1 e1, T2 e2);
        public event XEventHandler<SeqCmd, object> SeqCommand = delegate { };
        #endregion

        private Message _message = new Message();
        private Communicator _communicator = null;

        //private Thread _sequenceThread = null;
        private bool _isRun = true;
        private MmiSeq _mmiState = MmiSeq.None;

        private string _lastErrMsg;

        public bool IsMmiConnected { get; set; } = false;

        public VisionSequence()
        {

        }

        private bool InitCommunicator()
        {
            if (SettingXml.Inst.CommType == CommunicatorType.WCF)
            {
                SLogger.Write("WCF 통신 초기화!");

                string ipAddr = SettingXml.Inst.CommIP;

                #region WCF

                _communicator = new Communicator();
                _communicator.ReceiveMessage += Communicator_ReceiveMessage;
                _communicator.Closed += Communicator_Closed;
                _communicator.Opened += Communicator_Opened;

                _communicator.Create(CommunicatorType.WCF, ipAddr);

                if (_communicator.State == System.ServiceModel.CommunicationState.Opened)
                    _communicator.SendMachineInfo();

                if (_communicator.State != System.ServiceModel.CommunicationState.Opened)
                {
                    SLogger.Write("MMI 연결 실패!", SLogger.LogType.Error);
                    return false;
                }

                #endregion WCF
            }
            else
            {
                return false;
            }

            return true;
        }

        public void InitSequence()
        {
            if (!InitCommunicator())
                return;

            //통신 초기화
            //통신 이벤트 등록
            _message.MachineName = SettingXml.Inst.MachineName;

            //현재 사용하지 않음
            //_sequenceThread = new Thread(SequenceThread);
            //_sequenceThread.IsBackground = true;
            //_sequenceThread.Start();
        }

        public void ResetCommunicator(Communicator communicator)
        {
            if (_communicator is null)
                return;

            _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
            _communicator = communicator;
            _communicator.ReceiveMessage += Communicator_ReceiveMessage;
        }

        private bool SendMessage(MmiMessageInfo message)
        {
            if (_communicator is null)
                return false;

            _message.Time = string.Format($"{DateTime.Now:HH:mm:ss:fff}");
            return _communicator.SendMessage(message);
        }

        private void SequenceThread()
        {
            while (_isRun)
            {
                UpdateSeqState();
                Thread.Sleep(1);
            }
        }

        private void UpdateSeqState()
        {
            switch (_mmiState)
            {
                case MmiSeq.None:
                    {
                    }
                    break;
                case MmiSeq.Error:
                    {
                        _message.Command = Message.MessageCommand.Error;
                        _message.Status = CommandStatus.Fail;
                        _message.ErrorMessage = _lastErrMsg;
                        SendMessage(_message);

                        _mmiState = MmiSeq.None;
                    }
                    break;
            }
        }

        private void Communicator_ReceiveMessage(object sender, Message e)
        {
            switch (e.Command)
            {
                case Message.MessageCommand.Reset:
                    {
                        ResetSequence();
                    }
                    break;
                case Message.MessageCommand.OpenRecipe:
                    {
                        SeqCommand(this, SeqCmd.OpenRecipe, (object)e.Tool);
                    }
                    break;
                case Message.MessageCommand.InspReady:
                    {
                        SeqCommand(this, SeqCmd.InspReady, e);
                    }
                    break;
                case Message.MessageCommand.InspStart:
                    {
                        SeqCommand(this, SeqCmd.InspStart, e);
                    }
                    break;
                case Message.MessageCommand.InspEnd:
                    {
                        SeqCommand(this, SeqCmd.InspEnd, e);
                    }
                    break;
            }
        }

        //비젼 -> MMI에게 Command 전송
        public void VisionCommand(Vision2Mmi visionCmd, Object e)
        {
            switch (visionCmd)
            {
                case Vision2Mmi.ModeLoaded:
                    {
                        string errMsg = (string)e;
                        if (errMsg != "")
                        {
                            _lastErrMsg = errMsg;
                            SendError();
                            break;
                        }

                        _message.Command = Message.MessageCommand.OpenRecipe;
                        _message.Status = CommandStatus.Success;
                        SendMessage(_message);
                    }
                    break;
                case Vision2Mmi.InspReady:
                    {
                        string errMsg = (string)e;
                        if (errMsg != "")
                        {
                            _lastErrMsg = errMsg; 
                            SendError();
                            break;
                        }

                        _message.Command = Message.MessageCommand.InspReady;
                        _message.Status = CommandStatus.Success;
                        SendMessage(_message);
                    }
                    break;
                case Vision2Mmi.InspDone:
                    {
                        string errMsg = (string)e;

                        if (errMsg != "")
                        {
                            _lastErrMsg = errMsg;
                            SendError();
                            break;
                        }

                        _message.Command = Message.MessageCommand.InspDone;
                        _message.Status = CommandStatus.Success;
                        SendMessage(_message);
                    }
                    break;
            }
        }

        private void SendError()
        {
            _message.Command = Message.MessageCommand.Error;
            _message.Status = CommandStatus.Fail;
            _message.ErrorMessage = _lastErrMsg;
            SendMessage(_message);
        }

        private void ResetSequence()
        {
            _mmiState = MmiSeq.None;
        }


        private void Communicator_Opened(object sender, EventArgs e)
        {
            SLogger.Write($"MMI에 연결되었습니다.");
            IsMmiConnected = true;
        }

        private void Communicator_Closed(object sender, EventArgs e)
        {
            SLogger.Write("서버와의 연결이 끊어졌습니다.", SLogger.LogType.Error);
            IsMmiConnected = false;
        }

        #region Disposable
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isRun = false;
                    if (_communicator != null)
                    {
                        _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
                        _communicator.Opened -= Communicator_Opened;
                        _communicator.Closed -= Communicator_Closed;
                        _communicator = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion //Disposable
    }
}
