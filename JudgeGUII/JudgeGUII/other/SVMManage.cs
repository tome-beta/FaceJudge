using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace MakeSVMFile
{
    class SVMManage
    {
        public SVMManage()
        {
          this.svm = new CvSVM();
        }

        //学習ファイルの作成
        public void LearningExec(List<FaceFeature.FeatureValue> FeatureList)
        {
            //特徴量をMatに移し替える　８個で一つ
            //8個のfloat * LISTの大きさの配列
            double[] feature_array = new double[8 * FeatureList.Count];

            //特徴量をSVMで扱えるように配列に置き換える
            SetFeatureListToArray(FeatureList,ref feature_array);
            CvMat dataMat = new CvMat(feature_array.Length / 8, 8, MatrixType.F32C1, feature_array, true);

            //これがラベル番号
            int[] id_array = new int[FeatureList.Count];
            for(int i = 0; i < id_array.Length;i++)
            {
                id_array[i] = FeatureList[i].ID;
            }
            CvMat resMat = new CvMat(id_array.Length, 1, MatrixType.S32C1, id_array, true);

            //正規化する0～１．０に収まるようにする
            //全部２で割る？最大値がだいたい１．６くらいのはずなので
            dataMat /= 2.0;

            //SVMの用意
            CvTermCriteria criteria = new CvTermCriteria(1000, 0.000001);
            CvSVMParams param = new CvSVMParams(
                SVMType.CSvc,
                SVMKernelType.Rbf,
                10.0,  // degree
                8.0,  // gamma        調整
                1.0, // coeff0
                10.0, // c               調整
                0.5, // nu
                0.1, // p
                null,
                criteria);

            //学習実行
            svm.Train(dataMat, resMat, null, null, param);

        }

        //SVM判定
        public int SVMPredict(FaceFeature.FeatureValue feature)
        {
            double[] feature_array = new double[8];
            SetFeatureToArray(feature, ref feature_array);
            CvMat dataMat = new CvMat(1, 8, MatrixType.F32C1, feature_array, true);

            //正規化する0～１．０に収まるようにする
            //全部２で割る？最大値がだいたい１．６くらいのはずなので
            dataMat /= 2.0;

            //学習ファイルを読み込む
            svm.Load(@"SvmLearning.xml");

            return (int)this.svm.Predict(dataMat);
        }

        /// <summary>
        /// 特徴量の値を配列にセットする
        /// </summary>
        /// <param name="FeatureList"></param>
        /// <param name="id_list"></param>
        public void SetFeatureListToArray(List<FaceFeature.FeatureValue> FeatureList, ref double[] value_array)
        {
            int idx = 0;

            for(int i = 0; i < FeatureList.Count;i++)
            {
                value_array[idx++] = (FeatureList[i].LeftEyeValuieL);
                value_array[idx++] = (FeatureList[i].LeftEyeValuieR);
                value_array[idx++] = (FeatureList[i].RightEyeValuieL);
                value_array[idx++] = (FeatureList[i].RightEyeValuieR);
                value_array[idx++] = (FeatureList[i].NoseLValuieL);
                value_array[idx++] = (FeatureList[i].NoseLValuieR);
                value_array[idx++] = (FeatureList[i].MouthLValuieL);
                value_array[idx++] = (FeatureList[i].MouthLValuieR);
            }
        }

        /// <summary>
        /// 単体の特徴量構造体を配列に直す　　要検討
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="value_array"></param>
        public void SetFeatureToArray(FaceFeature.FeatureValue feature, ref double[] value_array)
        {
            int idx = 0;
            value_array[idx++] = (feature.LeftEyeValuieL);
            value_array[idx++] = (feature.LeftEyeValuieR);
            value_array[idx++] = (feature.RightEyeValuieL);
            value_array[idx++] = (feature.RightEyeValuieR);
            value_array[idx++] = (feature.NoseLValuieL);
            value_array[idx++] = (feature.NoseLValuieR);
            value_array[idx++] = (feature.MouthLValuieL);
            value_array[idx++] = (feature.MouthLValuieR);
        }
        public CvSVM svm { get; set; }
    }
}
