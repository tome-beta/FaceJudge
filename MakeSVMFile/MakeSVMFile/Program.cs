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
        const int SMALL_IMAGE_LIMIT = 100;  //顔画像を拡大対象にする
        const int IMAGE_RESIZE_RATE = 4;    //拡大率

        
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

                //リストにあるファイルを一枚づつデータにする
                using (IplImage img = new IplImage(input_file_path))
                {
                    IplImage tmp_image;
                    //サイズが小さければ拡大して使う
                    if(img.Size.Width < SMALL_IMAGE_LIMIT)
                    {
                        tmp_image = Cv.CreateImage(new CvSize(img.Width * IMAGE_RESIZE_RATE, img.Height * IMAGE_RESIZE_RATE), BitDepth.U8, 3);
                        Cv.Resize(img,tmp_image);
                    }
                    else
                    {
                        tmp_image = Cv.CreateImage(new CvSize(img.Width, img.Height), BitDepth.U8, 3);
                        Cv.Resize(img, tmp_image);
                    }

                    //グレースケールに変換
                    IplImage gray_image = Cv.CreateImage(new CvSize(tmp_image.Width, tmp_image.Height), BitDepth.U8, 1);
                    Cv.CvtColor(tmp_image, gray_image, ColorConversion.BgrToGray);

                    //発見した矩形
                    //TODO まゆの位置も有る方がいいかも
                    this.EyeResult = Cv.HaarDetectObjects(gray_image, eye_cascade, strage);
                    this.NoseResult = Cv.HaarDetectObjects(gray_image, nose_cascade, strage);
                    this.MouthResult = Cv.HaarDetectObjects(gray_image, mouth_cascade, strage);

                    //初期化
                    DataInit();


                    //デバッグ用の表示
//                    DebugPrint(tmp_image, read_count);

                    //左眼、右目、鼻、口の矩形を確定させる。
                    DecidePartsRect(gray_image);

                    //パーツ確定後
//                    DebugPrint2(gray_image, read_count);


                    PartsRectInfo parts_info;
                    parts_info.RightEye = this.RightEyeRect;
                    parts_info.LeftEye = this.LeftEyeRect;
                    parts_info.Nose = this.NoseRect;
                    parts_info.Mouth = this.MouthRect;

                    FeatureValue feature_value;
                    //基点を作る
                    MakeBasePoint(gray_image, ref parts_info, out feature_value);


                    read_count++;
                }
            }
        }

        /// <summary>
        /// パラメータの初期化
        /// </summary>
        private void DataInit()
        {
            this.LeftEyeRect = new CvRect(0, 0, 0, 0);
            this.RightEyeRect = new CvRect(0, 0, 0, 0);
            this.MouthRect = new CvRect(0, 0, 0, 0);
            this.NoseRect = new CvRect(0, 0, 0, 0);

        }

        /// <summary>
        /// パーツの矩形を確定させる
        /// </summary>
        private void DecidePartsRect(IplImage img)
        {
            //両目の矩形を探す　左眼は画像の半分より左で逆は右
            int image_half_x = img.Width / 2;
            int image_half_y = img.Height / 2;
            for (int i = 0; i < this.EyeResult.Total; i++)
            {
                CvRect rect = this.EyeResult[i].Value.Rect;
                int rect_size = rect.Height * rect.Width;

                //右目
                if (rect.X < image_half_x)
                {
                    //サイズの大きい矩形を採用
                    if (this.RightEyeRect.Width * this.RightEyeRect.Height <= rect_size)
                    {
                        this.RightEyeRect = rect;
                    }
                }
            }

            for (int i = 0; i < this.EyeResult.Total; i++)
            {
                CvRect rect = this.EyeResult[i].Value.Rect;
                int rect_size = rect.Height * rect.Width;

                //左目
                if (rect.X >= image_half_x)
                {
                    //サイズの大きい矩形を採用
                    if (this.LeftEyeRect.Width * this.LeftEyeRect.Height <= rect_size)
                    {
                        this.LeftEyeRect = rect;
                    }
                }
            }

            //鼻の矩形を確定させる。
            for (int i = 0; i < this.NoseResult.Total; i++)
            {
                CvRect rect = this.NoseResult[i].Value.Rect;
                int rect_size = rect.Height * rect.Width;

                //画像の中央に位置するはず
                if(rect.X < image_half_x && image_half_x < rect.X + rect.Width)
                {
                    if(rect.Y < image_half_y && image_half_y < rect.Y + rect.Height)
                    {
                        //サイズの大きい矩形を採用
                        if (this.NoseRect.Width * this.NoseRect.Height <= rect_size)
                        {
                            this.NoseRect = rect;
                        }
                    }
                }
            }

            //口の矩形を確定させる。
            for (int i = 0; i < this.MouthResult.Total; i++)
            {
                CvRect rect = this.MouthResult[i].Value.Rect;
                int rect_size = rect.Height * rect.Width;

                //画像の下半分にあるはず
                if(image_half_y < rect.Y)
                {
                    //サイズの大きい矩形を採用
                    if (this.MouthRect.Width * this.MouthRect.Height <= rect_size)
                    {
                        this.MouthRect = rect;
                    }
                }
            }
        }

        /// <summary>
        /// 目と目の間の座標。基点を作る
        /// </summary>
        private bool MakeBasePoint(IplImage img,ref PartsRectInfo input_info,out FeatureValue output_info)
        {
            //仮に代入
            output_info.basepoint = new CvPoint(0, 0);
            output_info.LeftEyeL = new CvPoint(0, 0);
            output_info.LeftEyeR = new CvPoint(0, 0);
            output_info.RightEyeL = new CvPoint(0, 0);
            output_info.RightEyeR = new CvPoint(0, 0);
            output_info.NoseL = new CvPoint();
            output_info.NoseR = new CvPoint();
            output_info.MouthL = new CvPoint();
            output_info.MouthR = new CvPoint();

            //パーツがすべてそろっているかの確認
            if (input_info.RightEye.X == 0)
            {
                return false;
            }
            if (input_info.LeftEye.X == 0)
            {
                return false;
            }
            if (input_info.Nose.X == 0)
            {
                return false;
            }
            if (input_info.Mouth.X == 0)
            {
                return false;
            }

            //瞳の間の場所を基点として各パーツとの比率をとる
            //（パーツ座標と基点との距離）/瞳の間の距離を学習データとする
            int LeftEyeCenterX = input_info.LeftEye.X + input_info.LeftEye.Width / 2;
            int LeftEyeCenterY = input_info.LeftEye.Y + input_info.LeftEye.Height / 2;
            int RightEyeCenterX = input_info.RightEye.X + input_info.RightEye.Width / 2;
            int RightEyeCenterY = input_info.RightEye.Y + input_info.RightEye.Height / 2;

            output_info.basepoint.X = LeftEyeCenterX - RightEyeCenterX / 2;
            output_info.basepoint.Y = LeftEyeCenterY - RightEyeCenterY / 2;

            //右目の中心と左目の中心を結んだ線の中点が基準点。

            //基準点から各パーツの右端、左端までの距離を特徴量とする

            return true;
        }


        private void DebugPrint2(IplImage img, int count)
        {
            //目の結果
            Cv.Rectangle(img, this.LeftEyeRect, new CvColor(255, 0, 0));
            Cv.Rectangle(img, this.RightEyeRect, new CvColor(255, 0, 0));

            //鼻の結果
            Cv.Rectangle(img, this.NoseRect, new CvColor(0, 255, 0));

            //口の結果
            Cv.Rectangle(img, this.MouthRect, new CvColor(0, 0, 255));

            using (new CvWindow(img))
            {
                string out_name = @"out\decide_parts" + count + @".jpeg";
                Cv.SaveImage(out_name, img);
                Cv.WaitKey();
            }                           

        }

        /// <summary>
        /// デバッグ用の表示
        /// </summary>
        /// <param name="img"></param>
        /// <param name="count"></param>
        private void DebugPrint(IplImage img,int count)
        {
            //目の結果
            for (int i = 0; i < this.EyeResult.Total; i++)
            {
                //矩形の大きさに書き出す
                CvRect rect = EyeResult[i].Value.Rect;
                Cv.Rectangle(img, rect, new CvColor(255, 0, 0));
            }

            //鼻の検出
            for (int i = 0; i < NoseResult.Total; i++)
            {
                //矩形の大きさに書き出す
                CvRect rect = NoseResult[i].Value.Rect;
                Cv.Rectangle(img, rect, new CvColor(0, 255, 0));
            }

            //口の検出
            for (int i = 0; i < MouthResult.Total; i++)
            {
                //矩形の大きさに書き出す
                CvRect rect = MouthResult[i].Value.Rect;
                Cv.Rectangle(img, rect, new CvColor(0, 0, 255));
            }

            using (new CvWindow(img))
            {
                string out_name = @"out\out" + count + @".jpeg";
                Cv.SaveImage(out_name, img);
                Cv.WaitKey();
            }                           


        }

        //顔写真リストファイルを読み込み
        private void ReadFileList()
        {
            string read_list = @"";
//            read_list = @"I:\myprog\github\kubokinJudge\data\kubota_face\kubota_face_list.txt";
            read_list = @"I:\myprog\github\data\fujikin_face\fujikin_face_list.txt";
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

        CvPoint BasePoint;    //目と目の間の座標。基点

        CvSeq<CvAvgComp> EyeResult, NoseResult, MouthResult;

        //各パーツの矩形
        struct PartsRectInfo
        {
            public CvRect RightEye;
            public CvRect LeftEye;
            public CvRect Nose;
            public CvRect Mouth;
        };

        //特徴量
        struct FeatureValue
        {
            public CvPoint basepoint;
            public CvPoint LeftEyeL;
            public CvPoint LeftEyeR;
            public CvPoint RightEyeL;
            public CvPoint RightEyeR;
            public CvPoint NoseL;
            public CvPoint NoseR;
            public CvPoint MouthL;
            public CvPoint MouthR;
        };


        CvRect RightEyeRect, LeftEyeRect, NoseRect, MouthRect;        //パーツの座標
    }
}
