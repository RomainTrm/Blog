---
title: "DO(N’T) REPEAT YOURSELF"
date: 2021-05-26T21:18:59+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2021-05-26/"]
---

## UN BIAIS COGNITIF ET UN USAGE ERRONÉ

Les développeurs aiment bien les acronymes pour énoncer des “bonnes pratiques” (KISS, DRY, SOLID, etc…). Souvent, l’idée véhiculée par ceux-ci est très simple à appréhender.  
Cependant, nous souffrons d’un biais cognitif énorme&nbsp;: plus une information est simple à intégrer, moins elle est remise en question / challengée. Et celle-ci est encore mieux intégrée si elle ne va pas en contradiction avec vos croyances.

DRY en est l’exemple parfait&nbsp;:

- L’idée sous-jacente est simple à comprendre&nbsp;: si on doit appliquer un changement, on veut l’appliquer à un seul endroit.
- DRY est assez connu et fréquemment énoncé pour être consciemment ou non étiqueté comme étant une bonne pratique (et elle l’est).
- Sa mise en application est simple et ne nécessite pas, a priori, d’effort intellectuel particulier&nbsp;: mutualiser la moindre ligne de code dupliquée ou les concepts portant le même nom.

Pourtant, il est fréquent d’observer ceci&nbsp;: en travaillant sur une base de code, nous sommes amenés à gérer des cas métiers de plus en plus variés. Pour ce faire, on applique de plus en plus de conditions pour tester différents cas sur une même structure de données. Ceci peut être un signe qu’il existe un problème d’abstraction.

Le premier enjeu du DRY est une meilleure gestion de la complexité, pourtant en l’appliquant de manière aussi basique/dogmatique, on observe une augmentation de la complexité.

## DON’T REPEAT YOURSELF, DÉFINITION

Si on regarde plus en détail le concept originel&nbsp;: The DRY principle is stated as « Every piece of knowledge must have a single, unambiguous, authoritative representation within a system »  (source [Wikipedia](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)).

Ici, c’est la notion de “peace of knowledge” qui est la plus importante. On peut représenter à un instant T des concepts différents qui peuvent évoluer de manière indépendante dans le temps. Utiliser le même bout de code pour les représenter revient d’une part à les coupler&nbsp;: si un concept doit évoluer, alors il va aussi falloir agir sur l’autre qui est lié. Et d’autre part à créer une ambiguïté entre ces concepts qui rend la compréhension et la maintenance du code plus complexes.

## PLUSIEURS NIVEAUX DE LECTURE

### LE MÊME CODE, MAIS PAS LE MÊME USAGE MÉTIER

On peut écrire deux bouts de code identiques, mais qui ne représentent pas la même chose conceptuellement. Pour les identifier, on veut surtout chercher les raisons pour lesquelles ces bouts de code sont appelés.

### LE MÊME NOM, MAIS PAS LE MÊME CONCEPT MÉTIER

On peut avoir plusieurs concepts portant le même nom, mais qui n’appartiennent pas au même contexte&nbsp;: ils ne représentent pas la même chose. Des processus métier différents, des informations différentes sont des bons signaux pour dire que ce sont des concepts distincts. Pour cela, des ateliers comme l’event storming, sont également de bons outils pour les identifier, et les patterns stratégiques du DDD une bonne approche pour les ségréguer.

### LE MÊME NOM, MAIS PAS LE MÊME USAGE

Dans un même contexte métier, il est encore possible de représenter de différentes manières un même concept. Un exemple typique serait une simple web API qui expose, persiste de l’information, et y applique de la logique métier&nbsp;: on peut avoir un modèle dédié à chacun de ces trois rôles. C’est d’ailleurs la principale motivation derrière des architectures comme l’architecture en couche ou encore une architecture hexagonale&nbsp;: ségréguer par usage technique. Des architectures comme CQRS vont encore plus loin en proposant un modèle d’écriture et des modèles de lecture dédiés. Un même concept peut alors être représenté de plusieurs façons en fonction du cas d’usage.

## MON HEURISTIQUE

Si dans un bout de code je retrouve les symptômes énoncés précédemment (beaucoup de if sur un état); alors mon heuristique est le suivant&nbsp;: je duplique le code qui pose problème et ensuite je supprime les conditions pour faire émerger deux cas distincts.

Quand j’ajoute un nouveau cas, si je le peux, j’évite de prendre une décision tout de suite, car je manque surement de connaissance et de feedbacks sur mon design. Dans ce cas, je duplique l’existant et je l’adapte. Si je me retrouve plus tard à devoir faire des modifications à deux endroits, alors il y a peut-être une opportunité pour mutualiser du code.

## LES BÉNÉFICES DU DO(N’T) REPEAT YOURSELF

En ne systématisant pas la mutualisation du code, on en augmente sa quantité (plus de classes, de méthodes), mais on réduit sa complexité. Un bout de code est utilisé idéalement dans un seul cas métier. Il est donc beaucoup plus simple à appréhender d’un point de vue cognitif, et donc plus simple à modifier puisqu’il ne faut pas se soucier d’autres cas en même temps. Cela réduit également le nombre d’effets de bords et donc de bugs potentiels.

---

## COMMENTAIRES

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
