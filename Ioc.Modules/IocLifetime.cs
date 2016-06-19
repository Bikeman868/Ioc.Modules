namespace Ioc.Modules
{
    /// <summary>
    /// This defines that packages expectation for the lifetime of instances
    /// </summary>
    public enum IocLifetime
    {
        /// <summary>
        /// The application will contain a single instance and each time the
        /// same interface is injected as a dependency the same instance will
        /// be injected. In this case the application can also create one instance
        /// per execution context without breaking any assumptions, for example
        /// if the concrete implementation is not thread safe and the code uses may
        /// threads then the application can construct one instance per thread.
        /// </summary>
        SingleInstance,

        /// <summary>
        /// The application will constrct a new instance every time a dependency
        /// is injected.
        /// </summary>
        MultiInstance
    }
}
