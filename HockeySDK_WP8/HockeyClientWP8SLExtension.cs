﻿using HockeyApp.Internal;
using System;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HockeyApp
{
    /// <summary>
    /// Static extension class containing the main extension methods for controlling the HockeySDK client
    /// </summary>
    public static class HockeyClientWP8SLExtension
    {

        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        #region Configuration
        /// <summary>
        /// main configuration method. call in app constructor
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appId"></param>
        /// <param name="rootFrame"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appId, string test, Frame rootFrame = null)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWP8SL();
            @this.AsInternal().AppIdentifier = appId;
            CrashHandler.Current.Application = Application.Current;
            CrashHandler.Current.Application.UnhandledException += (sender, args) => { 
                CrashHandler.Current.HandleException(args.ExceptionObject);
                if (customUnhandledExceptionAction != null)
                {
                    customUnhandledExceptionAction(args);
                }
            };

            if (rootFrame != null)
            {
                //Idea based on http://www.markermetro.com/2013/01/technical/handling-unhandled-exceptions-with-asyncawait-on-windows-8-and-windows-phone-8/
                //catch async void Exceptions
                AsyncSynchronizationContext.RegisterForFrame(rootFrame, CrashHandler.Current);
            }

            return @this as IHockeyClientConfigurable;
        }

        /// <summary>
        /// Provide a custom resource manager to override standard sdk i18n strings
        /// </summary>
        /// <param name="this"></param>
        /// <param name="manager">resource manager to use</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UseCustomResourceManager(this IHockeyClientConfigurable @this, ResourceManager manager)
        {
            //TODO make LocalizedStrings.CustomResourceManager internal in next major version
            #pragma warning disable 0618
            LocalizedStrings.CustomResourceManager = manager;
            #pragma warning restore 0618
            return @this;
        }


        #endregion

        #region Wrappers for functions

        #region CrashHandling

        [Obsolete("Please use SendCrashesAsync() instead")]
        public static async Task<bool> HandleCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await CrashHandler.Current.HandleCrashesAsync(sendAutomatically).ConfigureAwait(false);
        }

        /// <summary>
        /// Send any collected crashes to the HockeyApp server. You should normally call this during startup of your app. 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendWithoutAsking">configures if available crashes are sent immediately or if the user should be asked if the crashes should be sent or discarded</param>
        /// <returns>true if crashes where sent successfully</returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, bool sendWithoutAsking = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await CrashHandler.Current.HandleCrashesAsync(sendWithoutAsking).ConfigureAwait(false);
        }


        internal static Action<ApplicationUnhandledExceptionEventArgs> customUnhandledExceptionAction;
        internal static Action<Exception> customUnobservedTaskExceptionAction;

        /// <summary>
        /// The action you set will be called after HockeyApp has written the crash-log and allows you to run custom logic like marking the exception as handled
        /// </summary>
        /// <param name="customAction">The custom action.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Action<ApplicationUnhandledExceptionEventArgs> customAction)
        {
            customUnhandledExceptionAction = customAction;
            return @this;
        }

        /// <summary>
        /// The action you set will be called after HockeyApp has written the crash-log and allows you to run custom logic like exiting the application
        /// </summary>
        /// <param name="customFunc">The custom action.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Action<Exception> customAction)
        {
            customUnobservedTaskExceptionAction = customAction;
            return @this;
        }


        #endregion

        #endregion
    }
}
