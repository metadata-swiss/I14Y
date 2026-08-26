using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.Business.Helpers;

internal static class GitCommitHelper
{
    /// <summary>
    /// Git filters the '\n' char in the commit messages, 
    /// so this char will be used as new line separator.
    /// </summary>
    private const char NewLineChar = '\x1d';

    /// <summary>
    /// Commit fields will be separated by this char, so it can be used to split the commit message into fields.
    /// </summary>
    private const char FieldSeparatorChar = '\x1f';
    private const string FieldSeparator = "%x1f";

    public static string GenerateCommitMessage(
        string operation,
        ResourceMetadata resourceMetadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation, nameof(operation));
        ArgumentNullException.ThrowIfNull(resourceMetadata, nameof(resourceMetadata));

        return $"'{operation}' of resource '{resourceMetadata.Type}' with identifier '{resourceMetadata.Identifier}' and ID '{resourceMetadata.Id}'.{NewLineChar}";
    }

    public static string[] GenerateSearchExpressionArguments(CommitSearchFilters filters)
    {
        ArgumentNullException.ThrowIfNull(filters, nameof(filters));

        var args = new List<string>()
        {
            "log",
            "--all-match" // Necessary to perform AND operations (OR is by default).
        };

        if (!string.IsNullOrWhiteSpace(filters.Author))
        {
            args.AddRange(
                "--author",
                filters.Author);
        }

        if (filters.ResourceId.HasValue)
        {
            args.AddRange(
                "--grep",
                $"ID '{filters.ResourceId}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.ResourceIdentifier))
        {
            args.AddRange(
                "--grep",
                $"identifier '{filters.ResourceIdentifier}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.ResourceType))
        {
            args.AddRange(
                "--grep",
                $"resource '{filters.ResourceType}'");
        }

        args.Add(GetSearchExpressionPrettyFormatOption());

        return [.. args];
    }

    private static string GetSearchExpressionPrettyFormatOption()
    {
        var fields = new[]
        {
            "%H", // commit hash
            "%an", // author name
            "%ae", // author email
            "%aI", // author date, ISO 8601 format
            "%s" // commit message (subject)
        };

        return $"--pretty=format:{string.Join(FieldSeparator, fields)}";
    }

    public static IEnumerable<Commit> GenerateCommitsFromGitResponse(RepositoryResponse gitResponse)
    {
        ArgumentNullException.ThrowIfNull(gitResponse, nameof(gitResponse));

        if (!gitResponse.Success || string.IsNullOrWhiteSpace(gitResponse.StdOut))
        {
            return [];
        }

        var commitLines = gitResponse.StdOut.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        return commitLines.Select(line =>
        {
            var trimmed = line.Trim().Trim('"');
            var fields = trimmed.Split(FieldSeparatorChar);

            return new Commit()
            {
                Sha = fields[0],
                Author = new Author()
                {
                    Email = fields[2],
                    Name = fields[1]
                },
                TimeStamp = DateTimeOffset.Parse(fields[3]),
                Changes = fields[4].Split(NewLineChar, StringSplitOptions.RemoveEmptyEntries)
            };
        });
    }

    public static class OperationMessageTags
    {
        public const string Add = "Add";
        public const string Delete = "Delete";
        public const string Update = "Update";
    }
}
