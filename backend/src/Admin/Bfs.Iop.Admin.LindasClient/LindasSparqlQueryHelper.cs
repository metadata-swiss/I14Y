using VDS.RDF.Query;

namespace Bfs.Iop.Admin.LindasClient;

internal static class LindasSparqlQueryHelper
{
    private const string I14YGraph = "https://lindas.admin.ch/fso/i14y";

    private const string Prefixes = """
PREFIX cube:   <https://cube.link/meta/>
PREFIX dcat:   <http://www.w3.org/ns/dcat#>
PREFIX dct:    <http://purl.org/dc/terms/>
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
PREFIX oa:     <https://www.w3.org/ns/oa#>
PREFIX owl:    <http://www.w3.org/2002/07/owl#>
PREFIX pav:    <http://purl.org/pav/>
PREFIX rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX schema: <http://schema.org/>
PREFIX sh:     <http://www.w3.org/ns/shacl#>
PREFIX skos:   <http://www.w3.org/2004/02/skos/core#>
PREFIX vl:     <https://version.link/>
PREFIX xkos:   <http://rdf-vocabulary.ddialliance.org/xkos#>
""";

    internal static string BuildConceptExistsQuery(string identifier, string version)
    {
        var query = CreateQuery(
            $$"""
ASK {
  GRAPH <{{I14YGraph}}> {
    ?concept a schema:DefinedTermSet, vl:Version ;
          schema:identifier @identifier ;
          pav:version @version .
  }
}
""");

        query.SetLiteral("identifier", identifier, false);
        query.SetLiteral("version", version, false);

        return Prefixes + query;
    }

    internal static string BuildConceptConstructQuery(string identifier, string version)
    {
        var query = CreateQuery(
            $$"""
CONSTRUCT {
  ?s ?p ?o .
}
WHERE {
  GRAPH <{{I14YGraph}}> {
    {
      {
        SELECT DISTINCT ?s
        WHERE {
          ?concept a schema:DefinedTermSet, vl:Version ;
                schema:identifier @identifier ;
                pav:version @version .

          ?concept (
            vl:Version|vl:Identity|
            schema:hasPart|schema:hasDefinedTerm|schema:member|schema:isPartOf|schema:inDefinedTermSet|
            skos:member|skos:broader|skos:narrower|skos:topConceptOf|skos:inScheme|
            xkos:level|
            cube:inHierarchy|cube:hierarchyRoot|cube:nextInHierarchy|
            sh:property|
            dct:subject|dct:conformsTo|
            oa:hasBody|
            rdf:rest
          )* ?s .

          FILTER(
            isIRI(?s) &&
            STRSTARTS(
              STR(?s),
              CONCAT("https://register.ld.admin.ch/i14y/concept/", STR(@identifier))
            )
          )
        }
      }

      ?s ?p ?o .
    }
    UNION
    {
      ?concept a schema:DefinedTermSet, vl:Version ;
            schema:identifier @identifier ;
            pav:version @version ;
            dct:publisher ?s .

      FILTER(
        isIRI(?s) &&
        STRSTARTS(STR(?s), "https://register.ld.admin.ch/i14y/agent/")
      )

      ?s ?p ?o .
    }
  }
}
""");

        query.SetLiteral("identifier", identifier, false);
        query.SetLiteral("version", version, false);

        return Prefixes + query;
    }

    internal static string BuildDatasetExistsQuery(string identifier)
    {
        var query = CreateQuery(
            $$"""
ASK {
  GRAPH <{{I14YGraph}}> {
    ?dataset a dcat:Dataset ;
             dct:identifier @identifier .
  }
}
""");

        query.SetLiteral("identifier", identifier, false);

        return Prefixes + query;
    }

    internal static string BuildDatasetConstructQuery(string identifier)
    {
        var query = CreateQuery(
            $$"""
CONSTRUCT {
  ?s ?p ?o .
}
WHERE {
  GRAPH <{{I14YGraph}}> {
    ?dataset a dcat:Dataset ;
             dct:identifier @identifier .

    {
      {
        BIND(?dataset AS ?s)
      }
      UNION
      {
        ?dataset ?rootPredicate ?start .
        FILTER(isIRI(?start) && STRSTARTS(STR(?start), STR(?dataset)))

        ?start
          !(rdf:type|dct:publisher|dct:conformsTo|dcat:theme|
            dcat:accessRights|dcat:accessService|dcat:accessURL|
            dcat:downloadURL|dcat:landingPage|dct:relation|
            dct:isReferencedBy|foaf:page|schema:image|sh:path|
            sh:class|sh:datatype|owl:imports)* ?s .

        FILTER(isIRI(?s) && STRSTARTS(STR(?s), STR(?dataset)))
      }

      ?s ?p ?o .
    }
    UNION
    {
      ?dataset dct:publisher ?s .
      FILTER(
        isIRI(?s) &&
        STRSTARTS(STR(?s), "https://register.ld.admin.ch/i14y/agent/")
      )
      ?s ?p ?o .
    }
  }
}
""");

        query.SetLiteral("identifier", identifier, false);

        return Prefixes + query;
    }

    private static SparqlParameterizedString CreateQuery(string commandText) => new()
    {
        CommandText = commandText
    };
}
