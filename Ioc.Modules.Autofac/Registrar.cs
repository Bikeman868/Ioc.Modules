using Autofac;

namespace Ioc.Modules.Autofac
{
    public class Registrar
    {
        public static void Register(PackageLocator packageLocator, ContainerBuilder builder)
        {
            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                var autofacRegistration = builder
                    .RegisterType(registration.ConcreteType)
                    .As(registration.Interfacetype);

                if (registration.Lifetime == IocLifetime.SingleInstance)
                    autofacRegistration.SingleInstance();
                else if (registration.Lifetime == IocLifetime.MultiInstance)
                    autofacRegistration.InstancePerDependency();
            }
        }
    }
}
