﻿using System;
using System.Collections.Generic;

namespace TickTrader.SeriesStorage
{
    public static class Extentions
    {
        public static int BinarySearch<T, TVal>(this IList<T> list, Func<T, TVal> getter, TVal value,
            BinarySearchTypes type = BinarySearchTypes.Exact, IComparer<TVal> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<TVal>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, getter(list[middle]));
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            if (type == BinarySearchTypes.Exact)
                return -1;
            else if (type == BinarySearchTypes.NearestLower)
            {
                return lower > 0 ? lower - 1 : 0;
            }
            else // if (type == BinarySearchTypes.NearestHigher)
            {
                if (lower == 0)
                    return lower;
                else if (lower == list.Count)
                    return lower - 1;
                return lower;
            }
        }

        public static int BinarySearch<T>(this IList<T> list, T value,
            BinarySearchTypes type = BinarySearchTypes.Exact, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<T>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            if (type == BinarySearchTypes.Exact)
                return -1;
            else if (type == BinarySearchTypes.NearestLower)
            {
                return lower > 0 ? lower - 1 : 0;
            }
            else // if (type == BinarySearchTypes.NearestHigher)
                return lower;
        }

        public static ICollectionStorage<TKey, TValue> GetCollection<TKey, TValue>(
            this ISeriesDatabase storage, string name,
            IKeySerializer<TKey> keySerializer, IValueSerializer<TValue> valueSerializer)
        {
            var collection = storage.GetBinaryCollection(name, keySerializer);
            return new CollectionSerializer<TKey, TValue>(collection, valueSerializer);
        }
    }

    public enum BinarySearchTypes { NearestLower, Exact, NearestHigher }
}
