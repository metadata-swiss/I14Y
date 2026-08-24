/*
// <copyright>
// Adapted from dotNetRDF's RdfXmlWriter (v3.5.1), which is free and open source software licensed
// under the MIT License.
// -------------------------------------------------------------------------
//
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using Bfs.Iop.Core.LinkedData.Serialization.Sort;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

// Kept close to upstream so a dotNetRDF bump can be diffed against it, including its pre-nullable style.
#nullable disable

namespace Bfs.Iop.Core.LinkedData.Serialization.Writers;

/// <summary>
/// RDF/XML writer that emits the triples in the order an <see cref="ITripleSorter"/> gives.
/// Copied from dotNetRDF's RdfXmlWriter with two changes: the sort is injected rather than its own
/// RdfXmlTripleComparer; compression is fixed at High, so the collection branch is always taken.
/// </summary>
internal sealed class SortRdfXmlWriter : BaseRdfWriter, IPrettyPrintingWriter, IDtdWriter, INamespaceWriter, IFormatterBasedWriter
{
    private readonly ITripleSorter _sorting;

    public SortRdfXmlWriter(ITripleSorter sorting)
    {
        _sorting = sorting ?? throw new ArgumentNullException(nameof(sorting));
    }

    public bool PrettyPrintMode { get; set; } = true;

    // Imported into the graph. A graph from a SPARQL CONSTRUCT declares almost no prefixes, and without
    // these the output is all full IRIs.
    public INamespaceMapper DefaultNamespaces { get; set; } = new NamespaceMapper();

#pragma warning disable CS0618 // Obsolete upstream as well; kept so the output matches RdfXmlWriter's.
    public bool UseDtd { get; set; } = Options.UseDtd;
#pragma warning restore CS0618

    public Type TripleFormatterType => typeof(RdfXmlFormatter);

    /// <summary>
    /// Saves a Graph to an arbitrary output stream.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="output">Stream to save to.</param>
    protected override void SaveInternal(IGraph g, TextWriter output)
    {
        GenerateOutput(g, output);
    }

    /// <summary>
    /// Internal method which generates the RDF/Json Output for a Graph.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="output">Stream to save to.</param>
    private void GenerateOutput(IGraph g, TextWriter output)
    {
        // Always force RDF Namespace to be correctly defined
        g.NamespaceMap.Import(DefaultNamespaces);
        g.NamespaceMap.AddNamespace("rdf", g.UriFactory.Create(NamespaceMapper.RDF));

        // Create our Writer Context and start the XML Document
        var context = new RdfXmlWriterContext(g, output) { CompressionLevel = WriterCompressionLevel.High, UseDtd = UseDtd };
        context.Writer.WriteStartDocument();

        if (context.UseDtd)
        {
            // Create the DOCTYPE declaration
            var entities = new StringBuilder();
            string uri;
            entities.Append('\n');
            foreach (var prefix in context.NamespaceMap.Prefixes)
            {
                uri = context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri;
                if (!uri.Equals(context.NamespaceMap.GetNamespaceUri(prefix).ToString()))
                {
                    context.UseDtd = false;
                    break;
                }
                if (!prefix.Equals(string.Empty))
                {
                    var escapedUri = WriterHelper.EncodeForXml(uri);
                    entities.AppendLine("\t<!ENTITY " + prefix + " '" + escapedUri + "'>");
                }
            }
            if (context.UseDtd) context.Writer.WriteDocType("rdf:RDF", null, null, entities.ToString());
        }

        // Create the rdf:RDF element
        context.Writer.WriteStartElement("rdf", "RDF", NamespaceMapper.RDF);
        if (context.Graph.BaseUri != null)
        {
            context.Writer.WriteAttributeString("xml", "base", null, context.Graph.BaseUri.AbsoluteUri);//Uri.EscapeUriString(context.Graph.BaseUri.ToString()));
        }
        context.NamespaceMap.IncrementNesting();
        foreach (var prefix in context.NamespaceMap.Prefixes)
        {
            if (prefix.Equals("rdf")) continue;

            if (!prefix.Equals(string.Empty))
            {
                context.Writer.WriteStartAttribute("xmlns", prefix, null);
                // String nsRef = "&" + prefix + ";";
                // context.Writer.WriteRaw(nsRef);
                // context.Writer.WriteEntityRef(prefix);
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));//Uri.EscapeUriString(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri)));
                context.Writer.WriteEndAttribute();
            }
            else
            {
                context.Writer.WriteStartAttribute("xmlns");
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));//Uri.EscapeUriString(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri)));
                context.Writer.WriteEndAttribute();
            }
        }

        // Find the Collections and Type References
        WriterHelper.FindCollections(context, CollectionSearchMode.ImplicitOnly);

        // Upstream marks a triple as written by adding it to context.TriplesDone, whose Add is internal
        // to dotNetRDF. Only its own additions go here; the ones FindCollections made are still read
        // off the context.
        var done = new HashSet<Triple>();
        Dictionary<INode, string> typerefs = FindTypeReferences(context, done);

        // Get the Triples in the order the injected sort gives
        var ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t) && !done.Contains(t)).ToList();
        _sorting.Sort(ts, context.Graph);

        // Variables we need to track our writing
        INode lastSubj, lastPred, lastObj;
        lastSubj = lastPred = lastObj = null;

        for (var i = 0; i < ts.Count; i++)
        {
            Triple t = ts[i];
            if (context.TriplesDone.Contains(t) || done.Contains(t)) continue; //Skip if already done

            if (lastSubj == null || !t.Subject.Equals(lastSubj))
            {
                // Start a new set of Triples
                if (lastSubj != null)
                {
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                }
                if (lastPred != null)
                {
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                }

                // Write out the Subject
                // Validate Subject
                // Use a Type Reference if applicable
                context.NamespaceMap.IncrementNesting();
                if (typerefs.ContainsKey(t.Subject))
                {
                    var tref = typerefs[t.Subject];
                    if (tref.StartsWith(":"))
                    {
                        context.Writer.WriteStartElement(tref.Substring(1));
                    }
                    else if (tref.Contains(":"))
                    {
                        context.Writer.WriteStartElement(tref.Substring(0, tref.IndexOf(':')), tref.Substring(tref.IndexOf(':') + 1), null);
                    }
                    else
                    {
                        context.Writer.WriteStartElement(tref);
                    }
                }
                else
                {
                    context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                }
                lastSubj = t.Subject;

                // Apply appropriate attributes
                switch (t.Subject.NodeType)
                {
                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                    case NodeType.Literal:
                        throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/XML"));
                    case NodeType.Blank:
                        if (context.Collections.ContainsKey(t.Subject))
                        {
                            GenerateCollectionOutput(context, t.Subject);
                        }
                        else
                        {
                            context.Writer.WriteAttributeString("rdf", "nodeID", null, context.BlankNodeMapper.GetOutputID(((IBlankNode)t.Subject).InternalID));
                        }
                        break;
                    case NodeType.Uri:
                        GenerateUriOutput(context, (IUriNode)t.Subject, "rdf:about");
                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }

                // Write the Predicate
                context.NamespaceMap.IncrementNesting();
                GeneratePredicateNode(context, t.Predicate);
                lastPred = t.Predicate;
                lastObj = null;
            }
            else if (lastPred == null || !t.Predicate.Equals(lastPred))
            {
                if (lastPred != null)
                {
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                }

                // Write the Predicate
                context.NamespaceMap.IncrementNesting();
                GeneratePredicateNode(context, t.Predicate);
                lastPred = t.Predicate;
                lastObj = null;
            }

            // Write the Object
            if (lastObj != null)
            {
                // Terminate the previous Predicate Node
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();

                // Start a new Predicate Node
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();
                context.NamespaceMap.IncrementNesting();
                GeneratePredicateNode(context, t.Predicate);
            }
            // Create an Object for the Object
            switch (t.Object.NodeType)
            {
                case NodeType.Blank:
                    if (context.Collections.ContainsKey(t.Object))
                    {
                        // Output a Collection
                        GenerateCollectionOutput(context, t.Object);
                    }
                    else
                    {
                        // Terminate the Blank Node triple by adding a rdf:nodeID attribute
                        context.Writer.WriteAttributeString("rdf", "nodeID", null, context.BlankNodeMapper.GetOutputID(((IBlankNode)t.Object).InternalID));
                    }

                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                case NodeType.Literal:
                    var lit = (ILiteralNode)t.Object;
                    GenerateLiteralOutput(context, lit);

                    break;
                case NodeType.Uri:
                    GenerateUriOutput(context, (IUriNode)t.Object, "rdf:resource");
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }
            lastObj = t.Object;

            // Force a new Predicate Node
            context.NamespaceMap.DecrementNesting();
            context.Writer.WriteEndElement();
            lastPred = null;

            done.Add(t);
        }

        // Check we haven't failed to output any collections
        foreach (KeyValuePair<INode, OutputRdfCollection> pair in context.Collections)
        {
            if (pair.Value.Triples.Count > 0)
            {
                if (typerefs.ContainsKey(pair.Key))
                {
                    var tref = typerefs[pair.Key];
                    context.NamespaceMap.IncrementNesting();
                    if (tref.StartsWith(":"))
                    {
                        context.Writer.WriteStartElement(tref.Substring(1));
                    }
                    else if (tref.Contains(":"))
                    {
                        context.Writer.WriteStartElement(tref.Substring(0, tref.IndexOf(':')), tref.Substring(tref.IndexOf(':') + 1), null);
                    }
                    else
                    {
                        context.Writer.WriteStartElement(tref);
                    }

                    GenerateCollectionOutput(context, pair.Key);

                    context.Writer.WriteEndElement();
                }
                else
                {
                    context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)pair.Key).InternalID));
                    GenerateCollectionOutput(context, pair.Key);
                    context.Writer.WriteEndElement();
                    // throw new RdfOutputException("Failed to output a Collection due to an unknown error");
                }
            }
        }

        context.NamespaceMap.DecrementNesting();
        context.Writer.WriteEndDocument();

        // Save to the Output Stream
        context.Writer.Flush();
    }

    private void GenerateCollectionOutput(RdfXmlWriterContext context, INode key)
    {
        OutputRdfCollection c = context.Collections[key];
        if (!c.IsExplicit)
        {
            if (context.NamespaceMap.NestingLevel > 2)
            {
                // Need to set the Predicate to have a rdf:parseType of Resource
                context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
            }

            var length = c.Triples.Count;
            while (c.Triples.Count > 0)
            {
                // Get the Next Item and generate the rdf:first element
                INode next = c.Triples.First().Object;
                c.Triples.RemoveAt(0);
                context.NamespaceMap.IncrementNesting();
                context.Writer.WriteStartElement("rdf", "first", NamespaceMapper.RDF);

                // Set the value of the rdf:first Item
                switch (next.NodeType)
                {
                    case NodeType.Blank:
                        context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)next).InternalID));
                        break;
                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                    case NodeType.Literal:
                        GenerateLiteralOutput(context, (ILiteralNode)next);
                        break;
                    case NodeType.Uri:
                        GenerateUriOutput(context, (IUriNode)next, "rdf:resource");
                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }

                // Now generate the rdf:rest element
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();
                context.NamespaceMap.IncrementNesting();
                context.Writer.WriteStartElement("rdf", "rest", NamespaceMapper.RDF);

                if (c.Triples.Count >= 1)
                {
                    // Set Parse Type to resource
                    context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                }
                else
                {
                    // Terminate list with an rdf:nil
                    context.Writer.WriteStartAttribute("rdf", "resource", NamespaceMapper.RDF);
                    if (context.UseDtd)
                    {
                        context.Writer.WriteRaw("&rdf;nil");
                    }
                    else
                    {
                        context.Writer.WriteRaw(NamespaceMapper.RDF + "nil");
                    }
                    context.Writer.WriteEndAttribute();
                }
            }
            for (var i = 0; i < length; i++)
            {
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();
            }
        }
        else
        {
            if (c.Triples.Count == 0)
            {
                // Terminate the Blank Node triple by adding a rdf:nodeID attribute
                context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)key).InternalID));
            }
            else
            {
                // Need to set the Predicate to have a rdf:parseType of Resource
                if (context.NamespaceMap.NestingLevel > 2)
                {
                    // Need to set the Predicate to have a rdf:parseType of Resource
                    context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                }

                // Output the Predicate Object list
                while (c.Triples.Count > 0)
                {
                    Triple t = c.Triples[0];
                    c.Triples.RemoveAt(0);
                    INode nextPred = t.Predicate;
                    INode nextObj = t.Object;

                    // Generate the predicate
                    GeneratePredicateNode(context, nextPred);

                    // Output the Object
                    switch (nextObj.NodeType)
                    {
                        case NodeType.Blank:
                            if (context.Collections.ContainsKey(nextObj))
                            {
                                // Output a Collection
                                GenerateCollectionOutput(context, nextObj);
                            }
                            else
                            {
                                context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)key).InternalID));
                            }
                            break;
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            GenerateLiteralOutput(context, (ILiteralNode)nextObj);
                            break;
                        case NodeType.Uri:
                            GenerateUriOutput(context, (IUriNode)nextObj, "rdf:resource");
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    context.Writer.WriteEndElement();
                }
            }
        }
    }

    private void GeneratePredicateNode(RdfXmlWriterContext context, INode p)
    {
        switch (p.NodeType)
        {
            case NodeType.GraphLiteral:
                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
            case NodeType.Blank:
                throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/XML"));
            case NodeType.Literal:
                throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/XML"));
            case NodeType.Uri:
                // OK
                UriRefType rtype;
                var predRef = GenerateUriRef(context, ((IUriNode)p).Uri, UriRefType.QName, out rtype);
                string prefix, uri;
                prefix = uri = null;
                if (rtype != UriRefType.QName)
                {
                    GenerateTemporaryNamespace(context, (IUriNode)p, out prefix, out uri);

                    predRef = GenerateUriRef(context, ((IUriNode)p).Uri, UriRefType.QName, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + p.ToString() + "'");
                    }
                }

                GenerateElement(context, predRef);

                // Add Temporary Namespace to current XML Element
                // CORE-431: This is unecessary and causes malformed XML under monotouch
                // if (prefix != null && uri != null)
                // {
                //    context.Writer.WriteStartAttribute("xmlns", prefix, null);
                //    context.Writer.WriteRaw(Uri.EscapeUriString(WriterHelper.EncodeForXml(uri)));
                //    context.Writer.WriteEndAttribute();
                // }

                break;
            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
        }

        // Write the Predicate
    }

    private void GenerateLiteralOutput(RdfXmlWriterContext context, ILiteralNode lit)
    {
        if (!lit.Language.Equals(string.Empty))
        {
            context.Writer.WriteAttributeString("xml", "lang", null, lit.Language);
            context.Writer.WriteString(lit.Value);
        }
        else if (lit.DataType != null)
        {
            if (RdfSpecsHelper.RdfXmlLiteral.Equals(lit.DataType.AbsoluteUri))
            {
                context.Writer.WriteAttributeString("rdf", "parseType", null, "Literal");
                context.Writer.WriteRaw(lit.Value);
            }
            else
            {
                UriRefType refType;
                var dtUri = GenerateUriRef(context, lit.DataType, UriRefType.UriRef, out refType);
                if (refType == UriRefType.Uri)
                {
                    context.Writer.WriteAttributeString("rdf", "datatype", null, lit.DataType.AbsoluteUri);//Uri.EscapeUriString(lit.DataType.ToString()));
                }
                else if (refType == UriRefType.UriRef)
                {
                    context.Writer.WriteStartAttribute("rdf", "datatype", null);
                    context.Writer.WriteRaw(dtUri);
                    context.Writer.WriteEndAttribute();
                }
                context.Writer.WriteString(lit.Value);
            }
        }
        else
        {
            // context.Writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
            context.Writer.WriteString(lit.Value);
        }
    }

    private void GenerateUriOutput(RdfXmlWriterContext context, IUriNode u, string attribute)
    {
        // Get a Uri Reference if the Uri can be reduced
        UriRefType rtype;
        var uriref = GenerateUriRef(context, u.Uri, UriRefType.UriRef, out rtype);

        if (attribute.Contains(':'))
        {
            context.Writer.WriteStartAttribute(attribute.Substring(0, attribute.IndexOf(':')), attribute.Substring(attribute.IndexOf(':') + 1), NamespaceMapper.RDF);
            if (rtype == UriRefType.UriRef)
            {
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));//Uri.EscapeUriString(WriterHelper.EncodeForXml(uriref)));
            }
            else
            {
                context.Writer.WriteString(uriref);//Uri.EscapeUriString(uriref));
            }
            context.Writer.WriteEndAttribute();
        }
        else
        {
            context.Writer.WriteStartAttribute(attribute);
            if (rtype == UriRefType.UriRef)
            {
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));//Uri.EscapeUriString(WriterHelper.EncodeForXml(uriref)));
            }
            else
            {
                context.Writer.WriteString(uriref);//Uri.EscapeUriString(uriref));
            }
            context.Writer.WriteEndAttribute();
        }
    }

    private string GenerateUriRef(RdfXmlWriterContext context, Uri u, UriRefType type, out UriRefType outType)
    {
        string uriref, qname;

        if (context.NamespaceMap.ReduceToQName(u.AbsoluteUri, out qname, RdfXmlSpecsHelper.IsValidQName))
        {
            // Reduced to QName OK
            uriref = qname;
            outType = UriRefType.QName;
            if (type == UriRefType.UriRef)
            {
                uriref = ConvertQNameToUriRef(context, uriref, out outType);
            }
        }
        else
        {
            uriref = u.AbsoluteUri;
            outType = UriRefType.Uri;
            if (type == UriRefType.UriRef && context.NamespaceMap.ReduceToQName(uriref, out qname))
            {
                // Attempt to compress the URI to a UriRef via a QName that is not XML-valid
                uriref = qname;
                if (uriref.Contains(":") && !uriref.StartsWith(":"))
                {
                    uriref = ConvertQNameToUriRef(context, uriref, out outType);
                }
            }
        }



        return uriref;
    }

    private static string ConvertQNameToUriRef(RdfXmlWriterContext context, string uriref, out UriRefType outType)
    {
        // Attempt to convert QName to a UriRef
        if (uriref.Contains(':') && !uriref.StartsWith(":"))
        {
            var prefix = uriref.Substring(0, uriref.IndexOf(':'));
            if (context.UseDtd && context.NamespaceMap.GetNestingLevel(prefix) == 0)
            {
                // Must have Use DTD enabled
                // Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
                uriref = "&" + uriref.Replace(':', ';');
                outType = UriRefType.UriRef;
            }
            else
            {
                uriref = context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri +
                         uriref.Substring(uriref.IndexOf(':') + 1);
                outType = UriRefType.Uri;
            }
        }
        else
        {
            if (context.NamespaceMap.HasNamespace(string.Empty))
            {
                uriref = context.NamespaceMap.GetNamespaceUri(string.Empty).AbsoluteUri + uriref.Substring(1);
                outType = UriRefType.Uri;
            }
            else
            {
                var baseUri = context.Graph.BaseUri.AbsoluteUri;
                if (!baseUri.EndsWith("#")) baseUri += "#";
                uriref = baseUri + uriref;
                outType = UriRefType.Uri;
            }
        }

        return uriref;
    }

    private void GenerateTemporaryNamespace(RdfXmlWriterContext context, IUriNode u, out string tempPrefix, out string tempUri)
    {
        RdfXmlFormatter.TryReduceUriToQName(u.Uri, out var qName, out var nsUri);

        // Create a Temporary Namespace ID
        // Can't use an ID if already in the Namespace Map either at top level (nesting == 0) or at the current nesting
        while (context.NamespaceMap.HasNamespace("ns" + context.NextNamespaceID) && (context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == 0 || context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == context.NamespaceMap.NestingLevel))
        {
            context.NextNamespaceID++;
        }
        var prefix = "ns" + context.NextNamespaceID;
        context.NextNamespaceID++;
        context.NamespaceMap.AddNamespace(prefix, context.UriFactory.Create(nsUri));

        tempPrefix = prefix;
        tempUri = nsUri;

        RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
    }

    private void GenerateElement(RdfXmlWriterContext context, string qname)
    {
        if (qname.Contains(':'))
        {
            if (qname.StartsWith(":"))
            {
                context.Writer.WriteStartElement(qname.Substring(1));
            }
            else
            {
                var prefix = qname.Substring(0, qname.IndexOf(':'));
                var ns = (context.NamespaceMap.GetNestingLevel(prefix) > 1) ? context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri : null;
                context.Writer.WriteStartElement(prefix, qname.Substring(prefix.Length + 1), ns);
            }
        }
        else
        {
            context.Writer.WriteStartElement(qname);
        }
    }

    private Dictionary<INode, string> FindTypeReferences(RdfXmlWriterContext context, HashSet<Triple> done)
    {
        // LINQ query to find all Triples which define the rdf:type of a Uri/BNode as a Uri
        IUriNode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDF + "type"));
        IEnumerable<Triple> ts = from t in context.Graph.Triples
                                 where (t.Subject.NodeType == NodeType.Blank || t.Subject.NodeType == NodeType.Uri)
                                        && t.Predicate.Equals(rdfType) && t.Object.NodeType == NodeType.Uri
                                        && !context.TriplesDone.Contains(t) && !done.Contains(t)
                                 select t;

        var typerefs = new Dictionary<INode, string>();
        foreach (Triple t in ts)
        {
            if (!typerefs.ContainsKey(t.Subject))
            {
                string typeref;
                UriRefType rtype;
                typeref = GenerateUriRef(context, ((IUriNode)t.Object).Uri, UriRefType.QName, out rtype);
                if (rtype != UriRefType.QName)
                {
                    // Generate a Temporary Namespace for the QName Type Reference
                    string prefix, uri;
                    GenerateTemporaryNamespace(context, (IUriNode)t.Object, out prefix, out uri);

                    // Add to current XML Element
                    context.Writer.WriteStartAttribute("xmlns", prefix, null);
#pragma warning disable SYSLIB0013 // Kept as upstream wrote it; the netstandard2.0 build sees no warning.
                    context.Writer.WriteRaw(Uri.EscapeUriString(WriterHelper.EncodeForXml(uri)));
#pragma warning restore SYSLIB0013
                    context.Writer.WriteEndAttribute();

                    typeref = GenerateUriRef(context, ((IUriNode)t.Object).Uri, UriRefType.QName, out rtype);
                    if (rtype == UriRefType.QName)
                    {
                        // Got a QName Type Reference in the Temporary Namespace OK
                        typerefs.Add(t.Subject, typeref);
                        if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                        {
                            done.Add(t);
                        }
                    }
                }
                else
                {
                    // Got a QName Type Reference OK
                    typerefs.Add(t.Subject, typeref);
                    if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                    {
                       done.Add(t);
                    }
                }
            }
        }

        return typerefs;
    }

    /// <summary>
    /// Internal Helper method for raising the Warning event.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message)
    {
        if (Warning != null)
        {
            Warning(message);
        }
    }

    /// <summary>
    /// Event which is raised when there is a non-fatal issue with the RDF being output
    /// </summary>
    public override event RdfWriterWarning Warning;

    /// <summary>
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "RDF/XML (Sorting Writer)";
    }
}

/// <summary>
/// Possible URI Reference Types. Copied from dotNetRDF's WriterUtilities because it is internal there.
/// </summary>
internal enum UriRefType : int
{
    /// <summary>
    /// Must be a QName
    /// </summary>
    QName = 1,
    /// <summary>
    /// May be a QName or a URI
    /// </summary>
    QNameOrUri = 2,
    /// <summary>
    /// URI Reference
    /// </summary>
    UriRef = 3,
    /// <summary>
    /// URI
    /// </summary>
    Uri = 4,
}
