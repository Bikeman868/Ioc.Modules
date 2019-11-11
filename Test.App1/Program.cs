using System;
using System.Reflection;
using Autofac;
using Ioc.Modules;
using Test.Interfaces;

namespace Test.App1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Probe assemblies in the bin folder to find all packages
            // This will also add the program executable, and any IoC
            // it defines will override those defined in libraries
            var packages = new PackageLocator()
                .Add(Assembly.GetExecutingAssembly())
                .ProbeBinFolderAssemblies();

            // Register IoC dependencies with Autofac and build the Autofac container
            var builder = new ContainerBuilder();
            var autofacBuilder = new Ioc.Modules.Autofac.Builder();
            var container = autofacBuilder.Build(packages, builder);

            // Use Autofac to construct instances with their dependencies
            using (var scope = container.BeginLifetimeScope())
            {
                // Set a break point here and step through. 
                // The package will call back into the application
                var class2 = scope.Resolve<Interface2>();
                class2.Method2();

                // Wait for key press to exit
                Console.ReadLine();
            }
        }
    }
}
