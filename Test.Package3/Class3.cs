using System;
using Test.Interfaces;

namespace Test.Package3
{
    public class Class3: Interface3
    {
        /// <summary>
        /// Dependencies injected into constructor by IoC
        /// </summary>
        public Class3(
            Interface1 interface1,
            Interface2 interface2)
        {

        }

        public void Method3()
        {
        }
    }
}
