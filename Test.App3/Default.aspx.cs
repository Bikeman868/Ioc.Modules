using System;
using System.Web.UI;
using Microsoft.Practices.Unity;
using Test.Interfaces;

namespace Test.App3
{
    public partial class _Default : Page
    {
        [Dependency]
        public Interface2 Interface2 { get; set; }

        [Dependency]
        public Interface3 Interface3 { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Global.IocContainer.BuildUp(GetType(), this, null);

            Interface2.Method2();
            Interface3.Method3();
        }
    }
}