using System.Diagnostics.CodeAnalysis;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;

internal static class ClientGeneratorOptionsBaseExtensions
{
    /// <summary>
    /// Verifies that the configured <see cref="BaseClientGeneratorOptions.OutputPath"/> exists. If not, tries to find the existing path with navigating up the directory tree from the current working up to the root.
    /// </summary>
    /// <returns>The output path to use for generating APi client sources.</returns>
    /// <exception cref="InvalidOperationException"><see cref="BaseClientGeneratorOptions.OutputPath"/> is not set or no suitable directory could be found.</exception>
    internal static string GetVerifiedOutputPath(this ClientGeneratorOptionsBase options)
    {
        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            throw new InvalidOperationException("OutputPath was empty.");
        }

        if (FindExistingDirectory(options.OutputPath, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Could not find the OutputPath '{options.OutputPath}' in the current working directory nor in any of the parent directories");
    }

    internal static bool FindExistingDirectory(string path, [NotNullWhen(true)] out string? existingPath)
    {
        if (Path.IsPathRooted(path))
        {
            var absolutePath = Path.GetFullPath(path);
            if (Directory.Exists(absolutePath))
            {
                existingPath = absolutePath;
                return true;
            }

            existingPath = null;
            return false;
        }

        var currentDir = Directory.GetCurrentDirectory();
        var result = Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(currentDir, path));

        while (!Directory.Exists(result))
        {
            var parentDir = Directory.GetParent(currentDir);

            if (parentDir == null)
            {
                existingPath = null;
                return false;
            }

            currentDir = parentDir.FullName;
            result = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(currentDir, path));
        }

        existingPath = result;
        return true;
    }

    internal static bool FindExistingFile(string path, [NotNullWhen(true)] out string? existingPath)
    {
        if (Path.IsPathRooted(path))
        {
            var absolutePath = Path.GetFullPath(path);
            if (File.Exists(absolutePath))
            {
                existingPath = absolutePath;
                return true;
            }

            existingPath = null;
            return false;
        }

        var currentDir = Directory.GetCurrentDirectory();
        var result = Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(currentDir, path));

        while (!File.Exists(result))
        {
            var parentDir = Directory.GetParent(currentDir);

            if (parentDir == null)
            {
                existingPath = null;
                return false;
            }

            currentDir = parentDir.FullName;
            result = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(currentDir, path));
        }

        existingPath = result;
        return true;
    }
}
