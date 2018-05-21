namespace Ioc.Modules
{
    /// <summary>
    /// This interface must be implemented by any wrapper around the IoC
    /// container being used by the application.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Returns an object of type T by resolving dependencies and
        /// constructing instances as necessary.
        /// </summary>
        /// <typeparam name="T">If this is a concrete type then it will
        /// be constructed by IoC. If this in an interface type then
        /// IoC will return an instance based on the lifetime defined
        /// for this interface mapping</typeparam>
        /// <returns></returns>
        T Resolve<T>();
    }
}
