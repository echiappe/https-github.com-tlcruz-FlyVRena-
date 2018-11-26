using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Drawing;
using System.IO;

namespace VRLibrary.ImageProcessing
{
    //pass the tracking data through a kalman filter to reduce noise
    public class KalmanFilterTrack : IDisposable
    {
        private ModelKalman mk;
        private Kalman kal;
        Matrix<float> estimated;

        bool start;
        
        
        public float[] pars;
        public KalmanFilterTrack()
        {
            //initialize new kalman filter with appropriate number of parameters
            kal = new Kalman(6, 3, 0);
            mk = new ModelKalman();
            Matrix<float> state = new Matrix<float>(new float[]
            {
                    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
            });

            //define kalman filter according with the kalman model
            kal.CorrectedState = state;
            kal.TransitionMatrix = mk.transitionMatrix;
            kal.MeasurementNoiseCovariance = mk.measurementNoise;
            kal.ProcessNoiseCovariance = mk.processNoise;
            kal.ErrorCovariancePost = mk.errorCovariancePost;
            kal.MeasurementMatrix = mk.measurementMatrix;
            start = true;
            pars = new float[3];
        }

        //Filter data and store values
        public void filterPoints(float[] pt)
        {
            //add tracking data as filter state
            mk.state[0, 0] = pt[0];
            mk.state[1, 0] = pt[1];
            if (start)
            {
                //if it is first run of the filter add orientation value directly
                mk.state[2, 0] = pt[2];
                start = false;
            }
            else
            {
                //if it is not the first frame corect orientation to avoid high derivatives in rotation
                //there is a factor of 5 to make rotations a similar order of magnitude as translation
                mk.state[2, 0] = 5f*CorrectedOrientation(pt[2], estimated[2, 0]/5f);
            }
            
            //run filter and estimate real position and orientation
            kal.Predict();
            estimated = kal.Correct(mk.GetMeasurement());
            mk.GoToNextState();
            
            //save estimated position
            pars[0] = estimated[0, 0];
            pars[1] = estimated[1, 0];
            pars[2] = estimated[2, 0]/5f;
        }

        //correct orientation to try to avoid jumps and make it contnuous (no modulus)
        int npi = 0;
        public float CorrectedOrientation(float or, float orp)
        {
            //number of pi rotations in one frame
            npi = (int) Math.Round((orp - or) / 180f);

            //remove or add appropriate pi rotations
            if (Math.Abs(Mod2pi(or - orp + 180f) / 2) > Math.Abs(Mod2pi(or - orp)))
            {
                return or + (npi - npi%2) * 180f;
            }
            else
            {
               return or + npi * 180f;
            }
        }

        //Make the angle modulus 2pi 
        public float Mod2pi(float or)
        {
            while (or <= -180f) or += 2 * (float)180f;
            while (or > 180f) or -= 2 * (float)180f;
            return or;
        }
        
        //Dispose of all initiated objects
        public void Dispose()
        {
            kal.Dispose();
            mk.Dispose();
        }
    }
}
