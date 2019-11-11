using System;
using Ioc.Modules;
using Ioc.Modules.Ninject;
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
                .Add(System.Reflection.Assembly.GetExecutingAssembly())
                .ProbeBinFolderAssemblies();

            // Register IoC dependencies with Ninject and build the Ninject container
            var ninject = new StandardKernel(new Module(packageLocator));

            // Now we can construct instances and their dependencies will be satisfied
            var class2 = ninject.Get<Interface2>();
            class2.Method2();

            // Wait for key press to exit
            Console.ReadLine();
        }
    }
}
