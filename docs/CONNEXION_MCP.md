# Se connecter au serveur MCP — configuration et test

Ce guide explique comment brancher Claude sur `PrevoyanceInsight.McpServer` et comment
vérifier que ça fonctionne. Deux façons de faire, selon ce que vous voulez tester :

| | Serveur distant (Render) | Serveur local (stdio) |
|---|---|---|
| Installation | Aucune | .NET 8 SDK + Postgres/RabbitMQ locaux ou secrets partagés |
| Quand l'utiliser | Tester rapidement, faire une démo | Développer/déboguer le serveur MCP lui-même |
| Transport | HTTP (Streamable HTTP + SSE legacy) | stdio |

## Option A — Serveur déjà déployé (le plus rapide)

Le serveur MCP tourne en continu sur Render (`render.yaml`, service `prevoyanceinsight-mcp`,
`McpServer__Transport=Http`) :

```
https://prevoyanceinsight-mcp.onrender.com
```

Il expose le transport MCP moderne ("Streamable HTTP") à la racine, et l'ancien
transport SSE à `/sse` pour compatibilité avec les clients plus anciens.

**Sur Render (plan gratuit)** : le service se met en veille après une période
d'inactivité. La toute première requête après un réveil peut prendre 30 à 60 secondes —
ce n'est pas un bug, patientez et réessayez.

### Claude.ai ou Claude Desktop (interface graphique)

1. Réglages → **Connecteurs** (Connectors) → **Ajouter un connecteur personnalisé**.
2. Nom : `PrevoyanceInsight`.
3. URL : `https://prevoyanceinsight-mcp.onrender.com`
4. Enregistrer, puis activer le connecteur dans une conversation.

### Claude Code (CLI)

```bash
claude mcp add --transport http prevoyance-insight https://prevoyanceinsight-mcp.onrender.com
```

Vérifier que la connexion est active :

```bash
claude mcp list
```

## Option B — Lancer le serveur en local (stdio)

Utile pour développer ou déboguer le serveur MCP lui-même. Nécessite l'infrastructure
(voir [`README.md`](../README.md)) :

```bash
docker compose up -d postgres ravendb rabbitmq
dotnet ef database update --project src/PrevoyanceInsight.Infrastructure --startup-project src/PrevoyanceInsight.Api
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=prevoyance;Username=prevoyance;Password=prevoyance" --project src/PrevoyanceInsight.McpServer
```

Puis, dans la configuration de Claude Desktop (`claude_desktop_config.json`) :

```json
{
  "mcpServers": {
    "prevoyance-insight": {
      "command": "dotnet",
      "args": ["run", "--project", "src/PrevoyanceInsight.McpServer"],
      "cwd": "<chemin absolu vers le dépôt PrevoyanceInsight>"
    }
  }
}
```

Ou avec Claude Code :

```bash
claude mcp add prevoyance-insight -- dotnet run --project src/PrevoyanceInsight.McpServer
```

Redémarrer Claude Desktop (ou relancer Claude Code) après modification de la config.

## Tester la connexion

Une fois le connecteur actif, cinq outils doivent être disponibles :
`ListerPlans`, `ComparerPlans`, `ObtenirStatistiques`, `DetecterAnomalies`,
`GenererRapport`. Le jeu de données de démo (voir `src/PrevoyanceInsight.Api/SeedData.cs`)
contient deux plans : **Plan Cadres** et **Plan Employés**.

Prompts à copier-coller pour vérifier que chaque outil répond :

```
Liste les plans de prévoyance disponibles.
```

```
Compare le Plan Cadres et le Plan Employés, et dis-moi si l'un des deux est sous surveillance.
```

```
Lance un contrôle de masse sur le Plan Employés.
```

```
Génère un rapport JSON pour le Plan Employés.
```

Si Claude répond avec du texte structuré (et non une erreur d'outil), et qu'un indicateur
du type "outils utilisés" apparaît dans la réponse, la connexion fonctionne.

## Dépannage

- **Pas de réponse / timeout sur la première requête** : cold start Render (option A),
  attendre puis réessayer.
- **Connecteur listé mais aucun outil disponible** : vérifier dans les logs Render du
  service `prevoyanceinsight-mcp` que le process a bien démarré (`McpServer__Transport`
  doit valoir `Http`, sinon le serveur attend sur stdio et n'écoute aucun port).
- **Erreur de connexion à la base en local (option B)** : vérifier que
  `docker compose up -d postgres` tourne et que les user-secrets pointent dessus, pas
  sur la base Render de production.
- **`claude mcp list` ne montre pas le connecteur** : relancer `claude mcp add`, l'URL
  doit être exactement `https://prevoyanceinsight-mcp.onrender.com` (sans `/sse`, sans
  slash final).
