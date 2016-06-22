using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App1
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "App 1"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // This application implements Interface1
                    new IocRegistration().Init<Interface1, Class1>(),

                    // This application depends on Interface2
                    new IocRegistration().Init<Interface2>(),
                };
            }
        }
    }
}
