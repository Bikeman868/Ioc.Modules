using Autofac;

namespace Ioc.Modules.Autofac
{
    public class Registrar
    {
        public static void Register(PackageLocator packageLocator, ContainerBuilder builder)
        {
            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                if (registration.ConcreteType == null)
                {
                    if (registration.Instance != null)
                    {
                        var reg = registration;
                        builder
                            .Register(i => reg.Instance)
                            .As(registration.InterfaceType)
                            .SingleInstance();
                    }
                }
                else
                {
                    var autofacRegistration = builder
                        .RegisterType(registration.ConcreteType)
                        .As(registration.InterfaceType);

                    if (registration.Lifetime == IocLifetime.SingleInstance)
                        autofacRegistration.SingleInstance();

                    else if (registration.Lifetime == IocLifetime.MultiInstance)
                        autofacRegistration.InstancePerDependency();
                }
            }
        }
    }
}
