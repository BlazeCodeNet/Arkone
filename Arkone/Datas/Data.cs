using Arkone.Datas;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Arkone
{
    public class Data
    {
        public Data( )
        {
            bool firstLoad = false;
            string targetFile = $"./{DatabaseFile}";
            if ( !File.Exists( targetFile ) )
            {
                SQLiteConnection.CreateFile( targetFile );
                firstLoad = true;
            }

            conn = new SQLiteConnection( "Data Source=arkone.sqlite;Version=3" );
            conn.Open( );

            string sql = "";
            if ( firstLoad )
            {
                sql = $"CREATE TABLE gamers ('steamid' VARCHAR(32), 'points' BIGINT, 'discordid' VARCHAR(32), 'arkPlayerId' VARCHAR(32))";
                SQLiteCommand sqlCmd = new SQLiteCommand( sql, conn );
                sqlCmd.ExecuteNonQuery( );

                Console.WriteLine( "Setup first time db!" );
            }

            targetFile = $"./config.json";

            if ( !File.Exists( targetFile ) )
            {
                DataConfig tmpConfig = new DataConfig( );

                string jsonString = JsonSerializer.Serialize( tmpConfig, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                } );

                File.WriteAllText( targetFile, jsonString );

                Console.WriteLine( "Setup first time config. Please edit it and restart this program." );
                Console.WriteLine( $"Config file is at:{ targetFile }" );

                loadFailed = true;

                return;
            }

            string jsonRawText = File.ReadAllText( targetFile );
            config = JsonSerializer.Deserialize<DataConfig>( jsonRawText );
            if ( config != null )
            {
                Console.WriteLine( "Loaded Configuration." );
            }
        }

        public DataGamer GetGamerBySteamId( ulong steamId )
        {
            string sql = "";

            sql = $"SELECT * FROM gamers WHERE steamid='{steamId}'";
            SQLiteCommand sqlCmd = new SQLiteCommand( sql, conn );
            SQLiteDataReader reader = sqlCmd.ExecuteReader( );

            if ( reader.Read( ) )
            {
                return new DataGamer
                {
                    steamId = (string)reader[ "steamid" ],
                    points = (long)reader[ "points" ],
                    discordId = (string)reader[ "discordid" ],
                    arkPlayerId = (string)reader[ "arkPlayerId" ],
                };
            }
            return null;
        }

        public DataGamer GetGamerByDiscordId( ulong discordId )
        {
            string sql = "";

            sql = $"SELECT * FROM gamers WHERE discordid='{discordId}'";
            SQLiteCommand sqlCmd = new SQLiteCommand( sql, conn );
            SQLiteDataReader reader = sqlCmd.ExecuteReader( );

            if ( reader.Read( ) )
            {
                return new DataGamer
                {
                    steamId = (string)reader[ "steamid" ],
                    points = (long)reader[ "points" ],
                    discordId = (string)reader[ "discordid" ],
                    arkPlayerId = (string)reader[ "arkPlayerId" ],
                };
            }
            return null;
        }

        public void ApplyGamer( DataGamer gamer )
        {
            try
            {
                string sql = "";

                sql = $"SELECT 'points' FROM gamers WHERE steamid='{gamer.steamId}'";
                SQLiteCommand sqlCmd = new SQLiteCommand( sql, conn );
                SQLiteDataReader reader = sqlCmd.ExecuteReader( );

                bool gamerExists = reader.Read( );

                if ( gamerExists )
                {
                    sql = $"UPDATE gamers SET steamid='{gamer.steamId.ToString()}', points='{gamer.points}', discordid='{gamer.discordId.ToString( )}', arkPlayerId='{gamer.arkPlayerId}' WHERE steamid='{gamer.steamId}'";
                    sqlCmd = new SQLiteCommand( sql, conn );
                    sqlCmd.ExecuteNonQuery( );
                }
                else
                {
                    sql = $"INSERT INTO gamers (steamid,points,discordid,arkPlayerId) VALUES ('{gamer.steamId}','{gamer.points}','{gamer.discordId}','{gamer.arkPlayerId}')";
                    sqlCmd = new SQLiteCommand( sql, conn );
                    sqlCmd.ExecuteNonQuery( );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "ApplyGamer() Failed:" + ex.ToString( ) );
            }
        }

        const string DatabaseFile = "arkone.sqlite";
        public SQLiteConnection conn { get; private set; }
        public DataConfig config { get; private set; }
        // Set to true to set if the configuration loading failed.
        public bool loadFailed { get; private set; } = false;
    }
}