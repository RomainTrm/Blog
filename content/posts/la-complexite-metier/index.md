---
title: "LA COMPLEXITÉ MÉTIER"
date: 2017-02-07T12:41:44+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2017-02-07/"]
---

Dans mon [précédent article](/posts/2016-12-20/), j’ai évoqué les raisons pour lesquelles il faut s’orienter ou non vers une architecture de type CQRS. Parmi ces raisons, la première que j’ai évoqué était le niveau de complexité du métier&nbsp;: plus le métier est complexe, plus CQRS devient pertinent.

Seulement, comment définir et évaluer la complexité métier de son application&nbsp;?

## LA COMPLEXITÉ, C’EST QUOI&nbsp;?

*“Complexité, n.f.&nbsp;: Caractère de ce qui est complexe, qui comporte des éléments divers qu’il est difficile de démêler”*&nbsp;: définition proposée par le [Larousse](http://www.larousse.fr/dictionnaires/francais/complexit%C3%A9/17700).

Cette définition met clairement en évidence une première notion, elle implique de fortes dépendances entre plusieurs éléments.

![Chaines](1.png)

J’ai récemment pu assister au talk “[Out The Tar Pit Vulgarized](https://www.youtube.com/watch?v=RugUTW-BPuE)” de [Romeu Moura](https://twitter.com/malk_zameth) où il est justement question de complexité logiciel. Il commence par y définir les termes simple et complexe&nbsp;:

- Simple&nbsp;: Qui n’est pas composé, c’est à dire, qui ne fait l’objet d’aucune dépendance et d’aucune récursivité.
- Complexe&nbsp;: Qui est composé, c’est à dire, qui fait l’objet de dépendances et / ou de récursivités.

Dans le monde de la finance, les [intérêts simples et composés](http://www.mathematiquesfaciles.com/interets-simples-et-composes_2_109876.htm) retranscrivent bien ces notions.

## LE MÉTIER ET SA COMPLEXITÉ

On peut définir le métier d’une application par l’ensemble des règles fonctionnelles qu’elle doit savoir gérer. C’est la partie essentielle d’un logiciel, la raison pour laquelle il est développé. C’est également ces règles qui permettent d’évaluer la complexité métier d’une application&nbsp;: sont-elles composées&nbsp;?

![Tampon "Know The Rules"](2.png)

Cependant il est important de ne pas confondre le métier tel qu’il existe dans la vraie vie avec le métier tel qu’il doit être traité dans l’application. Si vous appliquez le [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design), vous allez vouloir expliciter dans votre code le métier et ses règles, notamment au travers de *bounded contexts* et leurs *ubiquitous language* respectifs. Cette démarche n’a pas pour but de refléter avec exactitude la réalité, au contraire, elle encourage à utiliser une abstraction adaptée au problème que l’on souhaite résoudre.

J’aime beaucoup cette [courte vidéo](https://www.youtube.com/watch?v=iD_Vv3faUcQ&app) de [Scott Millett](https://twitter.com/ScottMillett) qui explique très simplement ce qu’est l’abstraction d’un domain. Dans cet exemple, il montre qu’un plan de métro est une abstraction de la réalité (le réseau) adaptée pour un problème donné&nbsp;: savoir comment se déplacer d’un point A vers un point B.

## UNE AUTRE FORME DE COMPLEXITÉ

La complexité métier ne reflète pas toujours la complexité du code source&nbsp;: l’usage de langages de programmation, de frameworks ainsi qu’un mauvais design ajoutent un niveau de complexité supplémentaire, la complexité accidentelle.

Pour refaire le lien avec CQRS, l’intérêt est d’éliminer une trop forte complexité dans le code en utilisant des modèles de lectures et d’écritures adaptés aux besoins. Ces modèles sont des abstractions qui ne comportent que les éléments nécessaires à l’exécution d’une fonction, d’une règle métier. Leurs niveaux de compositions sont donc réduits à leurs minimums.

Une autre solution pour se protéger contre cette complexité accidentelle est l’[architecture hexagonale](http://blog.xebia.fr/2016/03/16/perennisez-votre-metier-avec-larchitecture-hexagonale/).

## CONCLUSION

La complexité métier est donc l’ensemble des règles métier et leurs dépendances. Plus il existe de dépendances entre ces règles, plus le métier peut être considéré comme étant complexe.

Merci à [Ouarzy](https://twitter.com/Ouarzy) pour ses retours.
