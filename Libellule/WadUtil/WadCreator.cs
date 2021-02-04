using System.Collections.Generic;
using LeagueToolkit.IO.WadFile;
using System.IO;

namespace Libellule.WadUtil
{
    class WadCreator
    {

        private readonly Dictionary<ulong , WadEntryBuilder> _entries = new Dictionary<ulong , WadEntryBuilder>();

        private readonly string _wadLocation, _saveDirectory;

        internal Wad _wad;
        
        internal WadCreator( string lolLocation , string saveDirectory ) 
        {
            string location = @"\DATA\FINAL\Champions";
            this._wadLocation = lolLocation + location + @"\Yasuo.wad.client";
            this._saveDirectory = saveDirectory + location;
            Directory.CreateDirectory( _saveDirectory );
        }

        public void Build()
        {
            this._wad = Wad.Mount( this._wadLocation , true );
            var base_recall = GetWadEntry( YasuoFiles.Base_Recall );
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
            foreach ( WadEntry entry in this._wad.Entries.Values )
                wadBuilder.WithEntry(
                    this._entries.ContainsKey(
                        entry.XXHash ) ? this._entries[ entry.XXHash ]
                        : CreateEntryBuilder( entry , null )
                 );
            string fileName = this._saveDirectory + @"\Yasuo.wad.client";
            if ( File.Exists( fileName ) ) 
                File.Delete( fileName );
            wadBuilder.Build( File.OpenWrite( fileName ) , false );
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
            Skin36_Spell4 = 7738377741580090003

        }

        public static void Build( string location , string saveDirectory ) 
            => new WadCreator( location , saveDirectory ).Build();

    }
}