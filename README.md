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
of your package. Here is a simple example:
```
    [Package]
    public class Package: IPackage
    {
        public string Name {get { return "Test Package"; }}

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
In this example the package is declaring that it needs the application to register
the interface `ILog` to resolve to the concrete type `TraceLog` (or any other class
that implements ILog) and that the package depends on the `IExceptionReporter` interface
but does not contain an implementation of it and therefore the application must provide one.

## Application developers

When you use a package in your application that needs IoC setup and it
has a dependency on this package, then all you need to do is initialize 
one of the IoC container adapters and you're done.

It might also make sense for your application to define its own Ioc needs by
adding package classes to your application. If you do this then you can swap IoC
containers with no impact on your code.
