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

        [SlashCommand( "setbalance", "Sets TargetUser's balance." )]
        public async Task SetBalanceCmd( InteractionContext ctx, [Option( "user", "Discord Target" )] DiscordUser user, [Option( "points", "New Amount" )] long points )
        {
            string responseText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            if ( ctx.Member.Roles.Any( x => x.Name == "Master" || x.Id.Equals( Program.data.config.ownerDiscordId ) ) )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    DataGamer gamer = Program.data.GetGamerByDiscordId( ctx.Member.Id );

                    if ( gamer != null )
                    {
                        gamer.points = points;
                        Program.data.ApplyGamer( gamer );
                        responseText = $"New balance is {gamer.points}!";
                    }
                    else
                    {
                        responseText = $"GameR doesnt exist :(";
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
                }
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }

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
        public async Task AddGamerCmd( InteractionContext ctx, [Option( "user", "Discord Target" )] DiscordUser user, [Option( "steamId", "Target Steam64" )] string steamId, [Option("arkPlayerId", "Ark Player Id")] string arkPlayerId )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            string responseText = "__NULL__";
            if ( ctx.Member.Roles.Any( x => x.Name == "Master" || x.Id.Equals( Program.data.config.ownerDiscordId ) ) )
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
                        points = 50,
                        discordId = user.Id.ToString(),
                        arkPlayerId = arkPlayerId,
                    };
                    responseText = $"Added gamer {user.Mention} ({gamer.steamId})[{gamer.arkPlayerId}] to the database.";
                    Program.data.ApplyGamer( gamer );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Error completing !addgamer:" + ex.ToString( ) );
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
    }
}
