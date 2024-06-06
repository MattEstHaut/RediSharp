# 📦 Commandes

Tous les mots-clés suivants sont insensibles à la casse.

> 💡 Toutes les commandes sont atomiques.
>
> 💡 La plupart des commandes ne renvoient pas d'erreur. Sauf lorsque la structure de la commande est incorrecte.

<!-- tableau Commande, Description -->
| Commande | Description |
| --- | --- |
| `PING` | Répond "PONG". |
| `ECHO message` | Renvoie le message. |
| `SET key value` | Définit la valeur associée à la clé. |
| `SET key value EX time` | Définit la valeur associée à la clé avec une expiration en millisecondes. |
| `GET key` | Récupère la valeur associée à la clé. |
| `DEL key` | Supprime la clé. |
| `LOCK key` | Verrouille la clé de manière atomique. Renvoie une erreur si la clé est déjà verrouillée. |
| `UNLOCK key` | Déverrouille la clé. |
| `TTL key` | Récupère le temps restant avant l'expiration de la clé. |
| `APPEND key value` | Ajoute `value` à la fin de la valeur associée à la clé. |
| `POP key n` | Récupère et supprime `n` caractères au début de la valeur associée à la clé. |
| `TAIL key n` | Récupère et supprime les `n` derniers caractères de la valeur associée à la clé. |

## 📚 Documentation détaillée

### `SET`

```bash
SET key value
SET key value EX time
```

Définit la valeur associée à la clé. Si la clé existe déjà, sa valeur est remplacée.
`EX` est un argument optionnel qui définit une expiration en millisecondes, après laquelle la clé est supprimée.
Cette commande ne renvoie pas d'erreur.

> 💡 Il est recommandé de définir une expiration à chaque fois que cela est possible afin d'éviter les fuites de mémoire.

### `GET`

```bash
GET key
```

Récupère la valeur associée à la clé. Si la clé n'existe pas ou qu'elle est expirée, la réponse est `null` (Un Null RESP). Cette commande ne renvoie pas d'erreur.

### `DEL`

```bash
DEL key
```

Supprime la clé. Si la clé n'existe pas, la commande ne fait rien. Cette commande ne renvoie pas d'erreur.

### `LOCK` et `UNLOCK`

```bash
LOCK key
UNLOCK key
```

`LOCK` verrouille la clé de manière atomique. Si la clé est déjà verrouillée, la commande renvoie une erreur. `UNLOCK` déverrouille la clé. Si la clé n'est pas verrouillée, la commande ne fait rien. Ces commandes permettent de gérer des accès concurrents à une ou des clés. `UNLOCK` ne renvoie pas d'erreur.

> 💡 Les verrous sont automatiquement libérés après `UNLOCK`.

#### Exemple

```bash
> LOCK user:32
OK
> GET user:32.age
"42"
# 42 + 1 = 43
> SET user:32.age 43
OK
> UNLOCK user:32
OK
```

Ici, le verrou sur la clé `user:32` permet de s'assurer que la valeur de `user:32.age` est incrémentée de manière atomique, à la condition que les autres clients respectent le verrou.

### `TTL`

```bash
TTL key
```

Récupère le temps restant avant l'expiration de la clé en millisecondes. Si la clé n'a pas d'expiration, ou si elle n'existe pas/plus, la réponse est `null` (Un Null RESP). Cette commande ne renvoie pas d'erreur.

### `APPEND`

```bash
APPEND key value
```

Ajoute `value` à la fin de la valeur associée à la clé. Si la clé n'existe pas, elle est créée avec la valeur `value`. Cette commande conserve l'expiration de la clé si elle existe. Cette commande ne renvoie pas d'erreur.

### `POP`

```bash
POP key n
```

Récupère et supprime `n` caractères au début de la valeur associée à la clé. Si la clé n'existe pas, ou si la valeur est vide, la réponse est `null` (Un Null RESP). Cette commande ne renvoie pas d'erreur.

### `TAIL`

```bash
TAIL key n
```

Récupère et supprime les `n` derniers caractères de la valeur associée à la clé. Si la clé n'existe pas, ou si la valeur est vide, la réponse est `null` (Un Null RESP). Cette commande ne renvoie pas d'erreur.
