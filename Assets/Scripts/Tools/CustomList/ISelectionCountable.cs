// Adapted from HiBoP (https://github.com/hbp-HiBoP/HiBoP), BSD-3-Clause. Licence and copyright: THIRD-PARTY-NOTICES.md.
using UnityEngine.Events;

public interface ISelectionCountable
{
    int NumberOfItemSelected { get; }
    UnityEvent OnSelectionChanged { get; }
}
