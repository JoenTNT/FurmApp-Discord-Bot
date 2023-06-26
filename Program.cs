/*
The link to invite the bot
https://discord.com/api/oauth2/authorize?client_id=1119162007690686474&permissions=28033184955511&scope=bot%20applications.commands
*/

using FurmAppDBot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
