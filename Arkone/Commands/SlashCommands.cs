using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkone.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand( "balance", "Gets your Gamer balance." )]
        public async Task BalanceCmd( InteractionContext ctx, [Option("user", "Target DiscordUser")] DiscordUser? user = null )
        {
            string respondText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            try
            {
                DataGamer gamer;
                if ( user == null )
                {
                    gamer = Program.data.GetGamerByDiscordId( ctx.Member.Id );
                }
                else
                {
                    gamer = Program.data.GetGamerByDiscordId( user.Id );
                }

                if ( gamer != null )
                {
                    if ( user == null )
                    {
                        respondText = $"Your balance is {gamer.points}!";
                    }
                    else
                    {
                        respondText = $"{user.Mention} 's balance is {gamer.points}!";
                    }
                }
                else
                {
                    if ( user == null )
                    {
                        respondText = $"You don't currently have a GameR Account.";
                    }
                    else
                    {
                        respondText = $"{user.Mention} doesn't have a GameR Account.";
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ respondText }" ) );
        }
        [SlashCommand( "addgamer", "Adds a gamer to the database." )]
        public async Task AddGamerCmd( InteractionContext ctx, [Option( "user", "Discord Target" )] DiscordUser user, [Option( "steamId", "Target Steam64" )] string steamId, [Option("arkPlayerId", "Ark Player Id")] string arkPlayerId, [Option("nickname", "Nickname")] string nick )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            string responseText = "__NULL__";
            if ( !Provider.IsMasterUserAsync(ctx.Member).GetAwaiter().GetResult() )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    DataGamer gamer = new DataGamer
                    {
                        steamId = steamId,
                        points = 25,
                        discordId = user.Id.ToString(),
                        arkPlayerId = arkPlayerId,
                        nickname = nick,
                    };
                    responseText = $"Added GameR {user.Mention} ;steam:{gamer.steamId};ark:{gamer.arkPlayerId};nick:{gamer.nickname} .";
                    Program.data.ApplyGamer( gamer );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Error completing /addgamer:" + ex.ToString( ) );
                    responseText = $"Internal error occured. Please try again later.";
                }
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( responseText ) );
        }
        [SlashCommand( "removegamer", "Modifies a GameR account" )]
        public async Task RemoveGamerCmd( InteractionContext ctx, [Option( "user", "Discord User" )] DiscordUser user)
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            string responseText = "__NULL__";
            if ( !Provider.IsMasterUserAsync( ctx.Member ).GetAwaiter( ).GetResult( ) )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    DataGamer gamer = Program.data.GetGamerByDiscordId( user.Id );
                    if ( gamer != null )
                    {
                        Program.data.RemoveGamer( gamer );

                        responseText = $"Deleted GameR account for {user.Mention}";
                    }
                    else
                    {
                        responseText = $"That user doesn't have a GameR account.";
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Error completing /removegamer:" + ex.ToString( ) );
                    responseText = $"Internal error occured. Please try again later.";
                }
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( responseText ) );
        }
        [SlashCommand("modifygamer", "Modifies a GameR account")]
        public async Task ModifyGamerCmd(InteractionContext ctx, [Option("user", "Discord User")] DiscordUser user, [Option("enum", "ModType")]ModifyGamerOptions modType, [Option("newValue", "New Value")] string newValue)
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            string responseText = "__NULL__";
            if ( !Provider.IsMasterUserAsync( ctx.Member ).GetAwaiter( ).GetResult( ) )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    DataGamer gamer = Program.data.GetGamerByDiscordId(user.Id);
                    if ( gamer != null )
                    {
                        switch( modType )
                        {
                            case ModifyGamerOptions.steamId:
                                gamer.steamId = newValue;
                                break;
                            case ModifyGamerOptions.arkPlayerId:
                                gamer.arkPlayerId = newValue;
                                break;
                            case ModifyGamerOptions.points:
                                gamer.points = long.Parse(newValue);
                                break;
                            case ModifyGamerOptions.nickname:
                                gamer.nickname = newValue;
                                break;
                        }

                        Program.data.ApplyGamer(gamer);
                        responseText = $"Modifed value for user {user.Mention}";
                    }
                    else
                    {
                        responseText = $"That user doesn't have a GameR account.";
                    }

                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Error completing /modifygamer:" + ex.ToString( ) );
                    responseText = $"Internal error occured. Please try again later.";
                }
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( responseText ) );
        }

        [SlashCommand( "steamprofile", "Get Discord user's Steam profile URL" )]
        public async Task GetSteamProfile( InteractionContext ctx, [Option( "user", "Discord Target" )] DiscordUser user )
        {
            string respondText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            try
            {
                DataGamer gamer;
                gamer = Program.data.GetGamerByDiscordId( ctx.Member.Id );

                if ( gamer != null )
                {
                    respondText = $"{user.Mention} 's steam account is ' http://steamcommunity.com/profiles/{gamer.steamId} '!";
                }
                else
                {
                    respondText = $"{user.Mention} doesn't have a GameR Account.";
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ respondText }" ) );
        }

        public enum ModifyGamerOptions
        {
            [ChoiceName("Steam Id")]
            steamId,
            [ChoiceName("Ark Player Id")]
            arkPlayerId,
            [ChoiceName("Points")]
            points,
            [ChoiceName("Nickname")]
            nickname,
        }
    }
}
