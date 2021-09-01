using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuse.Controls
{
    public static class SortedDictionaryExtensions
	{
		private static Tuple<int, int> GetPossibleIndices<TKey, TValue>(SortedDictionary<TKey, TValue> dictionary, TKey key, bool strictlyDifferent, out List<TKey> list)
		{
			list = dictionary.Keys.ToList();
			var index = list.BinarySearch(key, dictionary.Comparer);
			if (index >= 0)
			{
				// exists
				return strictlyDifferent ? Tuple.Create(index - 1, index + 1) : Tuple.Create(index, index);
			}

			// doesn't exist
			var indexOfBiggerNeighbour = ~index; //bitwise complement of the return Value

			if (indexOfBiggerNeighbour == list.Count)
			{
				// bigger than all elements
				return Tuple.Create(list.Count - 1, list.Count);
			}

			if (indexOfBiggerNeighbour == 0)
			{
				// smaller than all elements
				return Tuple.Create(-1, 0);
			}

			// Between 2 elements
			var indexOfSmallerNeighbour = indexOfBiggerNeighbour - 1;
			return Tuple.Create(indexOfSmallerNeighbour, indexOfBiggerNeighbour);
		}

		public static TKey LowerKey<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (item1, _) = GetPossibleIndices(dictionary, key, true, out var list);
			return item1 < 0 ? default : list[item1];
		}
		public static KeyValuePair<TKey, TValue> LowerEntry<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (item1, _) = GetPossibleIndices(dictionary, key, true, out var list);
			if (item1 < 0)
				return default;

			var newKey = list[item1];
			return new KeyValuePair<TKey, TValue>(newKey, dictionary[newKey]);
		}
	
		public static TKey FloorKey<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (item1, _) = GetPossibleIndices(dictionary, key, false, out var list);
			return item1 < 0 ? default : list[item1];
		}
		public static KeyValuePair<TKey, TValue> FloorEntry<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (item1, _) = GetPossibleIndices(dictionary, key, false, out var list);
			if (item1 < 0)
				return default;

			var newKey = list[item1]; 
			return new KeyValuePair<TKey, TValue>(newKey, dictionary[newKey]);
		}

		public static TKey CeilingKey<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (_, item2) = GetPossibleIndices(dictionary, key, false, out var list);
			if (item2 == list.Count)
				return default(TKey);

			return list[item2];
		}
		public static KeyValuePair<TKey, TValue> CeilingEntry<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var (_, item2) = GetPossibleIndices(dictionary, key, false, out var list);
			if (item2 == list.Count)
				return default(KeyValuePair<TKey, TValue>);

			var newKey = list[item2];
			return new KeyValuePair<TKey, TValue>(newKey, dictionary[newKey]);
		}

		public static TKey HigherKey<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			var indices = GetPossibleIndices(dictionary, key, true, out var list);
			return indices.Item2 == list.Count ? default(TKey) : list[indices.Item2];
		}
		public static KeyValuePair<TKey, TValue> HigherEntry<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key)
		{
			List<TKey> list;
			var indices = GetPossibleIndices(dictionary, key, true, out list);
			if (indices.Item2 == list.Count)
				return default(KeyValuePair<TKey, TValue>);

			var newKey = list[indices.Item2];
			return new KeyValuePair<TKey, TValue>(newKey, dictionary[newKey]);
		}
	}
}