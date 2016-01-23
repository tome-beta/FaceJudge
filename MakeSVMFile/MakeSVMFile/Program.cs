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
            svm_make.Exec();
        }
    }

    class MakeSvmFile
    {
        public void Exec()
        {
            //学習するファイルを読み込む
            ReadFileList();

            //特徴点を出す
            DetectFacePoint();

        }

        //画像ファイルから特徴点をだす
        private void DetectFacePoint()
        {
            string eye_cascade_xml = @"C:\opencv2.4.8\sources\data\haarcascades\haarcascade_eye.xml";
            string nose_cascade_xml = @"C:\opencv2.4.8\sources\data\haarcascades\haarcascade_mcs_nose.xml";
            string mouth_cascade_xml = @"C:\opencv2.4.8\sources\data\haarcascades\haarcascade_mcs_mouth.xml";

            CvMemStorage strage = new CvMemStorage(0);   // メモリを確保
            CvHaarClassifierCascade eye_cascade = CvHaarClassifierCascade.FromFile(eye_cascade_xml);
            CvHaarClassifierCascade nose_cascade = CvHaarClassifierCascade.FromFile(nose_cascade_xml);
            CvHaarClassifierCascade mouth_cascade = CvHaarClassifierCascade.FromFile(mouth_cascade_xml);


            int read_count = 0;
            while (read_count < this.KubotaList.Count())
            {
                string input_file_path = this.KubotaList[read_count];

                using (IplImage img = new IplImage(input_file_path))
                {
                    //グレースケールに変換
                    IplImage gray_image = Cv.CreateImage(new CvSize(img.Width,img.Height),BitDepth.U8,1);
                    Cv.CvtColor(img, gray_image, ColorConversion.BgrToGray);

                    //発見した矩形
                    var eye_result = Cv.HaarDetectObjects(gray_image,eye_cascade, strage);
                    var nose_result = Cv.HaarDetectObjects(gray_image, nose_cascade, strage);
                    var mouth_result = Cv.HaarDetectObjects(gray_image, mouth_cascade, strage);

                    //目の結果
                    for (int i = 0; i < eye_result.Total; i++)
                    {
                        //矩形の大きさに書き出す
                        CvRect rect = eye_result[i].Value.Rect;
                        Cv.Rectangle(img, rect, new CvColor(255, 0, 0));
                    }

                    //鼻の検出
                    for (int i = 0; i < nose_result.Total; i++)
                    {
                        //矩形の大きさに書き出す
                        CvRect rect = nose_result[i].Value.Rect;
                        Cv.Rectangle(img, rect, new CvColor(255, 255, 0));
                    }

                    //口の検出
                    for (int i = 0; i < mouth_result.Total; i++)
                    {
                        //矩形の大きさに書き出す
                        CvRect rect = mouth_result[i].Value.Rect;
                        Cv.Rectangle(img, rect, new CvColor(255, 0, 255));
                    }


                    using (new CvWindow(img))
                    {
                        Cv.WaitKey();
                    }                           

                    read_count++;
                }
            }


        }

        //顔写真リストファイルを読み込み
        private void ReadFileList()
        {
            string read_list = @"";
            read_list = @"I:\myprog\github\kubokinJudge\data\kubota_face\kubota_face_list.txt";

            //リストファイルと読みこんでファイル名をとる
            using (StreamReader sr = new StreamReader(read_list))
            {
                //1行づつ読み込む
                while (sr.Peek() > -1)
                {
                    this.KubotaList.Add(sr.ReadLine());
                }
                //閉じる
                sr.Close();
            }
        }

        List<string> KubotaList = new List<string>();
    }
}
