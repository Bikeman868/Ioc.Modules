using Ninject;
using Ninject.Modules;

namespace Ioc.Modules.Ninject
{
    public class Module : NinjectModule, IContainer
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
                    if (registration.Instance == null)
                    {
                        var instanceFunction = registration.InstanceFunction;
                        if (instanceFunction != null)
                        {
                            bind
                                .ToMethod(c => instanceFunction(this))
                                .InSingletonScope();
                        }
                    }
                    else
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

        T IContainer.Resolve<T>()
        {
            return Kernel.Get<T>();
        }
    }
}
