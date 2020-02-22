using System;
using System.Web;
using Ioc.Modules;
using Unity;

namespace Test.App3
{
    public class Global : HttpApplication
    {
        public static UnityContainer IocContainer;

        void Application_Start(object sender, EventArgs e)
        {
            var properties = new PropertyBag();

            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    var ec2Identity = webClient.DownloadString("http://169.254.169.254/latest/dynamic/instance-identity/document");
                    properties.Set(!string.IsNullOrEmpty(ec2Identity), "IsAmazonWebServices");
                }
                catch
                {
                    properties.Set(false, "IsAmazonWebServices");
                }
            }

            IocContainer = new UnityContainer();
            var packageLocator = new PackageLocator().ProbeBinFolderAssemblies(properties);
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
