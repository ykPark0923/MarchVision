﻿using Common.Util.Helpers;
using JidamVision.Algorithm;
using JidamVision.Core;
using JidamVision.Setting;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JidamVision.Teach
{
    /*
    #MODEL# - <<<티칭 정보를 저장,관리하기 위한 클래스 만들기>>> 
    검사에 필요한 모든 데이터를 관리하는 클래스
    InspWindow 정보와 검사 알고리즘 정보를 모두 가지고 있음
    */

    //#MODEL#3 모델 클래스 생성
    public class Model
    {

        //#MODEL SAVE#1 모델 정보 저장을 위해 추가한 프로퍼티
        public string ModelName { get; set; } = "";
        public string ModelInfo { get; set; } = "";
        public string ModelPath { get; set; } = "";

        //#MODEL#1 InspStage에 있던 InspWindowList 위치를 이곳으로 변경
        [XmlElement("InspWindow")]
        public List<InspWindow> InspWindowList {  get; set; } 
            
        public Model()
        {
            InspWindowList = new List<InspWindow>();
        }

        //#MODEL#4 새로운 InspWindow를 추가할때
        public InspWindow AddInspWindow(InspWindowType windowType)
        {
            InspWindow inspWindow = InspWindowFactory.Inst.Create(windowType);
            InspWindowList.Add(inspWindow);

            return inspWindow;
        }

        //#MODEL#5 기존 InspWindow를 삭제할때
        public bool DelInspWindow(InspWindow inspWindow)
        {
            if(InspWindowList.Contains(inspWindow))
            {
                InspWindowList.Remove(inspWindow);
                return true;
            }
            return false;
        }

        public GroupWindow AddGroupWindow(List<InspWindow> inspWindowList)
        {
            GroupWindow groupWindow = (GroupWindow)InspWindowFactory.Inst.Create(InspWindowType.Group);
            if (groupWindow is null)
                return null;

            foreach (var inspWindow in inspWindowList)
                groupWindow.AddMember(inspWindow);

            InspWindowList.Add(groupWindow);

            return groupWindow;
        }

        public bool BreakGroupWindow(GroupWindow groupWindow)
        {
            if (groupWindow is null)
                return false;

            if(InspWindowList.Contains(groupWindow))
            {
                InspWindowList.Remove(groupWindow);
            }

            foreach (var inspWindow in groupWindow.Members)
                InspWindowList.Add(inspWindow);

            return true;
        }

        //#MODEL SAVE#2 모델 생성,열기,저장을 위한 함수 구현

        //신규 모델 생성
        public void CreateModel(string path, string modelName, string modelInfo)
        {
            ModelPath = path;
            ModelName = modelName;
            ModelInfo = modelInfo;
        }

        //모델 로딩함수
        public Model Load(string path)
        {
            Model model = XmlHelper.LoadXml<Model>(path);
            if (model == null)
                return null;

            return model;
        }

        //모델 저장함수
        public void Save()
        {
            XmlHelper.SaveXml(ModelPath, this);
        }

        //모델 다른 이름으로 저장함수
        public void SaveAs(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (Directory.Exists(filePath) == false)
            {
                ModelPath = Path.Combine(filePath, fileName + ".xml");
                ModelName = fileName;
                Save();
            }
        }
    }
}
