// Adapted from HiBoP (https://github.com/hbp-HiBoP/HiBoP), BSD-3-Clause. Licence and copyright: THIRD-PARTY-NOTICES.md.
namespace UnityEngine.Events
{
    public class GenericEvent<T1> : UnityEvent<T1> { };
    public class GenericEvent<T1, T2> : UnityEvent<T1, T2> { };
    public class GenericEvent<T1, T2, T3> : UnityEvent<T1, T2, T3> { };
    public class GenericEvent<T1, T2, T3, T4> : UnityEvent<T1, T2, T3, T4> { };
}