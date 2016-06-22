using Test.Interfaces;

namespace Test.Package2
{
    public class Class2: Interface2
    {
        private readonly Interface1 _interface1;

        /// <summary>
        /// Dependency on Interface1 injected into constructor by IoC
        /// </summary>
        public Class2(Interface1 interface1)
        {
            _interface1 = interface1;
        }

        public void Method2()
        {
            _interface1.Method1();
        }
    }
}
