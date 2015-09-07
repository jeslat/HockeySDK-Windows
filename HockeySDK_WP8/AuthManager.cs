using HockeyApp.Model;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using HockeyApp.Tools;

namespace HockeyApp
{
    /// <summary>
    /// Policy if an exisitng locally saved token should be revalidated
    /// </summary>
    public enum TokenValidationPolicy
    {
        /// <summary>
        /// revalidate never
        /// </summary>
        Never,
        /// <summary>
        /// revalidate on every login
        /// </summary>
        EveryLogin,
        /// <summary>
        /// revalidate on new version
        /// </summary>
        OnNewVersion
    }

    /// <summary>
    /// Authorize means with full credentials, identify only validates the email address
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// authorize
        /// </summary>
        Authorize,
        /// <summary>
        /// identify
        /// </summary>
        Identify
    }

    /// <summary>
    /// If an existing token is availabe and validation is not possible (no network) deny (strict) or allow (graceful) access
    /// </summary>
    public enum AuthValidationMode
    {
        /// <summary>
        /// strict
        /// </summary>
        Strict,
        /// <summary>
        /// graceful
        /// </summary>
        Graceful
    }

    /// <summary>
    /// Class for authentication of users with hockeyapp credentials
    /// </summary>
    public class AuthManager
    {
        private static readonly AuthManager instance = new AuthManager();

        static AuthManager() { }
        private AuthManager() { }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static AuthManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Store data encrypted in the isolatedStorage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to write</param>
        /// <param name="data">the data to store</param>
        public void StoreStringProtected(string dataIdentifier, string data)
        {
            byte[] protectedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes("hockeyAppIsCool"));

            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            using (IsolatedStorageFileStream writestream = new IsolatedStorageFileStream(dataIdentifier + ".crypt", FileMode.Create, FileAccess.Write, file))
            {
                using (Stream writer = new StreamWriter(writestream).BaseStream)
                {
                    writer.Write(protectedData, 0, protectedData.Length);
                }
            }
        }

        /// <summary>
        /// Retreive encrypted Data from the isolated storage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to read</param>
        /// <returns>Decrypted string or null if no value has been stored for the given dataIdentifier</returns>
        public string RetrieveProtectedString(string dataIdentifier)
        {
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            byte[] protectedData;
            try
            {
                using (IsolatedStorageFileStream readstream = new IsolatedStorageFileStream(dataIdentifier + ".crypt", FileMode.Open, FileAccess.Read, file))
                {
                    using (Stream reader = new StreamReader(readstream).BaseStream)
                    {
                        protectedData = new byte[reader.Length];
                        reader.Read(protectedData, 0, protectedData.Length);
                    }
                }

            }
            catch (IsolatedStorageException)
            {
                return null;
            }

            byte[] dataBytes = ProtectedData.Unprotect(protectedData, Encoding.UTF8.GetBytes("hockeyAppIsCool"));
            return Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);
        }

        private Uri _successRedirect;
        internal Uri SuccessRedirect
        {
            get { return _successRedirect; }
            set { _successRedirect = value; }
        }

        private IAuthStatus authStatus = null;
        internal IAuthStatus CurrentAuthStatus
        {
            get { return authStatus; }
            set
            {
                authStatus = value;
                if (authStatus as AuthStatus != null && authStatus.IsIdentified)
                {
                    StoreStringProtected(Constants.AuthStatusKey, (authStatus as AuthStatus).SerializeToString());
                    IsolatedStorageSettings.ApplicationSettings.SetValue(Constants.AuthLastAuthorizedVersionKey, ManifestHelper.GetAppVersion());
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
        }

        /// <summary>
        /// Removes the user token from the phone store, so on the next call to AuthenticateUser the Loginscreen is shown.
        /// Effectively this serves as a logout from your app. In most cases you want to call AuthenticateUser() immediatley after RemoveUserToken
        /// </summary>
        public void RemoveUserToken()
        {
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            var filename = Constants.AuthStatusKey + ".crypt";
            if (file.FileExists(filename))
            {
                file.DeleteFile(filename);
            }
        }

        
        // Starts the Web-Authentication-Flow of hockeyapp.
        // Prerequisite is to register your app for a URI-association as describe here:
        // http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206987%28v=vs.105%29.aspx#BKMK_URIassociations
        // Make sure to add the following line to your registered protocols (inside the <Extension>-Tag):
        // <Protocol Name="hockeyAuth" NavUriFragment="encodedLaunchUri=%s" TaskID="_default" />
        // In your UriMapperBase- Implementation in MapURI() call UpdateManager.HandleLoginURI(uri)
        
        /*public void InitiateWebAuthentication()
        {
            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(HockeyClient.Instance.ApiBaseVersion3 + "identity/TODO", UriKind.Absolute);
            webBrowserTask.Show();
        }

        public void HandleLoginURI(Uri uri)
        {
         tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());

            // URI association launch for contoso.
            if (tempUri.Contains("contoso:ShowProducts?CategoryID="))
            {
                // Get the category ID (after "CategoryID=").
                int categoryIdIndex = tempUri.IndexOf("CategoryID=") + 11;
                string categoryId = tempUri.Substring(categoryIdIndex);

                // Map the show products request to ShowProducts.xaml
                return new Uri("/ShowProducts.xaml?CategoryID=" + categoryId, UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
         
        }*/

    }
}
