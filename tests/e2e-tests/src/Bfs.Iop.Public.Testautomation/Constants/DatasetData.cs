namespace Bfs.Iop.Public.Testautomation.Constants
{
    public static class DatasetData
    {
        public static readonly string DatasetTitle = "Gebäudestammdaten";
        public static readonly string DatasetRegistrationstatusDe = "Registriert";

        public static readonly string DatasetDescriptionDe = @"Im Bereich der Gebäude verfügt der Bund über das Eidgenössische Gebäude- und Wohnungsregister (GWR), dessen Führung in der Verordnung über das eidgenössische Gebäude- und Wohnungsregister (VGWR) geregelt ist. Diese Verordnung enthält die Definitionen der erfassten Entitäten und etabliert das GWR als Referenzinformationssystem. Eine Pflicht zur Nutzung der Gebäudedaten des GWR gibt es nach der derzeitigen Rechtsgrundlage jedoch nicht. Es besteht lediglich die Pflicht für die Verwaltungsbehörden, die Gebäudeadressen gemäss Verordnung über die geografischen Namen (GeoNV) zu verwenden.
Die von den zuständigen Behörden im GWR erfassten Informationen sind in Art. 8 VGWR definiert. Im Merkmalskatalog werden die Einzelheiten, insbesondere die Codes und Qualitätsanforderungen, festgelegt. Das GWR enthält Angaben zu Bauprojekten, Gebäuden und Wohnungen. Darüber hinaus stellt ein eCH-Standard eine klare und anwendbare Basis dar und bietet eine Definition der Merkmale für den Kernbestand der Stammdaten.
Grundsätzlich sind alle Gebäude-Stammdaten öffentlich zugänglich (Stufe A gemäss VGWR). Die Bereitstellung von GWR-Daten an dritte erfolgt über mehrere Kanäle. Ein Download-Dienst für komplette Datensätze für einen bestimmten geografischen Perimeter und Web-Services (eCH-0206) stehen zur Verfügung. Die öffentlichen Daten (Stufe A) werden den Anwendern auch der Bundes Geodaten-Infrastruktur (BGDI) im GWR-Datenlayer zur Verfügung gestellt.
Da das Eidgenössische Gebäude- und Wohnungsregister seit rund zwanzig Jahren besteht, verfügt das Bundesamt für Statistik (BFS) über eine umfangreiche Erfahrung und Kenntnisse auf dem Gebiet des Führens eines Registers mit einer Vielzahl von Partnern.";


        public static readonly string DatasetIdentifierDe = "BUILDING_MASTER_DATA";
        public static readonly string DatasetIssuedDe = "20.10.2022";
        public static readonly string DatasetModifiedDe = "29.10.2021";
        public static readonly string DatasetPublisherDe = "I14Y Test Organisation_de";
        public static readonly string DatasetContactPointsDe = @"Sektion Gebäude und Wohnungen (GEWO) Espace de l'Europe 10 CH-2010 Neuchâtel Schweiz0800 866 600 housing-stat@bfs.admin.ch  - Öffnet in einem neuen Tab."
;
        public static readonly string DatasetContactPointsDe2 = "housing-stat@bfs.admin.ch";
        public static readonly string DatasetLanguagesDe = "Deutsch, Französisch, Italienisch";
        public static readonly string DatasetKeywordsDe = "Geokoordinaten";
        public static readonly string DatasetLandingPageDe = "Eidg. Gebäude- und Wohnungsregister - Öffnet in einem neuen Tab.";
        public static readonly string DatasetAccessRightDe = "Öffentlich";
        
        // The frontend renders each spatial coverage in a separate div. Wrapper.NormalizeText removes
        // the whitespace between those child divs when validating the parent container.
        public static readonly string DatasetSpatialDe = "Suissecantonscommunes";
        public static readonly string DatasetFrequencyDe = "jährlich";

        //public static readonly string DatasetVersionDe = "2022.1";
        //public static readonly string DatasetVersionNotesDe = "Erstellt im 2022";
        //public static readonly string DatasetPreviousVersionDe = "Betriebs- und Unternehmensregister (BUR)";
    }
}
