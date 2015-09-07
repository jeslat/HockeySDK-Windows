using System;
using Windows.Phone.Management.Deployment;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace HockeyApp
{

    public enum UpdateCheckFrequency
    {
        Always
        //TODO daily/weekly/monthly
    }

    public enum UpdateMode
    {
        Startup,
        InApp
    }

    /// <summary>
    /// Settings for update-checking
    /// </summary>
    public class UpdateCheckSettings
    {

        public static UpdateCheckSettings DefaultStartupSettings
        {
            get
            {
                return new UpdateCheckSettings();
           }
        }

        private UpdateMode updateMode = UpdateMode.Startup;
        /// <summary>
        /// Defines the mode in which the Startup-check should be run (InApp vs. during Startup)
        /// </summary>
        public UpdateMode UpdateMode
        {
            get { return updateMode; }
            set { updateMode = value; }
        }
        
        private UpdateCheckFrequency updateCheckFrequency = UpdateCheckFrequency.Always;
        /// <summary>
        /// Set the frequency to check for updates
        /// </summary>
        public UpdateCheckFrequency UpdateCheckFrequency
        {
            get { return updateCheckFrequency; }
            set { updateCheckFrequency = value; }
        }
        
        private Func<IAppVersion,bool> customDoShowUpdateFunc = null;
        /// <summary>
        /// Handle a found update with custom code (no default ui shown)
        /// </summary>
        public Func<IAppVersion,bool> CustomDoShowUpdateFunc
        {
            get { return customDoShowUpdateFunc; }
            set { customDoShowUpdateFunc = value; }
        }

        private bool enforceUpdateIfMandatory = true;
        /// <summary>
        /// Enforce the update if new version is marked as mandatory (default: true)
        /// </summary>
        public bool EnforceUpdateIfMandatory
        {
            get { return enforceUpdateIfMandatory; }
            set { enforceUpdateIfMandatory = value; }
        }

    }

    /// <summary>
    /// Provides automatic update functionality with HockeyApp
    /// </summary>
    public class UpdateManager
    {

        private static readonly UpdateManager instance = new UpdateManager();

        static UpdateManager() { }
        private UpdateManager() { }

        public static UpdateManager Instance
        {
            get
            {
                return instance;
            }
        }

        internal bool CheckWithUpdateFrequency(UpdateCheckFrequency frequency)
        {
            //TODO implement. store and check last update timestamp...
            return true;
        }

        internal async void DoUpdate(IAppVersion availableUpdate)
        {
            var aetxUri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + HockeyClient.Current.AsInternal().AppIdentifier + ".aetx", UriKind.Absolute);
            var downloadUri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + HockeyClient.Current.AsInternal().AppIdentifier + "/app_versions/" + availableUpdate.Id + ".xap", UriKind.Absolute);

            Exception installError = null;
            try
            {
                var result = await InstallationManager.AddPackageAsync(availableUpdate.Title, downloadUri);
            }
            catch (Exception e)
            {
                installError = e;
                HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
            }
            if (installError != null)
            {
                MessageBox.Show(String.Format(LocalizedStrings.LocalizedResources.UpdateAPIError, installError.Message));
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = downloadUri;
                webBrowserTask.Show();
            }
        }
    }
}
