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

        public enum ShopDisplayType
        {
            [ChoiceName("ARK GameR Points")]
            arkPoints,
        }
        public enum ShopBuyType
        {
            [ChoiceName("Vulture")]
            vulture,
            [ChoiceName("Sinomacrops")]
            sinomacrops,
            [ChoiceName( "Otter" )]
            otter,
            [ChoiceName( "Ferox" )]
            ferox,
            [ChoiceName( "Mantis" )]
            mantis,
            [ChoiceName( "Fiber" )]
            fiber,
            [ChoiceName( "Paste" )]
            paste,
            [ChoiceName("Flare Gun")]
            flaregun,
        }
        [SlashCommand( "buy", "Triggers a shop display to be created." )]
        public async Task ShopBuyCmd( InteractionContext ctx, [Option( "item", "Item Name" )] ShopBuyType item )
        {
            string responseText = "__NULL__";
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource);
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
                        switch ( item )
                        {
                            case ShopBuyType.vulture:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat scriptcommand spawndino_ds {gamer.steamId} /Game/ScorchedEarth/Dinos/Vulture/Vulture_Character_BP.Vulture_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                break;
                            case ShopBuyType.ferox:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat scriptcommand spawndino_ds {gamer.steamId} /Game/Genesis/Dinos/Shapeshifter/Shapeshifter_Small/Shapeshifter_Small_Character_BP.Shapeshifter_Small_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                break;
                            case ShopBuyType.otter:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat scriptcommand spawndino_ds {gamer.steamId} /Game/PrimalEarth/Dinos/Otter/Otter_Character_BP.Otter_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                break;
                            case ShopBuyType.sinomacrops:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat scriptcommand spawndino_ds {gamer.steamId} /Game/LostIsland/Dinos/Sinomacrops/Sinomacrops_Character_BP.Sinomacrops_Character_BP_C 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                break;
                            case ShopBuyType.mantis:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat scriptcommand spawndino_ds {gamer.steamId} /Game/ScorchedEarth/Dinos/Mantis/Mantis_Character_BP.Mantis_Character_BP 220 0 0 0 1 ? 1 0 1 1 1 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? Shop_Creature Remember_what_you_bought?" );
                                break;
                            case ShopBuyType.paste:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat giveitemtoplayer {gamer.arkPlayerId} \"Blueprint'/Game/PrimalEarth/CoreBlueprints/Resources/PrimalItemResource_ChitinPaste.PrimalItemResource_ChitinPaste'\" 1000 0 0" );
                                break;
                            case ShopBuyType.flaregun:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat giveitemtoplayer {gamer.arkPlayerId} \"Blueprint'/Game/Mods/LethalReusable/FlareGun_LR.FlareGun_LR'\" 1 0 0" );
                                break;
                            case ShopBuyType.fiber:
                                await Program.ExecuteRCONAsync( curServAddr, $"admincheat giveitemtoplayer {gamer.arkPlayerId} \"Blueprint'/Game/PrimalEarth/CoreBlueprints/Resources/PrimalItemResource_Fibers.PrimalItemResource_Fibers'\" 1000 0 0" );
                                break;
                            default:
                                responseText = $"Invalid ShopBuyType! Please contact Stimz.";
                                break;
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
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }
        [SlashCommand( "shopdisplay", "Triggers a shop display to be created." )]
        public async Task ShopDisplayCmd( InteractionContext ctx, [Option( "channel", "Target Channel" )] DiscordChannel channel, [Option( "enum", "Type of shop" )] ShopDisplayType shopType )
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
                    DiscordEmbed embed = new DiscordEmbedBuilder( ).
                        WithTitle( "__ARK GameR Points Shop__" ).
                        WithDescription( "ARK Points Shop" ).
                        AddField( "*Shoulder Creatures*", "Vulture - 25p\nSinomacrops - 25p\nOtter - 50p\nFerox - 100p", true ).
                        AddField( "*Other Creatures*", "Mantis - 200p\n-\n-\n-", true ).
                        AddField( " ", " ", false ).
                        AddField( "*Resources*", "Fiber(1000) - 20\nBio Toxin(100) - 25p\nCementing paste(1000) - 40p\nSilica Pearls(300) - 30p\nElement(5) - 100p", true ).
                        WithColor( DiscordColor.Orange ).
                        WithFooter( "Type ``/buy NAME`` to purchase a item with GameR points!" );
                    _ = channel.SendMessageAsync( embed );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "SlashCommands#BalanceCommand crashed:" + ex.ToString( ) );
                }
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }

        [SlashCommand( "clearbotchat", "Clears chat of all bot-sent messages" )]
        public async Task ClearBotChatCmd( InteractionContext ctx, [Option( "channel", "Target Channel" )] DiscordChannel channel )
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
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ responseText }" ) );
        }
    }
}
