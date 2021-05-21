# CommunityBot

**DockerHub:** https://hub.docker.com/repository/docker/mrmodest/communitybot

## Description

Simple telegram bot which was created for internal closed community.

New bot command can be implemented by inhering base class for this: [`BotCommandHandlerBase`](/CommunityBot/Handlers/BotCommands/BotCommandHandlerBase.cs). Then you need override [`Config`](/CommunityBot/Handlers/BotCommands/BotCommandConfig.cs) for set command name and using flags. After you must write the handle logic and return result which will be interpreted to send response if needed. 

If you need more complex handle logic than simple bot command, you can inherit [`UpdateHandlerBase`](/CommunityBot/Handlers/UpdateHandlerBase.cs). For this you need override `AllowedUpdates`, `CanHandle` and similarly implement your handle logic.

BotClient's sending methonds calling in [`BotService`](/CommunityBot/Services/BotService.cs) in method `SendResult` based on [`IUpdateHandlerResult`](/CommunityBot/Handlers/Results) (which returned by update handlers).
