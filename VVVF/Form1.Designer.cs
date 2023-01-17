namespace VVVF {
    partial class Form1 {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.btnPlayStop = new System.Windows.Forms.Button();
            this.cmbDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trbTargetFreq = new System.Windows.Forms.TrackBar();
            this.trbAcc = new System.Windows.Forms.TrackBar();
            this.lblTargetFreq = new System.Windows.Forms.Label();
            this.lblAcc = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblPower = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.trbPower = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.trbFilter = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.trbVolume = new System.Windows.Forms.TrackBar();
            this.cmbDisplayMode = new System.Windows.Forms.ComboBox();
            this.lblCarrierFreq = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblOutputPower = new System.Windows.Forms.Label();
            this.lblOutputFreq = new System.Windows.Forms.Label();
            this.picWave = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this.trbTargetFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbAcc)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbPower)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picWave)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPlayStop
            // 
            this.btnPlayStop.Location = new System.Drawing.Point(349, 183);
            this.btnPlayStop.Name = "btnPlayStop";
            this.btnPlayStop.Size = new System.Drawing.Size(75, 23);
            this.btnPlayStop.TabIndex = 0;
            this.btnPlayStop.Text = "再生";
            this.btnPlayStop.UseVisualStyleBackColor = true;
            this.btnPlayStop.Click += new System.EventHandler(this.btnPlayStop_Click);
            // 
            // cmbDevices
            // 
            this.cmbDevices.FormattingEnabled = true;
            this.cmbDevices.Location = new System.Drawing.Point(8, 186);
            this.cmbDevices.Name = "cmbDevices";
            this.cmbDevices.Size = new System.Drawing.Size(290, 20);
            this.cmbDevices.TabIndex = 1;
            this.cmbDevices.SelectedIndexChanged += new System.EventHandler(this.cmbDevices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "音声の出力先";
            // 
            // trbTargetFreq
            // 
            this.trbTargetFreq.Location = new System.Drawing.Point(6, 45);
            this.trbTargetFreq.Maximum = 200;
            this.trbTargetFreq.Name = "trbTargetFreq";
            this.trbTargetFreq.Size = new System.Drawing.Size(320, 45);
            this.trbTargetFreq.TabIndex = 3;
            this.trbTargetFreq.TickFrequency = 10;
            this.trbTargetFreq.Value = 120;
            this.trbTargetFreq.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // trbAcc
            // 
            this.trbAcc.Location = new System.Drawing.Point(6, 108);
            this.trbAcc.Maximum = 100;
            this.trbAcc.Name = "trbAcc";
            this.trbAcc.Size = new System.Drawing.Size(320, 45);
            this.trbAcc.TabIndex = 4;
            this.trbAcc.TickFrequency = 10;
            this.trbAcc.Value = 24;
            this.trbAcc.Scroll += new System.EventHandler(this.trbAcc_Scroll);
            // 
            // lblTargetFreq
            // 
            this.lblTargetFreq.AutoSize = true;
            this.lblTargetFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTargetFreq.Location = new System.Drawing.Point(332, 45);
            this.lblTargetFreq.Name = "lblTargetFreq";
            this.lblTargetFreq.Size = new System.Drawing.Size(55, 19);
            this.lblTargetFreq.TabIndex = 5;
            this.lblTargetFreq.Text = "000Hz";
            this.lblTargetFreq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAcc
            // 
            this.lblAcc.AutoSize = true;
            this.lblAcc.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblAcc.Location = new System.Drawing.Point(332, 108);
            this.lblAcc.Name = "lblAcc";
            this.lblAcc.Size = new System.Drawing.Size(92, 19);
            this.lblAcc.TabIndex = 6;
            this.lblAcc.Text = "00.0Hz/sec";
            this.lblAcc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "目標周波数";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "加速度";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblPower);
            this.groupBox1.Controls.Add(this.lblAcc);
            this.groupBox1.Controls.Add(this.lblTargetFreq);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.trbPower);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.trbTargetFreq);
            this.groupBox1.Controls.Add(this.trbAcc);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 214);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "操作";
            // 
            // lblPower
            // 
            this.lblPower.AutoSize = true;
            this.lblPower.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPower.Location = new System.Drawing.Point(332, 171);
            this.lblPower.Name = "lblPower";
            this.lblPower.Size = new System.Drawing.Size(51, 19);
            this.lblPower.TabIndex = 11;
            this.lblPower.Text = "100%";
            this.lblPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "出力";
            // 
            // trbPower
            // 
            this.trbPower.Location = new System.Drawing.Point(6, 171);
            this.trbPower.Maximum = 100;
            this.trbPower.Name = "trbPower";
            this.trbPower.Size = new System.Drawing.Size(320, 45);
            this.trbPower.TabIndex = 9;
            this.trbPower.TickFrequency = 10;
            this.trbPower.Value = 100;
            this.trbPower.Scroll += new System.EventHandler(this.trbPower_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnPlayStop);
            this.groupBox2.Controls.Add(this.cmbDevices);
            this.groupBox2.Controls.Add(this.lblMode);
            this.groupBox2.Controls.Add(this.trbFilter);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.trbVolume);
            this.groupBox2.Controls.Add(this.cmbDisplayMode);
            this.groupBox2.Controls.Add(this.lblCarrierFreq);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lblOutputPower);
            this.groupBox2.Controls.Add(this.lblOutputFreq);
            this.groupBox2.Location = new System.Drawing.Point(456, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(438, 214);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "モニター";
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblMode.Location = new System.Drawing.Point(248, 26);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(54, 19);
            this.lblMode.TabIndex = 17;
            this.lblMode.Text = "非同期";
            // 
            // trbFilter
            // 
            this.trbFilter.Location = new System.Drawing.Point(5, 67);
            this.trbFilter.Maximum = 0;
            this.trbFilter.Minimum = -60;
            this.trbFilter.Name = "trbFilter";
            this.trbFilter.Size = new System.Drawing.Size(426, 45);
            this.trbFilter.TabIndex = 16;
            this.trbFilter.TickFrequency = 6;
            this.trbFilter.Value = -24;
            this.trbFilter.Scroll += new System.EventHandler(this.trbFilter_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "音量";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 12);
            this.label9.TabIndex = 15;
            this.label9.Text = "フィルター";
            // 
            // trbVolume
            // 
            this.trbVolume.Location = new System.Drawing.Point(6, 130);
            this.trbVolume.Maximum = -18;
            this.trbVolume.Minimum = -72;
            this.trbVolume.Name = "trbVolume";
            this.trbVolume.Size = new System.Drawing.Size(426, 45);
            this.trbVolume.TabIndex = 11;
            this.trbVolume.TickFrequency = 6;
            this.trbVolume.Value = -48;
            this.trbVolume.Scroll += new System.EventHandler(this.trbVolume_Scroll);
            // 
            // cmbDisplayMode
            // 
            this.cmbDisplayMode.FormattingEnabled = true;
            this.cmbDisplayMode.Items.AddRange(new object[] {
            "U-V, V-W, W-U",
            "U-V",
            "V-W",
            "W-U",
            "U",
            "V",
            "W",
            "位相"});
            this.cmbDisplayMode.Location = new System.Drawing.Point(308, 26);
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.Size = new System.Drawing.Size(116, 20);
            this.cmbDisplayMode.TabIndex = 14;
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
            // 
            // lblCarrierFreq
            // 
            this.lblCarrierFreq.AutoSize = true;
            this.lblCarrierFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblCarrierFreq.Location = new System.Drawing.Point(152, 27);
            this.lblCarrierFreq.Name = "lblCarrierFreq";
            this.lblCarrierFreq.Size = new System.Drawing.Size(78, 19);
            this.lblCarrierFreq.TabIndex = 5;
            this.lblCarrierFreq.Text = "0000.0Hz";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(154, 14);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "搬送波周波数";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 12);
            this.label7.TabIndex = 3;
            this.label7.Text = "出力(%)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(70, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "出力周波数";
            // 
            // lblOutputPower
            // 
            this.lblOutputPower.AutoSize = true;
            this.lblOutputPower.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblOutputPower.Location = new System.Drawing.Point(2, 27);
            this.lblOutputPower.Name = "lblOutputPower";
            this.lblOutputPower.Size = new System.Drawing.Size(51, 19);
            this.lblOutputPower.TabIndex = 1;
            this.lblOutputPower.Text = "000%";
            // 
            // lblOutputFreq
            // 
            this.lblOutputFreq.AutoSize = true;
            this.lblOutputFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblOutputFreq.Location = new System.Drawing.Point(68, 27);
            this.lblOutputFreq.Name = "lblOutputFreq";
            this.lblOutputFreq.Size = new System.Drawing.Size(69, 19);
            this.lblOutputFreq.TabIndex = 0;
            this.lblOutputFreq.Text = "000.0Hz";
            // 
            // picWave
            // 
            this.picWave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picWave.Location = new System.Drawing.Point(12, 234);
            this.picWave.Name = "picWave";
            this.picWave.Size = new System.Drawing.Size(882, 271);
            this.picWave.TabIndex = 6;
            this.picWave.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 517);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.picWave);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trbTargetFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbAcc)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbPower)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picWave)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPlayStop;
        private System.Windows.Forms.ComboBox cmbDevices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trbTargetFreq;
        private System.Windows.Forms.TrackBar trbAcc;
        private System.Windows.Forms.Label lblTargetFreq;
        private System.Windows.Forms.Label lblAcc;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar trbPower;
        private System.Windows.Forms.Label lblPower;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblOutputFreq;
        private System.Windows.Forms.Label lblOutputPower;
        private System.Windows.Forms.Label lblCarrierFreq;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox picWave;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TrackBar trbVolume;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbDisplayMode;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TrackBar trbFilter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblMode;
    }
}

