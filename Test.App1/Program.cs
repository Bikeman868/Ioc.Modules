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
            // Probe assemblies to find all packages
            var packages = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            // Register IoC dependencies with Autofac and build the container
            var builder = new ContainerBuilder();
            Ioc.Modules.Autofac.Registrar.Register(packages, builder);
            var container = builder.Build();

            // This is how Autofac does IoC
            using (var scope = container.BeginLifetimeScope())
            {
                // Set a break point here and step through. 
                // The package will call back into the application
                var class2 = scope.Resolve<Interface2>();
                class2.Method2();
            }
        }
    }
}
