using System;
using System.Threading.Tasks;
using Android.Content;
using Mojio;
using Mojio.Client;

namespace AFLib
{
    public class MojioConnectionHelper
    {
        ///<summary>
        /// Attempts to setup the Mojio Client with the AppID and SecretID and attempt a login if credentials are existing. 
        /// </summary>
        ///<returns><c>true</c>, if the client has been setup and is logged in. <c>false</c> in any other case.</returns>
        /// <param name="prefs">SharedPreferences</param> 
        public async static Task<bool> setupMojioConnection (ISharedPreferences prefs)
        {
            Guid appID = new Guid (Configurations.appID);
            Guid secretKey = new Guid (Configurations.secretKey);

            try {
                await Globals.client.BeginAsync (appID, secretKey);
            } catch (UnauthorizedAccessException uae) {
                return false;
            }

            if ((prefs.GetString ("email", null)) != null && (prefs.GetString ("password", null)) != null) { //Check if credentials have already been entered. If so, log in.
                try {
                    await Globals.client.SetUserAsync (prefs.GetString ("email", null), prefs.GetString ("password", null)); // Logs the user in.
                } catch (Exception exception) {
                    return false;
                }
            }

            return Globals.client.IsLoggedIn ();
        }

        /// <summary>
        /// Setups the mojio connection first time. If login is successful, the credentials are saved.
        /// </summary>
        /// <returns><c>true</c>, if the client has been setup and is logged in. <c>false</c> in any other case.</returns>
        /// <param name="email">Email.</param>
        /// <param name="password">Password.</param>
        public async static Task<bool> setupMojioConnectionFirstTime (String email, String password, ISharedPreferencesEditor prefsEditor)
        {
            Guid appID = new Guid (Configurations.appID);
            Guid secretKey = new Guid (Configurations.secretKey);

            try {
                await Globals.client.BeginAsync (appID, secretKey);
            } catch (UnauthorizedAccessException uae) {
                return false;
            }

            try {
                await Globals.client.SetUserAsync (email, password); // Logs the user in.
            } catch (Exception exception) {
                return false;
            }
            if (Globals.client.IsLoggedIn ()) {
                setSharedPreferenceCredentials (email, password, prefsEditor);
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks whether the client is logged in.
        /// </summary>
        /// <returns><c>true</c>, if client is logged in, <c>false</c> otherwise.</returns>
        public static Boolean isClientLoggedIn ()
        {
            return Globals.client.IsLoggedIn ();
        }

        /// <summary>
        /// Sets the shared preference credentials.
        /// </summary>
        /// <param name="email">Email.</param>
        /// <param name="password">Password.</param>
        public static void setSharedPreferenceCredentials (String email, String password, ISharedPreferencesEditor prefEditor)
        {
            prefEditor.PutString ("email", email);
            prefEditor.PutString ("password", password);
            prefEditor.Apply ();
        }
    }
}

