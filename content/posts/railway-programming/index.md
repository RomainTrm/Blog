---
title: "Railway Programming : La voie vers un code plus honnête"
date: 2023-01-27T18:00:00+01:00
tags: [talk, fr]
draft: false
---

<style>
  img {
    width: 15em
  }
</style>

![Dessin d'une locomotive sur des rails]({{< relref "1.png" >}})

Après l'avoir mis en pratique ensemble au cours de l'une de nos missions, [Sylvain COUDERT](https://www.linkedin.com/in/sylvain-coudert-punkindev/) et moi-même avons souhaité vous présenter le Railway Programming.

## Abstract

Le code auquel nous sommes pour la plupart confronté.e.s comporte beaucoup d’informations implicites (erreurs, exceptions, nullables, …). Celles-ci se montrent problématiques car elles nécessitent des connaissances poussées de la codebase et génèrent une charge cognitive supplémentaire. Malheureusement, tout ceci augmente la probabilité que les développeur.euse.s fassent des erreurs.
Au cours de ce talk, exemples à l’appui, nous vous montrerons comment rendre plus explicites ces comportements cachés à l’aide de votre système de types. Nous mettrons ensuite en lumière les problématiques qui en découlent et comment les résoudre.

Au cours de ce talk, nous allons présenter :

- les informations qui peuvent exister de façon implicite dans une codebase
- comment rendre ces comportements plus explicites avec des types (Option<>, Result<>, etc)
- highlight des problématiques de composition générés par ces types
- exemple de composition similaire et mieux maîtrisée par les développeur.euse.s : les listes avec les map, flatmap, etc
- Railway programming : transposition de ces concepts sur le Option<> et le Result<>
- bénéfices et inconvénients : quand l’utiliser et quand ne pas l’utiliser (il nous semble important de souligner que ça n’est pas une silver bullet)
- Par défaut, nos langages de prédilection sont C# & F#, nous pouvons tout de même envisager d’autres langages pour nos exemples si les organisateurs pensent que cela serait plus adapté à l'audience.

## Les sessions

- Devfest Dijon - décembre 2022 (pas de captation)
- SnowCamp - janvier 2023 (pas de captation)

## Code et slides

Vous pouvez retrouver les slides de cet talk au format pdf : [slides](railway-programming-slides.pdf).  
Le code de ce talk est accessible sur ce [repo GitHub](https://github.com/RomainTrm/Talk-RailwayProgramming).

---

## Commentaires

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
