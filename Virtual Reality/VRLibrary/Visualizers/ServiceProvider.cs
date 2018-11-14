using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Visualizers
{
    public class ServiceProvider : IServiceProvider
    {
        public List<IDialogTypeVisualizerService> services = new List<IDialogTypeVisualizerService>();
        public object GetService(Type serviceType)
        {
            foreach (object service in services)
            {
                return service;
            }

            return null;
        }
    }
}
