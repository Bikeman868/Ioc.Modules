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

        public List<string> ProbingErrors { get; private set; }

        private const string _tracePrefix = "Package locator: ";

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
            ProbingErrors = new List<string>();
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
        public PackageLocator Add(Assembly assembly, IPropertyBag properties = null)
        {
            if (properties == null)
                properties = new PropertyBag();

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
                        Trace.WriteLine("  Found package " + type.FullName + " in assembly " +  assembly.GetName().Name);
                        if (packageInterface.IsAssignableFrom(type))
                        {
                            if (!_addedPackages.ContainsKey(type.FullName))
                            {
                                _addedPackages.Add(type.FullName, type);

                                var propertyBagConstructor = type.GetConstructor(new[] { typeof(IPropertyBag) });
                                if (propertyBagConstructor != null)
                                {
                                    var package = (IPackage)propertyBagConstructor.Invoke(new[] { properties });
                                    Add(package);
                                }
                                else
                                {
                                    var defaultPublicConstructor = type.GetConstructor(Type.EmptyTypes);
                                    if (defaultPublicConstructor != null)
                                    {
                                        var package = (IPackage)defaultPublicConstructor.Invoke(null);
                                        Add(package);
                                    }
                                    else
                                    {
                                        var msg = "Type " + type.FullName +
                                                  " in assembly " + assembly.FullName +
                                                  " implements IPackage but does not have a usable constructor." +
                                                  " The constructor must be public, and it should either take no" +
                                                  " parameters, or take one parameter of type IPropertyBag";
                                        Trace.WriteLine(_tracePrefix + msg);
                                        ProbingErrors.Add(msg);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var msg = "Type " + type.FullName +
                                      " in assembly " + assembly.FullName +
                                      " has the [Package] attribute but does not implement the IPackage interface.";
                            Trace.WriteLine(_tracePrefix + msg);
                            ProbingErrors.Add(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Exception whilst examining type " + type.FullName
                              + " in assembly " + assembly.FullName
                              + ". " + ex.Message;
                    Trace.WriteLine(_tracePrefix + msg);
                    ProbingErrors.Add(msg);
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
        public PackageLocator Add(FileInfo assemblyFile, IPropertyBag properties = null)
        {
            var assembly = Assembly.LoadFrom(assemblyFile.FullName);
            return Add(assembly, properties);
        }

        /// <summary>
        /// Adds all packages from an assembly with the specified name. 
        /// If the assembly has been added before then its packages are moved 
        /// to the end of the list so that their IoC registrations will take 
        /// priority.
        /// </summary>
        public PackageLocator Add(AssemblyName assemblyName, IPropertyBag properties = null)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly, properties);
        }

        /// <summary>
        /// Adds all packages from an assembly with the specified name. 
        /// If the assembly has been added before then its packages are moved 
        /// to the end of the list so that their IoC registrations will take 
        /// priority.
        /// </summary>
        public PackageLocator Add(string assemblyName, IPropertyBag properties = null)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            return Add(assembly, properties);
        }

        /// <summary>
        /// Adds only assemblies that have not been added already.
        /// </summary>
        public PackageLocator Add(IEnumerable<Assembly> assemblies, IPropertyBag properties = null)
        {
            if (properties == null)
                properties = new PropertyBag();

            foreach (var assembly in assemblies)
            {
                if (!_probedAssemblies.ContainsKey(assembly.FullName))
                {
                    try
                    {
                        Add(assembly, properties);
                    }
                    catch(ReflectionTypeLoadException ex)
                    {
                        var msg = "Exception probiing assembly " + assembly.FullName + ". " + ex.Message;
                        Trace.WriteLine(_tracePrefix + msg);
                        ProbingErrors.Add(msg);
                        if (ex.LoaderExceptions != null)
                        {
                            foreach (var loaderException in ex.LoaderExceptions)
                            {
                                var loaderMsg = "  Loader exception " + loaderException.Message;
                                Trace.WriteLine(loaderMsg);
                                ProbingErrors.Add(loaderMsg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Exception probiing assembly " + assembly.FullName + ". " + ex.Message;
                        Trace.WriteLine(_tracePrefix + msg);
                        ProbingErrors.Add(msg);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds only assemblies loaded into the app domain that have not been added already.
        /// </summary>
        public PackageLocator ProbeAllLoadedAssemblies(IPropertyBag properties = null)
        {
            return Add(AppDomain.CurrentDomain.GetAssemblies(), properties);
        }

        /// <summary>
        /// Loads all assemblies in the application's bin folder and adds packages from
        /// those assemblies if they have not been added already
        /// </summary>
        public PackageLocator ProbeBinFolderAssemblies(IPropertyBag properties = null)
        {
            if (properties == null)
                properties = new PropertyBag();

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
                .Select(name => 
                    {
                        try
                        {
                            Trace.WriteLine(_tracePrefix + "Probing bin folder assembly " + name);
                            return AppDomain.CurrentDomain.Load(name);
                        }
                        catch (FileNotFoundException ex)
                        {
                            var msg = "File not found exception loading " + name + ". " + ex.FileName;
                            Trace.WriteLine(_tracePrefix + msg);
                            ProbingErrors.Add(msg);
                            return null;
                        }
                        catch (BadImageFormatException ex)
                        {
                            var msg = "Bad image format exception loading " + name + ". The DLL is probably not a .Net assembly. " + ex.FusionLog;
                            Trace.WriteLine(_tracePrefix + msg);
                            ProbingErrors.Add(msg);
                            return null;
                        }
                        catch (FileLoadException ex)
                        {
                            var msg = "File load exception loading " + name + ". " + ex.FusionLog;
                            Trace.WriteLine(_tracePrefix + msg);
                            ProbingErrors.Add(msg);
                            return null;
                        }
                        catch (Exception ex)
                        {
                            var msg = "Failed to load assembly " + name + ". " + ex.Message;
                            Trace.WriteLine(_tracePrefix + msg);
                            ProbingErrors.Add(msg);
                            return null;
                        }
                    })
                .Where(a => a != null);

            Add(assemblies, properties);

            // The application entry point assembly should take priority over any
            // libraries referenced by the application.

            if (AppDomain.CurrentDomain.DomainManager != null &&
                AppDomain.CurrentDomain.DomainManager.EntryAssembly != null)
                Add(AppDomain.CurrentDomain.DomainManager.EntryAssembly, properties);

            return this;
        }

        public interface IErrorReporter
        {
            void ReportIoCConfigurationError(string message);
        }

        public interface ILogger
        {
            void WriteLine(string message);
        }

        /// <summary>
        /// Gets a list of all IoC registrations that need to be made for all packages
        /// to have their dependencies satisfied. An exception will be thrown if any
        /// interfaces do not have concrete implementations. A list of these issues
        /// will also be output to System.Diagnostics.Trace.
        /// </summary>
        public IList<IocRegistration> GetAllRegistrations()
        {
            return GetAllRegistrations(null);
        }

        public IList<IocRegistration> GetAllRegistrations(IErrorReporter errorReporter)
        {
            return GetAllRegistrations(packages => packages, null, errorReporter);
        }


        /// <summary>
        /// Gets a list of all IoC registrations that need to be made for all packages
        /// to have their dependencies satisfied. An exception will be thrown if any
        /// interfaces do not have concrete implementations. A list of these issues
        /// will also be output to System.Diagnostics.Trace.
        /// </summary>
        public IList<IocRegistration> GetAllRegistrations(
            Func<List<IPackage>, IEnumerable<IPackage>> sort, 
            ILogger logger, 
            IErrorReporter errorReporter)
        {
            Action<string> log;
            if (logger == null) log = msg => { }; 
            else log = logger.WriteLine;

            var registrations = new Dictionary<Type, IocRegistration>();
            foreach (var package in sort(_packages))
            {
                log("================================================================================");
                log("Adding IoC registrations for package " + package.Name);
                foreach (var registration in package.IocRegistrations)
                {
                    if (registration.ConcreteType == null && registration.InstanceFunction == null)
                    {
                        if (registrations.ContainsKey(registration.InterfaceType))
                        {
                            log("Skipping IoC dependency on " + registration.InterfaceType.FullName + " because its already in the list");
                        }
                        else
                        {
                            log("Adding IoC dependency on " + registration.InterfaceType.FullName + " as " + registration.Lifetime);
                            registrations.Add(registration.InterfaceType, registration);
                        }
                    }
                    else
                    {
                        if (registration.ConcreteType == null)
                            log("Registering singleton instance of " + registration.InterfaceType.FullName);
                        else
                            log("Registering concrete implementation of " + registration.InterfaceType.FullName + " with " + registration.Lifetime + " lifetime as " + registration.ConcreteType.FullName);
                        registrations[registration.InterfaceType] = registration;
                    }
                }
            }

            var issues = new List<string>();
            Action<string> reportError;
            if (errorReporter == null) reportError = issues.Add;
            else reportError = errorReporter.ReportIoCConfigurationError;

            log("================================================================================");
            log("Resolving IoC dependencies");

            foreach (var registration in registrations.Values)
            {
                if (registration.ConcreteType == null)
                {
                    string message;
                    if (registration.InstanceFunction == null)
                    {
                        message = "There is no concrete implementation of \"" + registration.InterfaceType + "\" and no registered function to construct an instance.";
                        log(message);
                        reportError(message);

                        foreach (var package in _packages)
                        {
                            foreach (var packageRegistration in package.IocRegistrations)
                            {
                                if (packageRegistration.InterfaceType == registration.InterfaceType)
                                {
                                    reportError("Package " + package.Name + " depends on " + registration.InterfaceType.Name + " with " + registration.Lifetime + " lifetime.");
                                }
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
                    Trace.WriteLine(_tracePrefix + issue);
                    exceptionMessage += "\n" + issue;
                }
                throw new Exception(exceptionMessage);
            }

            return registrations.Values.ToList();
        }
    }
}
