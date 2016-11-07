using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public static class Dynamic
    {
        public static IDynamicListSource<TResult> Select<TSource, TResult>(this IDynamicListSource<TSource> src, Func<TSource, TResult> selector)
        {
            var chain = src as ListChainToken<TSource>;

            if (chain != null)
                return new ListSelector<TSource, TResult>(chain.Src, selector, true);

            return new ListSelector<TSource, TResult>(src, selector, false);
        }

        public static IDynamicListSource<T> Where<T>(this IDynamicListSource<T> src, Predicate<T> condition)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ListFilter<T>(chain.Src, condition, true);

            return new ListFilter<T>(src, condition, false);
        }

        public static IDynamicListSource<T> ToList<T>(this IDynamicListSource<T> src)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ListCopy<T>(chain.Src, true);

            return new ListCopy<T>(src, false);
        }

        public static IObservableListSource<T> AsObservable<T>(this IDynamicListSource<T> src)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ObservableWrapper2<T>(chain.Src, true);

            return new ObservableWrapper2<T>(src, false);
        }

        public static IDynamicListSource<T> Chain<T>(this IDynamicListSource<T> src)
        {
            return new ListChainToken<T>(src);
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> src,
            Func<TSource, IDynamicListSource<TResult>> selector)
        {
            var srcAdapter = new ListAdapter<TSource>(src.ToList());
            return new ListComposition<TSource, TResult>(srcAdapter, selector, true);
        }

        public static IDynamicListSource<T> Combine<T>(params IDynamicListSource<T>[] collections)
        {
            var srcAdapter = new ListAdapter<IDynamicListSource<T>>(collections);
            return new ListComposition<IDynamicListSource<T>, T>(srcAdapter, c => c, false);
        }

        public static IDynamicListSource<T> CombineChained<T>(params IDynamicListSource<T>[] collections)
        {
            var srcAdapter = new ListAdapter<IDynamicListSource<T>>(collections);
            return new ListComposition<IDynamicListSource<T>, T>(srcAdapter, c => c, true);
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IDynamicListSource<TSource> src,
            Func<TSource, IDynamicListSource<TResult>> selector)
        {
            var chain = src as ListChainToken<TSource>;

            if (chain != null)
                return new ListComposition<TSource, TResult>(chain.Src, selector, true);

            return new ListComposition<TSource, TResult>(src, selector, false);
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IDynamicListSource<TSource> src,
            Func<TSource, IEnumerable<TResult>> selector)
        {
            var token = src as ListChainToken<TSource>;

            IDynamicListSource<TSource> trueSrc = token == null ? src : token.Src;
            bool proporgate = token != null;

            Func<TSource, IDynamicListSource<TResult>> wrappingSelector = c => new ListAdapter<TResult>(selector(c).ToList());

            return new ListComposition<TSource, TResult>(trueSrc, wrappingSelector, proporgate);
        }

        public static IDynamicListSource<T> AsDynamic<T>(this IEnumerable<T> src)
        {
            return new ListAdapter<T>(src.ToList());
        }

        public static IDynamicDictionarySource<TKey, TValue> Where<TKey, TValue>(this IDynamicDictionarySource<TKey, TValue> src,
            Func<TKey, TValue, bool> condition)
        {
            return new DictionaryFilter<TKey, TValue>(src, condition);
        }

        public static IDynamicDictionarySource<TKey, TResult> Select<TKey, TSource, TResult>(
            this IDynamicDictionarySource<TKey, TSource> src,
            Func<TKey, TSource, TResult> selector)
        {
            return new DictionarySelector<TKey, TSource, TResult>(src, selector);
        }

        public static IDynamicListSource<TValue> OrderBy<TKey, TValue, TBy>(
            this IDynamicDictionarySource<TKey, TValue> src,
            Func<TKey, TValue, TBy> orderPropSelector)
        {
            return OrderBy(src, orderPropSelector, Comparer<TBy>.Default);
        }

        public static IDynamicListSource<TValue> OrderBy<TKey, TValue, TBy>(
            this IDynamicDictionarySource<TKey, TValue> src,
            Func<TKey, TValue, TBy> orderPropSelector, IComparer<TBy> comparer)
        {
            return new DictionarySorter<TKey, TValue, TBy>(src, orderPropSelector, comparer);
        }

        public static IDynamicDictionarySource<TKey, TValue> SelectMany<TKey, TValue, TSource>(
            this IEnumerable<TSource> src,
            Func<TSource, IDynamicDictionarySource<TKey, TValue>> selector)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>.StaticCompositionSource<TSource>(src, selector);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IDynamicDictionarySource<TKey, TValue> SelectMany<TKey, TValue, TSource>(
            this IDynamicListSource<TSource> src,
            Func<TSource, IDynamicDictionarySource<TKey, TValue>> selector)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>.ListCompositionSource<TSource>(src, selector);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IDynamicDictionarySource<TKey, TValue> Combine<TKey, TValue>(
            params IDynamicDictionarySource<TKey, TValue>[] dictionaries)
        {
            var compositionSrc = new DictionaryComposition<TKey, TValue>
                .StaticCompositionSource<IDynamicDictionarySource<TKey, TValue>>(dictionaries, i => i);
            return new DictionaryComposition<TKey, TValue>(compositionSrc);
        }

        public static IDynamicDictionarySource<TGrouping, IDynamicDictionaryGrouping<TKey, TValue, TGrouping>>
            GroupBy<TKey, TValue, TGrouping>(
            this IDynamicDictionarySource<TKey, TValue> src,
            Func<TKey, TValue, TGrouping> groupingKeySelector)
        {
            return new DictionaryGrouping<TKey, TValue, TGrouping>(src, groupingKeySelector);
        }

        // <summary>
        /// Note: Supplied collection must be empty!
        /// Note: You should not update supplied collection in any other way or from any other source!
        /// </summary>
        public static IDisposable ConnectTo<T>(this IDynamicListSource<T> src, IList target)
        {
            var chain = src as ListChainToken<T>;

            if (chain != null)
                return new ListConnector<T>(chain.Src, target) { PropogateDispose = true };

            return new ListConnector<T>(src, target);
        }
    }
}
