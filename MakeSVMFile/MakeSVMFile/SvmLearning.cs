using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace MakeSVMFile
{
    class SvmLearning
    {
        public void Exec(List<MakeSvmFile.FeatureValue>FeatureList, List<int>IDList)
        {
            //特徴量をMatに移し替える　８個で一つ
            //8個のfloat * LISTの大きさの配列
            double[] feature_array = new double[8 * FeatureList.Count];

            //特徴量をSVMで扱えるように配列に置き換える
            SetFeatureToArray(FeatureList,ref feature_array);
            //入力データ
            CvMat dataMat = new CvMat(feature_array.Length /8, 8, MatrixType.F32C1, feature_array, true);

            int[] id_array = new int[IDList.Count];

            for(int i = 0; i < id_array.Length;i++)
            {
                id_array[i] = IDList[i];
            }
            //test 
            id_array[0] = 2;

            //これがラベル番号
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
            svm = new CvSVM();
            svm.Train(dataMat, resMat, null, null, param);

        }

        /// <summary>
        /// 特徴量の値を配列にセットする
        /// </summary>
        /// <param name="FeatureList"></param>
        /// <param name="id_list"></param>
        private void SetFeatureToArray(List<MakeSvmFile.FeatureValue> FeatureList, ref double[] value_array)
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
        public CvSVM svm { get; set; }
    }
}
