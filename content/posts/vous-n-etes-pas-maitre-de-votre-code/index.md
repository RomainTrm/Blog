---
title: "Vous n’êtes pas maître de votre code"
date: 2017-05-24T13:30:19+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2017-05-24/"]
---

J’ai récemment pu participer à un atelier animé par [Romeu Mourra](https://twitter.com/malk_zameth) lors des [NCrafts](https://twitter.com/ncraftsConf). Pas de technique ici, le but était de mettre en lumière des problèmes d’ordres systémiques. Pour cela, nous avons fait un [Kebab Kata](https://github.com/ldez/kebab-kata) sous forme d’itérations aux-cours desquelles Romeu jouait le rôle du client, puis également de l’architecte. Son but était de nous faire échouer en usant de différents comportements toxiques que l’on retrouve fréquemment dans de vraies missions.

<blockquote class="twitter-tweet"><p lang="in" dir="ltr"><a href="https://twitter.com/hashtag/nCrafts?src=hash&amp;ref_src=twsrc%5Etfw">#nCrafts</a> doing the kebab kata <a href="https://t.co/GpWZM03T0T">pic.twitter.com/GpWZM03T0T</a></p>&mdash; @romeu@mastodon.social (@malk_zameth) <a href="https://twitter.com/malk_zameth/status/865131834673434624?ref_src=twsrc%5Etfw">May 18, 2017</a></blockquote> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

## Objectif rempli

L’atelier s’est déroulé de la façon suivante&nbsp;:

- à chaque itération le client donne un périmètre et un budget (du temps) pour le réaliser.
- pendant les itérations, le client répond aux sollicitations des équipes et va voir spontanément en tentant de les influencer.
- à la fin de chaque itération, le client attend une démonstration.
- pendant qu’une équipe réalise sa démonstration, les autres équipes ont le droit de “tricher” en continuant à coder.
- une courte revue de code auto-organisée avec les autres équipes est mise en place après les démonstrations, il est interdit de coder durant cette période.
- au bout de trois sprints, nous sommes “virés” puis recrutés en tant que nouvelle équipe. Il nous faut alors réaliser un audit et énoncer des actions à prendre sur le code.
- un architecte, appuyé par le client, apporte alors des directives de conception en parallèle de notre audit.

Comme prévu, nous avons tous échoué&nbsp;: à l’issue de l’atelier, toutes les équipes ont considéré leur code comme étant du legacy.

## Des responsabilités partagées

<blockquote class="twitter-tweet"><p lang="en" dir="ltr">Good ways to fail your project with <a href="https://twitter.com/malk_zameth?ref_src=twsrc%5Etfw">@malk_zameth</a> , as expected we failed ! <a href="https://twitter.com/hashtag/NCrafts?src=hash&amp;ref_src=twsrc%5Etfw">#NCrafts</a> <a href="https://t.co/qP12zIqDg2">pic.twitter.com/qP12zIqDg2</a></p>&mdash; Romain Berthon (@RomainTrm) <a href="https://twitter.com/RomainTrm/status/865158052818952192?ref_src=twsrc%5Etfw">May 18, 2017</a></blockquote> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

Il en ressort clairement que les développeurs ne sont pas les seuls responsables de la qualité finale du code. De façon synthétique et non exhaustive&nbsp;:

La contrainte la plus évidente est le temps sur un périmètre donné&nbsp;: les délais sont très courts et incitent à prendre des “raccourcis” comme ne pas utiliser de tests unitaires. Le client n’hésite pas à demander s’ils sont nécessaires. Il écoute les rares équipes qui tentent de négocier les délais mais cela n’aboutit à rien d’autre qu’à un moyen pour les développeurs d’exprimer leur frustration.

Les demandes du client ne sont pas claires ni priorisées&nbsp;: “si vous avez le temps, j’aimerais aussi cette feature”. Ce comportement ne fournit aucune visibilité à l’équipe, elle ne connaît pas la finalité du logiciel, ni le véritable besoin.

Tout comme le droit de “tricher” pendant les démonstrations, ce manque de visibilité incite les développeurs à constamment coder pour rattraper leur retard, ce qui a plusieurs effets pervers&nbsp;:

- Aucun temps n’est alors accordé à la prise de recul, à la remise en question du code et de sa conception&nbsp;: l’équipe est constamment maintenue occupée au détriment de la qualité.
- Le client n’a pas besoin d’exercer la moindre forme de management, les développeurs sont livrés à eux mêmes et subissent la situation.
- L’équipe ne communique pas avec les autres pour échanger sur les solutions possibles. De plus, les temps accordés aux revues de codes sont inutiles car bien trop courts (et désorganisés) pour être constructifs&nbsp;: il n’est pas possible de faire émerger de réels axes d’amélioration

Enfin, les équipes subissent des pressions sur leurs choix techniques. Le client fait part des retours fait par l’équipe front end (dont l’existence n’avait d’ailleurs jamais été évoquée avant&nbsp;!) et des difficultés qu’elle rencontre lors de l’intégration. L’architecte impose, appuyé par le client qui “le paie très chère”, une architecture basée sur le *design pattern composit*. Il s’avère que cette solution répond bien au problème de conception de ce kata, mais ne reflète pas du tout la façon dont le métier du client peut évoluer, ce qui rend toute évolution encore plus coûteuse.

## Prisonniers et gardiens d’un système

Avec tous ces éléments, les développeurs se sentent isolés puisque considérés comme de simples exécutants de décisions qu’ils ne comprennent pas et pour lesquelles ils n’ont pas été consultés. Il n’existe aucune confiance entre l’équipe de développement et ses interlocuteurs.

[Romeu](https://twitter.com/malk_zameth) décrit le système dans lequel sont pris les développeurs comme étant un [panoptique](https://fr.wikipedia.org/wiki/Panoptique). La majeure partie des comportements observés lors de l’exercice peuvent être associés à trois piliers qu’il a identifiés&nbsp;:

- le manque / l’absence de communication entre les équipes
- la bonne visibilité du management sur les équipes
- l’opacité du management pour les équipes

Une fois pris dans un tel système, les développeurs ont l’impression d’être constamment surveillés et ne se sentent plus libres de leurs manières de travailler. Ils s’imposent alors un mode de fonctionnement qu’ils finissent, à terme, par trouver normal. Malgré des lacunes plus ou moins évidentes de sa part, le système n’est alors plus remis en question.

C’est ainsi que ces mêmes développeurs peuvent se montrer hostiles à l’introduction de nouvelles pratiques comme le TDD ou le pair programming. Parce que cela ne leurs semble pas concevable et qu’ils craignent que le système rejette cela.

## Tenter et innover

Bien que fréquents, les comportements évoqués plus haut ne sont pas adoptés pour sciemment nuire au projet. Il est tout de même important de savoir les identifier, les remettre en cause et initier des changements de méthode, de comportement.

Parmi les pratiques à mettre en place, [Romeu](https://twitter.com/malk_zameth) proposait les suivantes&nbsp;:

- Le [mob programming](/posts/a-whole-team-approach/) pour rassembler les gens, les pousser à communiquer, comprendre ce qu’ils développent et pourquoi cela est nécessaire.
- Supprimer la double contrainte temps / périmètre en appliquant notamment le [no estimate](https://blog.goood.pro/2014/07/25/developper-sans-faire-destimation-le-mouvement-noestimates/). Un comportement qui peut être adopté serait de dire “Ok, je te livrerai uniquement ce qui sera prêt à cette date là” tout en ayant une vision claire des prioritées métier. Ce discours est parfaitement entendable contrairement à ce que l’on a tendance à penser.
- Ne plus travailler à flux tendu&nbsp;: une équipe de développement est souvent perçue comme une source de coût, encore plus si elle n’est pas occupée. Les managers et clients cherchent donc à constamment les alimenter en tâches. Il est important de dégager du temps pour des activités annexes&nbsp;: refactoring, automatisation, veille technique, etc… Aujourd’hui, de plus en plus d’entreprises ont un jour par semaine dédié à ces activités.

Essayer de convaincre les gens avant de tenter quoi que ce soit est généralement un effort vain. Il ne faut donc pas avoir peur de prendre des initiatives, les résultats sont souvent plus parlant que les débats.

Merci à [Ouarzy](https://twitter.com/Ouarzy) et Léna pour leurs retours, merci à [Romeu](https://twitter.com/malk_zameth) pour cet atelier très instructif.

---

## Commentaires

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
