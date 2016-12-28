# DrugBot
DrugWars clone using Microsoft Bot Framework

To deploy this to Azure, you'll need the following app settings:
- MicrosoftAppPassword - this is an app password you get when registering a bot with the Microsoft Bot Framework (https://dev.botframework.com/bots/new)
- MicrosoftAppId - similar to the above, but it's an ID

You'll also need a connection string for the Entity Framework (Code First) `DrugBotDataContext`
