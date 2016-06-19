using System;

namespace Ioc.Modules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PackageAttribute: Attribute
    {
    }
}
