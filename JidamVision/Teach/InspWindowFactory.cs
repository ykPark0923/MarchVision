using JidamVision.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Teach
{
    //#MODEL#2 InspWindow를 유니크한 이름으로 관리하기 위한, InspWindow 생성 클래스
    public class InspWindowFactory
    {
        #region Singleton Instance
        private static readonly Lazy<InspWindowFactory> _instance = new Lazy<InspWindowFactory>(() => new InspWindowFactory());

        public static InspWindowFactory Inst
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion

        //같은 타입의 일련번호 관리를 위한 딕셔너리
        private Dictionary<string, int> _windowTypeNo = new Dictionary<string, int>();

        public InspWindowFactory() { }

        //InspWindow를 생성하기 위해, 타입을 입력받아, 생성된 InspWindow 반환
        public InspWindow Create(InspWindowType windowType)
        {
            string name, prefix;
            if (!GetWindowName(windowType, out name, out prefix))
                return null;

            InspWindow inspWindow = null;

            if(InspWindowType.Dent == windowType)
                inspWindow = new GroupWindow(name);
            else
                inspWindow = new InspWindow(windowType,name);

            if(inspWindow is null) 
                return null;

            if(!_windowTypeNo.ContainsKey(name))
                _windowTypeNo[name] = 0;

            int curID = _windowTypeNo[name];
            curID++;

            inspWindow.UID = string.Format("{0}_{1:D6}", prefix, curID);

            _windowTypeNo[name] = curID;

            AddInspAlgorithm(inspWindow);

            return inspWindow;
        }

        private bool AddInspAlgorithm(InspWindow inspWindow)
        {
            switch(inspWindow.InspWindowType)
            {
                case InspWindowType.Crack:
                    inspWindow.AddInspAlgorithm(InspectType.InspCrack);
                    break;
                case InspWindowType.Soot:
                    inspWindow.AddInspAlgorithm(InspectType.InspSoot);
                    break;
                case InspWindowType.Scratch:
                    inspWindow.AddInspAlgorithm(InspectType.InspScratch);
                    break;
                case InspWindowType.Dent:
                    inspWindow.AddInspAlgorithm(InspectType.InspDent);
                    break;
                //case InspWindowType.Package:
                //    inspWindow.AddInspAlgorithm(InspectType.InspMatch);
                //    inspWindow.AddInspAlgorithm(InspectType.InspBinary);
                //    break;
                //case InspWindowType.Chip:
                //    inspWindow.AddInspAlgorithm(InspectType.InspMatch);
                //    inspWindow.AddInspAlgorithm(InspectType.InspBinary);
                //    break;
                //case InspWindowType.Pad:
                //    inspWindow.AddInspAlgorithm(InspectType.InspMatch);
                //    inspWindow.AddInspAlgorithm(InspectType.InspBinary);
                //    break;
            }

            return true;
        }

        //타입을 입력하면, 해당 타입의 이름과 UID 이름 반환
        private bool GetWindowName(InspWindowType windowType, out string name, out string prefix)
        {
            name = string.Empty;
            prefix = string.Empty;
            switch (windowType)
            {
                case InspWindowType.Scratch:
                    name = "Global";
                    prefix = "GLB";
                    break;
                case InspWindowType.Dent:
                    name = "Group";
                    prefix = "GRP";
                    break;
                case InspWindowType.Crack:
                    name = "Base";
                    prefix = "BAS";
                    break;
                case InspWindowType.Soot:
                    name = "Body";
                    prefix = "BDY";
                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}
