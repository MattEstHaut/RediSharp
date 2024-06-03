# ⚙️ Arguments de la ligne de commande

## Serveur

- `--port <port>` : Port sur lequel le serveur écoute. (Par défaut: 6379)
- `--file <file>` : Fichier lié à la base de données. (Par défaut: aucun)
- `--save-interval <interval>` : Intervalle de sauvegarde de la base de données en millisecondes. (Par défaut: 5 minutes)

> ⚠️ Sans le paramètre `--file`, la base de données ne sera pas sauvegardée.

## Client

- `--host <host>` : Adresse du serveur. (Par défaut: localhost)
- `--port <port>` : Port du serveur. (Par défaut: 6379)
