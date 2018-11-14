using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VRLibrary.Stimulus.Services
{
    /* Service that gives the World Object name */
    public class NameService
    {
        public string name;
        public NameService(string str, Game game)
        {
            name = str;
        }
        /* Method that returns the name */
        public string ObjectName()
        {
            return name;
        }
    }
}
