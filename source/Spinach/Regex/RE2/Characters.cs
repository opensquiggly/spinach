// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/compile.go

namespace Spinach.Regex;

public static class Characters
{
  public static int ToLowerCase(int codePoint)
  {
    // Convert UTF-32 character to a UTF-16 String.
    string strC = char.ConvertFromUtf32(codePoint);

    // Casing rules depends on the culture.
    // Consider using ToLowerInvariant().
    string lower = strC.ToLower(CultureInfo.InvariantCulture);

    // Convert the UTF-16 String back to UTF-32 character and return it.
    return char.ConvertToUtf32(lower, 0);
  }

  public static int ToUpperCase(int codePoint)
  {
    // Convert UTF-32 character to a UTF-16 String.
    string strC = char.ConvertFromUtf32(codePoint);

    // Casing rules depends on the culture.
    // Consider using ToLowerInvariant().
    string lower = strC.ToUpper(CultureInfo.InvariantCulture);

    // Convert the UTF-16 String back to UTF-32 character and return it.
    return char.ConvertToUtf32(lower, 0);
  }
}
