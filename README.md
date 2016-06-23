# Ioc.Modules

This project is aimed at providing a mechanism for NuGet packages to 
provide information about their IoC requirements to applications. This
will allow the application developer to integrate third party libraries
seamlessly without having to read documentation about the types in the
library that need to be registered with IoC. It also means that library
developers can add new interfaces and the classes that implement them
without breaking applications that use these libraries.

## Package developers

You need to add classes into your packages that define the IoC needs
of your package. I recommend that you call your class `Package` and place it 
in the root folder of your project. For this to work you must decorate the class 
with the `[Package]` attribute and to implement the `IPackage` interface.

Here is a simple example:
```
    [Package]
    public class Package: IPackage
    {
        public string Name {get { return "My next great thing"; }}

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<ILog, TraceLog>(IocLifetime.SingleInstance),
                    new IocRegistration().Init<IExceptionReporter>(IocLifetime.SingleInstance)
                };
            }
        }
    }
```
In this example the package is declaring that 
* This package needs the application to register the interface `ILog` to resolve to the concrete type 
`TraceLog` in its IoC container (the application can also choose to register `ILog` to any other class that 
implements `ILog`).
* the package depends on the `IExceptionReporter` interface but does not contain an implementation of it 
and therefore the application must provide one. This can be provided by installing another package that
registers a concrete implemntation, or the application developer can write one.

Note that adding this class to your package does not force applications to use it. They can choose to
use `Ioc.Modules` to configure their IoC container or they can configure IoC in some other way, or 
choose not to use IoC at all. By adding this class to your package you are providing a very simple and
convenient way for the application developer to configure IoC only if they choose to use it.

## Application developers

When you use a package in your application that needs IoC setup and it
has a dependency on this package, then all you need to do is initialize 
one of the IoC container adapters and you're done. For example if you want to use `Ninject` as
your IoC container, then add the `Ioc.Modules.Ninject` NuGet package to your application and
initialize it as described in its readme file.

This is what the `Ninject` example would look like if you want to probe all loaded assemblies for packages:

```
    var packageLocator = new PackageLocator().ProbeAllLoadedAssemblies();
    var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));
```

And the Unity version is also very little code
```
    var unityContainer = new UnityContainer();
    var packageLocator = new PackageLocator().ProbeAllLoadedAssemblies();
	Ioc.Modules.Unity.Registrar.Register(packageLocator, unityContainer);
```

And the Autofac version looks like this
```
    var packages = new PackageLocator().ProbeBinFolderAssemblies();
    var builder = new ContainerBuilder();
    Ioc.Modules.Autofac.Registrar.Register(packages, builder);
    var container = builder.Build();
```

This `Ioc.Modules` package is very convenient for package authors because they can configure 
IoC for thier package without knowing anything about the IoC container you are using in your 
application. You can also use the same mechanism to configure IoC within your application 
making it agnostic to the IoC container too.

Note that if you are using a package that has dependencies and you are providing a concrete implementation of
that depencency in your application, then your application must contain a `Package` file defining these implementations
so that `Ioc.Modules` knows that the dependencies have been satisfied.
