---
title: "Le piège des tests unitaires"
date: 2020-06-04T21:28:12+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2020-06-04/"]
---

Voilà maintenant plus de 5 ans que j’applique une approche TDD (test-driven development) sur l’ensemble des projets sur lesquels j’interviens. Si j’utilise toujours cette méthode, c’est parce que la présence de tests me donne confiance dans le code que j’écris&nbsp;:

- Je m’assure qu’il fait bien ce que je souhaite.
- J’améliore constamment son design par du refactoring.
- Les tests mettent en lumière la très grande majorité des régressions que je peux introduire lors d’un refactoring ou d’une évolution.
- Je réduis ma charge cognitive et me focalise sur le cas métier que je suis entrain de traiter (les tests s’occupent de vérifier les autres cas pour moi).

Modifier mon code est donc une capacité permise grâce aux tests et que je souhaite conserver tout au long d’un projet&nbsp;: je peux améliorer son design pour le garder constamment adapté aux problèmes métier que je veux résoudre.

Avec le temps, je réalise qu’écrire des tests est une discipline difficile et que certaines pratiques peuvent être dommageables. Les tests unitaires “figent” le code et m’empêchent de le modifier facilement, ce qui peut sembler paradoxal puisqu’ils sont censés au contraire me le permettre.

Dans cet article, je vais essayer de mettre en lumière les raisons de cette dérive. Attention, mon propos ne sera pas focalisé sur la pratique du TDD mais sur les tests unitaires (TU) et la manière dont ils influencent notre capacité à modifier du code.

## Écris une classe, écris une classe de tests

À chaque classe, chaque méthode son ou ses tests&nbsp;: C’est une définition du TU qui semble très répandue, je la rencontre beaucoup au cours de discussions avec d’autres développeurs, sur twitter, dans des articles de blog, etc. C’est même de cette manière que l’on m’a initié au TDD.

Il est vrai que c’est une façon simple d’écrire un test. Le périmètre que l’on souhaite tester est petit, avec un nombre de dépendances normalement raisonnable. C’est une approche vers laquelle on peut facilement se tourner lorsque l’on n’est pas à l’aise avec la rédaction de TU.

Cette pratique est souvent associée à l’injection de dépendance pour fonctionner. C’est à ce moment-là que l’on commence à introduire des mocks, on peut ainsi spécifier le comportement d’une dépendance sans dépendre de sa véritable implémentation. Notre classe/méthode reste donc bien isolée du reste du système lors du test.

Mon point doit peut-être vous déranger&nbsp;: pourquoi vouloir absolument tester en isolement une classe qui de toute façon dépend d’autres classes&nbsp;? Le comportement de la dépendance correspondra-t-il à celui que j’ai spécifié avec mon mock&nbsp;?

Certains vous répondront qu’il vous faut également des tests d’intégrations. Afin de vérifier que les différents éléments du système interagissent de la bonne manière, et que le logiciel retourne le résultat attendu.

## Vous avez déjà perdu

Si vous adoptez cette stratégie, vous allez surement souffrir, ou souffrez déjà, d’une forte adhérence entre votre code et vos tests. En effet, il existe une contrainte forte pour chaque élément de votre système&nbsp;: des tests.

Cela signifie qu’à chaque signature de méthode que l’on veut changer, en plus du code l’appelant, il va falloir corriger les tests et les mocks qui lui sont associés. Nous avons donc perdu notre capacité à facilement modifier/refactorer notre code. J’ai encore le souvenir (douloureux) de journées complètes à “réparer les tests” suite à des modifications.

Ajoutez à cela un biais dont nous souffrons tous&nbsp;: celui des coûts irrécupérables. Alors qu’un test ne fait peut-être plus sens, nous avons tendance (consciemment ou non) à vouloir le conserver et le modifier, ceci uniquement parce qu’il est déjà écrit.

Vous l’avez compris, aligner de manière aussi systématique ses tests avec son implémentation génère un couplage important contre lequel vous luttez à chaque modification que vous souhaitez apporter.

## Du coup, comment fait-on&nbsp;?

Les TU impliquent obligatoirement un couplage avec le code. Même s’il existe des techniques pour le limiter, la première question qu’il faut se poser est&nbsp;: à quoi veut-on se coupler&nbsp;?

De manière caricaturale, si vous travaillez sur des logiciels business, ce que l’on attend de vous est de développer des use cases. Ceux-ci sont des comportements que l’on attend de l’application, et il fait sens de vouloir les tester. Ces use cases peuvent être implémentés en une seule classe, ou en plusieurs. Ce sont des choix de design qui vous reviennent, mais aussi des détails d’implémentation que l’on veut pouvoir facilement changer et qu’un observateur externe du système doit ignorer.

Clairement, la stratégie que l’on vient d’explorer s’attache beaucoup à ces détails d’implémentation. Unitaire ne doit donc pas définir la taille de la portion de code que l’on veut tester.

Je vous propose maintenant la définition que j’ai en tête quand je parle de TU: un test que l’on peut exécuter en isolation et dont le comportement est répétable et constant.

Son résultat ne dépend donc pas du résultat d’autres tests ni de l’état de dépendances externes au système (appel à une web API, à une base de données). Notez que je ne définis pas la taille du périmètre testé.

## Des tests de comportement

Aujourd’hui, j’adopte autant que possible une stratégie qui me permet de conserver ma capacité à modifier le code.

J’écris mes tests de sorte qu’ils dépendent uniquement des contrats entre mon système et le monde extérieur (endpoint REST, base de données, bus de données, etc.). Tout le reste est une boîte noire inaccessible.

Ainsi, un test se présente typiquement de la façon suivante&nbsp;:

1. Je définis l’état du système (ex: données en BDD)
2. Je lance une action métier via une API publique (ex: endpoint REST)
3. Je vérifie le nouvel état de mon système (ex: données en BDD) et les éventuelles interactions avec le monde extérieur (ex: publication d’un message sur un bus)

![Schema explicatif](1.png)

Pour écrire ces tests et les garder indépendants, j’utilise des implémentations “in memory” de mes dépendances externes (typiquement la base de données). Je m’assure également que ces implémentations ont des comportements identiques à celles que j’utilise réellement en production. Pour cela, j’écris quelques tests d’intégrations paramétrés qui testent uniquement les accès au monde extérieur.

![Schema explicatif](2.png)

Cette approche peut paraître extrême, mais elle répond au problème que je souhaite adresser dans cet article.

Je dois tout de même lui reconnaître certaines faiblesses&nbsp;:

- Des boucles de feedbacks plus lentes.
- Des erreurs parfois plus dures à analyser.
- Il reste un couplage à certaines couches applicatives.

## Créer des abstractions

Il est tout de même nécessaire d’aller un peu plus loin pour réduire l’adhérence avec les tests. En effet, si nous avons réduit aux interfaces publiques la surface à laquelle nous sommes couplés, celles-ci deviennent encore plus critiques en cas de modification.

Par expérience, un bon moyen de mesurer le couplage à un contrat est de compter le nombre d’endroits où celui-ci est instancié.

<blockquote class="twitter-tweet"><p lang="en" dir="ltr">Every time I delay the creation of builders to populate data structures in my tests it comes back to bite me in the ass, forcing me to refactor many testes as my data structures evolve. My tests should not rely on their constructors and only care about the data they need.</p>&mdash; Sandro Mancuso (@sandromancuso) <a href="https://twitter.com/sandromancuso/status/1138172904347246592?ref_src=twsrc%5Etfw">June 10, 2019</a></blockquote> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

Builders, générateurs ou encore données statiques, quel que soit la ou les technique(s) utilisée(s), l’objectif reste toujours le même&nbsp;: isoler la création d’une donnée, d’un service, de l’application, de sorte que si sa structure change, il ne faille appliquer ce changement qu’à un seul endroit dans les tests.

Un autre bénéfice de ces méthodes est qu’elles rendent les tests plus clairs, puisque vous n’avez à spécifier que les données qui font sens pour le scénario. Avec le temps, vous arriverez sans doute à faire émerger un DSL pour vos tests, l’ajout de nouveaux use cases en sera facilité voir presque trivial.

```C#
var app = MonApp.Create();

// Given
app.AddHistory(evt1, evt2, evt3);
app.DefineTime(givenTime);

// When
var controller = app.GetSomeController();
var result = controller.ExecuteCommand(arg1, arg2);

// Then
VerifyAssertion(result);
app.VerifyOnHistory(myAssertion);
```

## Pour conclure

Il m’est arrivé plusieurs fois de rencontrer des gens qui ont tenté de mettre en place des tests unitaires ou du TDD sur leur projet, et qui y ont finalement renoncé après quelque temps parce que “ça ne marche pas”. Je pense que le problème que j’ai évoqué au cours de cet article est la principale raison de ces abandons.

Attention également aux dogmes&nbsp;! Si je me montre critique envers les tests unitaires “petite maille”, ceux-ci peuvent se révéler utiles et parfois plus simples qu’un test “boîte noire”. De la même manière, il est parfois plus simple d’utiliser un mock plutôt que de définir l’état du système dans sa globalité. Il est ici question de compromis, de choix qui doivent être faits en connaissance de cause.

Si cet article vous a plu, je vous recommande de regarder la conférence [DevTernity 2017: Ian Cooper – TDD, Where Did It All Go Wrong](https://www.youtube.com/watch?v=EZ05e7EMOLM).

Edit 1&nbsp;: Je vous partage également cet article que l’on m’a montré en réaction à ce post et qui explique sans doute mieux que moi le point que je souhaitais traiter ici.

Edit 2&nbsp;: Deux autres remarques m’ont été faites&nbsp;:

- Certains tests sont déjà naturellement isolés (domaine, hexagone) et ne dépendent pas de détails d’implémentations. Ceux-ci sont aussi viables que la stratégie que je propose dans cet article.
- Si les tests « petite maille » ne sont pas idéals sur le long terme, ils peuvent être très utiles comme « échaffaudage » pour implémenter progressivement un scénario plus vaste. À condition de les supprimer une fois les conditions d’un test « grosse maille » remplies.

---

## Commentaires

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
