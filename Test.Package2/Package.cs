using System;
using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.Package2
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Test package 2"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // This package provides Class2 as an implementation of Interface2
                    new IocRegistration().Init<Interface2, Class2>(), 

                    // This package has a dependency on Interface1 which must be 
                    // provided by another package or by the application
                    new IocRegistration().Init<Interface1>()
                };
            }
        }
    }
}
