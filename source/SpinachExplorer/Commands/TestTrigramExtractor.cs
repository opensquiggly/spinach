namespace SpinachExplorer;

internal static partial class Program
{
  private static void TestTrigramExtractor()
  {
    throw new NotImplementedException();

    // Console.WriteLine();
    //
    // Console.Write("Enter first internal file id: ");
    // string firstResponse = Console.ReadLine();
    //
    // Console.Write("Enter last internal file id: ");
    // string lastResponse = Console.ReadLine();
    //
    // if (int.TryParse(firstResponse, out int firstFileId) && int.TryParse(lastResponse, out int lastFileId))
    // {
    //   for (int fileId = firstFileId; fileId <= lastFileId; fileId++)
    //   {
    //     long nameAddress = TextSearchIndex.InternalFileIdTree.Find(fileId);
    //     DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
    //     string name = nameString.GetValue();
    //
    //     if (name.Contains("/.git/"))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     if (name.Contains("/obj/"))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     if (name.Contains("/bin/"))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     if (name.Contains("node_modules"))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     if (name.EndsWith(".jpg") || name.EndsWith(".gif") || name.EndsWith(".png") || name.EndsWith(".dll"))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     if (FileHelper.FileIsBinary(name))
    //     {
    //       Console.Write(".");
    //       continue;
    //     }
    //
    //     string content = File.ReadAllText(name);
    //
    //     var trigramExtractor = new TrigramExtractor(content);
    //     int count = 0;
    //
    //     foreach (TrigramInfo trigramInfo in trigramExtractor)
    //     {
    //       char ch1 = (char)(trigramInfo.Key / 128L / 128L);
    //       char ch2 = (char)(trigramInfo.Key % (128L * 128L) / 128L);
    //       char ch3 = (char)(trigramInfo.Key % 128L);
    //       count++;
    //       Console.Write(".");
    //     }
    //
    //     Console.WriteLine($"{fileId} : {count} : {nameString.GetValue()}");
    //   }
    // }
    //
    // Pause();
  }
}
