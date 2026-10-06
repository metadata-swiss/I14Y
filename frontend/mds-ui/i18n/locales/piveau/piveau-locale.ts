import { de, en } from '@piveau/sdk-vue/locale'

export const piveauLocale = {
  messages: {
    de: {
      ...de,
      metadata: {
        ...de.metadata,
        created: 'Veröffentlichungsdatum', // created dct:issued
        modificationDate: 'Aktualisierungsdatum', // modified dct:modified
        publisher: 'Publizierender', // publisher dct:publisher
        landingPage: 'Landing Page', // landing_page dcat:landingPage
        contactPoints: 'Kontaktstellen', // contact_point dcat:contactPoint
        accrualPeriodicity: 'Aktualisierungsfrequenz', // accrual periodicity dct:accrualPeriodicity
        identifiers: 'Identifikator', // identifier dct:identifier
        languages: 'Sprachen', // language dct:language
        license: 'Nutzungsbedingungen', // license dct:license
        mediaType: 'Media type', // media_type dcat:mediaType
        isReferencedBy: 'Wird referenziert von',
        qualifiedRelation: 'Qualifizierte Beziehungen',
        sample: 'Datenbeispiel',
        temporalResolution: 'Zeitliche Abdeckung',
        compressFormat: 'Komprimierungsformat',
        packageFormat: 'Paketformat',
        byteSize: 'Dateigrösse',

      },
      tooltip: {
        ...de.tooltip,
        datasetDetails: {
          ...de.tooltip.datasetDetails,
          isReferencedBy: 'Wird referenziert von',
          qualifiedRelation: 'Qualifizierte Beziehungen',
          sample: 'Datenbeispiel',
          temporalResolution: 'Zeitliche Abdeckung',
          distributions: {
            ...de.tooltip.datasetDetails.distributions,
            compressFormat: 'Komprimierungsformat',
            packageFormat: 'Paketformat',
          },
        },
      },
    },
    en: {
      ...en,
      metadata: {
        ...en.metadata,
        created: 'Release date', // created dct:issued
        modificationDate: 'Modification date', // modified dct:modified
        publisher: 'Publisher', // publisher dct:publisher
        landingPage: 'Landing Page', // landing_page dcat:landingPage
        contactPoints: 'Contact points', // contact_point dcat:contactPoint
        accrualPeriodicity: 'Frequency', // accrual periodicity dct:accrualPeriodicity
        identifiers: 'Identifier', // identifier dct:identifier
        languages: 'Languages', // language dct:language
        license: 'Terms of use', // license dct:license
        mediaType: 'Media type', // media_type dcat:mediaType
        byteSize: 'File size',
      },
      tooltip: {
        ...en.tooltip,
      },
    },
    fr: {
      metadata: {
        created: 'Date de publication', // created dct:issued
        modificationDate: 'Date de modification', // modified dct:modified
        publisher: 'Éditeur', // publisher dct:publisher
        landingPage: 'Landing Page', // landing_page dcat:landingPage
        contactPoints: 'Points de contact', // contact_point dcat:contactPoint
        accrualPeriodicity: 'Fréquence de mise à jour', // accrual periodicity dct:accrualPeriodicity
        identifiers: 'Identificateur', // identifier dct:identifier
        languages: 'Langues', // language dct:language
        license: 'Conditions d\'utilisation', // license dct:license
        mediaType: 'Media type', // media_type dcat:mediaType
        catalogRecord: 'Fiche de catalogue',
        conformsTo: 'Conforme à',
        resource: 'Ressource',
        rights: 'Conditions d\'utilisation',
        status: 'Statut',
        byteSize: 'Taille des données',
        temporalResolution: 'Couverture temporelle',
        type: 'Type de données',
        sample: 'Exemple de données',
        qualifiedRelation: 'Relations qualifiées',
        isReferencedBy: 'Est référencé par',
        versionNotes: 'Notes de version',
        versionInfo: 'Informations de version',
        temporal: 'Temporel',
        isVersionOf: 'Est une version de',
        hasVersion: 'A une version',
        accessRights: 'Droits d\'accès',
        otherIdentifiers: 'Autres identifiants',
        packageFormat: 'Format du paquet',
        compressFormat: 'Format de compression',
        pages: 'Pages',
        provenances: 'Provenance',
        name: 'Nom',
        tooltipPublisherName: 'Nom de l\'éditeur',
        homepage: 'Page d\'accueil',
        email: 'Email',
        telephone: 'Téléphone',
        organizationName: 'Nom de l\'organisation',
        tooltipPublisherHomepage: 'Page d\'accueil de l\'éditeur',
        address: 'Adresse',
        url: 'URL',
        // TOUS LES CLÉS MANQUANTES ICI POUR FR (voir erreur console)
      },
      tooltip: {
        contactPoints: 'Points de contact',
        catalogRecord: 'Fiche de catalogue',
        datasetDetails: {
          created: 'Date de publication',
          publisher: 'Éditeur',
          conformsTo: 'Conforme à',
          hasVersion: 'A une version',
          updated: 'Date de modification',
          landingPage: 'Page d\'accueil',
          temporalResolution: 'Couverture temporelle',
          type: 'Type de données',
          sample: 'Exemple de données',
          license: 'Conditions d\'utilisation',
          relatedResource: 'Ressource liée',
          language: 'Langue',
          otherIdentifier: 'Autres identifiants',
          qualifiedRelation: 'Relations qualifiées',
          isReferencedBy: 'Est référencé par',
          versionNotes: 'Notes de version',
          versionInfo: 'Informations de version',
          provenance: 'Provenance',
          distributions: {
            rights: 'Conditions d\'utilisation',
            created: 'Date de publication',
            licence: 'Conditions d\'utilisation',
            status: 'Statut',
            mediaType: 'Type de média',
            byteSize: 'Taille des données',
            packageFormat: 'Format du paquet',
            compressFormat: 'Format de compression',
          },
        },
      },
    },
    it: {
      metadata: {
        created: 'Data di rilascio', // created dct:issued
        modificationDate: 'Data di modifica', // modified dct:modified
        publisher: 'Editore', // publisher dct:publisher
        landingPage: 'Landing Page', // landing_page dcat:landingPage
        contactPoints: 'Punti di contatto', // contact_point dcat:contactPoint
        accrualPeriodicity: 'Intervallo di aggiornamento', // accrual periodicity dct:accrualPeriodicity
        identifiers: 'Identificatore', // identifier dct:identifier
        languages: 'Lingue', // language dct:language
        license: 'Condizioni d\'uso', // license dct:license
        mediaType: 'Media type', // media_type dcat:mediaType
        catalogRecord: 'Scheda catalogo',
        conformsTo: 'Conforme a',
        resource: 'Risorsa',
        rights: 'Condizioni d\'uso',
        status: 'Stato',
        byteSize: 'Grandezza dei dati',
        temporalResolution: 'Copertura temporale',
        type: 'Tipo di dato',
        sample: 'Esempio di dati',
        qualifiedRelation: 'Relazioni qualificate',
        isReferencedBy: 'È referenziato da',
        versionNotes: 'Note di versione',
        versionInfo: 'Informazioni di versione',
        temporal: 'Temporale',
        isVersionOf: 'È versione di',
        hasVersion: 'Ha versione',
        accessRights: 'Diritti di accesso',
        otherIdentifiers: 'Altri identificatori',
        packageFormat: 'Formato del pacchetto',
        compressFormat: 'Formato di compressione',
        pages: 'Pagine',
        provenances: 'Provenienza',
        name: 'Nome',
        tooltipPublisherName: 'Nome dell\'editore',
        homepage: 'Pagina iniziale',
        email: 'Email',
        telephone: 'Telefono',
        organizationName: 'Nome dell\'organizzazione',
        tooltipPublisherHomepage: 'Homepage dell\'editore',
        address: 'Indirizzo',
        url: 'URL',
        // ALL MISSING KEYS HERE FOR IT (see console error)
      },
      tooltip: {
        contactPoints: 'Punti di contatto',
        catalogRecord: 'Scheda catalogo',
        datasetDetails: {
          created: 'Data di rilascio',
          publisher: 'Editore',
          conformsTo: 'Conforme a',
          hasVersion: 'Ha versione',
          updated: 'Data di modifica',
          landingPage: 'Landing page',
          temporalResolution: 'Copertura temporale',
          type: 'Tipo di dato',
          sample: 'Esempio di dati',
          license: 'Condizioni d\'uso',
          relatedResource: 'Risorsa correlata',
          language: 'Lingua',
          otherIdentifier: 'Altri identificatori',
          qualifiedRelation: 'Relazioni qualificate',
          isReferencedBy: 'È referenziato da',
          versionNotes: 'Note di versione',
          versionInfo: 'Informazioni di versione',
          provenance: 'Provenienza',
          distributions: {
            rights: 'Condizioni d\'uso',
            created: 'Data di rilascio',
            licence: 'Condizioni d\'uso',
            status: 'Stato',
            mediaType: 'Tipo di media',
            byteSize: 'Grandezza dei dati',
            packageFormat: 'Formato del pacchetto',
            compressFormat: 'Formato di compressione',
          },
        },
      },
    },
  },
  locale: 'de',
  fallbackLocale: 'de',
}
