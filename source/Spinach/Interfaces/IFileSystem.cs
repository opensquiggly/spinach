namespace Spinach.Interfaces;

/// <summary>
/// Interface for abstracting file system operations to support testing.
/// </summary>
public interface IFileSystem
{
  /// <summary>
  /// Checks if a directory exists at the specified path.
  /// </summary>
  bool DirectoryExists(string path);

  /// <summary>
  /// Enumerates all directories in the specified directory.
  /// </summary>
  IEnumerable<string> EnumerateDirectories(string path);

  /// <summary>
  /// Enumerates all files in the specified directory.
  /// </summary>
  IEnumerable<string> EnumerateFiles(string path);
}
