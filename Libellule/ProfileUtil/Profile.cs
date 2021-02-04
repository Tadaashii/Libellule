using System;
using System.IO;

namespace Libellule.ProfileUtil
{
    class Profile
    {

        public string _profileName, _folderLocation, _lolLocation, _lastModified;

        internal Profile( string profileName, string folderLocation , string lolLocation , string lastModified )
        {
            this._profileName = profileName;
            this._folderLocation = folderLocation;
            this._lolLocation = lolLocation;
            this._lastModified = lastModified;
        }

        internal Profile() {}

        public void WriteFile()
        {
            Directory.CreateDirectory( this._folderLocation );
            using ( var writer = new StreamWriter( this._folderLocation + @"\" + this._profileName + ".profile" ) )
            {
                writer.WriteLine( this._profileName );
                writer.WriteLine( this._folderLocation );
                writer.WriteLine( this._lolLocation );
                writer.WriteLine( this._lastModified );
            }
        }

        public bool isTheSameDate() 
            => _lastModified.Equals( GetLolWadLastWriteTime() );

        public string GetLolWadLastWriteTime()
        {
            return File.GetLastWriteTime
                ( _lolLocation + @"\DATA\FINAL\Champions\Yasuo.wad.client" ).ToString();
        }

        public static Profile LoadProfile( FileInfo fileInfo )
        {
            using ( var reader = new StreamReader( fileInfo.OpenRead() ) )
            {
                try
                {
                    return new Profile(
                        reader.ReadLine(),
                        reader.ReadLine(),
                        reader.ReadLine(),
                        reader.ReadLine()
                        );
                } catch ( Exception ex )
                {
                    Console.WriteLine( $"The profile {fileInfo.Name} did not load successfully." );
                    Console.WriteLine( ex.Message );
                }
            }
            return null;
        }

        public static Profile CreateProfile( string profileName , string folderLocation , string lolLocation )
        {
            Profile profile = new Profile();
            profile._profileName = profileName;
            profile._folderLocation = folderLocation;
            profile._lolLocation = lolLocation;
            profile._lastModified = File.GetLastWriteTime( lolLocation +
                @"\DATA\FINAL\Champions\Yasuo.wad.client" ).ToString();
            Console.WriteLine( $"Profile {profileName} has been created successfully." );
            return profile;
        }

    }
}
