using NUnit.Framework;
using System;
using AFLib;



namespace AFTest
{
    [TestFixture ()]
    /*
    * <summary>Tests simple case of global instance passing </summary>
    */
    public class GlobalsTests
    {
        Globals instance;

        [SetUp]
        public void GlobalsTestSetup(){

        }

        //Test that instance is null
        [Test ()]
        public void TestInstanceNull ()
        {
            Assert.IsNotNull(instance); 
        }
            
        [Test,Ignore]
        public void NegativeTest ()
        {
            if (true)
                Assert.Fail ("This is a standard-procudure deliberate fail!");
        }

        //Standard NotSupportedException Throw
        [Test, ExpectedException(typeof(NotSupportedException))]
        public void ExpectedExceptionTest()
        {
            throw new NotSupportedException ();
        }

        [Test, Ignore]
        public void NotImplementedException()
        {
            throw new NotImplementedException ();
        }

    }
}

