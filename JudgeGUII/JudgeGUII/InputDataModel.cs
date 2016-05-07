using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace JudgeGUII
{
    class InputDataModel
    {
        public void Exec(String file_name)
        {
            this.ImageFileName = file_name;
            LoadImageFile(this.ImageFileName);
            FeatureFromIpl();
        }

        //画像ファイルロード
        public bool LoadImageFile(String file_name)
        {
            //カスケード分類器の特徴量を取得する
            CvHaarClassifierCascade cascade = CvHaarClassifierCascade.FromFile(@"C:\opencv2.4.8\sources\data\haarcascades\haarcascade_frontalface_alt.xml");
            CvMemStorage strage = new CvMemStorage(0);   // メモリを確保
            this.ImageFileName = file_name;

            using (IplImage img = new IplImage(this.ImageFileName))
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

                        //iplimageをコピー
                        img.ROI = rect;
                        CvRect roi_rect = img.ROI;
                        IplImage ipl_image = Cv.CreateImage(new CvSize(img.Width, img.Height), BitDepth.U8, 1);
                        ipl_image = img.Clone(img.ROI);

                        //確認
                        new CvWindow(ipl_image);
                        Cv.WaitKey();

                        //見つけた顔候補をすべてチェックするために記録する
                        this.FaceIplList.Add(ipl_image);
                    }
                }
                return true;
            }
        }

        //特徴量作成
        private bool FeatureFromIpl()
        {
            foreach(IplImage ipl_image in this.FaceIplList)
            {
                FaceFeature.MakeFeatureFromIpl(ipl_image, 0);
            }
            return true;
        }


        //===================================
        //変数
        //===================================
        String ImageFileName = @"";
        List<IplImage> FaceIplList = new List<IplImage>();
        MakeSVMFile.FaceFeature FaceFeature = new MakeSVMFile.FaceFeature();
    }
}
