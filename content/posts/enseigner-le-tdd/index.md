---
title: "Enseigner le TDD"
date: 2016-04-27T14:19:48+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2016-04-27/"]
---

Cela fait maintenant un an que je travaille comme prestataire pour un grand groupe. Ma mission y est des plus critique. Elle consiste à développer et maintenir l’ensemble des projets utilisés pour la programmation d’objets connectés nécessaires à l’activité de l’entreprise.

Mon équipe est constituée de plusieurs profils&nbsp;: des développeurs, un product owner ainsi qu’un recetteur. Pour répondre à un fort besoin de qualité, les tests, et notamment les tests unitaires, constituent une composante majeure de notre travail. Les développements se font donc systématiquement avec une approche TDD / BDD. Ces pratiques ont été mises en place par l’équipe qui est complètement libre d’un point de vue opérationnel.

Au cours de cette mission, j’ai à deux reprises animé des ateliers d’initiation au TDD. La première était destinée à notre recetteur qui souhaitait participer au développement de ses outils de test. La seconde concernait un nouveau développeur ayant déjà reçu une courte formation, mais qu’il n’a jamais pu (su&nbsp;?) appliquer par la suite. Ces deux profils étant très différents, les problématiques rencontrées n’ont donc pas été les mêmes, ce qui a rendu l’exercice extrêmement intéressant.

## Certains pré-requis

Après plus d’un an passé à l’appliquer sur tous mes projets, le TDD est devenu une habitude pour moi, l’appliquer me semblait simple. Dans un premier temps, j’ai donc pensé qu’il serait simple de l’inculquer. Que nenni&nbsp;!

Lors de mon premier atelier avec notre recetteur, la principale problématique que j’ai rencontré était liée à des questions de design. En effet, celui-ci n’avait jamais reçu de véritable formation, ses connaissances se résumant à ce qu’il avait appris sur le tas. Comme tout débutant (moi le premier à mes débuts), son code souffrait d’un véritable manque d’organisation, il était fortement couplé. Selon mon expérience, le premier pré-requis consiste donc en un minimum de compétences en terme de conception (le sujet porte à [débat](http://codurance.com/2015/05/12/does-tdd-lead-to-good-design/)), ceci pour que le développeur puisse isoler les différentes responsabilités de son code.

Toujours sur des problématiques de design, nous avons ensuite travaillé sur la façon dont les éléments de notre code interagissent entre eux. Je lui ai donc expliqué l’utilisation de l’injection de dépendance. Cela consiste à expliciter ce que va utiliser le code (une fonction, une classe) en injectant les dépendances nécessaires pour son fonctionnement. De plus, pour réduire le couplage, les dépendances sont abstraites grâce à des interfaces (ou des classes de bases). Cette pratique répond au L, au I et au D de [SOLID](https://fr.wikipedia.org/wiki/SOLID_(informatique)).

```csharp
public class CashRegister
{
    public int ComputePrice(IBasket basket) [...]
}
```

Dans mon exemple, j’explicite que pour calculer un prix, ma classe `CashRegister` a besoin d’un objet qui respecte le contrat d’interface `IBasket`. L’usage d’une interface réduisant le couplage, il est facile d’injecter des comportements spécifiques lors de mes tests sans dépendre d’une fonctionnalité annexe.

Une fois que le développeur maîtrise ces concepts, il lui devient possible d’apprendre à écrire des tests unitaires. Dans le cas contraire, il aura du mal à percevoir le fonctionnement du TDD car il aura beaucoup trop de problématiques annexes à gérer.

## Donner du sens aux tests&nbsp;: une approche métier

Pour l’apprentissage du TDD en lui même, les difficultés ont été plus grandes pour notre nouveau développeur. En effet, ayant plus d’expérience, il lui a été plus difficile de sortir d’une de ses habitudes&nbsp;: essayer d’apporter une réponse technique à un besoin métier. Que l’on ne se méprenne pas, je parle ici d’un problème de méthodologie.

Très concrètement, lors d’un atelier, j’exprime successivement des besoins métiers en aidant le développeur à appliquer le TDD pour les implémenter. Les besoins évoluent de sorte que le code nécessite un refactoring au cours de l’exercice. A ce moment survient un problème récurent&nbsp;: *“Comment vais je coder ça&nbsp;?”*. Le développeur n’est pas capable d’écrire son test si il ne sait comment va être écrit son code. C’est là que j’explique l’importance des concepts métiers dans les tests.

Un avantage des tests unitaires est qu’ils documentent le code. Un test spécifie le comportement qu’il valide. Mais ce test n’a finalement pas de réel valeur si il n’explicite pas l’utilité de ce comportement. Par exemple&nbsp;: `WhenComputePriceThenReturnValue` n’est pas un intitulé très clair. Je calcule un prix, très bien, mais le prix de quoi&nbsp;? Comment est-il calculé&nbsp;?

Le même test avec pour nom `WhenComputePriceThenReturnSumOfArticlesPriceOfBasket` exprime mieux la règle métier testée. Personnellement j’utilise le formalisme proposé par [Sandro Mancuso](https://twitter.com/sandromancuso)&nbsp;: le nom de la classe et de la méthode de test se lisent comme une phrase.

```csharp
[TestFixture]
public class CashRegisterShould
{
    [Test]
    public void ReturnSumOfArticlesPriceWhenComputePriceOfBasket() [...]
}
```

Si votre élève commence à se questionner sur la façon dont il peut implémenter, coupez court et recentrez son attention sur la rédaction d’un test explicite.

Maintenant que le test est écrit, le développeur peut réfléchir à comment implémenter. Le test décrit les éléments nécessaires ainsi que leurs interactions, c’est donc la façon dont le code doit être écrit. Au cours de l’exercice, il est donc important de faire attention aux termes que vous allez employer quand vous formulerez un nouveau besoin.

## Faire passer un cap

Bien entendu, la maîtrise de cette méthodologie ne s’acquière pas en un simple atelier, seule la pratique le permet. Généralement, il arrive un moment où apprendre le TDD peut devenir décourageant pour le développeur. Parce que cela change ses vieilles habitudes. Parce que c’est une façon de réfléchir qui est fatigante, qui n’est pas encore naturelle, ce qui la rend difficile. Il est donc important d’accompagner le développeur jusqu’à ce que celui-ci soit familier avec le TDD. L’essentiel est de le suivre et de rester disponible pour l’aider quand il en ressent le besoin. Pour cela, des revues de son code (et ses tests) ainsi que des séances de pair programming peuvent être des bons moyens pour l’aider à progresser.

Lors de travail en pair programming avec mon équipe, il nous arrive d’appliquer le [ping-pong programming](http://c2.com/cgi/wiki?PairProgrammingPingPongPattern). Nous procédons de la manière suivante&nbsp;: un développeur écrit un test, le second le fait passer puis écrit le test suivant, et ainsi de suite. Cela nous permet de nous challenger et ainsi d’améliorer le niveau de chacun. C’est sans doute la meilleure technique que je peux conseiller pour un travail dans la durée.

## Conclusion

L’apprentissage du TDD est un travail quotidien qui nécessite une implication du développeur et un accompagnement pour débuter.

Merci à mes reviewers [Ouarzy](https://twitter.com/Ouarzy) et [Nadège](https://twitter.com/nadegerouelle).

---

## Commentaires

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
