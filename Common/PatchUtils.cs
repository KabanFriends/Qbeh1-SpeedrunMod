using Steamworks;
using System.Collections;

namespace Qbeh1_SpeedrunMod.Common
{
    public class PatchUtils
    {
        public static IEnumerator RunOriginalEnumerator(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
