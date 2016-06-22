using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App2
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "App 2"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // This application depends on Interface2
                    new IocRegistration().Init<Interface2>(),
                };
            }
        }
    }
}
