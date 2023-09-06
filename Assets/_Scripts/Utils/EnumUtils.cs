using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public struct EnumMap<TKey, TValue> : IEnumerable<TValue> where TKey : System.Enum
{
    [Serializable]
    public struct Entry
    {
//#if UNITY_EDITOR
        // This handles drawing the enum labels in the inspector,
        // without showing a human-editable field, or taking up memory in-build.
        [HideInInspector]
        public string name;
//#endif
        public TValue value;
    }

    [SerializeField] Entry[] _map;

    // Handles error reporting if we somehow get bad enum values 
    // (eg. from disc/network or faulty math that's cast to enum).
    static void Validate(TKey key)
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(TKey), key),
            $"Invalid {typeof(TKey).Name} value {key} used as an EnumMap key.");
    }

    static int GetIndex(TKey key)
    {
        Validate(key);
        // Don't worry about this cast - the compiler does the right thing when
        // it reifies the generic into a concrete type, so there's no boxing.
        return (int)(object)key;
    }

    // Provide 
    public TValue this[TKey key]
    {
        get
        {
            return _map[GetIndex(key)].value;
        }
        set
        {
            _map[GetIndex(key)].value = value;
        }
    }

    public void Initialize()
    {
//#if UNITY_EDITOR
        // Make a blank map from scratch.
        string[] names = System.Enum.GetNames(typeof(TKey));
        _map = new Entry[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            _map[i].name = names[i];
        }
//#endif
    }

    /// <summary>
    /// Call this method in OnValidate() and Reset() when using this 
    /// collection in a MonoBehaviour or ScriptableObject. 
    /// This handles changes in the key enumeration type.
    /// </summary>
    public void TryRevise()
    {
        // Compiles to a no-op in player builds and should be elided.
#if UNITY_EDITOR        
        if (_map == null) {
            Initialize();
            return;
        }

        // Scan enum names for changes that force data to be remapped.
        var names = System.Enum.GetNames(typeof(TKey));
        // TODO: also validate that names are unique, and use consecutive indices starting at zero?
        if (names.Length != _map.Length) {
            // Update if the number of enum members changed.
            RemapData(names);
        } else {
            // Update if any of the names of enum members
            // changed or re-odered.
            for (int i = 0; i < names.Length; i++) {
                if (names[i] != _map[i].name) {
                    RemapData(names);
                    break;
                }
            }
        }
#endif       
    }

#if UNITY_EDITOR
    // Editor-only container for unmapped data - gives an opportunity to recover
    // after accidentally leaving an old name unmapped.
    // TODO: use PropertyDrawer to show/manually delete orphaned data.
    [SerializeField, HideInInspector] List<Entry> _orphaned;

    // Used to track re-mapping from old names to new.
    [System.Flags]
    enum SlotStatus {
        Unmapped = 0,
        OldEntryMatchedFromHere = 1 << 0,
        NewEntryMatchedHere = 1 << 1,
        NewEntryDefault = 1 << 2
    }

    // Called when the enum has changed, to move data to the best-matched slots.
    void RemapData(string[] names) {
        // Make a fresh array to handle any changes in size/order 
        // without unnecessary loss of data.
        var newMap = new Entry[names.Length];

        // Book-keeping to detect which old data has not been mapped.
        var slotStatus = new SlotStatus[Mathf.Max(newMap.Length, _map.Length)];
        int matches = 0;
        for (int i = 0; i < newMap.Length; i++) {
            Entry existingOrBlank;
            // Try to find existing data mapped to exactly the same enum value name.
            int matchIndex = Array.FindIndex(_map, item => item.name == names[i]);
            if (matchIndex >= 0) {
                // If we found it, insert it into the new map at its new index.
                existingOrBlank = _map[matchIndex];                
                slotStatus[matchIndex] |= SlotStatus.OldEntryMatchedFromHere;
                slotStatus[i] |= SlotStatus.NewEntryMatchedHere;
                matches++;
            } else {
                // Otherwise, check if we saved it in the orphanded list.
                matchIndex = _orphaned.FindIndex(item => item.name == names[i]);
                if (matchIndex >= 0) {
                    existingOrBlank = _orphaned[matchIndex];
                    _orphaned.RemoveAt(matchIndex);
                    slotStatus[i] |= SlotStatus.NewEntryMatchedHere;
                } else {
                    // Final fallback: make a new, blank entry for the new name.
                    existingOrBlank = default;
                    existingOrBlank.name = names[i];
                    slotStatus[i] |= SlotStatus.NewEntryDefault;
                }
            }
            newMap[i] = existingOrBlank;
        }

        if (matches < _map.Length) {
            // If some old data was not remapped to new, go find it!
            for (int i = 0; i < _map.Length; i++) {
                if ((slotStatus[i] & SlotStatus.OldEntryMatchedFromHere) == 0) {
                    // If no old data was mapped to the same integer slot as this unmapped data,
                    // assume it was just renamed in-place and copy its data to the new name.                  
                    if (i < newMap.Length && slotStatus[i] == SlotStatus.NewEntryDefault)  {
                        Debug.LogWarning($"Unmapped enum value {i}: {_map[i].name} found - assuming it should map to new name {names[i]}.");
                        newMap[i] = _map[i];
                        newMap[i].name = names[i];
                    } else {
                        // Otherwise, we don't know where this data goes. 
                        // Log an error that it's lost, for one of two reasons...
                        string problem = i < names.Length ? $"its slot has been mapped to {names[i]}" : "it's outside the range of the enum";
                        Debug.LogError($"Unmapped enum value {i}: {_map[i].name} found, but {problem}.");

                        // Cache unmaped data as an orphan.
                        string oldName = _map[i].name;
                        _orphaned.RemoveAll(item => item.name == oldName);
                        _orphaned.Add(_map[i]);
                    }
                }
            }
        }

        // Use the new map from now on.
        _map = newMap;
    }
#endif

    // Enabling use with ForEach:
    public struct Enumerator : IEnumerator<TValue>
    {
        Entry[] _map;
        int _index;

        public Enumerator(Entry[] map)
        {
            _map = map;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _map.Length;
        }

        public TValue Current => _map[_index].value;
        object IEnumerator.Current => Current;

        public void Reset()
        {
            _index = -1;
        }

        void IDisposable.Dispose() { }
    }

    public IEnumerator<TValue> GetEnumerator() => new Enumerator(_map);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}