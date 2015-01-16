using System;
using Mojio;
using Mojio.Client;

namespace AutoFences
{
    public class Globals
    {
        private static Globals instance;

        private Globals ()
        {
        }

        public static Globals Instance {
            get {
                if (instance == null) {
                    instance = new Globals ();
                }
                return instance;
            }
        }

        public static MojioClient client = new MojioClient (MojioClient.Live);
    }
}

