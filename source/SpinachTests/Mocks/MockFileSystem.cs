namespace SpinachTests.Mocks;

public class MockFileSystem : IFileSystem
{
  private readonly Dictionary<string, List<string>> _directories;
  private readonly Dictionary<string, List<string>> _files;

  public MockFileSystem()
  {
    _directories = new Dictionary<string, List<string>>();
    _files = new Dictionary<string, List<string>>();
  }

  public void AddDirectory(string path, List<string> subdirectories) => _directories[path] = subdirectories;

  public void AddFiles(string directory, List<string> files) => _files[directory] = files;

  public bool DirectoryExists(string path) => _directories.ContainsKey(path);

  public IEnumerable<string> EnumerateDirectories(string path) => _directories.TryGetValue(path, out List<string>? dirs) ? dirs : Enumerable.Empty<string>();

  public IEnumerable<string> EnumerateFiles(string path) => _files.TryGetValue(path, out List<string>? files) ? files : Enumerable.Empty<string>();
}
