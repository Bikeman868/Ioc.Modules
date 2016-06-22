﻿using System;
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
        private SortedList<string, Assembly> _probedAssemblies;
        private SortedList<string, Type> _addedPackages;

        public PackageLocator()
        {
            Clear();
        }

        /// <summary>
        /// Removes all the registered packages and starts over
        /// </summary>
        public void Clear()
        {
            _packages = new List<IPackage>();
            _probedAssemblies = new SortedList<string, Assembly>();
            _addedPackages = new SortedList<string, Type>();
        }

        /// <summary>
        /// Adds a package to the list of packages to register with IoC
        /// If the package was already added then it will be moved to the end of the list
        /// </summary>
        public PackageLocator Add(IPackage package)
        {
            if (_packages.Contains(package))
                _packages.Remove(package);
             _packages.Add(package);
            return this;
        }

        /// <summary>
        /// Adds all packages from an assembly. If the assembly has been added
        /// before then its packages are moved to the end of the list so that
        /// their IoC registrations will take priority.
        /// </summary>
        public PackageLocator Add(Assembly assembly)
        {
            if (!_probedAssemblies.ContainsKey(assembly.FullName))
                _probedAssemblies.Add(assembly.FullName, assembly);

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
                            if (!_addedPackages.ContainsKey(type.FullName))
                            {
                                _addedPackages.Add(type.FullName, type);
                                var defaultPublicConstructor = type.GetConstructor(Type.EmptyTypes);
                                if (defaultPublicConstructor != null)
                                {
                                    var package = (IPackage)defaultPublicConstructor.Invoke(null);
                                    Add(package);
                                }
                                else
                                {
                                    Trace.WriteLine(
                                        "Type " + type.FullName +
                                        " in assembly " + assembly.FullName +
                                        " implements IPackage but does not have a default public constructor.");
                                }
                            }
                        }
                        else
                        {
                            Trace.WriteLine(
                                "Type " + type.FullName + 
                                " in assembly " + assembly.FullName + 
                                " has the [Package] attribute but does not implement the IPackage interface.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(
                        "Exception whilst examining type " + type.FullName 
                        + " in assembly " + assembly.FullName 
                        + ". " + ex.Message);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds all packages from an assembly contained in the specified file. 
        /// If the assembly has been added before then its packages are moved 
        /// to the end of the list so that their IoC registrations will take 
        /// priority.
        /// </summary>
        public PackageLocator Add(FileInfo assemblyFile)
        {
            var assembly = Assembly.LoadFrom(assemblyFile.FullName);
            return Add(assembly);
        }

        /// <summary>
        /// Adds all packages from an assembly with the specified name. 
        /// If the assembly has been added before then its packages are moved 
        /// to the end of the list so that their IoC registrations will take 
        /// priority.
        /// </summary>
        public PackageLocator Add(AssemblyName assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly);
        }

        /// <summary>
        /// Adds all packages from an assembly with the specified name. 
        /// If the assembly has been added before then its packages are moved 
        /// to the end of the list so that their IoC registrations will take 
        /// priority.
        /// </summary>
        public PackageLocator Add(string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly);
        }

        /// <summary>
        /// Adds only assemblies that have not been added already.
        /// </summary>
        public PackageLocator Add(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (!_probedAssemblies.ContainsKey(assembly.FullName))
                    Add(assembly);
            }
            return this;
        }

        /// <summary>
        /// Adds only assemblies loaded into the app domain that have not been added already.
        /// </summary>
        public PackageLocator ProbeAllLoadedAssemblies()
        {
            return Add(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Loads all assemblies in the application's bin folder and adds packages from
        /// those assemblies if they have not been added already
        /// </summary>
        public PackageLocator ProbeBinFolderAssemblies()
        {
            // Getting the location of the bin folder for all application types
            // is rediculously hard in .Net.
            // http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var assemblyUri = new UriBuilder(codeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var binFolderPath = Path.GetDirectoryName(assemblyPath);

            var assemblyFileNames = Directory.GetFiles(binFolderPath, "*.dll");

            var assemblies = assemblyFileNames
                .Select(fileName => new AssemblyName(Path.GetFileNameWithoutExtension(fileName)))
                .Select(name => AppDomain.CurrentDomain.Load(name));

            return Add(assemblies);
        }

        /// <summary>
        /// Gets a list of all IoC registrations that need to be made for all packages
        /// to have their dependencies satisfied. An exception will be thrown if any
        /// interfaces do not have concrete implementations. A list of these issues
        /// will also be output to System.Diagnostics.Trace.
        /// </summary>
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

            var issues = new List<string>();

            foreach (var registration in registrations.Values)
            {
                if (registration.ConcreteType == null)
                {
                    issues.Add("There is no concrete implementation of \"" + registration.Interfacetype + "\".");
                    foreach (var package in _packages)
                    {
                        foreach (var packageRegistration in package.IocRegistrations)
                        {
                            if (packageRegistration.Interfacetype == registration.Interfacetype)
                            {
                                issues.Add("Package " + package.Name + " depends on " + registration.Interfacetype.Name + " with " + registration.Lifetime + " lifetime.");
                            }
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

            return registrations.Values.ToList();
        }
    }
}
