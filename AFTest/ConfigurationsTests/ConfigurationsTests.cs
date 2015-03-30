using NUnit.Framework;
using System;
using AFLib;



namespace AFTest
{
    [TestFixture ()]
    /*
    * <summary>Tests simple case of Configurations(Key Store) </summary>
    */
    public class ConfigurationsTests
    {
        Guid appID = new Guid (Configurations.appID);
        Guid secretKey = new Guid (Configurations.secretKey);
        Guid vehicleID = new Guid (Configurations.vehicleID);
         /*
         * Creates the objects that are used to test on, one for existing timeEnd, one for null timeEnd
         */ 
        [SetUp]
        public void ConfigurationsTestSetup(){

        }

        [Test,Ignore]
        public void NegativeTest ()
        {
            if (true)
                Assert.Fail ("This is a standard-procudure deliberate fail!");
        }

        //Test that unique appID is correct
        [Test ()]
        public void TestappID ()
        {
            Assert.AreEqual ("0cf32aca-3792-4929-a322-9797d9d5048f", appID);

        }

        //Test that unique secretKey is correct
        [Test ()]
        public void TestsecretKey ()
        {
            Assert.AreEqual ("05589578-54f4-4a7b-af6d-4e096292228f", secretKey);
        }

        //Test that unique vehicleID is correct
        [Test ()]
        public void TestvehicleID ()
        {
            Assert.AreEqual ("03626728-8ed2-4e1a-89f0-dae48016fa96", vehicleID);
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

