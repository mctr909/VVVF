using System;
using System.Drawing;
using System.Windows.Forms;

namespace VVVF {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private VvvfOut mWaveOut;
        private DoubleBufferGraphic mWaveGraph;
        private float mScopeA = 0.0f;
        private float mScopeB = 0.0f;

        private void Form1_Load(object sender, EventArgs e) {
            mWaveOut = new VvvfOut();
            var waveOutList = mWaveOut.WaveOutList();
            foreach(var device in waveOutList) {
                cmbDevices.Items.Add(device.Item1);
            }
            cmbDevices.SelectedIndex = 0;
            mWaveOut.Open(0xFFFFFFFF);
            mWaveGraph = new DoubleBufferGraphic(picWave, null);
            cmbDisplayMode.SelectedIndex = 0;
            timer1.Interval = 10;
            timer1.Start();
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e) {
            uint selectedDevice;
            if (0 == cmbDevices.SelectedIndex) {
                selectedDevice = 0xFFFFFFFF;
            } else {
                selectedDevice = (uint)(cmbDevices.SelectedIndex - 1);
            }
            mWaveOut.Open(selectedDevice);
        }

        private void btnPlayStop_Click(object sender, EventArgs e) {
            mWaveOut.IsPlay = !mWaveOut.IsPlay;
            if (mWaveOut.IsPlay) {
                btnPlayStop.Text = "停止";
            } else {
                btnPlayStop.Text = "再生";
            }
        }

        private void trbVolume_Scroll(object sender, EventArgs e) {
            mWaveOut.Volume = Math.Pow(10.0, trbVolume.Value / 20.0);
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
            mWaveOut.TargetFreq = trbTargetFreq.Value;
            lblTargetFreq.Text = string.Format("{0}Hz", mWaveOut.TargetFreq.ToString("000"));
        }

        private void trbAcc_Scroll(object sender, EventArgs e) {
            mWaveOut.Acc = trbAcc.Value * 0.1;
            lblAcc.Text = string.Format("{0}Hz/sec", mWaveOut.Acc.ToString("00.0"));
        }

        private void trbPower_Scroll(object sender, EventArgs e) {
            mWaveOut.TargetPower = trbPower.Value * 0.01;
            lblPower.Text = string.Format("{0}%", trbPower.Value.ToString("000"));
        }

        private void cmbDisplayMode_SelectedIndexChanged(object sender, EventArgs e) {
            switch (cmbDisplayMode.SelectedIndex) {
            case 0:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.U_V;
                break;
            case 1:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.U;
                break;
            case 2:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.PHASE;
                break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            lblOutputPower.Text = string.Format("{0}%", (100 * mWaveOut.CurrentPower).ToString("000.0"));
            lblOutputFreq.Text = string.Format("{0}Hz", mWaveOut.CurrentFreq.ToString("000.0"));
            lblCarrierFreq.Text = string.Format("{0}Hz", mWaveOut.CarrierFreq.ToString("0000.0"));

            var graph = mWaveGraph.Graphics;

            if (mWaveOut.DisplayMode == VvvfOut.EDisplayMode.PHASE) {
                graph.DrawLine(Pens.Red, 0, picWave.Height / 2, picWave.Width, picWave.Height / 2);
                graph.DrawLine(Pens.Red, picWave.Width / 2, 0, picWave.Width / 2, picWave.Height);
                var ofsX = picWave.Width / 2.0f;
                var ofsY = picWave.Height / 2.0f;
                for (int i = 0; i < mWaveOut.ScopeA.Length; i++) {
                    var x1 = ofsX + (float)mWaveOut.ScopeA[i] * picWave.Height / 2.0f;
                    var y1 = ofsY - (float)mWaveOut.ScopeB[i] * picWave.Height / 2.0f;
                    graph.DrawLine(Pens.Green, mScopeA, mScopeB, x1, y1);
                    mScopeA = x1;
                    mScopeB = y1;
                }
            } else {
                const int SPEED = 7;
                const float scale = 0.8f;
                var top = picWave.Height * (0.5f - 0.5f * scale);
                var bottom = picWave.Height * (0.5f + 0.5f * scale);
                graph.DrawLine(Pens.Gray, 0, top, picWave.Width, top);
                graph.DrawLine(Pens.Gray, 0, bottom, picWave.Width, bottom);
                mScopeA = (float)(picWave.Height * (0.5 - 0.5 * scale * mWaveOut.ScopeA[0]));
                mScopeB = (float)(picWave.Height * (0.5 - 0.5 * scale * mWaveOut.ScopeB[0]));
                for (int i = 0, s = 0; s < picWave.Width; i += SPEED, s++) {
                    var sumA = 0.0f;
                    var sumB = 0.0f;
                    for (int j = i; j < i + SPEED && j < mWaveOut.ScopeA.Length; j++) {
                        sumA += (float)(picWave.Height * (0.5 - 0.5 * scale * mWaveOut.ScopeA[j]));
                        sumB += (float)(picWave.Height * (0.5 - 0.5 * scale * mWaveOut.ScopeB[j]));
                    }
                    sumA /= SPEED;
                    sumB /= SPEED;
                    graph.DrawLine(Pens.Green, s, mScopeA, s + 1, sumA);
                    graph.DrawLine(Pens.DeepSkyBlue, s, mScopeB, s + 1, sumB);
                    mScopeA = sumA;
                    mScopeB = sumB;
                }
                graph.DrawLine(Pens.Red, 0, picWave.Height / 2, picWave.Width, picWave.Height / 2);
            }
            mWaveGraph.Render();
        }
    }
}
