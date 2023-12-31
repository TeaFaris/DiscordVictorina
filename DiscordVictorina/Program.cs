﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordVictorina;
using DiscordVictorina.Configuration;
using DiscordVictorina.Handlers;
using DiscordVictorina.Services.SlashCommandsRegisterServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OGA.AppSettings.Writeable.JSONConfig;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
	.AddWriteableJsonFile("appsettings.json");

#if DEBUG
builder.Configuration
	.AddUserSecrets(typeof(Program).Assembly);
#endif

builder.Services
	.ConfigureWritable<BotConfiguration>(builder.Configuration.GetSection("Bot"));

var discordConfig = new DiscordSocketConfig
{
	GatewayIntents = GatewayIntents.All,
	HandlerTimeout = Timeout.Infinite,
	AlwaysDownloadUsers = true
};
var interactionServiceConfig = new InteractionServiceConfig { DefaultRunMode = Discord.Interactions.RunMode.Async };

builder.Services.AddSingleton(provider => provider);

// Hosted services
builder.Services.AddHostedService<ApplicationStart>();

// Configurations
builder.Services.AddSingleton(discordConfig);
builder.Services.AddSingleton(interactionServiceConfig);

// Services
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<InteractionService>();
builder.Services.AddSingleton<SlashCommandsRegisterService>();

////Handlers
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddSingleton<SlashCommandHandler>();

var app = builder.Build();

await app.StartAsync();
await app.WaitForShutdownAsync();