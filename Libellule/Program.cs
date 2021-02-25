using System;
using System.IO;
using System.Collections.Generic;
using static System.Console;

using Libellule.WadUtil;
using Libellule.ProfileUtil;
using LoLCustomSharp;

namespace Libellule
{
    class Program
    {

        private static readonly string version = "v1.2.0";

        public static readonly string location = @"\DATA\FINAL\Champions\Yasuo.wad.client";

        public Profile actualProfile;

        public void Start( string[] args )
        {
            Title = "Libellule";
            WriteLine( $"Libellule [{version}]" );
            Dictionary<string , Profile> profiles = new Dictionary<string , Profile>();
            DirectoryInfo directoryInfo = Directory.CreateDirectory( "profiles" );
            foreach ( DirectoryInfo info in directoryInfo.GetDirectories() )
            {
                FileInfo fileInfo = GetProfileFile( info );
                if ( fileInfo != null )
                {
                    Profile profile = Profile.LoadProfile( fileInfo );
                    if ( profile == null )
                        continue;
                    if ( !IsValidLocation( profile._lolLocation ) )
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine( $"The profile {fileInfo.Name} has an invalid lol location." );
                        ForegroundColor = ConsoleColor.Gray;
                        continue;
                    }
                    profiles.Add( profile._profileName , profile );
                }
            }
            if ( args.Length >= 1 )
            {
                string profileName = args[ 0 ];
                if ( profiles.ContainsKey( profileName ) )
                {
                    this.actualProfile = profiles[ profileName ];
                    profiles.Clear();
                    StartOverlay();
                }
                WriteLine( $"The profile {profileName} does not exist or have any issues. " );
                PressAnyKey();
            }
            if ( profiles.Count == 0 )
            {
                WriteLine( "You need to create a profile." );
                WriteLine( "Type: help" );
            } else 
                HelpCommand();
            while( true )
            {
                string[] consoleLine = ReadLine().Split(" ");
                if ( consoleLine.Length >= 1 )
                {
                    switch ( consoleLine[ 0 ] )
                    {
                        case "createprofile":
                            if ( consoleLine.Length >= 3 )
                            {
                                string profileName = consoleLine[ 1 ];
                                if ( profiles.ContainsKey( profileName ) )
                                {
                                    WriteLine( $"The profile {profileName} already exists. " );
                                    continue;
                                }
                                string location = GetLolLocation( consoleLine );
                                if ( !IsValidLocation( location ) )
                                {
                                    WriteLine( $"The location {location} is not valid" );
                                    WriteLine( "You need to select a valid league of legends location." );
                                    WriteLine( @"Example: C:\Riot Games\League of Legends\Game" );
                                    continue;
                                }
                                Profile profile = Profile.CreateProfile( profileName , directoryInfo + @"\" + profileName , location );
                                profile.WriteFile();
                                WadCreator.Build( profile._lolLocation , profile._folderLocation + @"\file" , false );
                                profiles.Add( profileName , profile );
                                WriteLine( $"You should use: 'run {profileName}' to start the process." );
                            } else
                                WriteLine( "Usage: createprofile {profile_name} {lol_location}" );
                            break;
                        case "list":
                            WriteLine( "Available profiles: " );
                            foreach ( Profile profile in profiles.Values )
                                WriteLine( $"{profile._profileName} - {profile._lolLocation}" );
                            break;
                        case "run":
                            if ( consoleLine.Length >= 2 )
                            {
                                string profileName = consoleLine[ 1 ];
                                if ( !profiles.ContainsKey( profileName ) )
                                {
                                    WriteLine( $"The profile {profileName} does not exist." );
                                    continue;
                                }
                                this.actualProfile = profiles[ profileName ];
                                profiles.Clear();
                                StartOverlay();
                            } else
                                WriteLine( "Usage: run {profile_name}" );
                            break;
                        case "about":
                            WriteLine( "Libellule" );
                            WriteLine( "Author: Tadaashii" );
                            WriteLine( $"Version: {version}" );
                            WriteLine( "Github: github.com/Tadaashii" );
                            break;

                        case "help":
                            HelpCommand();
                            break;
                    }
                } else HelpCommand();
            }
        }

        public void StartOverlay()
        {
            string folderLocation = this.actualProfile._folderLocation + @"\file";
            bool fileExists = File.Exists( folderLocation + @"\" + location );
            if ( !fileExists || !this.actualProfile.isTheSameDate() )
            {
                WadCreator.Build( this.actualProfile._lolLocation ,  folderLocation , fileExists );
                this.actualProfile._lastModified = this.actualProfile.GetLolWadLastWriteTime();
                this.actualProfile.WriteFile();
            }
            Clear();
            WriteLine( $"Libellule [{version}]" );
            OverlayPatcher patcher = new OverlayPatcher();
            patcher._exeLocation = this.actualProfile._lolLocation;
            patcher.Start( folderLocation , OnPatcherMessage , OnPatcherError );
            patcher.Join();
            PressAnyKey();
        }

        private void PressAnyKey()
        {
            WriteLine( "Press any key to exit..." );
            ReadKey();
            Environment.Exit( 0 );
        }

        private string GetLolLocation( string[] array )
        {
            List<string> list = new List<string>();
            for ( int index = 2; index < array.Length; index++ )
                list.Add( array[ index ] );
            return string.Join( ' ' , list );
        }

        private void HelpCommand()
        {
            WriteLine( "Available commands: " );
            ForegroundColor = ConsoleColor.Green;
            WriteLine( "createprofile | Create a new profile. " );
            WriteLine( "list | Show all profiles." );
            WriteLine( "run" );
            WriteLine( "about" );
            ForegroundColor = ConsoleColor.Gray;
        }

        private FileInfo GetProfileFile( DirectoryInfo info )
        {
            foreach ( var fileInfo in info.GetFiles() )
                if ( fileInfo.Extension == ".profile" )
                    return fileInfo;
            return null;
        }

        private bool IsValidLocation( string lolLocation )
        {
            return File.Exists( lolLocation + @"\League of Legends.exe" )
                && File.Exists( lolLocation + @"\DATA\FINAL\Champions\Yasuo.wad.client" );

        }

        private void OnPatcherError( Exception exception )
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine( "[ERROR] PATCHER: {0}" , exception );
            ForegroundColor = ConsoleColor.Gray;
        }

        private void OnPatcherMessage( string message )
            => WriteLine( "[INFO] PATCHER: {0}" , message );

        static void Main( string[] args )
        {
            Program program = new Program();
            program.Start( args );
        }
    }
}
