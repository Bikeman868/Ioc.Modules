using System;
using System.Reflection;
using Ioc.Modules;
using Ninject;
using Test.Interfaces;

namespace Test.App2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Probe assemblies to find all packages
            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            // Register IoC dependencies with Ninject and build the container
            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            var class2 = ninject.Get<Interface2>();
            class2.Method2();
        }
    }
}
