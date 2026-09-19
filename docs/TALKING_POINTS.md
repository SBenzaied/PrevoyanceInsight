# Points à porter en entretien — PrévoyanceInsight

Un pense-bête court, pas un script à réciter. L'objectif est de rester capable de
répondre à "pourquoi ce choix ?" sur chaque brique.

## En une phrase

"J'ai construit un démonstrateur qui répond directement à vos deux objectifs : une
architecture CQRS partagée entre une API REST classique et un serveur MCP, pour que
l'actuariat puisse aussi bien consulter un dashboard que demander une comparaison de
plans en langage naturel à un agent IA."

## Si on demande "pourquoi un serveur MCP et pas juste un chatbot par-dessus l'API ?"

Un chatbot classique doit être re-câblé à chaque nouvelle capacité. Un serveur MCP
expose des outils typés, découvrables par n'importe quel agent compatible (Claude
Desktop, Claude Code, ou un futur outil interne) sans coupler l'agent à un protocole
maison. Et comme les outils MCP appellent la même couche `Application` que l'API REST,
il n'y a pas de logique dupliquée à maintenir en double.

## Si on demande "pourquoi PostgreSQL ET RavenDB, pas un seul ?"

Parce que les deux types de données n'ont pas les mêmes contraintes : les taux et
effectifs sont structurés et interrogés par agrégation → relationnel. Les règlements de
plans sont des documents longs, versionnés, interrogés par contenu → document store.
Utiliser RavenDB pour forcer des taux dans des colonnes JSON, ou PostgreSQL pour un texte
réglementaire de 40 pages avec ses versions, serait le mauvais outil dans les deux sens.

## Si on demande "et la performance ?"

Renvoyer vers le cas concret chez Medimaps : optimisation de requêtes LINQ ayant divisé
par 4 le temps de traitement, avec une démarche reproductible (profilage, réécriture,
mesure). Le point commun avec ce projet : `AsNoTracking()` sur les requêtes de lecture,
et un léger travail lorsque nécessaire ; mais surtout la même **méthode**.

## Si on demande "vous n'avez jamais travaillé avec RabbitMQ/PostgreSQL/CQRS avant ?"

Assumer honnêtement : c'est un projet construit pour apprendre vite et démontrer une
capacité de montée en compétence rapide sur une stack proche de celle maîtrisée (C#,
.NET, architecture propre), pas une revendication de 5 ans d'expérience dessus. C'est
justement ce que la démarche "j'ai construit ça avant l'entretien" est censée montrer.

## Limites à assumer sans détour

- Le code n'a pas été compilé/testé dans l'environnement de rédaction (pas d'accès
  réseau pour restaurer les paquets NuGet) — c'est un scaffold structurant et
  idiomatique, pas un livrable validé en CI. Le dire clairement évite toute
  surprise si on ouvre le repo en direct.
- L'authentification, le multi-tenant, l'observabilité ne sont pas traités : hors
  périmètre volontaire d'un démonstrateur d'architecture.
