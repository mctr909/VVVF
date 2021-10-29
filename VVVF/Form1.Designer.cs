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
            this.trbFilter = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbDisplayMode = new System.Windows.Forms.ComboBox();
            this.picWave = new System.Windows.Forms.PictureBox();
            this.lblCarrierFreq = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblOutputPower = new System.Windows.Forms.Label();
            this.lblOutputFreq = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.trbVolume = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this.trbTargetFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbAcc)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbPower)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trbFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picWave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbVolume)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPlayStop
            // 
            this.btnPlayStop.Location = new System.Drawing.Point(479, 230);
            this.btnPlayStop.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.btnPlayStop.Name = "btnPlayStop";
            this.btnPlayStop.Size = new System.Drawing.Size(163, 46);
            this.btnPlayStop.TabIndex = 0;
            this.btnPlayStop.Text = "再生";
            this.btnPlayStop.UseVisualStyleBackColor = true;
            this.btnPlayStop.Click += new System.EventHandler(this.btnPlayStop_Click);
            // 
            // cmbDevices
            // 
            this.cmbDevices.FormattingEnabled = true;
            this.cmbDevices.Location = new System.Drawing.Point(13, 58);
            this.cmbDevices.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.cmbDevices.Name = "cmbDevices";
            this.cmbDevices.Size = new System.Drawing.Size(624, 32);
            this.cmbDevices.TabIndex = 1;
            this.cmbDevices.SelectedIndexChanged += new System.EventHandler(this.cmbDevices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "音声の出力先";
            // 
            // trbTargetFreq
            // 
            this.trbTargetFreq.Location = new System.Drawing.Point(13, 90);
            this.trbTargetFreq.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trbTargetFreq.Maximum = 120;
            this.trbTargetFreq.Name = "trbTargetFreq";
            this.trbTargetFreq.Size = new System.Drawing.Size(433, 90);
            this.trbTargetFreq.TabIndex = 3;
            this.trbTargetFreq.TickFrequency = 10;
            this.trbTargetFreq.Value = 120;
            this.trbTargetFreq.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // trbAcc
            // 
            this.trbAcc.Location = new System.Drawing.Point(13, 216);
            this.trbAcc.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trbAcc.Maximum = 100;
            this.trbAcc.Name = "trbAcc";
            this.trbAcc.Size = new System.Drawing.Size(433, 90);
            this.trbAcc.TabIndex = 4;
            this.trbAcc.TickFrequency = 10;
            this.trbAcc.Value = 32;
            this.trbAcc.Scroll += new System.EventHandler(this.trbAcc_Scroll);
            // 
            // lblTargetFreq
            // 
            this.lblTargetFreq.AutoSize = true;
            this.lblTargetFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTargetFreq.Location = new System.Drawing.Point(444, 90);
            this.lblTargetFreq.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblTargetFreq.Name = "lblTargetFreq";
            this.lblTargetFreq.Size = new System.Drawing.Size(111, 38);
            this.lblTargetFreq.TabIndex = 5;
            this.lblTargetFreq.Text = "000Hz";
            this.lblTargetFreq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAcc
            // 
            this.lblAcc.AutoSize = true;
            this.lblAcc.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblAcc.Location = new System.Drawing.Point(444, 216);
            this.lblAcc.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblAcc.Name = "lblAcc";
            this.lblAcc.Size = new System.Drawing.Size(183, 38);
            this.lblAcc.TabIndex = 6;
            this.lblAcc.Text = "00.0Hz/sec";
            this.lblAcc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 24);
            this.label2.TabIndex = 7;
            this.label2.Text = "目標周波数";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 186);
            this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 24);
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
            this.groupBox1.Location = new System.Drawing.Point(26, 324);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox1.Size = new System.Drawing.Size(654, 460);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "操作";
            // 
            // lblPower
            // 
            this.lblPower.AutoSize = true;
            this.lblPower.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPower.Location = new System.Drawing.Point(444, 342);
            this.lblPower.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblPower.Name = "lblPower";
            this.lblPower.Size = new System.Drawing.Size(105, 38);
            this.lblPower.TabIndex = 11;
            this.lblPower.Text = "100%";
            this.lblPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 312);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 24);
            this.label4.TabIndex = 10;
            this.label4.Text = "出力";
            // 
            // trbPower
            // 
            this.trbPower.Location = new System.Drawing.Point(13, 342);
            this.trbPower.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trbPower.Maximum = 100;
            this.trbPower.Name = "trbPower";
            this.trbPower.Size = new System.Drawing.Size(433, 90);
            this.trbPower.TabIndex = 9;
            this.trbPower.TickFrequency = 10;
            this.trbPower.Value = 100;
            this.trbPower.Scroll += new System.EventHandler(this.trbPower_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trbFilter);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.cmbDisplayMode);
            this.groupBox2.Controls.Add(this.picWave);
            this.groupBox2.Controls.Add(this.lblCarrierFreq);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lblOutputPower);
            this.groupBox2.Controls.Add(this.lblOutputFreq);
            this.groupBox2.Location = new System.Drawing.Point(693, 24);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox2.Size = new System.Drawing.Size(826, 758);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "モニター";
            // 
            // trbFilter
            // 
            this.trbFilter.Location = new System.Drawing.Point(13, 154);
            this.trbFilter.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trbFilter.Maximum = 0;
            this.trbFilter.Minimum = -60;
            this.trbFilter.Name = "trbFilter";
            this.trbFilter.Size = new System.Drawing.Size(800, 90);
            this.trbFilter.TabIndex = 16;
            this.trbFilter.TickFrequency = 6;
            this.trbFilter.Value = -24;
            this.trbFilter.Scroll += new System.EventHandler(this.trbFilter_Scroll);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 124);
            this.label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 24);
            this.label9.TabIndex = 15;
            this.label9.Text = "フィルター";
            // 
            // cmbDisplayMode
            // 
            this.cmbDisplayMode.FormattingEnabled = true;
            this.cmbDisplayMode.Items.AddRange(new object[] {
            "U相-V相",
            "V相-W相",
            "W相-U相",
            "U相",
            "V相",
            "W相",
            "位相"});
            this.cmbDisplayMode.Location = new System.Drawing.Point(583, 60);
            this.cmbDisplayMode.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.Size = new System.Drawing.Size(225, 32);
            this.cmbDisplayMode.TabIndex = 14;
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
            // 
            // picWave
            // 
            this.picWave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picWave.Location = new System.Drawing.Point(13, 256);
            this.picWave.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.picWave.Name = "picWave";
            this.picWave.Size = new System.Drawing.Size(799, 488);
            this.picWave.TabIndex = 6;
            this.picWave.TabStop = false;
            // 
            // lblCarrierFreq
            // 
            this.lblCarrierFreq.AutoSize = true;
            this.lblCarrierFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblCarrierFreq.Location = new System.Drawing.Point(340, 54);
            this.lblCarrierFreq.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblCarrierFreq.Name = "lblCarrierFreq";
            this.lblCarrierFreq.Size = new System.Drawing.Size(159, 38);
            this.lblCarrierFreq.TabIndex = 5;
            this.lblCarrierFreq.Text = "0000.0Hz";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(345, 30);
            this.label8.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(154, 24);
            this.label8.TabIndex = 4;
            this.label8.Text = "搬送波周波数";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 30);
            this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 24);
            this.label7.TabIndex = 3;
            this.label7.Text = "出力(%)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(167, 30);
            this.label6.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(130, 24);
            this.label6.TabIndex = 2;
            this.label6.Text = "出力周波数";
            // 
            // lblOutputPower
            // 
            this.lblOutputPower.AutoSize = true;
            this.lblOutputPower.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblOutputPower.Location = new System.Drawing.Point(4, 54);
            this.lblOutputPower.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblOutputPower.Name = "lblOutputPower";
            this.lblOutputPower.Size = new System.Drawing.Size(105, 38);
            this.lblOutputPower.TabIndex = 1;
            this.lblOutputPower.Text = "000%";
            // 
            // lblOutputFreq
            // 
            this.lblOutputFreq.AutoSize = true;
            this.lblOutputFreq.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblOutputFreq.Location = new System.Drawing.Point(163, 54);
            this.lblOutputFreq.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblOutputFreq.Name = "lblOutputFreq";
            this.lblOutputFreq.Size = new System.Drawing.Size(140, 38);
            this.lblOutputFreq.TabIndex = 0;
            this.lblOutputFreq.Text = "000.0Hz";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // trbVolume
            // 
            this.trbVolume.Location = new System.Drawing.Point(9, 141);
            this.trbVolume.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trbVolume.Maximum = -18;
            this.trbVolume.Minimum = -72;
            this.trbVolume.Name = "trbVolume";
            this.trbVolume.Size = new System.Drawing.Size(628, 90);
            this.trbVolume.TabIndex = 11;
            this.trbVolume.TickFrequency = 6;
            this.trbVolume.Value = -48;
            this.trbVolume.Scroll += new System.EventHandler(this.trbVolume_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 111);
            this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 24);
            this.label5.TabIndex = 12;
            this.label5.Text = "音量";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnPlayStop);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.cmbDevices);
            this.groupBox3.Controls.Add(this.trbVolume);
            this.groupBox3.Location = new System.Drawing.Point(26, 24);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBox3.Size = new System.Drawing.Size(654, 288);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1547, 806);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
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
            ((System.ComponentModel.ISupportInitialize)(this.picWave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbVolume)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbDisplayMode;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TrackBar trbFilter;
        private System.Windows.Forms.Label label9;
    }
}

