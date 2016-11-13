using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.IO;
using LibSVMsharp;
using LibSVMsharp.Helpers;

namespace MakeSVMFile
{
    class SVMManage
    {
        const float SVM_COST = 10.0f;
        const float SVM_GAMMA = 100.0f;

        const int FEATURE_COUNT = 8;        //使う特徴量の個数

        public SVMManage()
        {
        }

        //学習ファイルの作成
        public void TrainingExec(List<FaceFeature.FeatureValue> FeatureList)
        {
            //３種類の学習ファイルを作る
            string model_file_1 = @"FaceFeature.csv";

            makeLearinigFile(FeatureList, model_file_1);
            Training(model_file_1, SVM_GAMMA, SVM_COST);       //学習を実行

        }

        //SVM判定
        public int SVMPredict(FaceFeature.FeatureValue feature)
        {
            //学習ファイルを読み込んでいなかったらロード
            if (this.LoadFlag == false)
            {
                this.libSVM_model_1 = SVM.LoadModel(@"model_Feature_L_EYE_NOSE.xml");
                this.LoadFlag = true;
            }

            double[] feature_array = new double[2];
            int[] answer = new int[3];

            {
                SetFeatureToArray(feature, ref feature_array);

                //問題を作成
                SVMNode[] node_array = new SVMNode[2];
                node_array[0] = new SVMNode(1, feature_array[0]);
                node_array[1] = new SVMNode(2, feature_array[1]);

                answer[0] = (int)SVM.Predict(libSVM_model_1, node_array);
                return answer[0];
            }
/*
            {
                SetFeatureToArray(feature, ref feature_array, LEARNING_TYPE.L_EYE_MOUTH);

                //問題を作成
                SVMNode[] node_array = new SVMNode[2];
                node_array[0] = new SVMNode(1, feature_array[0]);
                node_array[1] = new SVMNode(2, feature_array[1]);

                answer[1] = (int)SVM.Predict(libSVM_model_2, node_array);
            }
            {
                SetFeatureToArray(feature, ref feature_array, LEARNING_TYPE.R_EYE_MOUTH);

                //問題を作成
                SVMNode[] node_array = new SVMNode[2];
                node_array[0] = new SVMNode(1, feature_array[0]);
                node_array[1] = new SVMNode(2, feature_array[1]);

                answer[2] = (int)SVM.Predict(libSVM_model_2, node_array);
            }
*/

            int final = answer[0] + answer[1] + answer[2];

            if(final < 5) { final = 1; }
            else { final = 2; }
            return final;
        }

        //作成した辞書を図でみる
        public void Debug_DispPredict()
        {
            //辞書ファイルのロード
            this.libSVM_model = SVM.LoadModel(@"libsvm_model.xml");

            using (IplImage retPlot = new IplImage(300, 300, BitDepth.U8, 3))
            {
                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 300; y++)
                    {
                        float[] sample = { x / 300f, y / 300f };
                        //問題を作成
                        SVMNode[] node_array = new SVMNode[2];
                        node_array[0] = new SVMNode(1, sample[0]);
                        node_array[1] = new SVMNode(2, sample[1]);
                        int ret_double = (int)SVM.Predict(libSVM_model, node_array);
                        int ret_i = (int)ret_double;
                        CvRect plotRect = new CvRect(x, 300 - y, 1, 1);
                        if (ret_i == 1)
                            retPlot.Rectangle(plotRect, CvColor.Red);
                        else if (ret_i == 2)
                            retPlot.Rectangle(plotRect, CvColor.GreenYellow);
                    }
                }
                CvWindow.ShowImages(retPlot);
            }
        }


        //--------------------------------------------------------------------------------------
        // private 
        //---------------------------------------------------------------------------------------

        /// <summary>
        /// 辞書ファイルを作成する
        /// </summary>
        /// <param name="input_learing_file"></param>
        /// <param name="gammma"></param>
        /// <param name="cost"></param>
        private void Training(string input_learing_file, float gammma, float cost)
        {
            //LibSVMのテスト
            //学習用のデータの読み込み
            SVMProblem problem = SVMProblemHelper.Load(input_learing_file);

            //SVMパラメータ
            SVMParameter parameter = new SVMParameter();
            parameter.Type = LibSVMsharp.SVMType.C_SVC;
            parameter.Kernel = LibSVMsharp.SVMKernelType.RBF;
            parameter.C = cost;
            parameter.Gamma = gammma;

            libSVM_model = SVM.Train(problem, parameter);
            //辞書ファイルを出力(xmlファイル)
            string xml_name = @"model_" + input_learing_file;
            xml_name = xml_name.Replace(@".csv", @".xml");
            SVM.SaveModel(libSVM_model, xml_name);

            //判定結果をファイルに出してみる
            SVMProblem testProblem = SVMProblemHelper.Load(input_learing_file);
            double[] target = new double[testProblem.Length];
            string debug_file_str = @"debug_" + input_learing_file;
            using (StreamWriter w = new StreamWriter(debug_file_str))
            {
                for (int i = 0; i < testProblem.Length; i++)
                {
                    target[i] = SVM.Predict(libSVM_model, testProblem.X[i]);
                    w.Write(target[i] + "\n");
                    Console.Out.WriteLine(@"{0} : {1}", i, target[i]);
                }
            }
            //正解率を出す。
            double accuracy = SVMHelper.EvaluateClassificationProblem(testProblem, target);
        }
        /// <summary>
        /// 学習用のデータファイルを作成する
        /// </summary>
        /// <param name="feature_list">特徴量</param>
        /// <param name="file_name">辞書ファイル名</param>
        /// <param name="type">辞書タイプ</param>
        private void makeLearinigFile(List<FaceFeature.FeatureValue> feature_list, string file_name)
        {
            //特徴量をMatに移し替える　2個で一つ
            //2個のfloat * LISTの大きさの配列
            double[] feature_array = new double[FEATURE_COUNT * feature_list.Count];

            //特徴量をSVMで扱えるように配列に置き換える
            SetFeatureListToArray(feature_list, ref feature_array);
            //これがラベル番号
            int[] id_array = new int[feature_list.Count];
            for (int i = 0; i < id_array.Length; i++)
            {
                id_array[i] = feature_list[i].ID;
            }

            //学習データを図にする
////            Debug_DrawInputFeature(points, id_array);
            //LibSVMで学習させるためのデータを出力
            OutPut_FeatureAndID(feature_array, id_array, file_name);
        }

        /// <summary>
        /// 特徴量を外部に出力する
        /// </summary>
        /// <param name="points"></param>
        /// <param name="id_array"></param>
        private void OutPut_FeatureAndID(double[] feature_array, int[] id_array, string file_name)
        {
            using (StreamWriter w = new StreamWriter(file_name))
            {
                for (int i = 0; i < id_array.Length; i++)
                {
                    w.Write(id_array[i] + " ");
                    w.Write("1:" + feature_array[i* FEATURE_COUNT + 0]+ " ");
                    w.Write("2:" + feature_array[i * FEATURE_COUNT + 1] + " ");
                    w.Write("3:" + feature_array[i * FEATURE_COUNT + 2] + " ");
                    w.Write("4:" + feature_array[i * FEATURE_COUNT + 3] + " ");
                    w.Write("5:" + feature_array[i * FEATURE_COUNT + 4] + " ");
                    w.Write("6:" + feature_array[i * FEATURE_COUNT + 5] + " ");
                    w.Write("7:" + feature_array[i * FEATURE_COUNT + 6] + " ");
                    w.Write("8:" + feature_array[i * FEATURE_COUNT + 7] + " ");  
                    w.Write("\n");
                }
            }
        }

        /// <summary>
        /// 入力特徴量を図にする
        /// </summary>
        /// <param name="data_array"></param>
        private void Debug_DrawInputFeature(CvPoint2D32f[] points, int[] id_array)
        {
            using (IplImage pointsPlot = Cv.CreateImage(new CvSize(300, 300), BitDepth.U8, 3))
            {
                pointsPlot.Zero();
                for (int i = 0; i < id_array.Length; i++)
                {
                    int x = (int)(points[i].X * 300);
                    int y = (int)(300 - points[i].Y * 300);
                    int res = id_array[i];
                    //                    CvColor color = (res == 1) ? CvColor.Red : CvColor.GreenYellow;
                    CvColor color = new CvColor();
                    if (res == 1)
                    {
                        color = CvColor.Red;
                    }
                    else if (res == 2)
                    {
                        color = CvColor.GreenYellow;
                    }
                    pointsPlot.Circle(x, y, 2, color, -1);
                }
                CvWindow.ShowImages(pointsPlot);
            }

        }

        /// <summary>
        /// 特徴量の値を配列にセットする
        /// </summary>
        /// <param name="FeatureList"></param>
        /// <param name="id_list"></param>
        /// 
        private void SetFeatureListToArray(List<FaceFeature.FeatureValue> FeatureList, ref double[] value_array)
        {
            int idx = 0;

                for (int i = 0; i < FeatureList.Count; i++)
                {
                    value_array[idx++] = (FeatureList[i].LeftEyeValueL);
                    value_array[idx++] = (FeatureList[i].LeftEyeValueR);
                    value_array[idx++] = (FeatureList[i].RightEyeValueL);
                    value_array[idx++] = (FeatureList[i].RightEyeValueR);
                    value_array[idx++] = (FeatureList[i].NoseLValueL);
                    value_array[idx++] = (FeatureList[i].NoseLValueR);
                    value_array[idx++] = (FeatureList[i].MouthLValueL);
                    value_array[idx++] = (FeatureList[i].MouthLValueR);
                }
        }

        /// <summary>
        /// 単体の特徴量構造体を配列に直す　　要検討
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="value_array"></param>
        private void SetFeatureToArray(FaceFeature.FeatureValue feature, ref double[] value_array)
        {
            int idx = 0;
                value_array[idx++] = (feature.LeftEyeValueL);
                //            value_array[idx++] = (feature.LeftEyeValueR);
                //            value_array[idx++] = (feature.RightEyeValueL);
                //            value_array[idx++] = (feature.RightEyeValueR);
                value_array[idx++] = (feature.NoseLValueL);
                //            value_array[idx++] = (feature.NoseLValueR);
                //            value_array[idx++] = (feature.MouthLValueL);
                //            value_array[idx++] = (feature.MouthLValueR);
        }

        private bool LoadFlag = false;
        public SVMModel libSVM_model { get; set; }

        public SVMModel libSVM_model_1 { get; set; }
        public SVMModel libSVM_model_2 { get; set; }
        public SVMModel libSVM_model_3 { get; set; }

    }
}
