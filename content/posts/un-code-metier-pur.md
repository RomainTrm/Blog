---
title: "UN CODE MÉTIER PUR"
date: 2020-02-04T12:46:38+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2020-02-04/"]
---

Il y a quelques jours, au cours d’une discussion, on m’a demandé quelles sont les pratiques que je pousse dans une équipe dans le but d’améliorer la qualité de code. Bon nombre de pratiques comme TDD, clean code ou encore DDD et ses bounded-contexts ayant déjà été cités, j’ai donc répondu&nbsp;: un code métier pur, parfois appelé [functional core](https://thinkbeforecoding.com/post/2018/01/25/functional-core).

Dans cet article, je pars du principe que vous faite une distinction et séparation forte entre le code métier qui répond à une logique business, et le code infra qui répond aux problématiques techniques.

## QUELS INTÉRÊTS&nbsp;?

Un code que l’on peut qualifier de pur a deux caractéristiques&nbsp;:

- Celui-ci retourne toujours le même résultat pour les mêmes entrées. Il ne dépend donc d’aucun état interne ni d’appels à des dépendances (base de données, heure système, etc.)
- Il ne modifie aucun état visible du système.

Les raisons pour lesquelles je pousse ce genre de pratiques sont extrêmement simples. Il m’est très facile de raisonner sur ce code puisque son comportement est à la fois prédictible et répétable.

Il est également très simple de rédiger des tests pour ce genre de code. Vous pouvez donc décrire tous vos cas métiers sous cette forme&nbsp;: “mon système est dans cet état, je lance cette action, alors j’obtiens ce résultat”.

Par exemple, un scénario pour la réservation d’un parking&nbsp;:

- J’ai renseigné mes dates et heures d’arrivée et de départ.
- Je valide ma réservation.
- Ma réservation est acceptée pour les dates.

Si l’on peut parfois considérer les problèmes de charge comme inhérents au métier, on a tout de même envie de les traiter comme des problématiques techniques. Gérer l’accès à un état partagé sur lequel on souhaite écrire se révèle vite complexe (usage de lock, de transactions par exemple) et empêche un code scaler. Nous ne voulons donc pas polluer de la logique métier avec ce genre de problématiques&nbsp;: garder le code pur est une façon simple de s’en assurer.

## LA RAISON D’ÊTRE D’UN LOGICIEL

Cependant, si nous écrivons des logiciels, c’est souvent pour produire ce que nous qualifions jusqu’à maintenant d’effet de bords&nbsp;: écrire en base de données, envoyer un mail, une notification, etc. Nous devons donc être capable de passer d’un code pur à impure et inversement.

Une façon (peut-être simpliste) de voir un logiciel est une succession de transformations de données. Je veux lire une donnée sur mon disque dur (imprédictible), puis la transformer (prédictible) et enfin écrire le résultat sur mon disque (imprédictible).

## COMMENT FAIRE VIVRE LES DEUX&nbsp;?

Nous avons vu jusqu’ici qu’il doit y avoir une distinction claire entre, le code métier que l’on veut pure, et le code infra qui lui est nécessairement impure puisque sa responsabilité est de traiter avec des appels réseaux et système.

Pour faire cohabiter ces deux mondes, il nous faut donc un bout de code dont la seule responsabilité est&nbsp;:

1. De récupérer les données nécessaires à une opération métier.
2. Appeler le code métier.
3. Envoyer le résultat à la couche d’infrastructure.

Répondre à cette problématique de séparation métier/infra est la principale motivation derrière l’[architecture hexagonale](https://medium.com/publicis-sapient-france). Dans cette architecture, nos services portent cette responsabilité&nbsp;:

```Csharp
public class MyService 
{
    private readonly IRepository _repository;

    public MyService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task DoSomething(int id)
    {
        var data = await _repository.Load(id);
        var result = Business.Function(data);
        await _repository.Save(result);
    }
}
```

Avec une architecture CQRS ou CQRS/ES, ce rôle est porté par le commandHandler.

```Csharp
public class MyCommandHandler : ICommandHandler<MyCommand> 
{
    private readonly IRepository _repository;

    public MyCommandHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(MyCommand cmd)
    {
        var aggregate = await _repository.Load(cmd.Id);
        var events = aggregate.RunLogic(cmd.arg1, cmd.arg2);
        await _repository.Save(events);
    }
}
```

Notez que la structure du code reste inchangée, seuls les types changent.

Ce pattern demande une certaine rigueur de la part des développeurs, il est en effet facile d’introduire des effets de bords dans le code métier. Pour cette raison la stratégie adoptée par Haskell consiste à encapsuler les effets de bord dans des `IO`  monade.

Je ne vais pas m’aventurer ici à définir ce qu’est une monade, mais si vous n’êtes pas familier avec ce concept, voici une image très grossière&nbsp;: Une monade est comme une boîte contenant de la donnée, pour manipuler cette donnée, vous devez fournir à la monade la fonction à appliquer. Une liste est par exemple une monade, l’`IO` monade en Haskell représente un effet de bord.

Dans cet exemple, j’ouvre le fichier `input.txt`, j’applique la fonction `toUpperString` puis j’écris le résultat dans le fichier `output.txt`. J’ai fait l’effort ici de décomposer les fonctions afin de voir les signatures.

```Haskell
import Data.Char(toUpper)
import Data.Functor((<&>))

type Lowercase = String
type Uppercase = String

-- Code infra : impure
readInput :: IO Lowercase
readInput = readFile "input.txt"

-- Code métier : pure
toUpperString :: Lowercase -> Uppercase
toUpperString :: map toUpper

-- Code infra : impure
writeOutput :: Uppercase -> IO ()
writeOutput = writeFile "output.txt"

main :: IO ()
main = readInput <&> toUpperString >>= writeOutput
```

La transition du monde de l’`IO` vers du code pur se fait grâce à une fonction appelée `fmap`, ici appelée via l’opérateur `<&>`. `fmap` prend une fonction pure et l’applique un contenu d’une `IO` pour produire une nouvelle `IO`. On obtient ici un `IO Uppercase`.

Enfin, pour écrire le résultat, on applique la fonction `writeOutput` via la méthode `bind` (opérateur `>>=`). `bind` nous permet d’appliquer une fonction retournant une `IO` au contenu d’une `IO`.

## OUT OF THE TAR PIT

Si cet article vu a plu et que vous souhaitez approfondir le sujet, je vous encourage à lire le papier [Out of the Tar Pit](https://curtclifton.net/papers/MoseleyMarks06a.pdf) qui traite de la complexité logiciel, et qui propose un découpage similaire du code. J’ai découvert ce papier grâce à un [talk](https://www.youtube.com/watch?v=lFiB-a3aqbE) explicatif de [Romeu Moura](https://twitter.com/malk_zameth).
