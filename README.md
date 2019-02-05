# Epicture

## Introduction

> Application Universal Windows Platform réalisée en C#
> Le but du projet est de créer une application utilisant l'API de différentes plateformes d'images.

### Fonctionnalités

- Grille d'images
- Connexion au compte utilisateur Imgur
- Affichage des images asynchrones
- Ordre d'affichage
- Upload d'image
- Gestion des favoris


## Comment compiler ?

> Ouvrez une console puis copiez :

```
msbuild epicture.sln /p:Configuration=Release;Platform=x64;AppxBundlePlatforms=x64;AppxBundle=always
```

## Comment installer l'application ?

> Lancez le paquet suivant :

```
DotNet_epicture_2017\epicture\epicture\bin\x64\Release\epicture_1.0.0.0_x64.appx
```

## Comment démarrer l'application ?

> Cherchez Epicture dans le menu démarrer.

## Crédits

- Jonathan Manassen
- Christophe Mei