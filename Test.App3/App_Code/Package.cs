using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App3
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "App 3"; } }

        private readonly IList<IocRegistration> _iocRegistrations;
        public IList<IocRegistration> IocRegistrations{ get { return _iocRegistrations; } }

        public Package(IPropertyBag properties)
        {
            // This allows the application to set a 'IsAmazonWebServices' property
            // which can affect how interfaces and concrete types are registered
            // with IoC.
            var isAmazonWebServices = properties.Get<bool>("IsAmazonWebServices");

            _iocRegistrations = new List<IocRegistration>
            {
                // Custom construction and initialization of Class3
                new IocRegistration().Init<Interface3>(() => new Class3(isAmazonWebServices)),

                // This application depends on Interface2
                new IocRegistration().Init<Interface2>(),
            };
        }
    }
}
