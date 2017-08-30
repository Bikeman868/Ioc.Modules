namespace Ioc.Modules
{
    public interface IPropertyBag
    {
        /// <summary>
        /// Gets or sets a named string in the property bag
        /// </summary>
        string this[string name] { get; set; }

        /// <summary>
        /// Gets a strongly typed value from this property bag
        /// </summary>
        /// <typeparam name="T">The type of value to store in the property bag</typeparam>
        /// <param name="name">Optional name in case you have more than one property of the same type.</param>
        /// <returns>The value of the property from this property bag if it exists.</returns>
        T Get<T>(string name = null);

        /// <summary>
        /// Replaces the value of a strongly typed property in the property bag.
        /// </summary>
        /// <typeparam name="T">The type of data to store</typeparam>
        /// <param name="value">The value to store</param>
        /// <param name="name">Optional. Only needed if you store multiple values of the same type</param>
        void Set<T>(T value, string name = null);
    }
}