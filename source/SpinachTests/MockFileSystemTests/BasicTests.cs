namespace SpinachTests.MockFileSystemTests;

[TestClass]
public class BasicTests
{
  private MockFileSystem MockFileSystem { get; set; }

  public BasicTests()
  {
    MockFileSystem = SetupMockFileSystem();
  }

  private MockFileSystem SetupMockFileSystem()
  {
    var mockFileSystem = new MockFileSystem();

    // Setup mock directory structure:
    // /root
    // ├── dir1
    // │   ├── file1.txt
    // │   └── file2.txt
    // └── dir2
    //     ├── subdir1
    //     │   └── file3.txt
    //     └── file4.txt

    mockFileSystem.AddDirectory("/root", new List<string> { "/root/dir1", "/root/dir2" });
    mockFileSystem.AddDirectory("/root/dir1", new List<string>());
    mockFileSystem.AddDirectory("/root/dir2", new List<string> { "/root/dir2/subdir1" });
    mockFileSystem.AddDirectory("/root/dir2/subdir1", new List<string>());

    mockFileSystem.AddFiles("/root", new List<string>());
    mockFileSystem.AddFiles("/root/dir1", new List<string> { "/root/dir1/file1.txt", "/root/dir1/file2.txt" });
    mockFileSystem.AddFiles("/root/dir2", new List<string> { "/root/dir2/file4.txt" });
    mockFileSystem.AddFiles("/root/dir2/subdir1", new List<string> { "/root/dir2/subdir1/file3.txt" });

    return mockFileSystem;
  }

  [TestMethod]
  public void Should_ThrowDirectoryNotFoundException_When_PathIsInvalid()
  {
    var action = () => new Basic("/nonexistent", MockFileSystem);
    action.Should().Throw<DirectoryNotFoundException>();
  }

  [TestMethod]
  public void Should_ThrowArgumentOutOfRangeException_When_StartIndexIsNegative()
  {
    var action = () => new Basic("/root", MockFileSystem, -1);
    action.Should().Throw<ArgumentOutOfRangeException>();
  }

  [TestMethod]
  public void Should_EnumerateAllFiles_When_StartingFromBeginning()
  {
    var enumerator = new Basic("/root", MockFileSystem);
    var files = new List<string>();

    while (enumerator.MoveNext())
    {
      files.Add(enumerator.Current);
    }

    var expectedFiles = new List<string>
    {
      "/root/dir1/file1.txt",
      "/root/dir1/file2.txt",
      "/root/dir2/file4.txt",
      "/root/dir2/subdir1/file3.txt"
    };

    files.Should().BeEquivalentTo(expectedFiles, options => options.WithStrictOrdering());
  }

  [TestMethod]
  public void Should_EnumerateRemainingFiles_When_StartingFromIndex()
  {
    var enumerator = new Basic("/root", MockFileSystem, 2);
    var files = new List<string>();

    while (enumerator.MoveNext())
    {
      files.Add(enumerator.Current);
    }

    var expectedFiles = new List<string>
    {
      "/root/dir2/file4.txt",
      "/root/dir2/subdir1/file3.txt"
    };

    files.Should().BeEquivalentTo(expectedFiles, options => options.WithStrictOrdering());
  }

  [TestMethod]
  public void Should_EnumerateFromCorrectPosition_When_ResetToSpecificIndex()
  {
    var enumerator = new Basic("/root", MockFileSystem);

    // Move past some files
    enumerator.MoveNext();
    enumerator.MoveNext();

    // Reset to index 1
    enumerator.Reset(1);
    var files = new List<string>();

    while (enumerator.MoveNext())
    {
      files.Add(enumerator.Current);
    }

    var expectedFiles = new List<string>
    {
      "/root/dir1/file2.txt",
      "/root/dir2/file4.txt",
      "/root/dir2/subdir1/file3.txt"
    };

    files.Should().BeEquivalentTo(expectedFiles, options => options.WithStrictOrdering());
  }

  [TestMethod]
  public void Should_EnumerateAllFiles_When_ResetToBeginning()
  {
    var enumerator = new Basic("/root", MockFileSystem);

    // Move past some files
    enumerator.MoveNext();
    enumerator.MoveNext();

    // Reset to beginning
    enumerator.Reset();
    var files = new List<string>();

    while (enumerator.MoveNext())
    {
      files.Add(enumerator.Current);
    }

    var expectedFiles = new List<string>
    {
      "/root/dir1/file1.txt",
      "/root/dir1/file2.txt",
      "/root/dir2/file4.txt",
      "/root/dir2/subdir1/file3.txt"
    };

    files.Should().BeEquivalentTo(expectedFiles, options => options.WithStrictOrdering());
  }

  [TestMethod]
  public void Should_TrackCurrentIndex_Correctly()
  {
    var enumerator = new Basic("/root", MockFileSystem);
    enumerator.CurrentIndex.Should().Be(-1);

    enumerator.MoveNext();
    enumerator.CurrentIndex.Should().Be(0);

    enumerator.MoveNext();
    enumerator.CurrentIndex.Should().Be(1);
  }

  [TestMethod]
  public void Should_ThrowInvalidOperationException_When_AccessingCurrentBeforeEnumeration()
  {
    var enumerator = new Basic("/root", MockFileSystem);
    var action = () => _ = enumerator.Current;
    action.Should().Throw<InvalidOperationException>();
  }

  [TestMethod]
  public void Should_ThrowInvalidOperationException_When_AccessingCurrentAfterEnumeration()
  {
    var enumerator = new Basic("/root", MockFileSystem);
    while (enumerator.MoveNext()) { }
    var action = () => _ = enumerator.Current;
    action.Should().Throw<InvalidOperationException>();
  }
}
