docker run -p 10445:443 -p 80:80 -v /home/me/app/db:/app/db --name community-tg-bot-container --env-file env.dev.list community-tg-bot
