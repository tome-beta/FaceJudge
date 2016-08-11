using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenCvSharp;
using MakeSVMFile;

namespace JudgeGUII
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //入力画像選択
        private void buttonStart_Click(object sender, EventArgs e)
        {
            //ファイルを選択
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter =
                "画像ファイル|*.png;*.jpg|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに
            //「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "判定する画像ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき
                //選択されたファイル名を表示する
                Console.WriteLine(ofd.FileName);

                this.InputFileName = ofd.FileName;

                InputDataModel idm = new InputDataModel();
                int ret = idm.Exec(this.InputFileName);

                if( ret >= 0)
                {
                    //エラー表示
                    MessageBox.Show("検出完了",
                    "完了",
                    MessageBoxButtons.OK
                    );
                }
                else if( ret == -1)
                {
                    //エラー表示
                    MessageBox.Show("入力画像から顔パーツを検出出来ませんでした",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                }

            }

        }

        //リストを読み込んで処理する
        private void buttonReadList_Click(object sender, EventArgs e)
        {
            //ファイルを選択
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter =
                "ファイルリスト|*.txt";
            //[ファイルの種類]ではじめに
            //「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "判定する画像ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            
            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ExecFromList(ofd.FileName);

            }
        }

        //リストから学習を行う
        private void buttonTrainFile_Click(object sender, EventArgs e)
        {
            //ファイルを選択
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter =
                "学習用ファイルリスト|*.txt";
            //[ファイルの種類]ではじめに
            //「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "判定する画像ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }

            //出力用のフォルダを設定する
            InputFileList = ofd.FileName;
            OutPutFolda = @"D:\myprog\github\svm_out"; // 仮設定

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
            svm_manage.TrainingExec(face_feature.FeatuerValueList);

            //エラー表示
            MessageBox.Show("lisvm_model.xmlを作成しました",
            "完了",
            MessageBoxButtons.OK
            );

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

        private void ExecFromList(String input_file_name)
        {
            //エラー処理 TODO

            //リスト文字列を作る
            //リストファイルと読みこんでファイル名をとる
            using (StreamReader sr = new StreamReader(input_file_name))
            {
                //1行づつ読み込む
                while (sr.Peek() > -1)
                {
                    // カンマ区切りで分割して配列に格納する
                    this.InputFileNameList.Add(sr.ReadLine());
                }
                //閉じる
                sr.Close();
            }

            InputDataModel idm = new InputDataModel();
            
            //ファイルの数だけ実行
            foreach (String file_name in this.InputFileNameList)
            {
                int ret = idm.Exec(file_name);
                this.PredictResultList.Add(ret); //結果も用意しておく

                String str = file_name + @" " +ret.ToString();
                Console.WriteLine(str);
                this.labelName.Text = str;
                labelName.Refresh();
            }


            //結果をファイルに出力
            //デバッグ用　学習させる特徴量を出力する
            using (StreamWriter w = new StreamWriter(@"preditct_result.csv"))
            {
                for (int i = 0; i < this.InputFileNameList.Count; i++)
                {
                    w.Write(this.InputFileNameList[i] + ",");
                    w.Write(this.PredictResultList[i] + "\n");
                }
            }
        }

        //デバッグ用　学習ファイルを確認する
        private void buttonSVMCheck_Click(object sender, EventArgs e)
        {
            SVMManage SVMManage = new SVMManage();
            SVMManage.Debug_DispPredict();
        }


        String InputFileName = @"";

        List<String> InputFileNameList = new List<string>();
        List<int> PredictResultList = new List<int>();

        //学習用の変数
        String InputFileList = @"";
        String OutPutFolda = @"";
        List<string> FaceList = new List<string>();
        List<int> IDList = new List<int>();//特徴量とセットで使うID

    }
}
