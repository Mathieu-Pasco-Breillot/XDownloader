# XDownloader
Projet de gestionnaire de téléchargement multiplateforme automatique afin de récupérer le fichier distant à partir d'un lien.

## Pré-requis
1. Installer [Visual Studio 2017 Community](https://www.visualstudio.com/fr/thank-you-downloading-visual-studio/?sku=Community&rel=15)
    * Installer tous les composants des sections :
        * **Développement web et ASP.NET**
        * **Développement multiplateforme .NET Core**
2. Cloner ce repository sur votre machine.

## Démarrage de la solution
Après avoir installer VS2017 et cloner le **XDownloader** repository vous pouvez démarrer la solution en exécutant le fichier **XDownloader.sln**.
Lancez une première compilation de la solution complète, cela devrait automatiquement restaurer les packages nugets manquant à la solution.

### En cas d'erreur de compilation du à la restauration des packages NuGet
Faites un clique droit sur la solution depuis l'**Explorateur de solutions** puis cliquez sur **Restaurer les packages NuGet**.

## Utiliser le XDownloader
Plusieurs fonctionnalités seront mises à disposition à terme.
Actuellement il existe 2 routes d'API fonctionnelles.

VERBE HTTP | NAME | URL | HEADERS | BODY
---------- | ---- | --- | ------- | ----
POST | Get_Link_From_Protector_Page | http://localhost:56254/api/LinksFromProtector | Content-Type : application/json | "https://www.dl-protect1.com/123455600123455602123455610123455615vt8yz1pa62zz"
POST | Get_All_Links_From_Source_Page | http://localhost:56254/api/LinksFromSource | Content-Type : application/json | "http://zone-telechargement1.com/31463-marvel-les-agents-du-s.h.i.e.l.d.-saison-5-vostfr-hd720p.html"

### Explications
La route *Get_All_Links_From_Source_Page* s'occupe de récupérer tous les liens de hoster protégés par dl-protect.
La réponse de cette requête renvoie une liste d'objet contenant tous les liens obtenus tous hosters confondus mais protégés. (non testé pour le moment)

La route *Get_Link_From_Protector_Page* s'occupe d'aller récupérer le lien de téléchargement protégé par dl-protect automatiquement, puis ensuite déclencher le téléchargement du fichier à partir du flux sous-jacent dans l'espace de destination, actuellement hardcodé (à la raçine de votre repository).
