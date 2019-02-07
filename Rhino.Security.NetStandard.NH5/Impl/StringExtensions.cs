using System.Text.RegularExpressions;

namespace Rhino.Security
{
    internal static class StringExtensions
    {
        static Regex tablePattern = new Regex(@"{{([\w]+)}}", RegexOptions.Compiled);
        internal static string PrefixTableName(this string query)
        {
            return tablePattern.Replace(query, m =>
            {
                return string.Format(
                    "security{0}{1}",
                    Security.TableStructure == SecurityTableStructure.Prefix ? "_" : ".",
                    m.Groups[1].Captures[0].Value);
            });
 
        }
    }
}
