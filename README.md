# **RediSharp**

**RediSharp** est une base de donnée clé-valeur en mémoire, à la manière de [Redis](https://github.com/redis/redis), mais écrite en C#.

## Démarrage rapide

### Dépendances

Il faut le [SDK .NET 8.0](https://dotnet.microsoft.com/download) pour compiler et exécuter **RediSharp**.

> Vous pouvez vérifier si le SDK est installé avec la commande `dotnet --version`.

### Compilation

Après avoir cloné le dépôt, vous pouvez compiler **RediSharp** avec la commande suivante:

```bash
dotnet build -c Release
```

### Exécution

Pour démarrer le serveur **RediSharp**, utilisez la commande :

```bash
dotnet run -c Release --project src/Server/Server.csproj
```

Et pour lancer un client, utilisez la commande :

```bash
dotnet run -c Release --project src/Client/Client.csproj
```

### Exemple

```bash
> set user:32.name "Paul Dupont"
OK
> get user:32.name
"Paul Dupont"
```

## Utiliser Docker

Vous pouvez aussi lancer le serveur **RediSharp** avec [Docker](https://www.docker.com/). Pour cela, utilisez la commande suivante :

```bash
docker compose up --build -d
```

## Prochaines étapes

- [Commandes supportées](docs/COMMANDS.md)
- [Arguments de la ligne de commande](docs/ARGUMENTS.md)
- [Documentation du protocole](docs/PROTOCOL.md)
