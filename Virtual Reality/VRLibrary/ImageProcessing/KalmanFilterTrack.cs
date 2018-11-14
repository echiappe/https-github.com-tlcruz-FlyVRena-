using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Drawing;
using System.IO;

namespace VRLibrary.ImageProcessing
{
    public class KalmanFilterTrack : IDisposable
    {
        private ModelKalman mk;
        private Kalman kal;
        bool start;
        int npi = 0;
        Matrix<float> estimated;
        public float[] pars;
        public KalmanFilterTrack()
        {
            kal = new Kalman(6, 3, 0);
            mk = new ModelKalman();
            Matrix<float> state = new Matrix<float>(new float[]
            {
                    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
            });
            kal.CorrectedState = state;
            kal.TransitionMatrix = mk.transitionMatrix;
            kal.MeasurementNoiseCovariance = mk.measurementNoise;
            kal.ProcessNoiseCovariance = mk.processNoise;
            kal.ErrorCovariancePost = mk.errorCovariancePost;
            kal.MeasurementMatrix = mk.measurementMatrix;
            start = true;
            pars = new float[3];
        }

        public void filterPoints(float[] pt)
        {
            mk.state[0, 0] = pt[0];
            mk.state[1, 0] = pt[1];
            if (start)
            {
                mk.state[2, 0] = pt[2];
                start = false;
            }
            else
            {
                mk.state[2, 0] = 5f*CorrectedOrientation(pt[2], estimated[2, 0]/5f);
            }
            kal.Predict();
            estimated = kal.Correct(mk.GetMeasurement());
            mk.GoToNextState();
            pars[0] = estimated[0, 0];
            pars[1] = estimated[1, 0];
            pars[2] = estimated[2, 0]/5f;
        }

        public float CorrectedOrientation(float or, float orp)
        {
            npi = (int) Math.Round((orp - or) / 180f);
            if (Math.Abs(Mod2pi(or - orp + 180f) / 2) > Math.Abs(Mod2pi(or - orp)))
            {
                return or + (npi - npi%2) * 180f;
            }
            else
            {
               return or + npi * 180f;
            }
        }

        public float Mod2pi(float or)
        {
            while (or <= -180f) or += 2 * (float)180f;
            while (or > 180f) or -= 2 * (float)180f;
            return or;
        }

        public void Dispose()
        {
            kal.Dispose();
            mk.Dispose();
        }
    }
}
