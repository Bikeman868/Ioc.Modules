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
                var bind = Bind(registration.InterfaceType);
                if (registration.ConcreteType == null)
                {
                    if (registration.Instance != null)
                    {
                        bind
                            .ToConstant(registration.Instance)
                            .InSingletonScope();
                    }
                }
                else
                {
                    switch (registration.Lifetime)
                    {
                        case IocLifetime.SingleInstance:
                            bind
                                .To(registration.ConcreteType)
                                .InSingletonScope();
                            break;
                        case IocLifetime.MultiInstance:
                            bind
                                .To(registration.ConcreteType)
                                .InTransientScope();
                            break;
                    }
                }
            }
        }
    }
}
