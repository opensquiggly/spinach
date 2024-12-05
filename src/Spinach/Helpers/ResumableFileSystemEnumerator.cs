using Spinach.Interfaces;
using System.Collections;

namespace Spinach.Helpers;

/// <summary>
/// Enumerates files in a repository directory with support for resuming from a specific index
/// and safe file system operations.
/// </summary>
public class ResumableFileSystemEnumerator : IEnumerator<string>
{
  private readonly string _rootPath;
  private readonly Queue<string> _pendingDirectories;
  private readonly List<string> _currentDirectoryFiles;
  private readonly int _startIndex;
  private readonly IFileSystem _fileSystem;
  private int _currentIndex;
  private string? _current;
  private bool _isDisposed;
  private bool _hasInitialized;

  /// <summary>
  /// Gets the current index in the enumeration sequence.
  /// </summary>
  public int CurrentIndex => _currentIndex;

  /// <summary>
  /// Initializes a new instance of the RepoFileEnumerator class.
  /// </summary>
  /// <param name="rootPath">The root directory path to enumerate files from.</param>
  /// <param name="fileSystem">The file system implementation to use.</param>
  /// <param name="startIndex">The index to start enumeration from. Default is 0.</param>
  public ResumableFileSystemEnumerator(string rootPath, IFileSystem fileSystem, int startIndex = 0)
  {
    if (string.IsNullOrEmpty(rootPath))
      throw new ArgumentNullException(nameof(rootPath));

    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

    if (!_fileSystem.DirectoryExists(rootPath))
      throw new DirectoryNotFoundException($"Directory not found: {rootPath}");

    if (startIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index cannot be negative.");

    _rootPath = rootPath;
    _startIndex = startIndex;
    _pendingDirectories = new Queue<string>();
    _currentDirectoryFiles = new List<string>();
    _currentIndex = -1;
    _pendingDirectories.Enqueue(rootPath);
    _hasInitialized = false;
  }

  /// <summary>
  /// Gets the current file path in the enumeration.
  /// </summary>
  public string Current
  {
    get
    {
      if (_current == null)
        throw new InvalidOperationException("Enumeration has not started or has finished.");
      return _current;
    }
  }

  /// <summary>
  /// Gets the current element in the enumeration.
  /// </summary>
  object IEnumerator.Current => Current;

  /// <summary>
  /// Fast forwards the enumerator to the start index if needed.
  /// </summary>
  private void InitializeToStartIndex()
  {
    if (_hasInitialized || _startIndex == 0)
      return;

    while (_currentIndex < _startIndex - 1 && MoveNextInternal()) { }

    if (_currentIndex != _startIndex - 1)
      throw new InvalidOperationException($"Could not reach start index {_startIndex} as it exceeds the number of available files.");

    _hasInitialized = true;
  }

  /// <summary>
  /// Internal implementation of MoveNext that doesn't handle initialization.
  /// </summary>
  private bool MoveNextInternal()
  {
    if (_isDisposed)
      throw new ObjectDisposedException(nameof(ResumableFileSystemEnumerator));

    while (true)
    {
      // If we have files in the current directory, process the next one
      if (_currentDirectoryFiles.Count > 0)
      {
        _current = _currentDirectoryFiles[0];
        _currentDirectoryFiles.RemoveAt(0);
        _currentIndex++;
        return true;
      }

      // If no more directories to process, we're done
      if (_pendingDirectories.Count == 0)
      {
        _current = null;
        return false;
      }

      // Get the next directory to process
      string currentDir = _pendingDirectories.Dequeue();

      try
      {
        // Get all subdirectories first and sort them to ensure consistent ordering
        var subdirectories = _fileSystem.EnumerateDirectories(currentDir)
          .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
          .ToList();

        // Get all files and sort them to ensure consistent ordering
        var files = _fileSystem.EnumerateFiles(currentDir)
          .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
          .ToList();

        // Add sorted subdirectories to the queue
        foreach (string dir in subdirectories)
        {
          try
          {
            _pendingDirectories.Enqueue(dir);
          }
          catch (UnauthorizedAccessException) { /* Skip inaccessible directories */ }
          catch (DirectoryNotFoundException) { /* Skip directories that were removed */ }
        }

        // Add sorted files to the current files list
        foreach (string file in files)
        {
          try
          {
            _currentDirectoryFiles.Add(file);
          }
          catch (UnauthorizedAccessException) { /* Skip inaccessible files */ }
          catch (FileNotFoundException) { /* Skip files that were removed */ }
        }
      }
      catch (UnauthorizedAccessException) { /* Skip inaccessible directories */ }
      catch (DirectoryNotFoundException) { /* Skip directories that were removed */ }
    }
  }

  /// <summary>
  /// Advances the enumerator to the next file.
  /// </summary>
  /// <returns>true if the enumerator was successfully advanced to the next file; false if the end was reached.</returns>
  public bool MoveNext()
  {
    if (!_hasInitialized)
    {
      InitializeToStartIndex();
    }
    return MoveNextInternal();
  }

  /// <summary>
  /// Resets the enumerator to a specific index.
  /// </summary>
  /// <param name="index">The index to reset to. Use -1 to start from the beginning.</param>
  public void Reset(int index = -1)
  {
    if (_isDisposed)
      throw new ObjectDisposedException(nameof(ResumableFileSystemEnumerator));

    if (index < -1)
      throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be less than -1.");

    // Clear existing state
    _pendingDirectories.Clear();
    _currentDirectoryFiles.Clear();
    _current = null;
    _hasInitialized = false;

    // Reset to initial state
    _pendingDirectories.Enqueue(_rootPath);
    _currentIndex = -1;

    // If index is -1, we're done with reset
    if (index == -1)
      return;

    // Otherwise, advance to the specified index
    while (_currentIndex < index - 1 && MoveNextInternal()) { }

    if (_currentIndex != index - 1)
      throw new ArgumentException($"Could not reset to index {index} as it exceeds the number of available files.");
  }

  /// <summary>
  /// Resets the enumerator to its initial state.
  /// </summary>
  void IEnumerator.Reset() => Reset();

  /// <summary>
  /// Disposes the enumerator.
  /// </summary>
  public void Dispose()
  {
    if (!_isDisposed)
    {
      _pendingDirectories.Clear();
      _currentDirectoryFiles.Clear();
      _current = null;
      _isDisposed = true;
    }
  }
}
