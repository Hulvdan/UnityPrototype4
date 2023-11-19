// From YouTube by git-amend. Easy and Powerful Extension Methods | Unity C#.
// https://youtu.be/Nk49EUf7yyU
using System.Collections.Generic;

public static class EnumeratorExtensions {
    /// <summary>
    /// Converts an IEnumerator<T> to an IEnumerable<T>.
    /// </summary>
    /// <param name="e">An instance of IEnumerator<T>.</param>
    /// <returns>An IEnumerable<T> with the same elements as the input instance.</returns>
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> e) {
        while (e.MoveNext()) {
            yield return e.Current;
        }
    }
}
