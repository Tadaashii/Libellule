using System.Collections.Generic;
using LeagueToolkit.IO.WadFile;
using System;
using System.IO;
using ZstdSharp;

using static Libellule.Program;

namespace Libellule.WadUtil
{
    class WadCreator
    {

        private readonly Dictionary<ulong , WadEntryBuilder> _entries = new Dictionary<ulong , WadEntryBuilder>();

        private readonly string _wadLocation, _fileName;

        internal Wad _wad;

        internal WadCreator( string lolLocation , string saveDirectory )
        {
            this._wadLocation = lolLocation + location;
            this._fileName = saveDirectory + location;
            Directory.CreateDirectory( Path.GetDirectoryName( _fileName ) );
        }

        public void Build( bool boolean )
        {
            Console.WriteLine( ( boolean ? "Updating" : "Creating" ) + " WAD file..." );
            this._wad = Wad.Mount( this._wadLocation , false );
            var base_recall = this.GetWadEntry( YasuoFiles.Base_Recall );
            //Base
            AddBuilder(
                CreateEntryBuilder( GetWadEntry( YasuoFiles.Base_Spell4 ) , base_recall ) );
            //Skin09
            AddBuilder(
                CreateEntryBuilder( GetWadEntry( YasuoFiles.Skin09_Spell4 ) , base_recall ) );
            AddBuilder(
                CreateEntryBuilder( GetWadEntry( YasuoFiles.Skin09_Spell4_Trans ) , GetWadEntry( YasuoFiles.Skin09_Idle ) ) );
            //Skin36
            AddBuilder(
                CreateEntryBuilder( GetWadEntry( YasuoFiles.Skin36_Spell4 ) , base_recall ) );
            WadBuilder wadBuilder = new WadBuilder();
            List<ulong> idleFiles = new List<ulong>()
            {
                //base_idle_out
                1287173847115689774, 
                //skin10_idle_out
                11233438142475437009, 
                //skin36_idle_out
                17914131572664223235
            };
            foreach ( WadEntry entry in this._wad.Entries.Values )
            {
                ulong hashEntry = entry.XXHash;
                if ( idleFiles.Contains( hashEntry ) )
                {
                    Stream stream = entry.GetDataHandle().GetDecompressedStream();
                    stream.Seek( 36 , SeekOrigin.Begin );
                    //duration
                    stream.Write( BitConverter.GetBytes( 3.0F ) );
                    stream.Seek( 0 , SeekOrigin.Begin );
                    WadEntryBuilder builder = new WadEntryBuilder();
                    builder.WithPathXXHash( hashEntry );
                    int uncompressedSize = ( int ) stream.Length;
                    MemoryStream compressedStream = new MemoryStream();
                    using ( ZstdStream zstdStream = new ZstdStream( compressedStream , ZstdStreamMode.Compress , true ) )
                    {
                        stream.CopyTo( zstdStream );
                    }
                    builder.WithZstdDataStream( compressedStream , ( int ) compressedStream.Length , uncompressedSize );
                    wadBuilder.WithEntry( builder );
                }
                else
                    wadBuilder.WithEntry(
                    this._entries.ContainsKey(
                        hashEntry ) ? this._entries[ hashEntry ]
                        : CreateEntryBuilder( entry , null )
                    );
            }
            wadBuilder.Build( File.OpenWrite( _fileName ) , false );
            _entries.Clear();
            Console.WriteLine( "The WAD file has been " + ( boolean ? "updated" : "created" ) + "..." );
        }

        private void AddBuilder( WadEntryBuilder builder ) => this._entries.Add( builder.PathXXHash , builder );

        private WadEntryBuilder CreateEntryBuilder( WadEntry originalEntry , WadEntry modify )
        {
            WadEntryBuilder builder = new WadEntryBuilder();
            WadEntry data = modify != null ? modify : originalEntry;

            builder.WithPathXXHash( originalEntry.XXHash );

            switch ( originalEntry.Type )
            {

                case WadEntryType.ZStandardCompressed:
                    builder.WithZstdDataStream(
                         data.GetDataHandle().GetCompressedStream() ,
                         data.CompressedSize ,
                         data.UncompressedSize );
                    break;

                case WadEntryType.Uncompressed:
                    builder.WithUncompressedDataStream(
                         data.GetDataHandle().GetDecompressedStream() );
                    break;

            }

            return builder;
        }

        private WadEntry GetWadEntry( YasuoFiles yasuoFile )
        {
            ulong hash = ( ulong ) yasuoFile;
            foreach ( WadEntry entry in _wad.Entries.Values )
                if ( entry.XXHash.Equals( hash ) )
                    return entry;
            return null;
        }

        enum YasuoFiles : ulong
        {
            Base_Recall = 4905833260576619879,
            Base_Spell4 = 5213630906865195970,
            Skin09_Idle = 7613741797256328795,
            Skin09_Spell4 = 4119068867002229939,
            Skin09_Spell4_Trans = 17698895096365980226,
            Skin36_Spell4 = 7738377741580090003,
        }

        public static void Build( string location , string saveDirectory, bool boolean ) 
            => new WadCreator( location , saveDirectory ).Build( boolean );

    }
}