using System;

namespace Ioc.Modules
{
    /// <summary>
    /// Encapsulates information needed to configure the IoC container
    /// </summary>
    public class IocRegistration
    {
        /// <summary>
        /// Initializes this IocRegistration where this package defines both the
        /// interface and also contains the concrete implementation of
        /// that inetrface
        /// </summary>
        /// <typeparam name="TInterface">The interface to register</typeparam>
        /// <typeparam name="TClass">The type that implements this interface</typeparam>
        /// <param name="lifetime">The lifetime management expected for this interface</param>
        /// <returns>this for fluid configuration</returns>
        public IocRegistration Init<TInterface, TClass>(IocLifetime lifetime = IocLifetime.SingleInstance) 
            where TInterface: class
            where TClass: class, TInterface
        {
            InterfaceType = typeof (TInterface);
            ConcreteType = typeof (TClass);
            Lifetime = lifetime;

            return this;
        }

        /// <summary>
        /// Initializes this an IoC registration where this package needs an interface
        /// to be implemented by the application or another package. In this case
        /// this package defines the interface but does not contain a concrete
        /// implementation of it.
        /// </summary>
        /// <typeparam name="TInterface">The interface to register</typeparam>
        /// <param name="lifetime">The lifetime management expected for this intarfece</param>
        /// <returns>this for fluid configuration</returns>
        public IocRegistration Init<TInterface>(IocLifetime lifetime = IocLifetime.SingleInstance)
            where TInterface : class
        {
            InterfaceType = typeof(TInterface);
            Lifetime = lifetime;

            return this;
        }

        /// <summary>
        /// Initializes this IoC registration where this package contains the concrete
        /// implementation of a singleton, but the singleton has some special initialization
        /// required that can not be carried out by the IoC container. In this case a function
        /// is supplied to do the construction and initialization
        /// </summary>
        /// <typeparam name="TInterface">The interface to register</typeparam>
        /// <param name="instanceFunction">A function that will return an instance that implements the interface</param>
        /// <returns>this for fluid configuration</returns>
        public IocRegistration Init<TInterface>(Func<IContainer, TInterface> instanceFunction)
            where TInterface : class
        {
            InterfaceType = typeof(TInterface);
            Lifetime = IocLifetime.SingleInstance;
            InstanceFunction = instanceFunction;

            return this;
        }

        /// <summary>
        /// The type of the interface to register with IoC
        /// </summary>
        public Type InterfaceType { get; set; }

        /// <summary>
        /// The concrete implementation of this interface, or null if the application must
        /// provide an implementation
        /// </summary>
        public Type ConcreteType { get; set; }

        /// <summary>
        /// The lifetime management that the package assumes for this interface
        /// </summary>
        public IocLifetime Lifetime { get; set; }

        /// <summary>
        /// A function that will construct and inisialize an object of Interfacetype
        /// </summary>
        public Func<IContainer, object> InstanceFunction { get; set; }

        /// <summary>
        /// A function that will construct and inisialize an object of Interfacetype
        /// </summary>
        public object Instance { get; set; }
    }
}
