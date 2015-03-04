using NUnit.Framework;
using System;
using AFLib;



namespace AFTest
{
    [TestFixture ()]
    /**
    * <summary>Tests simple case of TripData </summary>
    */ 
    public class TripDataTest
    {
        //simple case
        TripData tripData;
        //null case
        TripData nullTripData;

        /*
         * Creates the objects that are used to test on, one for existing timeEnd, one for null timeEnd
         */ 
        [SetUp]
        public void TripDataTestSetup(){
            var timeStart = new DateTime (2009, 01, 01, 4, 20, 00);
            var timeEnd = new DateTime (2009, 01, 01, 6, 27, 1);
            string lng = "50";
            string lat = "50";
            string maxSpeed = "80";
            tripData = new TripData (timeStart, timeEnd, maxSpeed, lat, lng);
            nullTripData = new TripData (timeStart, null, maxSpeed, lat, lng);
        }

        //Test that longitude is set correctly
        [Test ()]
        public void TestLng ()
        {
            Assert.AreEqual ("50", tripData.lng);
        }

        //test that latitude is set correctly
        [Test ()]
        public void TestLat (){
            Assert.AreEqual ("50", tripData.lat);
        }

        //test that maxSpeed is set correctly
        [Test ()]
        public void TestMaxSpeed (){
            Assert.AreEqual ("80km/h", tripData.maxSpeed);
        }

        //test that the start time is set correctly
        [Test ()]
        public void TestStartTime (){
            Assert.AreEqual ("4:20am", tripData.startTime);
        }

        //test that the trip start date is set correctly
        [Test ()]
        public void TestStartripDate (){
            Assert.AreEqual("Jan 1", tripData.startDate);
        }

        //test that the trip length is calculated correctly
        [Test ()]
        public void TestTripLength (){
            Assert.AreEqual("2 hrs, 7 min", tripData.tripLength);
        }

        //test that the end date time is formed correctly [non null case]
        [Test ()]
        public void TestTripEndDateTime(){
            Assert.AreEqual ("Jan 01 @ 06:27 AM", tripData.endDateTime);
        }

        //test that a null endtime trip is listed as "in progress" for tripLength
        [Test ()]
        public void TestNullTripLength (){
            Assert.AreEqual("In Progress", nullTripData.tripLength);
        }

        //test that a null endtime trip is listed as "in progress" for endDateTime
        [Test ()]
        public void TestNullTripEndDateTime (){
            Assert.AreEqual("In Progress", nullTripData.endDateTime);
        }
    }
}

