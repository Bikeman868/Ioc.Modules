using System.Diagnostics;
using Test.Interfaces;

namespace Test.App3
{
    public class Class3: Interface3
    {
        private readonly bool _isAmazonWebServices;

        public Class3(bool isAmazonWebServices)
        {
            _isAmazonWebServices = isAmazonWebServices;
        }

        public void Method3()
        {
            Trace.WriteLine("AWS: " + _isAmazonWebServices);
        }
    }
}