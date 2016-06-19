using System.Collections.Generic;

namespace Ioc.Modules
{
    public interface IPackage
    {
        string Name { get; }
        IList<IocRegistration> IocRegistrations { get; }
    }
}
