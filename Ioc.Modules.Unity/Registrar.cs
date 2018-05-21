using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace Ioc.Modules.Unity
{
    public static class Registrar
    {
        public static void Register(PackageLocator packageLocator, UnityContainer unity)
        {
            var functionRegistrations = new List<IocRegistration>();

            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                if (registration.ConcreteType == null)
                {
                    var instance = registration.Instance;
                    if (instance == null)
                    {
                        if (registration.InstanceFunction != null)
                        {
                            functionRegistrations.Add(registration);
                        }
                    }
                    else
                    {
                        unity.RegisterInstance(registration.InterfaceType, registration.Instance);
                    }
                }
                else
                {
                    switch (registration.Lifetime)
                    {
                        case IocLifetime.SingleInstance:
                            unity.RegisterType(
                                registration.InterfaceType,
                                registration.ConcreteType,
                                new ContainerControlledLifetimeManager());
                            break;
                        case IocLifetime.MultiInstance:
                            unity.RegisterType(
                                registration.InterfaceType,
                                registration.ConcreteType,
                                new ExternallyControlledLifetimeManager());
                            break;
                    }
                }
            }

            // We need to postpone these registrations because the registration function
            // will construct instances and all of their dependencies must already be
            // registered for this to work.
            // This won't always work in the case where registrations with instance functions
            // depend on each other. In this case don't use Unity, use Autofac or Ninject instead.

            if (functionRegistrations.Count > 0)
            {
                var wrapper = new UnityWrapper(unity);
                foreach (var registration in functionRegistrations)
                {
                    unity.RegisterInstance(registration.InterfaceType, registration.InstanceFunction(wrapper));
                }
            }
        }

        private class UnityWrapper : IContainer
        {
            private readonly UnityContainer _unity;
 
            public UnityWrapper(UnityContainer unity)
            {
                _unity = unity;
            }

            T IContainer.Resolve<T>()
            {
                return _unity.Resolve<T>();
            }
        }
    }
}
