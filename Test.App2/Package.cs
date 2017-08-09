using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App2
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "App 2"; } }

        private readonly IList<IocRegistration> _iocRegistrations;
        public IList<IocRegistration> IocRegistrations{ get { return _iocRegistrations; } }

        /// <summary>
        /// This version of the constructor takes priority. The application can
        /// pass a set of properties when probing for packages, and those properties
        /// will be passed to this constructor.
        /// </summary>
        public Package(IPropertyBag properties)
        {
            _iocRegistrations = new List<IocRegistration>
            {
                // This application depends on Interface2
                new IocRegistration().Init<Interface2>(),
            };
        }

        /// <summary>
        /// This constructor will never get called because the one that
        /// accepts a properties argument takes preference
        /// </summary>
        public Package()
        {
        }
    }
}
