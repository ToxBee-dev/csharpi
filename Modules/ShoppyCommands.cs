using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using csharpi.Services;

namespace csharpi.Modules
{
    public class ShoppyCommands :  ModuleBase<SocketCommandContext>
    {

       private readonly IConfiguration _config;
       
       public ShoppyCommands(IServiceProvider services)
        {
            // we can pass in the db context via depedency injection
            _config = services.GetRequiredService<IConfiguration>();

        }


        // Configuration Laden
        Shoppy ShoppyAPI = new Shoppy("p3YPKT70ufqsuyRa6drd5sjMnJV8uYMLhRM1sX4MbKzMqfvSjn");  // ShoppyAPI laden mit dem API key              GRCcUpPThLXRP9aAcR2Hxwp6MEwp9MgYoRqqCtEG9r9SeB6aFf
        ulong ShoppyChannelToListen = 982915864338763776;           // In welchem Channel soll der Shoppy Bot lauschen                          => Support Ticket
        ulong LogChannelID          = 985989040081891348;           // ChannelID wohin die Log Meldungen gehen sollen.                          => Botlogger
        bool LogChannelStatus       = true;                         // Sollen Status meldungen zum Aktivieren von einer Lizenz erstellt werden?
        // Klassen Rollen
        ulong MemberRogue           = 985989907661066260;
        ulong MemberWarrior         = 985990061218734111;
        ulong MemberDruid           = 985990158920867841;
        ulong MemberRolle           = 961740042081034260;           // Diese Rolle bekommt der Kunde fürs aktivieren der Bestellung

        
        // Eine ShoppyID abfragen
        [Command("shoppy getID")]
        public async Task ShoppyGetID(string ShoppyID){

            // Probiere die ShoppyID zu finden
            try{
                var Orders = JsonConvert.DeserializeObject<Shoppy.Order>(ShoppyAPI.GetOrder(ShoppyID));

            
                var embedBot = new EmbedBuilder()
                    .WithTitle("ShoppyID Status (BOT)")
                    .AddField("ShoppyID: ", Orders.id)
                    .AddField("Rotation: ", Orders.product.title)
                    .AddField("Paid at: ", Orders.paid_at)
                    .WithColor(new Color(Color.Green))
                    .WithCurrentTimestamp();


                await ReplyAsync(null, false, embedBot.Build());

            }catch{
               await ReplyAsync("Die ShoppyID konnte in der Datenbank nicht gefunden werden.");
            }
            
        }
       
        // Lizense Aktivieren
        [Command("shoppy")]
        public async Task ActivateAccount(string orderID)
        {
            _ = Task.Run(async () =>
            {
                // Prüfe ob es im richtigem Channel gepostet wurde
                if(Context.Channel.Id == ShoppyChannelToListen)
                {
                    // Prüfe ob die OrderID in Shoppy vorhanden ist
                    try
                    {
                        var Orders = JsonConvert.DeserializeObject<Shoppy.Order>(ShoppyAPI.GetOrder(orderID));

                        // Prüfe ob die Bestellung bereits bezahlt wurde
                        if (Orders.id == orderID && Orders.confirmations == 1)
                        {
                            // Prüfe ob die ShoppyID bereits aktiviert wurde
                            if (true)
                            {
                                // Dem Benutzer die Rolle geben
                                var user = Context.Guild.GetUser(Context.User.Id);
                                var role = user.Guild.Roles.FirstOrDefault(x => x.Id == MemberRolle);

                                // Prüfe welche Rotation gekauft wurde und gebe nur die Rolle dem User ------ OFFEN
                                await (user as SocketGuildUser).AddRoleAsync(role);

                                var NachrichtenID = await Context.Message.Channel.SendMessageAsync(Context.User.Mention +  "Thank you for your support. Your role has been changed to " + role.Name);
                                await Context.Message.DeleteAsync();    // Nachricht vom User löschen
                                await Task.Delay(2000); // 2s warten
                                await Context.Channel.DeleteMessageAsync(NachrichtenID.Id); // Nachricht vom bot löschen

                                // Eine Nachricht in den LogChannel schreiben
                                if (LogChannelStatus)
                                {
                                    await LogNachricht(LogChannelID, Orders.id, Orders.product.title, Orders.paid_at);
                                }
                            }
                            else{

                                var NachrichtenID = await Context.Message.Channel.SendMessageAsync("This ShoppyID has already been activated");
                                await Context.Message.DeleteAsync();    // Nachricht vom User löschen
                                await Task.Delay(2000); // 2s warten
                                await Context.Channel.DeleteMessageAsync(NachrichtenID.Id); // Nachricht vom bot löschen
                            
                            }
                        }
                        else
                        {
                            var NachrichtenID = await Context.Message.Channel.SendMessageAsync("Your payment is pending, please try again later");
                            await Context.Message.DeleteAsync();    // Nachricht vom User löschen
                            await Task.Delay(2000); // 2s warten
                            await Context.Channel.DeleteMessageAsync(NachrichtenID.Id); // Nachricht vom bot löschen
                        }
                    }
                    catch (Exception ex)
                    {
                        // ShoppyID nicht bekannt, gib einen Fehler aus
                        var NachrichtenID = await Context.Channel.SendMessageAsync("We don't know this ShoppyID, please contact the admin or try again.");
                        Console.WriteLine("Die OrderID: " + orderID + " konnte nicht gefunden werden. " +ex.Message);

                        await Context.Message.DeleteAsync();    // Nachricht vom User löschen
                        await Task.Delay(2000); // 2s warten
                        await Context.Channel.DeleteMessageAsync(NachrichtenID.Id); // Nachricht vom bot löschen
                    }
                }
                else
                {
                    // Channel ermitteln in dem eigentlich aktiviert werden soll
                    var channel = Context.Guild.GetChannel(ShoppyChannelToListen);
                    // Einen fehler ausgeben
                    var NachrichtenID = await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " To activate your license please use the following channel " + channel.Name + ".");

                    // Bot Nachricht nach einer Zeit wieder löschen
                    await Context.Message.DeleteAsync();    // Nachricht vom User löschen
                    await Task.Delay(3000); // 3s warten
                    await Context.Channel.DeleteMessageAsync(NachrichtenID.Id); // Nachricht vom bot löschen
                }
            });
        }
        
        // Eine Rank von einem User ändern
        [Command("ChangeRolle")]
        public async Task ChangeRolle(IGuildUser user)
        {
            //message.Channel.SendMessageAsync("Hallo " + message.Author.Mention);

            //SocketGuildUser user = message.Author as SocketGuildUser;

            //await user.AddRoleAsync(961740042081034260);        // Eine Rolle geben
            //await user.RemoveRoleAsync(961740042081034260);     // Eine Rolle wieder weg nehmen

            //var user = Context.User;
            var role = user.Guild.Roles.FirstOrDefault(x => x.Name == "Warrior");

            await Context.Channel.SendMessageAsync("Hallo " + Context.Message.Author.Username + " deine Rolle wurde geändert.");
            await (user as SocketGuildUser).AddRoleAsync(role);
        }

        // In einem bestimmten Channel eine Nachricht schicken
        [Command("TalkInChannel")]
        public async Task TalkInChannel(IMessageChannel cch, [Remainder] string repeat)
        {
            await Context.Message.DeleteAsync();
            await cch.SendMessageAsync(repeat);
        }

        public async Task LogNachricht(ulong channelID, string ShoppyID, string RotationsTitel, object PaidAt)
        {
            var logchannel = Context.Guild.GetChannel(channelID) as SocketTextChannel;
            
            var embedBot = new EmbedBuilder()
                .WithTitle("Account activation (BOT)")
                .AddField("User: ", Context.Message.Author.Mention)
                .AddField("ShoppyID: ", ShoppyID)
                .AddField("Rotation: ", RotationsTitel)
                .AddField("Paid at: ", PaidAt)
                .WithColor(new Color(Color.Green))
                .WithCurrentTimestamp();

            await logchannel.SendMessageAsync(null, false, embedBot.Build());

        }
    }
}
