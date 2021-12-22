using Arkone.Datas;

using Newtonsoft.Json;

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
                sql = $"CREATE TABLE gamers ('steamid' BIGINT UNSIGNED, 'points' BIGINT, 'discordid' BIGINT UNSIGNED, 'nickname' VARCHAR(32))";
                SQLiteCommand sqlCmd = new SQLiteCommand( sql, conn );
                sqlCmd.ExecuteNonQuery( );

                Console.WriteLine( "Setup first time db!" );
            }

            targetFile = $"./config.json";

            if ( !File.Exists( targetFile ) )
            {
                DataConfig tmpConfig = new DataConfig
                {
                    discordBotSecret = "",
                    ownerDiscordId = 0,
                    playerCountEnabled = true,
                    playerCountChannelDiscordId = 0,
                    discordGuildId = 0,
                    rewardsEnabled = true,
                    rewardsCooldownSeconds = 360,
                    rewardsAmount = 3,
                    rconPrimaryAddress = "127.0.0.1:27025",
                    rconSecondaryAddress = "127.0.0.1:27025",
                    rconPassword = "password",
                    rconQueryCooldownSeconds = 6,

                };

                string jsonString = JsonConvert.SerializeObject( tmpConfig );

                File.WriteAllText( targetFile, jsonString );

                Console.WriteLine( "Setup first time config. Please edit it and restart this program." );
                Console.WriteLine( $"Config file is at:{ targetFile }" );

                return;
            }

            string jsonRawText = File.ReadAllText( targetFile );
            config = JsonConvert.DeserializeObject<DataConfig>( jsonRawText );
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
                    steamId = (ulong)(long)reader[ "steamid" ],
                    points = (long)reader[ "points" ],
                    discordId = (ulong)(long)reader[ "discordid" ],
                    nickname = (string)reader[ "nickname" ],
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
                    steamId = (ulong)(long)reader[ "steamid" ],
                    points = (long)reader[ "points" ],
                    discordId = (ulong)(long)reader[ "discordid" ],
                    nickname = (string)reader[ "nickname" ],
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
                    sql = $"UPDATE gamers SET steamid='{gamer.steamId}', points='{gamer.points}', discordid='{gamer.discordId}', nickname='{gamer.nickname}' WHERE steamid='{gamer.steamId}'";
                    sqlCmd = new SQLiteCommand( sql, conn );
                    sqlCmd.ExecuteNonQuery( );
                }
                else
                {
                    sql = $"INSERT INTO gamers (steamid,points,discordid,nickname) VALUES ('{gamer.steamId}','{gamer.points}','{gamer.discordId}','{gamer.nickname}')";
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
    }
}