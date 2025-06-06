---
title: "Le code, c’est mieux à deux"
date: 2016-06-01T13:52:46+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2016-06-01/"]
---

Parmi toutes les bonnes pratiques mises en place dans mon équipe actuelle, il y en a une que j’apprécie tout particulièrement&nbsp;: le pair programming. Nous l’utilisons pour plusieurs raisons&nbsp;:

- traiter des tâches complexes
- faciliter les montées en compétence métier et technique des différents membres
- augmenter la qualité du code

Je suis réellement convaincu par les avantages et les bénéfices qu’une équipe peut tirer du pair programming. Cependant, je constate que cette méthode n’est pas toujours bien comprise. J’entends régulièrement le même discours&nbsp;: si cette méthode marche “sur le papier”, elle ne semble pas “applicable en entreprise”.

## Qu’est-ce que c’est&nbsp;?

Le pair programming est l’une des [pratiques](http://www.extremeprogramming.org/rules.html) portées par l’Extreme Programming, son principe est simple&nbsp;: deux développeurs travaillent en binôme sur la même tâche. On observe deux rôles&nbsp;:

- Le *driver* qui écrit le code.
- Le *navigator / observer*, il aide le driver en lui suggérant des solutions et en vérifiant le code au fil de l’implémentation.

Bien entendu, ces rôles sont régulièrement échangés. La règle d’or lors de ces échanges est&nbsp;: *“déplacez le clavier, pas la chaise”*. C’est pourquoi il est important que l’écran soit placé entre les deux développeurs pour faciliter les interactions et les échanges.

## Une impression de “gaspillage”

Une crainte récurrente chez toutes les personnes qui n’ont jamais travaillé en pair programming est la perte de productivité&nbsp;: *“Il y en a un qui écrit pendant que l’autre regarde.”*, sous entendu celui qui n’écrit pas est passif.

![Homer Simpson affalé entrain de boire une bière](1.png)

En vérité, si vous comparez la charge consommée par une personne et la charge consommée par un binôme, en effet vous consommerez plus. Il a ainsi été déterminé que des développeurs consomment environ [15% de charge supplémentaire](http://collaboration.csc.ncsu.edu/laurie/Papers/XPSardinia.PDF) lorsqu’ils pratiquent le pair programming, pourtant les bénéfices dépassent ce surcoût.

Pour calculer ce surcoût vous allez vous baser la grande majorité du temps sur des chiffrages, des estimations (Avez-vous vraiment fait deux fois le travail pour comparer&nbsp;?). Selon mon expérience, les chiffrages sont par définition faux&nbsp;: ils se font généralement avec une vision idéaliste. Ils partent du principe que le besoin métier sera parfaitement compris et que la dette technique sera nulle. Ces deux facteurs pourtant majeurs sont généralement mal voire pas du tout pris en compte.

Ensuite, je trouve que s’appuyer uniquement sur ce 15% révèle une vision du projet à très court terme. Je pense qu’une charge de travail ne peut pas uniquement prendre en compte le temps de réalisation, il faudrait y ajouter le temps passé à corriger les anomalies ainsi que le coût de la dette technique engendrée.

Pour moi, il est donc beaucoup plus intéressant de comparer la qualité du code produit avec ces deux méthodes.

## Faire les choses, mais les faire bien

Il ne faut pas oublier que ce sont bien deux personnes qui travaillent sur la même tâche, même s’il n’écrit pas, le *navigator* n’est absolument pas passif. La réflexion est partagée, les échanges sont dynamiques. Le fait d’échanger permet d’explorer plus facilement tous les aspects d’un problème. Cela a pour effet de dégager une compréhension claire du besoin et une solution appropriée de manière beaucoup plus efficace, tout en éludant plus rapidement les incompréhensions et les fausses routes. De plus, le *navigator* n’ayant pas à se concentrer sur l’écriture du code, il peut plus facilement prendre du recul et apporter un regard critique sur l’implémentation.

Le pair programming permet donc de proposer des solutions plus rapidement qu’un développeur seul. Il permet également d’améliorer la qualité sous deux aspects.

Tout d’abord, une quantité d’anomalies moindre. Une grande force du pair programming réside dans la revue continue. Quand vous écrivez du code, malgré toute votre bonne volonté, il n’est pas rare que vous fassiez des erreurs. Si la probabilité qu’une erreur vous échappe existe, la probabilité qu’elle échappe également à votre binôme est beaucoup plus réduite, elle sera ainsi signalée et corrigée immédiatement.

La qualité se retrouve également dans le design du code. Travailler à deux permet encore une fois de confronter sa compréhension du problème. Une solution technique qui peut sembler évidente pour une personne ne le sera peut être pas pour son partenaire, parce qu’il ne la comprend pas, ou parce qu’elle ne lui semble pas être la plus adaptée. Le pair programming fait émerger un meilleur design dans le code en confrontant les opinions et les expériences des deux développeurs.

![La croisée de deux idées donne une meilleure idée](2.png)

Un autre avantage que je trouve au pair programming, c’est qu’il impose une grande rigueur dans le vocabulaire que vous utilisez. Employer une mauvaise notion conduit souvent à une incompréhension entre les deux membres. Comme il est important d’éliminer ces incompréhensions, vous allez attacher de l’importance à utiliser le terme, la métaphore la plus juste possible. Une fois la notion claire, explicitez la dans le code (réutilisez le terme employé à l’oral), vous dégagerez ainsi une forme d’*ubiquitous langage*. Cela rend votre travail plus simple à comprendre, et permet également à des personnes sans bagage technique de lire votre code. Ainsi, il nous arrive parfois de faire du pair programming avec notre product owner, il est parfaitement capable de comprendre ce que nous écrivons et de nous corriger si nécessaire.

Le pair programming permet donc de réaliser des tâches de manière plus rapide qu’un développeur seul, tout en garantissant un nombre d’anomalies plus réduit. On observe des gains à court terme avec moins de corrections à apporter suite aux développements. Le code étant plus propre, il est plus facilement maintenable et évolutif. Les gains se font également sur le long terme grâce à une dette technique réduite.

## Certains comportements à éviter

Comme pour n’importe quelle méthode, il faut faire attention à certains détails lors de la mise en pratique du pair programming. Ici, le facteur humain et la bonne communication sont les deux clés pour assurer l’efficience de votre travail.

Quand vous travaillez en binôme, il faut faire attention au comportement de chacun. Si l’un des deux développeurs est passif, qu’il parle peu ou qu’il n’ose pas proposer des solutions, alors vous avez un problème. Le pair programming peut parfois générer de la défiance vis à vis de son partenaire. Il ne faut pas oublier de rester humble et de faire attention à son comportement.

Concrètement, travailler de cette manière va donner à votre binôme une vision claire de vos compétences, de votre façon de travailler. Si vous craignez de les exposer, c’est que vous avez probablement des choses à améliorer. Voyez le pair programming comme une excellente occasion pour progresser et vous ouvrir à de nouvelles pratiques, la séance n’en sera que plus profitable pour vous.

Nous avons tous une expérience différente, il faut en être conscient et veiller à ne pas s’enfermer dans une réflexion du type *“il est trop fort, il est trop nul pour moi”*. Tout d’abord parce que ce raisonnement ne se concentre que sur des compétences techniques purs. Comme je l’évoquais plus haut, la connaissance métier est essentielle pour répondre à un besoin. Si une personne est effectivement plus expérimentée, cela ne lui garanti pas d’avoir la meilleure compréhension du besoin, ni la meilleure solution technique pour y répondre. Personnellement, je travaille avec deux autres développeurs beaucoup plus expérimentés que moi, pour autant, je n’ai pas peur de proposer des solutions. Certaines sont retenues, d’autres non. L’essentiel est que toutes les propositions alimentent notre réflexion&nbsp;: *Est-ce une bonne solution&nbsp;? Pourquoi&nbsp;?*

![More the Knowlegde, lesser the ego; Lesser the Knowledge, more the ego... - Albert Einstein](3.png)

Un autre comportement à éviter, et qu’il m’est déjà arrivé de rencontrer (à ma grande surprise ce jour là), est un esprit de compétition. Concrètement, mon collègue refusait ma solution qu’il jugeait pourtant comme étant bonne parce qu’elle ne venait pas de lui, nous avons perdu beaucoup de temps dans des débats qui étaient inutiles. Gardez à l’esprit que le pair programming vous fait travailler ensemble, pas l’un contre l’autre. Si un développeur se sent obligé de revendiquer le travail réalisé, alors il ne semble clairement pas fait pour travailler en binôme, ni en équipe. Lui imposer le pair programming ne sera absolument pas bénéfique pour lui, son partenaire ainsi que pour le projet.

## Gagner en confiance

Quand je développe seul, il m’arrive régulièrement de me demander après coup *“Est-ce que j’ai bien géré ce cas&nbsp;?”*, la grande majorité du temps oui, mais je me sens obligé de vérifier. Avec le temps, je constate que je suis beaucoup plus serein après une séance de pair programming. Mon argument est toujours le même&nbsp;: si quelque chose m’a échappé, il n’a probablement pas échappé à mon partenaire. Si effectivement je réalise que nous avons oublié un cas, j’en serai certain car je n’aurai aucun souvenir d’avoir échangé autour de son implémentation (Quel test écrire&nbsp;? Quel design adopter&nbsp;? …).

Avoir une telle confiance en ses collègues ne se fait pas du jour au lendemain, mais au fil des séances. Parce que vous apprenez comment l’autre réfléchit et procède, et parce que vous constatez qu’il est capable de déceler vous erreurs. Une fois cette confiance installée, vous redécouvrez le vrai sens du mot “équipe”&nbsp;: des gens avec qui vous collaborez et sur qui vous pouvez compter.

Merci à mes reviewers [Ouarzy](https://twitter.com/Ouarzy) et [Nadège](https://twitter.com/nadegerouelle).

---

## Commentaires

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
