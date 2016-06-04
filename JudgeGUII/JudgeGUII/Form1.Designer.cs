namespace JudgeGUII
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonReadList = new System.Windows.Forms.Button();
            this.labelName = new System.Windows.Forms.Label();
            this.buttonSVMCheck = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 31);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(132, 79);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonReadList
            // 
            this.buttonReadList.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonReadList.Location = new System.Drawing.Point(183, 31);
            this.buttonReadList.Name = "buttonReadList";
            this.buttonReadList.Size = new System.Drawing.Size(159, 81);
            this.buttonReadList.TabIndex = 2;
            this.buttonReadList.Text = "リスト読み込み";
            this.buttonReadList.UseVisualStyleBackColor = true;
            this.buttonReadList.Click += new System.EventHandler(this.buttonReadList_Click);
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelName.Location = new System.Drawing.Point(22, 245);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(103, 28);
            this.labelName.TabIndex = 3;
            this.labelName.Text = "file_name";
            // 
            // buttonSVMCheck
            // 
            this.buttonSVMCheck.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonSVMCheck.Location = new System.Drawing.Point(363, 31);
            this.buttonSVMCheck.Name = "buttonSVMCheck";
            this.buttonSVMCheck.Size = new System.Drawing.Size(159, 81);
            this.buttonSVMCheck.TabIndex = 4;
            this.buttonSVMCheck.Text = "学習ファイルチェック";
            this.buttonSVMCheck.UseVisualStyleBackColor = true;
            this.buttonSVMCheck.Click += new System.EventHandler(this.buttonSVMCheck_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 330);
            this.Controls.Add(this.buttonSVMCheck);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.buttonReadList);
            this.Controls.Add(this.buttonStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonReadList;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Button buttonSVMCheck;
    }
}

