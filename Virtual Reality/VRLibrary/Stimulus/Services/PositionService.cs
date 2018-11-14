using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Stimulus.Services
{
    /* Service that gives the World Object position in space */
    public class PositionService
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 reference;
        public PositionService(Game game) { }

        /* Method that returns the world matrix */
        public Matrix PosMatrix()
        {
            return Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);
        }
    }
}
