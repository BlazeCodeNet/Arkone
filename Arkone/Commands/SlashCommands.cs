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
            string respondText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            try
            {
                DataGamer gamer = Program.data.GetGamerByDiscordId( ctx.Member.Id );

                if ( gamer != null )
                {
                    gamer.points = points;
                    Program.data.ApplyGamer( gamer );
                    respondText = $"{gamer.nickname}'s new balance is {gamer.points}!";
                }
                else
                {
                    respondText = $"Gamer doesnt exist :(";
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ respondText }" ) );
        }

        [SlashCommand( "balance", "Gets your Gamer balance." )]
        public async Task BalanceCmd( InteractionContext ctx )
        {
            Console.WriteLine( "Balance CMD Triggered..." );
            string respondText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            try
            {
                DataGamer gamer = Program.data.GetGamerByDiscordId( ctx.Member.Id );

                if ( gamer != null )
                {
                    respondText = $"Your balance is {gamer.points}!";
                }
                else
                {
                    respondText = $"You dont currently have a GamerWallet setup :(";
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ respondText }" ) );
        }
        [SlashCommand( "addgamer", "Adds a gamer to the database." )]
        public async Task AddGamerCmd( InteractionContext ctx, [Option( "user", "Discord Target" )] DiscordUser user, [Option( "steamId", "Target Steam64" )] string steamId )
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
                        steamId = ulong.Parse( steamId ),
                        points = 0,
                        discordId = user.Id,
                        nickname = user.Username,
                    };
                    responseText = $"Added gamer '{gamer.nickname}'({gamer.steamId}) to the database.";
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
    }
}
