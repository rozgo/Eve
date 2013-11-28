using System;
using System.Globalization;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace BeyondGames.Utility
{
  /// <summary>
  /// Provides methods for working with <see cref="string"/>.
  /// </summary>
  public static class StringUtility
  {
    /// <summary>
    /// Formats the string using the invariant culture.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatInvariant (this string format, params object[] args)
    {
      return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    /// <summary>
    /// Converts the string to title case using the specified culture, or current culture if not specified.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>The title-cased string.</returns>
    public static string ToTitleCase (this string str, CultureInfo culture = null)
    {
      CultureInfo ci = Thread.CurrentThread.CurrentCulture;
      return ci.TextInfo.ToTitleCase(str);
    }

    /// <summary>
    /// Join the specified strings in to a single string, separated by a specified separator.
    /// </summary>
    /// <param name="strings">The strings to join.</param>
    /// <param name="separator">The separator to use between each string.</param>
    public static string Join(this IEnumerable<string> strings, string separator)
    {
      return string.Join(separator, strings.ToArray());
    }

    
    public static string GetMD5Hash (this string str)
    {
      // step 1, calculate MD5 hash from input
      MD5 md5 = System.Security.Cryptography.MD5.Create();
      byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
      byte[] hash = md5.ComputeHash(inputBytes);
  
      // step 2, convert byte array to hex string
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hash.Length; i++) {
        sb.Append(hash[i].ToString("X2"));
      }
      return sb.ToString();
    }
    
  }
}
