# 📨 Protocole

**RediSharp** utilise le protocole [RESP](https://redis.io/docs/latest/develop/reference/protocol-spec/) (REdis Serialization Protocol) pour communiquer avec les clients. Pour ouvrir une connexion avec un serveur, il suffit d'ouvrir une socket TCP et d'envoyer des commandes en suivant le protocole.

## 📝 Format des commandes

Une commande est un tableau RESP de BulkStrings. Le premier élément de l'Array est la commande, et les éléments suivants sont les arguments.

## 📩 Format des réponses

Une réponse est un objet RESP quelconque. Il peut s'agir de :

- SimpleString
- SimpleError
- Integer
- BulkString
- Array
- Map
- Null

> 💡 Pour chaque commande envoyée, le client reçoit une réponse du serveur.
