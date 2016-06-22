using System;
using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.Package3
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Test package 3"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // This package contains Class3 as an implementation of Interface3
                    new IocRegistration().Init<Interface3, Class3>(), 

                    // This package contains classes with dependencies on Interface1 and Interface2
                    new IocRegistration().Init<Interface1>(),
                    new IocRegistration().Init<Interface2>(),
                };
            }
        }
    }
}
