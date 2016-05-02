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
            svm_make.Exec(args[0],args[1],args[2]);
        }
    }

    class MakeSvmFile
    {
        const int SMALL_IMAGE_LIMIT = 100;  //顔画像を拡大対象にする
        const int IMAGE_RESIZE_RATE = 4;    //拡大率

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
            public CvPoint basepoint;   //両目の間の基点
            public double BothEyeDistance; //目と目の間の距離
            public double LeftEyeValuieL;
            public double LeftEyeValuieR;
            public double RightEyeValuieL;
            public double RightEyeValuieR;
            public double NoseLValuieL;
            public double NoseLValuieR;
            public double MouthLValuieL;
            public double MouthLValuieR;
        };

        
        public void Exec(String input, String output, String label_id)
        {
            this.InputFileList = input;
            this.OutPutFolda = output;
            this.SVMLabelID = int.Parse(label_id);

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

                    FeatureValue feature_value =  new FeatureValue();
                    //特徴量を作る
                    MakeFeatureValue(gray_image, ref parts_info, out feature_value);


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
        /// 特徴量をだす
        /// </summary>
        private bool MakeFeatureValue(IplImage img,ref PartsRectInfo input_info,out FeatureValue output_info)
        {
            //仮に代入  
            output_info.basepoint = new CvPoint(0, 0);
            output_info.BothEyeDistance = 0;
            output_info.LeftEyeValuieL = 0;
            output_info.LeftEyeValuieR = 0;
            output_info.RightEyeValuieL = 0;
            output_info.RightEyeValuieR = 0;
            output_info.NoseLValuieL = 0;
            output_info.NoseLValuieR = 0;
            output_info.MouthLValuieL = 0;
            output_info.MouthLValuieR = 0;

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

            //右目の中心と左目の中心を結んだ線の中点が基準点。
            output_info.basepoint.X = LeftEyeCenterX + RightEyeCenterX / 2;
            output_info.basepoint.Y = LeftEyeCenterY + RightEyeCenterY / 2;

            //目と目の距離をとる
            output_info.BothEyeDistance = makeTwoPointDistance(LeftEyeCenterX, RightEyeCenterX, LeftEyeCenterY, RightEyeCenterY);
            //基準点から各パーツの右端、左端までの距離をとる
            output_info.LeftEyeValuieL = makeTwoPointDistance(input_info.LeftEye.X, 
                                                              output_info.basepoint.X,
                                                              input_info.LeftEye.Y,
                                                              output_info.basepoint.Y);
            output_info.LeftEyeValuieR = makeTwoPointDistance(input_info.LeftEye.X + input_info.LeftEye.Width,
                                                              output_info.basepoint.X,
                                                              input_info.LeftEye.Y,
                                                              output_info.basepoint.Y);

            output_info.RightEyeValuieL = makeTwoPointDistance(input_info.RightEye.X,
                                                              output_info.basepoint.X,
                                                              input_info.RightEye.Y,
                                                              output_info.basepoint.Y);
            output_info.RightEyeValuieR = makeTwoPointDistance(input_info.RightEye.X + input_info.RightEye.Width,
                                                              output_info.basepoint.X,
                                                              input_info.RightEye.Y,
                                                              output_info.basepoint.Y);

            output_info.NoseLValuieL = makeTwoPointDistance(input_info.Nose.X,
                                                  output_info.basepoint.X,
                                                  input_info.Nose.Y,
                                                  output_info.basepoint.Y);
            output_info.NoseLValuieR = makeTwoPointDistance(input_info.Nose.X + input_info.Nose.Width,
                                                              output_info.basepoint.X,
                                                              input_info.Nose.Y,
                                                              output_info.basepoint.Y);

            output_info.MouthLValuieL = makeTwoPointDistance(input_info.Mouth.X,
                                                  output_info.basepoint.X,
                                                  input_info.Mouth.Y,
                                                  output_info.basepoint.Y);
            output_info.MouthLValuieR = makeTwoPointDistance(input_info.Mouth.X + input_info.Mouth.Width,
                                                              output_info.basepoint.X,
                                                              input_info.Mouth.Y,
                                                              output_info.basepoint.Y);


            //基準点からパーツまでの距離と瞳間距離の比率を特徴量とする
            output_info.LeftEyeValuieL /= output_info.BothEyeDistance;
            output_info.LeftEyeValuieR /= output_info.BothEyeDistance;
            output_info.RightEyeValuieL /= output_info.BothEyeDistance;
            output_info.RightEyeValuieR /= output_info.BothEyeDistance;
            output_info.NoseLValuieL /= output_info.BothEyeDistance;
            output_info.NoseLValuieR /= output_info.BothEyeDistance;
            output_info.MouthLValuieL /= output_info.BothEyeDistance;
            output_info.MouthLValuieR /= output_info.BothEyeDistance;

            return true;
        }

        /// <summary>
        /// ２点間の距離を出す
        /// </summary>
        /// <returns></returns>
        private double makeTwoPointDistance(int x1,int y1, int x2 ,int y2)
        {
            double answer = 0;

            answer = Math.Pow(x2- x1,2) + Math.Pow(y2-y1,2);
            answer = Math.Sqrt(answer);

            return answer;
        }

        /// <summary>
        /// 矩形選択後の画像を表示
        /// </summary>
        /// <param name="img"></param>
        /// <param name="count"></param>
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
                string out_name = this.OutPutFolda + @"\decide_parts" + count + @".jpeg";
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
                string out_name = this.OutPutFolda + @"\out" + count + @".jpeg";
                Cv.SaveImage(out_name, img);
                Cv.WaitKey();
            }                           


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
                    this.KubotaList.Add(sr.ReadLine());
                }
                //閉じる
                sr.Close();
            }
        }

        List<string> KubotaList = new List<string>();
        CvSeq<CvAvgComp> EyeResult, NoseResult, MouthResult;

        CvRect RightEyeRect, LeftEyeRect, NoseRect, MouthRect;        //パーツの座標

        String InputFileList = @"";
        String OutPutFolda = @"";

        int SVMLabelID = -1;
    }
}
