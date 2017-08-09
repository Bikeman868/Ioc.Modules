using System;
using System.Collections.Generic;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.Package1
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Test package 1"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // This package provides Class1 as an implementation of Interface1
                    new IocRegistration().Init<Interface1, Class1>()
                };
            }
        }
    }
}
