using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MythicLauncher
{
    public enum ModuleState
    {
        Invalid,                //No game available
        Failed,                 //Download or update failed.
        Idle_RequiresGame,      //Launcher is doing nothing. Game is not yet installed
        Idle_RequiresUpdate,    //Launcher is doing nothing. Update available
        Idle_GameUptoDate,      //Launcher is doing nothing. Game is latest version
        DownloadingGame,        //Launcher is downloading the game
        DownloadingUpdate,      //Launcher is downloading an update
        GameRunning             //Game is running
    }

    public class GameModule
    {
        const string VersionFileName = "\\\\VRYZL\\DistributedGames\\VersionFile.txt";

        private string rootPath;
        private string versionFile;
        private string gameZip;
        public string workingDirectory { get; }
        public string gameExe { get; }
        public string GameName { get; set; }
        public Version localversion { get; set; }
        public EventHandler<ModuleState> StateChanged;

        private ModuleState _state;

        internal ModuleState State
        {
            get => _state;
            set
            {
                _state = value;
                if(StateChanged != null)
                {
                    StateChanged.Invoke(this, State);
                }
            }
        }

        public GameModule(string gameName)
        {
            this.rootPath = Directory.GetCurrentDirectory();
            this.gameZip = gameZip;
            this.gameExe = gameExe;
            State = ModuleState.Idle_GameUptoDate;
            GameName = gameName;

            this.gameZip = Path.Combine(rootPath, "Build.Zip");
            this.workingDirectory = Path.Combine(rootPath, $"Builds\\{gameName}\\Windows");
            this.gameExe = Path.Combine(workingDirectory, $"{gameName}.exe");

            versionFile = Path.Combine(Directory.GetCurrentDirectory(), "Version.txt");

            //CheckForUpdates();
        }

        public void ChechForUpdates()
        {
            if (File.Exists(versionFile))
            {
                localversion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString(VersionFileName));

                    if (onlineVersion.IsDifferentThan(localversion))
                    {
                        InstallGameFiles(false, onlineVersion);
                    }
                    else
                    {
                        State = ModuleState.Idle_GameUptoDate;
                    }
                }
                catch (Exception ex)
                {
                    State = ModuleState.Failed;
                    MessageBox.Show($"Error checking for game update: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    State = ModuleState.DownloadingUpdate;
                }
                else
                {
                    State = ModuleState.DownloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString(VersionFileName));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("GameLink"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                State = ModuleState.Failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs eventArgs)
        {
            try
            {
                string onlineVersion = ((Version)eventArgs.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                //VersionText.Text = onlineVersion;
            }
            catch (Exception ex)
            {
                State = ModuleState.Failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
    }

    public struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        internal Version(string _version)
        {
            string[] versionStrings = _version.Split('.');
            if (versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
            }

            major = short.Parse(versionStrings[0]);
            minor = short.Parse(versionStrings[1]);
            subMinor = short.Parse(versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else if (minor != _otherVersion.minor)
            {
                return true;
            }
            else if (subMinor != _otherVersion.subMinor)

            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
