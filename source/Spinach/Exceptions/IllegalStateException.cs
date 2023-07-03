namespace Spinach.Exceptions;

[Serializable]
public class IllegalStateException : Exception
{
  public IllegalStateException(string str) : base(str) { }
}

