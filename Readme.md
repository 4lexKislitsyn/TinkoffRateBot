# About this project

This is simple [telegram bot](http://t.me/TinkoffRateBot) for [Tinkoff Bank USD/RUB rate](https://www.tinkoff.ru/about/exchange/) notifications.

# Env variables

- `TinkoffRateBot:Token` - telegram bot token;
- `AWS_ACCESS_KEY_ID` - access key id for AWS ;
- `AWS_SECRET_ACCESS_KEY` - access key's secret for AWS;
- `BasicAuthHandler:Users:<N>:Name` - name of N user; it's used for sending custom message to clients;
- `BasicAuthHandler:Users:<N>:Password` - password of N user;

# Docker build and startup

```
docker build https://github.com/4lexKislitsyn/TinkoffRateBot.git#<tag or branch>:TinkoffRateBot -t tinkoff-rate-bot:latest -t tinkoff-rate-bot:v<version number>
docker run --detach --restart always --name <image-name> tinkoff-rate-bot -p <port>:80 -p <SSL port>:443
```

You can use env-file to provide environment variables with similar content:

```
TinkoffRateBot:Token="token"
AWS_ACCESS_KEY_ID="access_key_id"
AWS_SECRET_ACCESS_KEY="access_key"
BasicAuthHandler:Users:0:Name="admin"
BasicAuthHandler:Users:0:Password="admin"
```
and use [env-file parameter](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file) of docker run command: `--env-file ./env.list`

Optionally you can add `AWS_REGION` variable.