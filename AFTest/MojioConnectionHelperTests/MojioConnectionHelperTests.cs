using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Android.Content;
using Mojio;
using Mojio.Client;
using AFLib;



namespace AFTest
{
    [TestFixture ()]
    /*
    * <summary>Tests simple cases of Mojio Connection </summary>
    */
    public class MojioConnectionHelperTests
    {
        Guid appID = new Guid (Configurations.appID);
        Guid secretKey = new Guid (Configurations.secretKey);


        [SetUp]
        public void MojioConnectionHelperTestsSetup(ISharedPreferences prefs){
            Globals.client.BeginAsync (appID, secretKey);
        }

        //Check for UnauthorizedAccessException 
        [Test, ExpectedException(typeof(UnauthorizedAccessException))]
        public void UAETest(ISharedPreferences prefs)
        {
            throw new UnauthorizedAccessException ();
        }

        //Check if email is null
        [Test ()]
        public void TestEmailNull (ISharedPreferences prefs)
        {
            Assert.IsNotNull (prefs.GetString ("email", null));
        }

        //Check if password is null
        [Test ()]
        public void TestPasswordNull (ISharedPreferences prefs)
        {
            Assert.IsNotNull (prefs.GetString ("password", null));
        }

        //Check if it is logged in
        [Test ()]
        public void TestLoggedIn (ISharedPreferences prefs)
        {
            Assert.IsTrue(Globals.client.IsLoggedIn ());
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

