using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenCvSharp;

namespace MakeSVMFile
{
    class Program
    {
        static void Main(string[] args)
        {
            MakeSvmFile svm_make = new MakeSvmFile();
            svm_make.Exec(args[0],args[1]);
        }
    }

    class MakeSvmFile
    {
        public void Exec(String input, String output)
        {
            this.InputFileList = input;
            this.OutPutFolda = output;

            //学習するファイルを読み込む
            ReadFileList();

            FaceFeature face_feature = new FaceFeature();
            face_feature.FaceList = this.FaceList;
            face_feature.IDList = this.IDList;
            face_feature.OutPutFolda = this.OutPutFolda;

            //特徴点を出す
            face_feature.DetectFacePoint();


            //学習実行
            SVMManage svm_manage = new SVMManage();
            svm_manage.LearningExec(face_feature.FeatuerValueList);

            //学習ファイルをxmlに書き出す
            String xml_name = @"SvmLearning.xml";
            svm_manage.svm.Save(xml_name);
//            svm_manage.SVMJudge();
            
        }

        //顔写真リストファイルを読み込み
        private void ReadFileList()
        {
            string read_list = @"";
            read_list = this.InputFileList;
            //リストファイルと読みこんでファイル名をとる
            using (StreamReader sr = new StreamReader(read_list))
            {
                //1行づつ読み込む
                while (sr.Peek() > -1)
                {
                    // カンマ区切りで分割して配列に格納する
                    string[] stArrayData = sr.ReadLine().Split(',');

                    this.FaceList.Add(stArrayData[0]);
                    this.IDList.Add(int.Parse(stArrayData[1]));
                }
                //閉じる
                sr.Close();
            }
        }

        List<string> FaceList = new List<string>();
        List<int> IDList = new List<int>();//特徴量とセットで使うID

        String InputFileList = @"";
        public String OutPutFolda = @"";

    }
}
