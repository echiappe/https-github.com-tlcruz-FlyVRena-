using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Stimulus.Services
{
    public class CameraService
    {
        public PositionService pos;
        public IServiceProvider obj;
        public float fieldOfView;
        public float aspectRatio;
        public float farPlane;
        public float nearPlane;
        public float left;
        public float right;
        public float bottom;
        public float top;
        public bool IsPerspective;

        /* Subscribe a World Object to be the Camera */
        public CameraService(IServiceProvider wo, Game game, float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
        {
            obj = wo;
            pos = (PositionService)obj.GetService(typeof(PositionService));
            this.fieldOfView = fieldOfView;
            this.aspectRatio = aspectRatio;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
            IsPerspective = true;
        }

        public CameraService(IServiceProvider wo, Game game, float left, float right, float bottom, float top, float nearPlane, float farPlane)
        {
            obj = wo;
            pos = (PositionService)obj.GetService(typeof(PositionService));
            this.left = left;
            this.right = right;
            this.bottom = bottom;
            this.top = top;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
            IsPerspective = false;
        }

        /* Methods that return the matrices view and perspective for this Camera*/
        public Matrix CamView()
        {
            return Matrix.CreateLookAt(pos.position, pos.reference, Vector3.UnitY);
        }

        public Matrix CamPerspective()
        {
            if (IsPerspective)
            {
                return Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);
            }
            else
                return Matrix.CreateOrthographicOffCenter(left, right, bottom, top, nearPlane, farPlane);
        }
    }
}
