using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenCvSharp;

namespace makeLearingFile
{
    class Program
    {
        static void Main(string[] args)
        {
            FaceImageManage manage = new FaceImageManage();
            manage.exec(args[0],args[1]);
        }

        class FaceImageManage
        {
            //コンストラクタ
            public FaceImageManage()
            {

            }

            public void exec(String input_list, String folda_name)
            {
                this.InputListFileName = input_list;
                this.OutputFoldaName = folda_name;

                //ファイルリストの読み込み
                //ファイルリストの読み込み
                ReadFileListTest();

                //顔部分の切り出し
                ExtractFace();
            }

            //顔部分を抜き出す
            private void ExtractFace()
            {
                //カスケード分類器の特徴量を取得する
                CvHaarClassifierCascade cascade = CvHaarClassifierCascade.FromFile(@"C:\opencv2.4.8\sources\data\haarcascades\haarcascade_frontalface_alt.xml");
                CvMemStorage strage = new CvMemStorage(0);   // メモリを確保

                //フジキンリストの処理
                int read_count = 0;
                while( read_count < this.FaceFileList.Count())
                {
                    string input_file_path = this.FaceFileList[read_count];

                    using (IplImage img = new IplImage(input_file_path))
                    {
                        //グレースケールに変換
                        using( IplImage gray_image = Cv.CreateImage(new CvSize(img.Width,img.Height),BitDepth.U8,1) )
                        {
                            Cv.CvtColor(img, gray_image, ColorConversion.BgrToGray);

                            //発見した矩形
                            var result = Cv.HaarDetectObjects(gray_image, cascade, strage);
                            for (int i = 0; i < result.Total; i++)
                            {
                                //矩形の大きさに書き出す
                                CvRect rect = result[i].Value.Rect;
                                Cv.Rectangle(img, rect, new CvColor(255, 0, 0));

                                //矩形部分をファイル出力する
                                img.ROI = rect;
                                string out_name = this.OutputFoldaName + @"\out" +  read_count + @"_" + i + @".bmp";
                                Cv.SaveImage(out_name, img);
                            }
                        }
                    }
                    read_count++;
                }
            }


            //顔写真リストファイルを読み込み
            private void ReadFileListTest()
            {
                //リストファイルと読みこんでファイル名をとる
                using (StreamReader sr = new StreamReader(this.InputListFileName))
                {
                    //1行づつ読み込む
                    while (sr.Peek() > -1)
                    {
                        FaceFileList.Add(sr.ReadLine());
                    }
                    //閉じる
                    sr.Close();
                }
            }

            List<string> FaceFileList = new List<string>();

            String InputListFileName = @"";
            String OutputFoldaName = @"";
        }
    }
}
