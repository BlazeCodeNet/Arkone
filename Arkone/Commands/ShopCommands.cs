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
    public class ShopCommands : ApplicationCommandModule
    {

        [SlashCommand( "buy", "Triggers a shop display to be created." )]
        public async Task ShopBuyCmd( InteractionContext ctx, [Option( "item", "Item Name" )] ShopBuyType item )
        {
            string responseText = "__NULL__";
            _ = ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource);
            try
            {
                DataGamer gamer = Program.data.GetGamerByDiscordId(ctx.Member.Id);

                if ( gamer != null )
                {
                    bool? isOnPrimary = await Program.IsSteamIdOnline( gamer.steamId );
                    if(!isOnPrimary.HasValue)
                    {
                        // Not on a server
                        responseText = $"You must be logged into a ARK Server to use this command!";
                    }
                    else
                    {
                        string curServAddr = isOnPrimary.Value ? Program.data.config.rconPrimaryAddress : Program.data.config.rconSecondaryAddress;
                        responseText = "Purchase Complete! Check your players inventory.";

                        bool didBuy = false;
                        if (item == ShopBuyType.vulture)
                        {
                            if(gamer.points >= 100)
                            {
                                didBuy = true;
                                await Program.ExecuteRCONAsync( curServAddr, $"scriptcommand spawndino_ds {gamer.steamId} /Game/ScorchedEarth/Dinos/Vulture/Vulture_Character_BP.Vulture_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                gamer.points -= 100;
                                responseText = $"Vulture purchase complete.";
                            }
                        }
                        else if(item == ShopBuyType.sinomacrops)
                        {
                            if(gamer.points >= 150)
                            {
                                didBuy = true;
                                await Program.ExecuteRCONAsync( curServAddr, $"scriptcommand spawndino_ds {gamer.steamId} /Game/LostIsland/Dinos/Sinomacrops/Sinomacrops_Character_BP.Sinomacrops_Character_BP_C 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                gamer.points -= 150;
                                responseText = $"Sinomacrops purchase complete.";
                            }
                        }
                        else if ( item == ShopBuyType.otter)
                        {
                            if(gamer.points >= 250)
                            {
                                didBuy = true;
                                await Program.ExecuteRCONAsync( curServAddr, $"scriptcommand spawndino_ds {gamer.steamId} /Game/PrimalEarth/Dinos/Otter/Otter_Character_BP.Otter_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                gamer.points -= 250;
                                responseText = $"Otter purchase complete.";
                            }
                        }
                        else if ( item == ShopBuyType.ferox)
                        {
                            if(gamer.points >=350)
                            {
                                didBuy = true;
                                await Program.ExecuteRCONAsync( curServAddr, $"scriptcommand spawndino_ds {gamer.steamId} /Game/Genesis/Dinos/Shapeshifter/Shapeshifter_Small/Shapeshifter_Small_Character_BP.Shapeshifter_Small_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                gamer.points -= 350;
                                responseText = $"Ferox purchase complete.";
                            }
                        }
                        else if ( item == ShopBuyType.flaregun)
                        {
                            if(gamer.points >= 50)
                            {
                                didBuy = true;
                                await Program.ExecuteRCONAsync( curServAddr, $"giveitemtoplayer {gamer.arkPlayerId} \"Blueprint'/Game/Mods/LethalReusable/FlareGun_LR.FlareGun_LR'\" 1 0 0" );
                                gamer.points -= 50;
                                responseText = $"Flaregun purchase complete.";
                            }
                        }
                        if(didBuy)
                        {
                            Program.data.ApplyGamer( gamer );
                        }
                    }
                }
                else
                {
                    responseText = $"You must be a GameR to use this command!";
                }

                
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
            }
            _ = ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }
        [SlashCommand( "shopdisplay", "Triggers a shop display to be created." )]
        public async Task ShopDisplayCmd( InteractionContext ctx, [Option( "channel", "Target Channel" )] DiscordChannel channel, [Option( "enum", "Type of shop" )] ShopDisplayType shopType )
        {
            DiscordEmbed embed = null;
            string responseText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            if ( !Provider.IsMasterUserAsync( ctx.Member ).GetAwaiter( ).GetResult( ) )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    responseText = "__EMBED__";
                    embed = new DiscordEmbedBuilder( ).
                        WithTitle( "__ARK GameR Points Shop__" ).
                        WithDescription( "ARK Points Shop" ).
                        AddField( "*Shoulder Creatures*", "Vulture - 100p\nSinomacrops - 150p\nOtter - 250p\nFerox - 350p", true ).
                        AddField( "*Other*", "Flaregun - 50p\n- \n- \n- ", true ).
                        WithColor( DiscordColor.Orange ).
                        WithFooter( "Type \"/buy NAME\" to purchase a item with GameR points!" );
                    _ = channel.SendMessageAsync( embed );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
                }
            }
            if ( embed == null )
            {
                _ = ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
            }
            else
            {
                _ = ctx.EditResponseAsync( new DiscordWebhookBuilder().AddEmbed( embed ) );
            }
        }

        [SlashCommand( "clearbotchat", "Clears chat of all bot-sent messages" )]
        public async Task ClearBotChatCmd( InteractionContext ctx, [Option( "channel", "Target Channel" )] DiscordChannel channel )
        {
            string responseText = "__NULL__";
            _ = ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            if ( !Provider.IsMasterUserAsync( ctx.Member ).GetAwaiter( ).GetResult( ) )
            {
                responseText = $"Insufficient Permissions.";
            }
            else
            {
                try
                {
                    IReadOnlyList<DiscordMessage> msgList = await channel.GetMessagesAsync( 10 );
                    foreach ( DiscordMessage msg in msgList )
                    {
                        if(msg.Author.IsBot)
                        {
                            _ = msg.DeleteAsync( );
                        }
                    }

                    responseText = $"Deleted all bot messages in this channel.";
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
                }
            }
            _ = ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }

        public enum ShopDisplayType
        {
            [ChoiceName( "ARK GameR Shop" )]
            arkShop,
        }
        public enum ShopBuyType
        {
            [ChoiceName( "Vulture" )]
            vulture,
            [ChoiceName( "Sinomacrops" )]
            sinomacrops,
            [ChoiceName( "Otter" )]
            otter,
            [ChoiceName( "Ferox" )]
            ferox,
            [ChoiceName( "Flare Gun" )]
            flaregun,
        }
    }
}
