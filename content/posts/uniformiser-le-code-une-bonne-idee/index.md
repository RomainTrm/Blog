---
title: "UNIFORMISER LE CODE, BONNE IDÉE ?"
date: 2021-09-01T21:03:32+02:00
tags: [post, fr]
draft: false
aliases: ["/posts/2021-09-01/"]
---

Au cours de ma carrière, j’ai été confronté à plusieurs reprises à des clients et/ou des acteurs internes qui souhaitent assurer un haut niveau d’uniformisation sur le code des applications. Cela se traduit par des conventions de style, de nommage, et parfois même des choix plus impactant comme des frameworks ou des architectures spécifiques. Dans les cas extrêmes, les propos de ces personnes laissaient entendre le refus d’un quelconque écart avec ces règles.

De mon expérience, la motivation derrière cette démarche est toujours la même&nbsp;: permettre à un développeur de facilement passer d’une application à une autre. En effet, les développeurs sont encore aujourd’hui des ressources rares et chères au regard du marché, et la plupart des entreprises n’ont pas les moyens d’attirer et d’en employer un grand nombre. Il s’agit donc ici pour l’entreprise de maximiser l’usage des ressources limitées qu’elle a à sa disposition. Cependant, cela implique d’appliquer ces conventions à l’échelle d’un sous-ensemble d’applications, voire même à l’ensemble des applications d’une entreprise.

Si ces motivations me paraissent tout à fait légitimes et raisonnables, je ne suis pas totalement en accord avec la solution envisagée, tout du moins sous ses formes extrêmes. Je vais tenter ici d’apporter ma réflexion sur le sujet.

## DISCLAIMER

Dans ce billet je vais raisonner à une échelle macro&nbsp;: un ensemble d’applications ou une seule application regroupant différents contextes métiers. Au sein d’un périmètre restreint (un contexte métier), il me semble en effet important de préserver un minimum de cohérence dans le code. À noter également que je ne remets pas en cause ici l’intérêt d’uniformiser la partie infrastructure et configuration de vos systèmes. Si par exemple vous travaillez avec Microsoft Azure ou Amazon AWS, vous voudrez sans doute maintenir une façon cohérente de vous interfacer avec ces plateformes.

## UN MÊME SI, MAIS DIFFÉRENTS CONTEXTES

Comme je l’ai déjà évoqué, le vrai bénéfice d’uniformiser le code est de permettre au développeur de rester en terrain connu. Il peut rapidement trouver ses marques comme la structure technique de l’application est déjà connue. Le plus gros de l’effort se focalise sur la compréhension de l’application, de son métier.

Hors, si l’on s’intéresse aux différents métiers d’une entreprise / d’une application, on se rend compte qu’ils ne sont pas homogènes en termes de valeur produite, de complexité, d’usage, de volumétrie des données, et même pourquoi pas de charge de travail. Certains métiers vont également être dépendants d’autres métiers pour produire de la valeur. Un outil comme les [Wardley Map](https://learnwardleymapping.com/) permet aujourd’hui de bien mettre en lumière ces dépendances, mais aussi les différences de maturité et de spécificité business des composants d’un SI.

La sphère DDD nous propose une ségrégation par Bounded Context pour structurer une application. En fonction du rôle (Core, Support, Generic) du domain associé, les solutions techniques proposées ne sont pas les mêmes (cf: [Domain-Driven Design Distilled](https://www.goodreads.com/book/show/28602719-domain-driven-design-distilled), Types of subdomains, p.46)&nbsp;:

- Core&nbsp;: métier très spécifique à l’entreprise, code avec des efforts de modélisation et développement importants.
- Support&nbsp;: métier plus simple et supportant le Core, code avec des efforts moindres.
- Generic&nbsp;: intégration et usage de systèmes tiers ou outsourcés.

De mon expérience, et de façon grossière, une application business s’articule souvent de cette manière&nbsp;: un métier complexe, dépendant de métiers périphériques (avec souvent des métiers de “paramétrage”), et qui vient parfois interagir avec des systèmes tiers, par exemple un CRM.

L’usage de conventions de code uniformisées peut faire sens dans le cas des supporting domains. Une simple architecture de type CRUD peut par exemple se révéler suffisante pour répondre à ces besoins. Cependant cela me semble inadapté aux core domain (par les contraintes métiers fortes) ainsi qu’aux generic domain (contraint par les systèmes tiers). Porter l’uniformisation du code à ces contextes augmente la complexité et fait tendre vers des solutions sous optimales.

## NOS LIMITES COGNITIVES

Nous sommes tous contraints par les limites de nos capacités cognitives, ce qui correspond à l’ensemble des informations que l’on est capable de traiter à un instant donné. Il faut garder en tête que toutes les applications ne représentent pas la même charge cognitive pour un développeur, et que plus il a d’applications à gérer, plus sa charge cognitive est importante. Uniformiser le code permet dans une certaine mesure de réduire cette charge et d’accélérer la transition vers une autre application.

Mais même avec cela, un trop grand nombre d’applications reste problématique&nbsp;:

- Trop d’informations à garder en tête pour être réellement efficace.
- Un temps et de l’énergie conséquents perdus lors de nombreux “context switching”.

Pour lutter efficacement contre cette limite, on peut envisager d’autres stratégies qui visent à limiter le nombre d’applications dont un développeur peut être responsable.

Si l’on en revient aux Wardley Maps, on peut déjà simplement se demander&nbsp;: “À quoi sert cette application&nbsp;? Est-elle vraiment essentielle pour mon business&nbsp;?” Posez-vous sérieusement ces questions sans les évacuer d’un revers de manche. Simplifiez autant que possible, car nous avons tendance à surcomplexifier les systèmes que nous produisons, et aussi à apporter des solutions à des problèmes qui ne devraient pas exister.

<blockquote class="twitter-tweet"><p lang="en" dir="ltr">You may not be interested by rocketry, but Elon gives us here a lot of heuristics for designing efficient systems. It matches perfectly well with software development.<a href="https://t.co/wjmePPe6VA">https://t.co/wjmePPe6VA</a></p>&mdash; Romain Berthon (@RomainTrm) <a href="https://twitter.com/RomainTrm/status/1424013279479877635?ref_src=twsrc%5Etfw">August 7, 2021</a></blockquote> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

Une autre stratégie (peut-être moins recommandable) est d’accepter que certaines applications ne soient plus maintenues par quiconque pendant un temps. Bien entendu, vous ne pouvez pas faire ça avec toutes vos applications, ici ce sont les contextes generic et supporting qui peuvent être concernés. Il est tout de même important d’identifier la fréquence des changements ainsi que les futures évolutions nécessaires avant de laisser vivre une application.

Parfois, le besoin d’uniformiser le code est exprimé par les développeurs eux-mêmes. De mon expérience, cette demande vient toujours de profils transverses à qui on demande d’intervenir sur plusieurs projets en même temps. Cela peut parfois mettre en lumière un problème d’organisation (pourquoi a-t-il besoin de travailler sur tous ces projets&nbsp;?). Mais il reste important d’écouter ce type de retour et d’évaluer si une action d’uniformisation est nécessaire.

## INNOVATIONS ET GESTION DES RISQUES

Lorsque l’on conçoit un système, il est rare de trouver immédiatement la bonne solution, le bon design. C’est particulièrement vrai dans le logiciel où le code change, les fonctionnalités évoluent. Il est donc très difficile d’anticiper les besoins futurs, et tenter d’anticiper n’est pas toujours souhaitable, car celà nous mène régulièrement à des optimisations prématurées et de l’over-engineering.

Pour cela, il est important de rester ouvert aux changements et aux propositions d’évolution. L’amélioration des pratiques passe alors nécessairement par une phase d’expérimentation et d’exploration.

Quand une nouvelle contrainte apparaît, il faut donc comprendre ses impacts sur la solution actuelle et identifier de potentielles évolutions à apporter pour mieux y répondre. Cependant, si l’on souhaite conserver un code toujours uniformisé, deux scénarios s’offrent à nous&nbsp;:

- La quantité de code à modifier est trop importante, et le changement n’a pas lieu, on continue donc à travailler sur solution sous-optimale et l’on accumule de la dette.
- On se lance dans une évolution “big-bang”, sans savoir si la solution envisagée est la bonne, ni même si elle va aboutir. Là aussi, la méthode est sous-optimale puisqu’il faut investir beaucoup de temps et d’énergie avant d’espérer un quelconque résultat. Sans compter l’introduction de potentiels bugs dans des fonctionnalités qui ne nécessitent aucune évolution et qui fonctionnent parfaitement, ce qui est très frustrant pour ses utilisateurs.

Si les standards peuvent évoluer, il est donc plus pertinent de d’abord les tester sur un périmètre limité, et ainsi rapidement obtenir des feedbacks. Si les changements ne sont pas pertinents, alors le temps et l’énergie engagés auront été limités et il est simple de faire machine arrière. Si ceux-ci se montrent pertinents, on peut alors les généraliser de façon opportuniste, progressivement au fils des développements afin de mieux gérer les risques liés à ces évolutions.

## MOTIVATION DES ÉQUIPES

Outre le fait de ne pas rester coincé dans une solution inadaptée, les livres [Accelerate](https://itrevolution.com/product/accelerate/) (Allow teams to choose their own tools, p.66) et [Team Topologies](https://teamtopologies.com/book) (Monolithic thinking (Standardization), p.114) soulignent également l’aspect humain; ces phases d’expérimentation et d’exploration tendent à augmenter l’engagement et la motivation des développeurs.

J’y suis pour ma part très sensible, et un entretien avec un potentiel client où celui-ci insiste trop sur l’importance de coder selon ses standards est pour moi un mauvais signal. Ceci parce que le message que j’en retiens est&nbsp;: “Tu devras subir des choix passés, que tu n’as pas pris, et ce sans aucune possibilité de t’en extraire, même s’ils s’avèrent mauvais aujourd’hui. » Bref, pas très motivant. J’ai déjà refusé des missions pour cette raison.

## POUR CONCLURE

Vous l’aurez compris, je ne suis pas un grand défenseur de l’uniformisation du code à l’échelle d’une entreprise. Parce que les avantages que l’on peut en tirer me semblent finalement assez limités (limites cognitives, innovations, motivations), et aussi parce que cette approche a vocation à soigner les symptômes plus que les causes du problème (organisation, complexité du système). Comme je l’ai déjà évoqué, cette pratique peut faire sens pour des métiers « satellites » à faible complexité et sur lesquels les efforts de développements seront limités. Sur ce sujet (comme pour beaucoup d’autres), tout est question d’équilibre, il faut constamment veiller à ne pas s’enfermer dans un extrême.

---

## COMMENTAIRES

<!--Ajoutez votre commentaire ici-->

Envie de commenter ? S’il vous plaît, ajoutez votre commentaire en m'[envoyant une pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
