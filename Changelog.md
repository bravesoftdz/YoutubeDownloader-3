### v1.7 (12-Apr-2022)
* Die Genauigkeit der Metadaten wurde verbessert. Außerdem wurde die Liste der eingefügten Medien-Tags um einige zusätzliche Informationen erweitert.
* Token $id für die Video-Id hinzugefügt.
* Automatische Erkennung des Dunklen Designs hinzugefügt.
* Dropdown für Untertitelauswahl wurde entfernt. Untertitel werden jetzt automatisch heruntergeladen und in die Videodatei eingebettet.
* Die Option „Medien-Tags einfügen“ wurde entfernt. Medien-Tags werden jetzt immer eingefügt.
* Die Option „Ausgeschlossene Formate“ wurde wegen geringer Nützlichkeit entfernt.
  Die Option „Zwischenablage automatisch einfügen“ wurde wegen geringer Nützlichkeit entfernt.
* Es wurde ein Problem behoben, das Video-Thumbnails nicht richtig in Videodateien eingefügt wurden.
* Es wurde ein Problem mit dem Lizenzen behoben, das diese versehentlich gelöscht wurden.
* Chrome extension ist nun mit dem YoutubeDownloader kompatibel (https://chrome.google.com/webstore/detail/open-in-youtubedownloader/ocjnlgpggmhcfjflphoalojankbkinoe)

### v1.6.8 (09-Mär-2022)
* Tokensystem überarbeitet.

### v1.6.7 (07-Mär-2022)
* Verschiedene Youtube bezogene Fehler wurden behoben

### v1.6.6 (13-Dez-2021)
* Verschiedene Youtube bezogene Fehler wurden behoben
* Ein Fehler mit der Speicherung von Daten wurde behoben

### v1.6.5 (6-Okt-2021)
* Ein Fehler wurde behoben, bei dem der Download bei fehlerhaftem Taging unterbrochen wurde
* Ein Fehler wurde behoben, bei dem sich Einstellungen bei Verbindungsabbruch gelöscht haben
* Ein Fehler mit der automatischen Installation von Microsoft Bibliotheken wurde behoben
* Ein visueller Fehler in einem Dialog wurde behoben

### v1.6.4 (29-Jul-2021)
* Verschiedene Youtube bezogene Fehler wurden behoben

### v1.6.3 (15-Jul-2021)
+ Benötigte Bibliotheken von Microsoft werden nun automatisch heruntergeladen

* Ein Fehler, der den Downloader bei Instabiler Internetverbindung zum Absturz brachte, wurde behoben
* Ein Fehler mit der Aktivierung des Tokens wurde behoben
* Ein Fehler mit dem Download von Livestreams wurde behoben
* Ein Fehler, der das News Fenster nicht richtig dargestellt hat, wurde behoben

### v1.6.2 (22-Jun-2021)
* Optimierung des Installationsprozesses
* Verschiedene Youtube bezogene Fehler wurden behoben

### v1.6.1 (22-Mai-2021)
+ Automatisches Importieren der Zwischenablage
* Interne Anpassung für zukünftige Updates vorgenommen
* Verschiedene Youtube bezogene Fehler wurden behoben
* Fehler mit HDR Videos behoben

### v1.6 (18-Apr-2021)
+ Unterstützung für 360° Videos hinzugefügt
- Unterstützung für das Veröffentlichungsdatum eingestellt
* Ein Fehler mit dem Download von alterbeschränkten Videos wurde behoben
* Ein Fehler mit dem Download von Playlisten wurde behoben
* Ein Fehler mit dem Download von Vorschaubildern wurde behoben
* Ein Fehler mit der Suchfunktion wurde behoben
* Ein Fehler mit dem Download von 360° Videos wurde behoben
* Ein Fehler beim Injizieren von Video bezogenen Informationen wurde behoben
* Weitere Youtube bezogene Fehler wurden behoben

### v1.5.2.15 (10-Apr-2021)
- Fixed various YouTube-related issues. Updated to YoutubeExplode v6.0.0-alpha2.
- Improved Token System
- Reworked update System

### v1.5.1 (15-Jan-2021)
- Added clear Query Text Button by request
- Added News Message
- Added Stats
- Fixed Issues related to Youtube
- Fixed Issues with the Token verification
- Updated Snackbar color. Thanks @Nick for the reminder and new design.

### v1.5.0 (15-Jan-2021)
- Added Multilanguage System
- Moved Token activation to MariaDB to an external Server

### v1.4.4 (28-Nov-2020)
- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.9.

### v1.4.3 (28-Nov-2020)
- Fixed Format exlude option

### v1.4.2 (26-Oct-2020)
- Hotfix for Token-Login, when Token is used

### v1.4.1 (25-Oct-2020)
- Hotfix for Token-Login

### v1.4.0 (25-Oct-2020)

- Added subtitle download option when downloading single videos. (Thanks [@beawolf](https://github.com/beawolf))
- Added format exclusion list. You can configure in settings a list of containers which you would like to not see, and they will be filtered out in the format selection dropdown. (Thanks [@beawolf](https://github.com/beawolf))
- Added dark mode. You can enable it in settings. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Added video quality preference selection when downloading multiple videos. (Thanks [@Bartłomiej Rogowski](https://github.com/brogowski))
- Added circular progress bars for each individual active download.
- Added meta tag injection for mp4 files. This adds channel and upload date information, as well as thumbnail. (Thanks [@beawolf](https://github.com/beawolf))
- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.8.

### v1.3.14 (25-Okt-2020)

- Fixed bug agagin that prevented download of videos
- Added Token Chache

### v1.3.13 (25-Okt-2020)

- Fixed bug that prevented download of videos

### v1.3.12 (29-Sep-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.6.
- Changed the order in which new downloads appear in the list so that newest downloads are at the top. (Thanks [@Max](https://github.com/badijm))


### v1.3.11 (18-Sep-2020)

- Test Update


### v1.3.10 (17-Sep-2020)

- changed updater back to default, because they fixed it

### v1.3.9 (13-Sep-2020)

- fixed updater for the last time

### v1.3.8 (13-Sep-2020)

- revert nuget packet change of the updater
- updater works finally
- added Token Verifyer
- Improved Code
- updatet YoutubeExplode

### v1.3.7 (10-Sep-2020)

- updated the updater nuget packet

### v1.3.6 (10-Sep-2020)

- try to fix the updater again

### v1.3.5 (10-Sep-2020)

- Finished Translation
- added Videolength token

### v1.3.4 (5-Sep-2020)

- try to fix the updater again
- (deleted)

### v1.3.3 (4-Sep-2020)

- fixed updater

### v1.3.2 (2-Sep-2020)

- updatet Changelog
- fixed small issues

### v1.3.1 (1-Sep-2020)

- Fixed Update service & installer

### v1.3.1 (1-Sep-2020)

- Fixed Update service & installer

### v1.3.0 (31-Aug-2020)

- Github Initial commit
