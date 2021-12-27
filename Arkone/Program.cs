using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;

using Arkone.Commands;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

namespace Arkone
{
    public class Program
    {
        public static void Main( string[ ] args )
        {
            // Using the following line due to reports of issues using async with console-app in C#/DotNet because of IL issues.
            // Assuming true, and trying to be safe.
            Console.WriteLine( $"Starting..." );

            rewardPlayersTimer = Stopwatch.StartNew( );

            try
            {
                data = new Data( );
                if(!data.loadFailed)
                {
                    MainTask( ).GetAwaiter( ).GetResult( );
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine( "Main() Crashed:" + ex.ToString( ) );
            }

            Console.WriteLine( "Closed." );
        }

        public static async Task MainTask( )
        {
            // Make a new discord client
            discord = new DiscordClient( new DiscordConfiguration( )
            {
                Token = data.config.discordBotSecret,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = LogLevel.Error,
                AutoReconnect = true,
            } );

            discord.GuildAvailable += OnDiscordGuildAvailableAsync;

            SlashCommandsExtension slashExtension = discord.UseSlashCommands( );
            slashExtension.RegisterCommands<SlashCommands>( data.config.discordGuildId );
            slashExtension.RegisterCommands<ShopCommands>( data.config.discordGuildId );

            // Connect the client to discord
            _ = discord.ConnectAsync( );


            // Wait for discord to serve us the Guild data so we have everything loaded before doing the playercount query.
            while ( !running )
            {
                await Task.Delay( 1000 );
            }

            // Repeatedly try to UpdatePlayerCountAsync() with a 6 second cooldown in between calls.
            while ( running )
            {
                try
                {
                    await UpdatePlayerCountAsync( );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Error occured in UpdatePlayerCountAsync:" + ex.ToString( ) );
                }

                await Task.Delay( ( data.config.rconQueryCooldownSeconds ) * 1000 );
            }

            // If it gets here, the program has set running to false.
            Console.WriteLine( "Closing Passively..." );
        }

        public static async Task<string> ExecuteRCONAsync(string address, string command)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "rcon.exe";
            pro.StartInfo.Arguments = $"-a { address } -p { data.config.rconPassword } \"{ command }\"";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.CreateNoWindow = true;
            pro.Start( );

            string result = await pro.StandardOutput.ReadToEndAsync();
            pro.WaitForExit( );

            return result;
        }

        /// <summary>
        /// Sends a RCON Command asking for each servers playercount.
        /// Then changes the displayed PlayerCount on the target Discord Guild
        /// </summary>
        /// <returns></returns>
        public static async Task UpdatePlayerCountAsync( )
        {
            int playerCount = 0;
            string response;

            List<ulong> foundPlayerSteamIds = new List<ulong>( );
            List<ulong> foundPlayersPrimary = new List<ulong>( );
            List<ulong> foundPlayersSecondary = new List<ulong>( );

            try
            {
                response = await ExecuteRCONAsync( data.config.rconPrimaryAddress, "listplayers" );
                response = response.TrimStart( ).TrimEnd( );
                if ( response.Contains( "0." ) )
                {
                    int serverPlrCount = 1;
                    if ( response.Contains( '\n' ) )
                    {
                        serverPlrCount = response.Split( '\n' ).Length;
                    }
                    playerCount += serverPlrCount;

                    string[ ] respLines = response.Split( '\n' );
                    foreach ( string l in respLines )
                    {
                        string[ ] splits = l.Split( ' ' );
                        ulong lineId = ulong.Parse( splits[ splits.Length - 1 ] );
                        if ( !foundPlayerSteamIds.Contains( lineId ) )
                        {
                            foundPlayerSteamIds.Add( lineId );
                            foundPlayersPrimary.Add( lineId );
                        }
                    }
                }

            }
            catch ( Exception ex )
            {
                //Console.WriteLine($"UpdatePlayerCountAsync Base RCON TRY/CATCH Triggered:{ex.ToString()}");
            }

            try
            {
                response = await ExecuteRCONAsync( data.config.rconSecondaryAddress, "listplayers" );
                response = response.TrimStart().TrimEnd();
                if ( response.StartsWith( "0." ) )
                {
                    int serverPlrCount = 1;
                    if ( response.Contains( '\n' ) )
                    {
                        serverPlrCount = response.Split( '\n' ).Length;
                    }
                    playerCount += serverPlrCount;

                    string[ ] respLines = response.Split( '\n' );
                    foreach ( string l in respLines )
                    {
                        string[ ] splits = l.Split( ' ' );
                        ulong lineId = ulong.Parse( splits[ splits.Length - 1 ] );
                        if ( !foundPlayerSteamIds.Contains( lineId ) )
                        {
                            foundPlayerSteamIds.Add( lineId );
                            foundPlayersSecondary.Add( lineId );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                //Console.WriteLine($"UpdatePlayerCountAsync Other RCON TRY/CATCH Triggered:{ex.ToString()}");
            }

            //Console.WriteLine($"Current PlayerCount={playerCount}");

            // If needed, add rewards
            if ( rewardPlayersTimer.ElapsedMilliseconds >= data.config.rewardsCooldownSeconds * 1000 )
            {
                // Console.WriteLine( "CHECKED REWARDS" );
                foreach ( ulong curId in foundPlayerSteamIds )
                {
                    if ( lastOnlinePlayers.Contains( curId ) )
                    {
                        // Player was on last check AND this check!
                        DataGamer rewardGamer = data.GetGamerBySteamId( curId );
                        if ( rewardGamer != null )
                        {
                            bool isOnPrimary = foundPlayersPrimary.Contains( curId );
                            string targetAddress = isOnPrimary ? data.config.rconPrimaryAddress : data.config.rconSecondaryAddress;
                            _ = ExecuteRCONAsync( targetAddress, $"'ServerChatTo \"{ curId }\" [GameR] Awarded { data.config.rewardsAmount } points for playtime!'" );

                            rewardGamer.points += data.config.rewardsAmount;
                            data.ApplyGamer( rewardGamer );
                        }
                    }
                }

                rewardPlayersTimer.Restart( );
                lastOnlinePlayers = foundPlayerSteamIds;
            }

            if ( lastPlayerCount != playerCount && discordModifyStopwatch.ElapsedMilliseconds > data.config.discordModifyCooldownSeconds * 1000 )
            {
                Console.WriteLine( $"Updating TotalPlayerCount to {playerCount}" );
                lastPlayerCount = playerCount;
                
                _ = playerCountChannel.ModifyAsync( x =>
                {
                    x.Name = $"ONLINE PLAYERS: { playerCount.ToString( ) }";
                } );
            }
        }

        public static async Task GrabDiscordInstancesAsync( )
        {
            // Grab the guild's Channels we want by their snowflake ID and store the instances
            playerCountChannel = guild.GetChannel( data.config.playerCountChannelDiscordId );
        }

        /// <summary>
        /// Checks if steamId is on a server. Returns true if on primary, false on secondary, null on neither.
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public static async Task<bool?> IsSteamIdOnline( string steamId )
        {
            try
            {
                string response = "__NULL__";
                response = await ExecuteRCONAsync( data.config.rconPrimaryAddress, "listplayers" );
                response = response.Trim( );
                if ( response.StartsWith( "0." ) )
                {
                    string[ ] respLines = response.Split( '\n' );
                    foreach ( string l in respLines )
                    {
                        string[ ] splits = l.Split( ' ' );
                        string lineId = splits[ splits.Length - 1 ];
                        
                        if(lineId == steamId)
                        {
                            return true;
                        }
                    }
                }

                response = await ExecuteRCONAsync( data.config.rconSecondaryAddress, "listplayers" );
                response = response.Trim( );
                if ( response.StartsWith( "0." ) )
                {
                    string[ ] respLines = response.Split( '\n' );
                    foreach ( string l in respLines )
                    {
                        string[ ] splits = l.Split( ' ' );
                        string lineId = splits[ splits.Length - 1 ];

                        if ( lineId == steamId )
                        {
                            return false;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"IsSteamIdOnline Crashed:{ex.ToString( )}" );
            }

            return null;
        }

        /// <summary>
        /// When our target guild becomes available, this event is called.
        /// It will grab the DiscordChannel instances from the library using their snowflake IDs
        /// </summary>
        /// <param name="client">Client this event was triggered for</param>
        /// <param name="args">The event data</param>
        public static async Task OnDiscordGuildAvailableAsync( DiscordClient cl, GuildCreateEventArgs e )
        {
            if ( e.Guild.Id.Equals( data.config.discordGuildId ) )
            {
                guild = e.Guild;
                await GrabDiscordInstancesAsync( );
                running = true;
                Console.WriteLine( "Discord Guild available and chached!" );
            }
        }

        public static Stopwatch rewardPlayersTimer { get; private set; }

        public static DiscordClient discord { get; private set; }
        public static DiscordGuild guild { get; private set; }
        public static DiscordChannel playerCountChannel { get; private set; }
        public static Stopwatch discordModifyStopwatch { get; private set; } = Stopwatch.StartNew( );

        public static bool running { get; private set; } = false;

        public static int lastPlayerCount { get; private set; } = 0;

        public static Data data { get; private set; }

        private static List<ulong> lastOnlinePlayers = new List<ulong>( );
    }
}