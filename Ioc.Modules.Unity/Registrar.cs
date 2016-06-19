using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace Ioc.Modules.Unity
{
    public class Registrar
    {
        public static void Register(PackageLocator packageLocator, UnityContainer unity)
        {
            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                switch (registration.Lifetime)
                {
                    case IocLifetime.SingleInstance:
                        unity.RegisterType(
                            registration.Interfacetype,
                            registration.ConcreteType,
                            new ContainerControlledLifetimeManager());
                        break;
                    case IocLifetime.MultiInstance:
                        unity.RegisterType(
                            registration.Interfacetype,
                            registration.ConcreteType,
                            new ExternallyControlledLifetimeManager());
                        break;
                }
            }
        }
    }
}
