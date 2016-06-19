using Ninject.Modules;

namespace Ioc.Modules.Ninject
{
    public class Module : NinjectModule
    {
        private readonly PackageLocator _packageLocator;

        public Module(PackageLocator packageLocator)
        {
            _packageLocator = packageLocator;
        }

        public override void Load()
        {
            var regitrations = _packageLocator.GetAllRegistrations();
            foreach (var registration in regitrations)
            {
                switch (registration.Lifetime)
                {
                    case IocLifetime.SingleInstance:
                        Bind(registration.Interfacetype).To(registration.ConcreteType).InSingletonScope();
                        break;
                    case IocLifetime.MultiInstance:
                        Bind(registration.Interfacetype).To(registration.ConcreteType).InTransientScope();
                        break;
                }
            }
        }
    }
}
