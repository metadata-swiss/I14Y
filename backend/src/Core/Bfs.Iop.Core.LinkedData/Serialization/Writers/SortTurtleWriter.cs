/*
// <copyright>
// Adapted from dotNetRDF's CompressingTurtleWriter (v3.5.1), which is free and open source software
// licensed under the MIT License.
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
/// Turtle writer that emits the triples in the order an <see cref="ITripleSort"/> gives.
/// Copied from dotNetRDF's CompressingTurtleWriter with three changes: the sort is injected rather than
/// the static WriterHelper.SortTriplesBySubjectPredicate; compression is fixed at High; high speed mode
/// is gone, since it drops compression and writes triple by triple, ignoring the sort.
/// </summary>
internal sealed class SortTurtleWriter : BaseRdfWriter, IPrettyPrintingWriter, INamespaceWriter, IFormatterBasedWriter
{
    private readonly TurtleSyntax _syntax;
    private readonly ITripleSort _sorting;

    public SortTurtleWriter(ITripleSort sorting, TurtleSyntax syntax = TurtleSyntax.Original)
    {
        _sorting = sorting ?? throw new ArgumentNullException(nameof(sorting));
        _syntax = syntax;
    }

    public bool PrettyPrintMode { get; set; } = true;

    // Imported into the graph. A graph from a SPARQL CONSTRUCT declares almost no prefixes, and without
    // these the output is all full IRIs.
    public INamespaceMapper DefaultNamespaces { get; set; } = new NamespaceMapper();

    public Type TripleFormatterType => _syntax == TurtleSyntax.Original ? typeof(TurtleFormatter) : typeof(TurtleW3CFormatter);

#pragma warning disable CS0067 // Upstream only warned when entering high speed mode, which no longer exists.
    public override event RdfWriterWarning Warning;
#pragma warning restore CS0067

    protected override void SaveInternal(IGraph g, TextWriter output)
    {
        g.NamespaceMap.Import(DefaultNamespaces);
        var context = new CompressingTurtleWriterContext(g, output, WriterCompressionLevel.High, PrettyPrintMode, false, _syntax);
        GenerateOutput(context);
    }

    private void GenerateOutput(CompressingTurtleWriterContext context)
    {
        if (context.Graph.BaseUri != null)
        {
            context.Output.WriteLine("@base <" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">.");
            context.Output.WriteLine();
        }

        foreach (var prefix in context.Graph.NamespaceMap.Prefixes)
        {
            if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
            {
                if (!prefix.Equals(string.Empty))
                {
                    context.Output.WriteLine("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix)) + ">.");
                }
                else
                {
                    context.Output.WriteLine("@prefix : <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(string.Empty)) + ">.");
                }
            }
        }

        context.Output.WriteLine();

        WriterHelper.FindCollections(context);
        if (_syntax == TurtleSyntax.Rdf11Star) WriterHelper.FindAnnotations(context);

        var ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
        _sorting.Sort(ts, context.Graph);

        INode lastSubj, lastPred;
        lastSubj = lastPred = null;
        int subjIndent = 0, predIndent = 0;
        string temp;

        foreach (Triple t in ts)
        {
            if (lastSubj == null || !t.Subject.Equals(lastSubj))
            {
                if (lastSubj != null) context.Output.WriteLine(".");

                temp = GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0);
                context.Output.Write(temp);
                context.Output.Write(" ");
                if (temp.Contains('\n'))
                {
                    subjIndent = temp.Split('\n').Last().Length + 1;
                }
                else
                {
                    subjIndent = temp.Length + 1;
                }
                lastSubj = t.Subject;

                temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
                context.Output.Write(temp);
                context.Output.Write(" ");
                predIndent = temp.Length + 1;
                lastPred = t.Predicate;
            }
            else if (lastPred == null || !t.Predicate.Equals(lastPred))
            {
                context.Output.WriteLine(";");

                if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent));

                temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
                context.Output.Write(temp);
                context.Output.Write(" ");
                predIndent = temp.Length + 1;
                lastPred = t.Predicate;
            }
            else
            {
                context.Output.WriteLine(",");

                if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent + predIndent));
            }

            temp = GenerateNodeOutput(context, t.Object, TripleSegment.Object, subjIndent + predIndent);
            context.Output.Write(temp);

            if (context.Annotations.ContainsKey(t))
            {
                context.Output.Write(GenerateAnnotationOutput(context, context.Annotations[t], subjIndent + predIndent + temp.Length + 1));
            }
        }

        if (ts.Count > 0) context.Output.WriteLine(".");
    }

    private string GenerateNodeOutput(CompressingTurtleWriterContext context, INode n, TripleSegment segment, int indent)
    {
        var output = new StringBuilder();

        switch (n.NodeType)
        {
            case NodeType.Blank:
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("Turtle"));

                if (context.Collections.ContainsKey(n))
                {
                    output.Append(GenerateCollectionOutput(context, context.Collections[n], indent));
                }
                else
                {
                    return context.NodeFormatter.Format(n, segment);
                }
                break;

            case NodeType.GraphLiteral:
                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("Turtle"));

            case NodeType.Literal:
                if (segment == TripleSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("Turtle"));
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("Turtle"));
                return context.NodeFormatter.Format(n, segment);

            case NodeType.Uri:
                return context.NodeFormatter.Format(n, segment);

            case NodeType.Triple:
                if (_syntax != TurtleSyntax.Rdf11Star)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodesUnserializable($"Turtle/{_syntax}"));
                }
                if (segment == TripleSegment.Predicate)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodePredicateUnserializable("Turtle"));
                }

                return context.NodeFormatter.Format(n, segment);

            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("Turtle"));
        }

        return output.ToString();
    }

    private string GenerateCollectionOutput(CompressingTurtleWriterContext context, OutputRdfCollection c, int indent)
    {
        var output = new StringBuilder();
        var first = true;

        if (!c.IsExplicit)
        {
            output.Append('(');

            while (c.Triples.Count > 0)
            {
                if (context.PrettyPrint && !first) output.Append(new string(' ', indent));
                first = false;
                output.Append(GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent));
                c.Triples.RemoveAt(0);
                if (c.Triples.Count > 0)
                {
                    if (context.PrettyPrint) output.AppendLine("");
                    output.Append(' ');
                }
            }

            output.Append(')');
        }
        else
        {
            if (c.Triples.Count == 0)
            {
                output.Append("[]");
            }
            else
            {
                output.Append('[');

                while (c.Triples.Count > 0)
                {
                    if (context.PrettyPrint && !first) output.Append(new string(' ', indent));
                    first = false;
                    var temp = GenerateNodeOutput(context, c.Triples.First().Predicate, TripleSegment.Predicate, indent);
                    output.Append(temp);
                    output.Append(' ');
                    int addIndent;
                    if (temp.Contains('\n'))
                    {
                        addIndent = temp.Split('\n').Last().Length;
                    }
                    else
                    {
                        addIndent = temp.Length;
                    }
                    output.Append(GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent + 2 + addIndent));
                    c.Triples.RemoveAt(0);

                    if (c.Triples.Count > 0)
                    {
                        output.AppendLine(" ; ");
                        output.Append(' ');
                    }
                }

                output.Append(']');
            }
        }

        return output.ToString();
    }

    private string GenerateAnnotationOutput(CompressingTurtleWriterContext context, List<Triple> annotationTriples, int indent)
    {
        var output = new StringBuilder();
        string temp;
        output.Append(" {| ");
        _sorting.Sort(annotationTriples, context.Graph);
        INode lastPred = null;
        indent += 3;
        int predIndent = 0;

        foreach (Triple t in annotationTriples)
        {
            if (lastPred == null || !lastPred.Equals(t.Predicate))
            {
                if (lastPred != null)
                {
                    // New line for the next predicate
                    output.AppendLine(";");
                    if (context.PrettyPrint) output.Append(' ', indent);
                }

                temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, indent);
                predIndent = temp.Length + 1;
                lastPred = t.Predicate;
                output.Append(temp);
                output.Append(' ');
            }
            else
            {
                output.AppendLine(",");
                if (context.PrettyPrint) output.Append(' ', indent + predIndent);
            }

            temp = GenerateNodeOutput(context, t.Object, TripleSegment.Object, indent + predIndent);
            output.Append(temp);

            if (context.Annotations.ContainsKey(t))
            {
                output.Append(GenerateAnnotationOutput(context, context.Annotations[t], indent + predIndent + temp.Length + 1));
            }
        }

        output.Append(" |}");

        return output.ToString();
    }

    public override string ToString()
    {
        return "Turtle (Sorting Writer)" + _syntax switch
        {
            TurtleSyntax.W3C => " (W3C)",
            TurtleSyntax.Rdf11Star => " (RDF-Star)",
            _ => ""
        };
    }
}
