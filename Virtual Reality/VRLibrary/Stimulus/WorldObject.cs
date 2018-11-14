using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRLibrary.Stimulus.ServiceFactories;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus
{
    /* Interface service container implements a service provider with the ability of manage the services */
    public interface IServiceContainer : IServiceProvider
    {
        void AddService(Type serviceType, object serviceInstance);
        void RemoveService(Type serviceType);
    }
    /* Basic object of this program, has the properties of a service container */
    public class WorldObject : IServiceContainer
    {
        // List of World Objects
        public List<WorldObject> WObjects = new List<WorldObject>();

        // List of services
        private Dictionary<Type, object> services = new Dictionary<Type, object>();

        // List of service factories
        public List<ServiceFactory> objectBuilder = new List<ServiceFactory>();

        // Add a child WorldObject to the WorldObject list
        public void AddWorldObject(WorldObject obj)
        {
            WObjects.Add(obj);
        }

        // Get a child WorldObject from the WorldObject list using its name
        public WorldObject GetWorldObject(string str)
        {
            foreach (WorldObject obj in WObjects)
            {
                NameService Name = (NameService)obj.GetService(typeof(NameService));
                if (str == Name.ObjectName())
                {
                    return obj;
                }
            }
            return null;
        }

        // Remove a child WorldObject from the WorldObject list using its name
        public void RemoveWorldObject(string str)
        {
            foreach (WorldObject obj in WObjects)
            {
                NameService Name = (NameService)obj.GetService(typeof(NameService));
                if (str == Name.ObjectName())
                {
                    WObjects.Remove(obj);
                }
            }
        }

        // Add a Service to the Service list
        public void AddService(Type Service, object provider)
        {
            // If we already have this type of service, throw an exception
            if (services.ContainsKey(Service))
                throw new Exception("The service container already has a " + "service provider of type " + Service.Name);

            this.services.Add(Service, provider);
        }

        // Get all Services in the Service list
        public object[] GetAllSevices()
        {
            object[] list = new object[services.Count];
            int i = 0;
            foreach (Type type in services.Keys)
            {
                list[i] = services[type];
                i++;
            }
            return list;
        }

        // Get a specific Service from the Service list
        public object GetService(Type Service)
        {

            object service;
            // If the required Service is that of a service container return this WorldObject itself
            if (Service == typeof(IServiceContainer))
            {
                return this;
            }
            // If we have this type of service, return it, otherwise return null
            services.TryGetValue(Service, out service);
            return service;
        }

        // Check if a type of Service already exists in the Service list
        public bool ContainService(Type Service)
        {
            return services.ContainsKey(Service);
        }

        // Remove a specific Service from the Service list
        public void RemoveService(Type Service)
        {
            if (services.ContainsKey(Service))
                services.Remove(Service);
        }

        // Remove all Services from the Service list
        public void RemoveAllServices()
        {
            services.Clear();
        }
    }
}
