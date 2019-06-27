using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public static class VarCollection
    {
        public static IVarList<TResult> Select<TSource, TResult>(this IVarList<TSource> src, Func<TSource, TResult> selector)
        {
            var chain = src as ListChainToken<TSource>;

            if (chain != null)
                return new ListSelector<TSource, TResult>(chain.Src, selector, true);

            return new ListSelector<TSource, TResult>(src, selector, false);
        }

        public static IVarSet<TKey, TValue> TransformToDicionary<TKey, TValue>(this IVarSet<TKey> src, Func<TKey, TValue> selector)
        {
            return new SetToDicionaryOperator<TKey, TValue>(src, selector);
        }

        public static IVarList<TResult> TransformToList<TSource, TResult>(this IVarSet<TSource> src, Func<TSource, TResult> transformFunc)
        {
            return new SetToListOperator<TSource, TResult>(src, transformFunc);
        }

        public static IVarList<T> TransformToList<T>(this IVarSet<T> src)
        {
            return new SetToListOperator<T, T>(src, v => v);
        }

        /// <summary>
        /// Transforms dynamic collection into new one by applying selector function for each element.
        /// Note: Transform function is called once and only once for each element in source collection!
        /// Note: This operator contains inner dictionary to keep transformed values. This may affect performance. Use 'Select' operator for more lightweight operation.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="src">Source dynamic collection.</param>
        /// <param name="transformFunc">Transform function applied tp each source element. </param>
        /// <returns></returns>
        public static IVarSet<TResult> Transform<TSource, TResult>(this IVarSet<TSource> src, Func<TSource, TResult> transformFunc)
        {
            return new SetTransformOperator<TSource, TResult>(src, transformFunc);
        }

        /// <summary>
        /// Transforms dynamic collection into new one by applying selector function for each element.
        /// Note: Selector function can be called multiple times (or not called at all) for each element in source collection.
        /// Note: This operator does not contain inner collection. Use 'Transform' operator for more persistant behavior.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="src"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IVarSet<TResult> Select<TSource, TResult>(this IVarSet<TSource> src, Func<TSource, TResult> selector)
        {
            return new SetTransformOperator<TSource, TResult>(src, selector);
        }

        public static IVarList<T> Where<T>(this IVarList<T> src, Predicate<T> condition)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ListFilter<T>(chain.Src, condition, true);

            return new ListFilter<T>(src, condition, false);
        }

        public static IVarSet<T> Where<T>(this IVarSet<T> src, Predicate<T> condition)
        {
            return new SetFilter<T>(src, condition);
        }

        public static IVarList<T> ToList<T>(this IVarList<T> src)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ListCopy<T>(chain.Src, true);

            return new ListCopy<T>(src, false);
        }

        public static IVarList<TResult> TransformToList<TKey, TValue, TResult>(this IVarSet<TKey, TValue> src, Func<TKey, TValue, TResult> transformFunc)
        {
            return new DictionaryToListOperator<TKey, TValue, TResult>(src, transformFunc);
        }

        public static IVarList<TValue> TransformToList<TKey, TValue>(this IVarSet<TKey, TValue> src)
        {
            return new DictionaryToListOperator<TKey, TValue, TValue>(src, (k, v) => v);
        }

        public static IObservableList<T> AsObservable<T>(this IVarList<T> src)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ObservableWrapper2<T>(chain.Src, true);

            return new ObservableWrapper2<T>(src, false);
        }

        public static IObservableList<T> AsObservable<T>(this IVarSet<T> src)
        {
            return new SetObservableCollection<T>(src, false);
        }

        public static IVarList<T> Chain<T>(this IVarList<T> src)
        {
            return new ListChainToken<T>(src);
        }

        public static IVarList<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> src,
            Func<TSource, IVarList<TResult>> selector)
        {
            var srcAdapter = new ListAdapter<TSource>(src.ToList());
            return new ListComposition<TSource, TResult>(srcAdapter, selector, true);
        }

        public static IVarList<T> Combine<T>(params IVarList<T>[] collections)
        {
            var srcAdapter = new ListAdapter<IVarList<T>>(collections);
            return new ListComposition<IVarList<T>, T>(srcAdapter, c => c, false);
        }

        public static IVarList<T> CombineChained<T>(params IVarList<T>[] collections)
        {
            var srcAdapter = new ListAdapter<IVarList<T>>(collections);
            return new ListComposition<IVarList<T>, T>(srcAdapter, c => c, true);
        }

        public static IVarSet<T> Union<T>(params IVarSet<T>[] sets)
        {
            return new SetUnionOperator<T>(sets);
        }

        public static IVarList<TResult> SelectMany<TSource, TResult>(this IVarList<TSource> src,
            Func<TSource, IVarList<TResult>> selector)
        {
            var chain = src as ListChainToken<TSource>;

            if (chain != null)
                return new ListComposition<TSource, TResult>(chain.Src, selector, true);

            return new ListComposition<TSource, TResult>(src, selector, false);
        }

        public static IVarList<TResult> SelectMany<TSource, TResult>(this IVarList<TSource> src,
            Func<TSource, IEnumerable<TResult>> selector)
        {
            var token = src as ListChainToken<TSource>;

            IVarList<TSource> trueSrc = token == null ? src : token.Src;
            bool proporgate = token != null;

            Func<TSource, IVarList<TResult>> wrappingSelector = c => new ListAdapter<TResult>(selector(c).ToList());

            return new ListComposition<TSource, TResult>(trueSrc, wrappingSelector, proporgate);
        }

        public static IVarList<T> AsDynamic<T>(this IEnumerable<T> src)
        {
            return new ListAdapter<T>(src.ToList());
        }

        public static IVarSet<TKey, TValue> Where<TKey, TValue>(this IVarSet<TKey, TValue> src,
            Func<TKey, TValue, bool> condition)
        {
            return new DictionaryFilter<TKey, TValue>(src, condition);
        }

        public static IVarSet<TKey, TResult> Select<TKey, TSource, TResult>(
            this IVarSet<TKey, TSource> src,
            Func<TKey, TSource, TResult> selector)
        {
            return new DictionarySelector<TKey, TSource, TResult>(src, selector);
        }

        public static IVarSet<TResultKey, TResult> Select<TSourceKey, TResultKey, TSource, TResult>(
            this IVarSet<TSourceKey, TSource> src, Func<TSourceKey, TSource, KeyValuePair<TResultKey, TResult>> selector)
        {
            return new DictionarySelectorFull<TSourceKey, TResultKey, TSource, TResult>(src, selector);
        }

        public static IVarList<TValue> OrderBy<TKey, TValue, TBy>(
            this IVarSet<TKey, TValue> src,
            Func<TKey, TValue, TBy> orderPropSelector)
        {
            return OrderBy(src, orderPropSelector, Comparer<TBy>.Default);
        }

        public static IVarList<TValue> OrderBy<TKey, TValue, TBy>(
            this IVarSet<TKey, TValue> src,
            Func<TKey, TValue, TBy> orderPropSelector, IComparer<TBy> comparer)
        {
            return new DictionarySortOperator<TKey, TValue, TBy>(src, orderPropSelector, comparer);
        }

        public static IVarSet<TKey, TValue> SelectMany<TKey, TValue, TSource>(
            this IEnumerable<TSource> src,
            Func<TSource, IVarSet<TKey, TValue>> selector)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>.StaticCompositionSource<TSource>(src, selector);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IVarSet<TKey, TValue> SelectMany<TKey, TValue, TSource>(
            this IVarList<TSource> src,
            Func<TSource, IVarSet<TKey, TValue>> selector)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>.ListCompositionSource<TSource>(src, selector);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IVarSet<TKey, TValue> Combine<TKey, TValue>(
            params IVarSet<TKey, TValue>[] dictionaries)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>
                .StaticCompositionSource<IVarSet<TKey, TValue>>(dictionaries, i => i);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IVarSet<TGrouping, IVarGrouping<TKey, TValue, TGrouping>>
            GroupBy<TKey, TValue, TGrouping>(
            this IVarSet<TKey, TValue> src,
            Func<TKey, TValue, TGrouping> groupingKeySelector)
        {
            return new DictionaryGrouping<TKey, TValue, TGrouping>(src, groupingKeySelector);
        }

        public static void EnableAutodispose<TKey, TVal>(this IVarSet<TKey, TVal> src)
            where TVal : IDisposable
        {
            src.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove)
                    a.OldItem.Dispose();
                else if (a.Action == DLinqAction.Replace)
                    a.OldItem.Dispose();
                else if (a.Action == DLinqAction.Dispose)
                {
                    foreach (var i in a.Sender.Snapshot.Values)
                        i.Dispose();
                }
            };
        }

        public static TValue GetOrDefault<Tkey, TValue>(this IVarSet<Tkey, TValue> set, Tkey key)
        {
            TValue val;
            set.Snapshot.TryGetValue(key, out val);
            return val;
        }
    }
}
