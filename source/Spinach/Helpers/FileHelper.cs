namespace Spinach.Helpers;

public static class FileHelper
{
  public static bool FileIsBinary(string filename)
  {
    byte[] buffer = new byte[8192];

    using (FileStream fileStream = File.OpenRead(filename))
    {
      int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

      for (int i = 0; i < bytesRead; i++)
      {
        if (buffer[i] < 0x09)
        {
          return true; // File contains a null byte or other control character; assume binary
        }
      }
    }

    return false;
  }
}
