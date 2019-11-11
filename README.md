# Ioc.Modules

This project is aimed at providing a mechanism for NuGet packages to 
provide information about their IoC needs to applications. This
will allow the application developer to integrate third party libraries
seamlessly without having to read documentation about the types in the
library that need to be registered with IoC. It also means that library
developers can add new interfaces and the classes that implement them
without breaking applications that use these libraries.

## Breaking changes

### Version 1.3.0

In this version the signature for the function that creates instances was changed
to support classes that have custom initialization needs but also have dependencies
that we want to resolve via IoC.

Instead of writing 

```
    new IocRegistration().Init<ICustomInterface>(() => new CustomClass().Init())
```

You must now write

```
    new IocRegistration().Init<ICustomInterface>(container => new CustomClass().Init())
```

The advantage of this change is that you can now write

```
    new IocRegistration().Init<ICustomInterface>(container => container.Resolve<CustomClass>().Init())
```

Which will use the IoC container to construct `CustomClass` passing any dependencies to
its constructor before calling the custom initialization function.

## Package developers

If you are developing a shared library and want to use IoC within it, then you need 
to add a class to your library that defines the IoC needs of your library. I recommend
that you call your class `Package` and place it in the root folder of your project. 
You must also decorate the class with the `[Package]` attribute so that the package
locator can find it, and to implement the `IPackage` interface so that your IoC needs
can be determined.

Here is a simple example of a `package.cs` file:
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
                    new IocRegistration().Init<IExceptionReporter>(IocLifetime.SingleInstance),
                    new IocRegistration().Init<IApplication>(container => new Application("My App"))
                };
            }
        }
    }
```
In this example the package is declaring that 
* This package needs the application to register the interface `ILog` so that IoC will resolve it to 
the concrete type `TraceLog` (the application can also choose to register `ILog` to any other class that 
implements `ILog`).
* This package depends on the `IExceptionReporter` interface but does not contain an implementation of it.
In this case the application must provide a concrete implementation. This can be provided by installing 
another package that registers a concrete implemntation, or the application developer can write one.
* This package has a concrete implementation of `IApplication` but it can not be constructed by IoC
because it has special initialization needs. In this case a Lambda expression is provided that will
construct the `Application` class on first usage.

Note that adding this `Package` class to your package does not force applications to use it. The
application developer can choose to use `Ioc.Modules` to configure their IoC container or they can 
configure IoC in some other way, or choose not to use IoC at all. By adding this class to your package 
you are providing a very simple and convenient way for the application developer to configure IoC only 
if they choose to use it.

Note that adding `Ioc.Modules` to your package does not drag in dependencies on anything else. It is
very small, lightweight and fully self-contained.

## Application developers

When you use a package in your application that needs IoC setup and it has a dependency on `Ioc.Modules` 
then all you need to do to get everything set up is configure one of the packages that registers
IoC with a specific container. For example if you want to use `Ninject` as your IoC container, then add 
the `Ioc.Modules.Ninject` NuGet package to your application and initialize it as described in its readme file.

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
    var autofacBuilder = new Ioc.Modules.Autofac.Builder();
    var container = autofacBuilder.Build(packages, builder);
```
You can also add support for other IoC containers very easily. Take a look at the source code for one of
the IoC container integrations and you will find it's less than 30 lines long.

This `Ioc.Modules` package is very convenient for package authors because they can configure 
IoC for thier package without knowing anything about the IoC container you are using in your 
application. You can also use the same mechanism to configure IoC within your application 
making it agnostic to the IoC container too.

Note that if you are using a package that has dependencies and you are providing a concrete implementation of
those depencencies in your application, then your application must contain a `Package` file defining these implementations
so that `Ioc.Modules` knows that the dependencies have been satisfied.

# Advanced configuration options

## One class implements multiple interfaces and is a singleton

If you have

```
public interface IInterface1{}
public interface IInterface2{}

public class MyClass: IInterface1, IInterface2 {}
```

And you want only a single instance of `MyClass` to be resolved for both `IInterface1` and `IInterface2`
then you should configure Ioc Modules like this:

```
    new IocRegistration().Init<IInterface1, MyClass>(),
    new IocRegistration().Init<IInterface2>(container => (IInterface2)container.Resolve<IInterface1>()),
```

In this case when `IInterface1` is resolved it will resolve to a singleton instance of `MyClass`. When
`IInterface2` is resolved the lambda function will resolve `IInterface1` then cast the instance of `MyClass`
that is returned to `IInterface2` returning the same instance.

## Passing a property bag to package constructors

This technique allows you to vary the way that packages register IoC needs based
on a set of properties configured by the application.

Below is an example of a package that looks for a property called `environment` and
configures a different implementation of the `ILog` interface when the environment
property is set to `"development"`.

```
    [Package]
    public class Package: IPackage
    {
      public string Name { get { return "My next great thing"; } }

      private readonly IList<IocRegistration> _iocRegistrations;
      public IList<IocRegistration> IocRegistrations { get { return _iocRegistrations; } }

      public Package(IPropertyBag properties)
      {
        _iocRegistrations = new List<IocRegistration>
        {
          new IocRegistration().Init<IExceptionReporter>(IocLifetime.SingleInstance),
          new IocRegistration().Init<IApplication>(() => new Application(Name))
        };

        if (string.Equals(properties["environment"], "development", StringComparison.OrdinalIgnoreCase))
          _iocRegistrations.Add(new IocRegistration().Init<ILog, TraceLog>(IocLifetime.SingleInstance))
        else
          _iocRegistrations.Add(new IocRegistration().Init<ILog, FileLog>(IocLifetime.SingleInstance))
      }
    }
```

For this example to work, the application must provide property values when loading packages. Below is
is the Ninject example from above modified to set the environment property.

```
    var properties = new PropertyBag();
	properties["environment"] = "development";

    var packageLocator = new PackageLocator().ProbeAllLoadedAssemblies(properties);
    var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));
```

If the application does not provide properties, then the package constructor will be passed a
property bag instance that has no properties set.
