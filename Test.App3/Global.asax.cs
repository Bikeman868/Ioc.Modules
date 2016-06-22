using System;
using System.Web;
using Ioc.Modules;
using Microsoft.Practices.Unity;

namespace Test.App3
{
    public class Global : HttpApplication
    {
        public static UnityContainer IocContainer;

        void Application_Start(object sender, EventArgs e)
        {
            IocContainer = new UnityContainer();
            var packageLocator = new PackageLocator().ProbeBinFolderAssemblies();
            Ioc.Modules.Unity.Registrar.Register(packageLocator, IocContainer);
        }

        void Application_End(object sender, EventArgs e)
        {
        }

        void Application_Error(object sender, EventArgs e)
        {
        }
    }
}
