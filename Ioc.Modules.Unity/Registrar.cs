using Microsoft.Practices.Unity;

namespace Ioc.Modules.Unity
{
    public class Registrar
    {
        public static void Register(PackageLocator packageLocator, UnityContainer unity)
        {
            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                if (registration.ConcreteType == null)
                {
                    if (registration.Instance != null)
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
        }
    }
}
