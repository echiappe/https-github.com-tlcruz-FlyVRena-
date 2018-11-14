using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Factory for a ModelService */
    public class ModelFactory : ServiceFactory
    {
        public string name;
        public string directory;

        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            ContentManager contentManager = new ContentManager(game.Services, directory);
            var model = contentManager.Load<Model>(name);
            wo.AddService(typeof(Model), model);
        }
    }
}
