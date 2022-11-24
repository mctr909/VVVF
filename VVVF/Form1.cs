using System;
using System.Drawing;
using System.Windows.Forms;

namespace VVVF {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private const int SCOPE_SPEED = 12;

        private VvvfOut mWaveOut;
        private DoubleBufferGraphic mWaveGraph;
        private float mScopeA = 0.0f;
        private float mScopeB = 0.0f;

        private void Form1_Load(object sender, EventArgs e) {
            mWaveOut = new VvvfOut(SCOPE_SPEED * picWave.Width);
            var waveOutList = mWaveOut.WaveOutList();
            foreach(var device in waveOutList) {
                cmbDevices.Items.Add(device.Item1);
            }
            cmbDevices.SelectedIndex = 0;
            mWaveOut.Open(0xFFFFFFFF);
            mWaveGraph = new DoubleBufferGraphic(picWave, null);
            cmbDisplayMode.SelectedIndex = 0;

            btnPlayStop_Click(null, null);
            trbVolume_Scroll(null, null);
            trackBar1_Scroll(null, null);
            trbAcc_Scroll(null, null);
            trbPower_Scroll(null, null);
            trbFilter_Scroll(null, null);

            timer1.Interval = 1;
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

        private void trbFilter_Scroll(object sender, EventArgs e) {
            mWaveOut.Filter = Math.Pow(10.0, trbFilter.Value / 20.0);
        }

        private void cmbDisplayMode_SelectedIndexChanged(object sender, EventArgs e) {
            switch (cmbDisplayMode.SelectedIndex) {
            case 0:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.U_V;
                break;
            case 1:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.V_W;
                break;
            case 2:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.W_U;
                break;
            case 3:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.U;
                break;
            case 4:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.V;
                break;
            case 5:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.W;
                break;
            case 6:
                mWaveOut.DisplayMode = VvvfOut.EDisplayMode.PHASE;
                break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            lblOutputPower.Text = string.Format("{0}%", (100 * mWaveOut.CurrentPower).ToString("000.0"));
            lblOutputFreq.Text = string.Format("{0}Hz", mWaveOut.CurrentFreq.ToString("000.0"));
            lblCarrierFreq.Text = string.Format("{0}Hz", mWaveOut.CarrierFreq.ToString("0000.0"));

            var graph = mWaveGraph.Graphics;
            var center = picWave.Width / 2.0f;
            var maxAmp = picWave.Height / 2.0f;
            var neutralLevel = maxAmp - 1.0f;

            if (VvvfOut.EDisplayMode.PHASE == mWaveOut.DisplayMode) {
                graph.DrawLine(Pens.Red, 0, neutralLevel, picWave.Width, neutralLevel);
                graph.DrawLine(Pens.Red, center, 0, center, picWave.Height);
                for (int i = 0; i < mWaveOut.ScopeA.Length; i++) {
                    var x = center + (float)mWaveOut.ScopeA[i] * maxAmp;
                    var y = neutralLevel - (float)mWaveOut.ScopeB[i] * maxAmp;
                    graph.DrawLine(Pens.Green, mScopeA, mScopeB, x, y);
                    mScopeA = x;
                    mScopeB = y;
                }
            } else {
                const float scale = 0.475f;
                var top = neutralLevel - maxAmp * scale;
                var bottom = neutralLevel + maxAmp * scale;
                graph.DrawLine(Pens.Gray, 0, top, picWave.Width, top);
                graph.DrawLine(Pens.Gray, 0, bottom, picWave.Width, bottom);
                top = neutralLevel - maxAmp * scale * 2;
                bottom = neutralLevel + maxAmp * scale * 2;
                graph.DrawLine(Pens.Gray, 0, top, picWave.Width, top);
                graph.DrawLine(Pens.Gray, 0, bottom, picWave.Width, bottom);
                mScopeA = neutralLevel - (float)(mWaveOut.ScopeA[0] * maxAmp * scale);
                mScopeB = neutralLevel - (float)(mWaveOut.ScopeB[0] * maxAmp * scale);
                for (int i = 1, s = 0; s < picWave.Width; i += SCOPE_SPEED, s++) {
                    var sumA = 0.0f;
                    var sumB = 0.0f;
                    for (int j = i; j < i + SCOPE_SPEED && j < mWaveOut.ScopeA.Length; j++) {
                        sumA += (float)mWaveOut.ScopeA[j];
                        sumB += (float)mWaveOut.ScopeB[j];
                    }
                    sumA = neutralLevel - sumA * maxAmp * scale / SCOPE_SPEED;
                    sumB = neutralLevel - sumB * maxAmp * scale / SCOPE_SPEED;
                    graph.DrawLine(Pens.Green, s, mScopeA, s + 1, sumA);
                    graph.DrawLine(Pens.DeepSkyBlue, s, mScopeB, s + 1, sumB);
                    mScopeA = sumA;
                    mScopeB = sumB;
                }
                graph.DrawLine(Pens.Red, 0, neutralLevel, picWave.Width, neutralLevel);
            }
            mWaveGraph.Render();
        }
    }
}
