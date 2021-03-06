﻿//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#define SPOT_WARN_NOT_RUNNING

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace VSIXSpotifyRemote
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command1
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("2599fe4a-622d-4cdc-8a4d-a08560938c54");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private OleMenuCommand buttonMenuCommand;


        private string commandText = "Next";


        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Command1(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                //commandService.AddCommand(menuItem);

                OleMenuCommand myOleCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                buttonMenuCommand = myOleCommand;
                if (!Command1Package.CommandShouldShowText())
                {
                    myOleCommand.Text = " ";
                    
                }
                commandService.AddCommand(myOleCommand);
            }

            

            
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command1 Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Command1(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "Command1";

            string message = string.Format(CultureInfo.CurrentCulture, "Spotify is not running");
            string title = "SpotifyRemote";

            if(!SpotifyAPI.Local.SpotifyLocalAPI.IsSpotifyRunning())
            {
#if SPOT_WARN_NOT_RUNNING
                VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
#endif
                SpotifyAPI.Local.SpotifyLocalAPI.RunSpotify();
                System.Threading.Thread.Sleep(300);
                Command1Package.SpotifyConnect();
                Command4.gCommand4Instance.SpotClientRegisterTrackChange();
                Command1Package.UpdateCommandsHiddenState();
                return;
            }

            if (Command1Package.spotClient == null)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("spotClient is null.");
                System.Diagnostics.Debug.Assert(false);
                
                return;
#endif
            }


            // Next
            Command1Package.spotClient.Skip();
            
        }

        public void SetStartupCommandTextState()
        {
            if (!Command1Package.CommandShouldShowText())
            {
                buttonMenuCommand.Text = " ";

            }
            else
            {
                buttonMenuCommand.Text = commandText;
            }
        }

        public void UpdateTextHiddenState()
        {
            if(!Command1Package.SpotifyCommandShouldShowText() && !UserPreferences.Default.HideButtonTextOnInactive)
            {
                return;
            }

            if (!Command1Package.IsSpotifyProcessRunning())
            {
                buttonMenuCommand.Text = " ";
            }
            else
            {
                buttonMenuCommand.Text = commandText;
            }

        }
    }
}
