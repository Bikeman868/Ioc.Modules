using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ioc.Modules
{
    /// <summary>
    /// This class examines assemblies and builds a complete list of all IoC
    /// configuration requirements. This can be used by classes that configure
    /// the IoC container.
    /// </summary>
    public class PackageLocator
    {
        private List<IPackage> _packages;

        public PackageLocator()
        {
            Clear();
        }

        public void Clear()
        {
            _packages = new List<IPackage>();
        }

        public PackageLocator Add(Assembly assembly)
        {
            try
            {
                var packageInterface = typeof (IPackage);
                foreach (var type in assembly.GetTypes())
                {
                    try
                    {
                        var packageAttribute = type
                            .GetCustomAttributes(typeof (PackageAttribute), true)
                            .FirstOrDefault();
                        if (packageAttribute != null)
                        {
                            if (packageInterface.IsAssignableFrom(type))
                            {
                                var defaultPublicConstructor = type.GetConstructor(Type.EmptyTypes);
                                if (defaultPublicConstructor != null)
                                {
                                    var package = defaultPublicConstructor.Invoke(null);
                                    _packages.Add((IPackage) package);
                                }
                                else
                                {
                                    Trace.WriteLine("Type " + type.FullName + " in assembly " + assembly.FullName + " implements IPackage but does not have a default public constructor.");
                                }
                            }
                            else
                            {
                                Trace.WriteLine("Type " + type.FullName + " in assembly " + assembly.FullName + " has the [Package] attribute but does not implement the IPackage interface.");
                            }
                        }
                    }
                    catch{}
                }
            }
            catch{}
            return this;
        }

        public PackageLocator Add(FileInfo assemblyFile)
        {
            var assembly = Assembly.LoadFrom(assemblyFile.FullName);
            return Add(assembly);
        }

        public PackageLocator Add(AssemblyName assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly);
        }

        public PackageLocator Add(string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly);
        }

        public PackageLocator ProbeAllLoadedAssemblies()
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                Add(assembly);
            return this;
        }

        public IList<IocRegistration> GetAllRegistrations()
        {
            var registrations = new Dictionary<Type, IocRegistration>();
            foreach (var package in _packages)
            {
                foreach (var registration in package.IocRegistrations)
                {
                    if (registration.ConcreteType == null)
                    {
                        if (!registrations.ContainsKey(registration.Interfacetype))
                            registrations.Add(registration.Interfacetype, registration);
                    }
                    else
                    {
                        registrations[registration.Interfacetype] = registration;
                    }
                }
            }

            foreach (var registration in registrations.Values)
            {
                var issues = new List<string>();
                if (registration.ConcreteType == null)
                {
                    issues.Add("There is no concrete implementation of \"" + registration.Interfacetype + "\".");
                    foreach (var package in _packages)
                    {
                        foreach (var packageRegistration in package.IocRegistrations)
                        {
                            if (packageRegistration.Interfacetype == registration.Interfacetype)
                            {
                                issues.Add("Package " + package.Name + " depends on " + registration.Interfacetype + " with " + registration.Lifetime + " lifetime.");
                            }
                        }
                    }
                }
                if (issues.Count > 0)
                {
                    var exceptionMessage = "Some IoC dependencies have not been met.";
                    foreach (var issue in issues)
                    {
                        Trace.WriteLine(issue);
                        exceptionMessage += "\n" + issue;
                    }
                    throw new Exception(exceptionMessage);
                }
            }

            return registrations.Values.ToList();
        }
    }
}
