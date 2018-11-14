using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VRLibrary.ImageProcessing
{
    public class ModelKalman : IDisposable
    {
        public Matrix<float> state;
        public Matrix<float> transitionMatrix;
        public Matrix<float> measurementMatrix;
        public Matrix<float> processNoise;
        public Matrix<float> measurementNoise;
        public Matrix<float> errorCovariancePost;
        public ModelKalman()
        {
            state = new Matrix<float>(6, 1);
            state[0, 0] = 0f; // x-pos
            state[1, 0] = 0f; // y-pos
            state[2, 0] = 0f; // th-pos
            state[3, 0] = 0f; // x-velocity
            state[4, 0] = 0f; // y-velocity
            state[5, 0] = 0f; // th-velocity

            transitionMatrix = new Matrix<float>(new float[,]
                    {
                        {1, 0, 0, 1/60f, 0, 0},  // x-pos, y-pos, x-velocity, y-velocity
                        {0, 1, 0, 0, 1/60f, 0},
                        {0, 0, 1, 0, 0, 1/60f},
                        {0, 0, 0, 1, 0, 0},
                        {0, 0, 0, 0, 1, 0},
                        {0, 0, 0, 0, 0, 1}
                    });
            measurementMatrix = new Matrix<float>(new float[,]
                    {
                        { 1, 0, 0, 0, 0, 0},
                        { 0, 1, 0, 0, 0, 0},
                        { 0, 0, 1, 0, 0, 0}
                    });
            measurementMatrix.SetIdentity();
            processNoise = new Matrix<float>(6, 6); //Linked to the size of the transition matrix
            processNoise.SetIdentity(new MCvScalar(5.0e-3)); //The smaller the value the more resistance to noise 
            measurementNoise = new Matrix<float>(3, 3); //Fixed accordiong to input data 
            measurementNoise.SetIdentity(new MCvScalar(3.0e-2));
            errorCovariancePost = new Matrix<float>(6, 6); //Linked to the size of the transition matrix
            errorCovariancePost.SetIdentity();
            //zerosc = new MCvScalar();
        }

        public Matrix<float> GetMeasurement()
        {
            //Matrix<float> measurementNoise = new Matrix<float>(3, 1);
            //measurementNoise.SetRandNormal(zerosc, new MCvScalar(Math.Sqrt(measurementNoise[0, 0])));
            return measurementMatrix * state;// + measurementNoise;
        }

        public void GoToNextState()
        {
            //Matrix<float> processNoise = new Matrix<float>(6, 1);
            //processNoise.SetRandNormal(zerosc, new MCvScalar(processNoise[0, 0]));
            state = transitionMatrix * state;// + processNoise;
        }

        public void Dispose()
        {
            state.Dispose();
            transitionMatrix.Dispose();
            measurementMatrix.Dispose();
            processNoise.Dispose();
            measurementNoise.Dispose();
            errorCovariancePost.Dispose();
        }
    }
}
