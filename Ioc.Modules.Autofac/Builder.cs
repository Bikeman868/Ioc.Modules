using Autofac;

namespace Ioc.Modules.Autofac
{
    public class Builder: IContainer
    {
        private global::Autofac.IContainer _autofacContainer;

        public global::Autofac.IContainer Build(PackageLocator packageLocator, ContainerBuilder builder)
        {
            foreach(var registration in packageLocator.GetAllRegistrations())
            {
                if (registration.ConcreteType == null)
                {
                    var instance = registration.Instance;
                    if (instance == null)
                    {
                        var instanceFunction = registration.InstanceFunction;
                        if (instanceFunction != null)
                        {
                            builder
                                .Register(i => instanceFunction(this))
                                .As(registration.InterfaceType)
                                .SingleInstance();
                        }
                    }
                    else
                    {
                        builder
                            .Register(i => instance)
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

            _autofacContainer = builder.Build();
            return _autofacContainer;
        }

        T IContainer.Resolve<T>()
        {
            return _autofacContainer.Resolve<T>();
        }
    }
}
