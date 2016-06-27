using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App3
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "App 3"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // Custom construction and initialization of Class3
                    new IocRegistration().Init<Interface3>(() => new Class3()),

                    // This application depends on Interface2
                    new IocRegistration().Init<Interface2>(),
                };
            }
        }
    }
}
