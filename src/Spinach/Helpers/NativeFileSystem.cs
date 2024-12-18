using Spinach.Interfaces;
using System.IO;

namespace Spinach.Helpers;

/// <summary>
/// Implementation of IFileSystem that works with the native file system.
/// </summary>
public class NativeFileSystem : IFileSystem
{
  public bool DirectoryExists(string path) => Directory.Exists(path);

  public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);

  public IEnumerable<string> EnumerateFiles(string path) => Directory.EnumerateFiles(path);

  public bool IsSymLink(string path)
  {
    FileAttributes attributes = File.GetAttributes(path);
    return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
  }
}
