using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Models;

namespace Qbicles.BusinessRules.Helper
{
    public static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T obj in collection)
                action(obj);
        }

        public static bool ContainsSameObjectsAs<T>(
          this IEnumerable<T> collection,
          IEnumerable<T> collection2)
        {
            if (collection.Count<T>() == collection2.Count<T>())
                return collection.Except<T>(collection2).IsEmpty<T>();
            return false;
        }

        public static bool ContainsSameObjectsAs<T>(
          this T[] collection,
          T[] collection2,
          bool enforceOrder = false)
          where T : IEquatable<T>
        {
            if (collection.Length != collection2.Length)
                return false;
            if (!enforceOrder)
                return ((IEnumerable<T>)collection).Except<T>((IEnumerable<T>)collection2).IsEmpty<T>();
            for (int index = 0; index < collection.Length; ++index)
            {
                if (!collection[index].Equals(collection2[index]))
                    return false;
            }
            return true;
        }

        public static bool ContainsSameObjectsAs<T>(
          this IList<T> collection,
          IList<T> collection2,
          bool enforceOrder = false)
          where T : IEquatable<T>
        {
            if (collection.Count != collection2.Count)
                return false;
            if (!enforceOrder)
                return collection.Except<T>((IEnumerable<T>)collection2).IsEmpty<T>();
            for (int index = 0; index < collection.Count; ++index)
            {
                if (!collection[index].Equals(collection2[index]))
                    return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection != null)
                return !collection.Any<T>();
            return true;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any<T>();
        }

        public static TSource JustOneOrDefault<TSource>(
          this IEnumerable<TSource> source,
          Func<TSource, bool> isMatching)
          where TSource : class
        {
            if (source == null)
                throw new ArgumentNullException("The source parameter cannot be null.");
            TSource source1 = default(TSource);
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TSource current = enumerator.Current;
                    if (isMatching(current))
                    {
                        if ((object)source1 != null)
                            return default(TSource);
                        source1 = current;
                    }
                }
                return source1;
            }
        }

        public static TSource JustOneOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source parameter cannot be null.");
            IList<TSource> sourceList = source as IList<TSource>;
            if (sourceList != null)
            {
                switch (sourceList.Count)
                {
                    case 0:
                        return default(TSource);
                    case 1:
                        return sourceList[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                        return default(TSource);
                    TSource current = enumerator.Current;
                    if (!enumerator.MoveNext())
                        return current;
                }
            }
            return default(TSource);
        }

        public static IEnumerable<TSource> Distinct<TSource>(
          this IEnumerable<TSource> source,
          Func<TSource, TSource, bool> areTheSame)
          where TSource : class
        {
            TSource[] array = source.ToArray<TSource>();
            List<TSource> source1 = new List<TSource>();
            foreach (TSource source2 in array)
            {
                TSource element = source2;
                if (!source1.Any<TSource>((Func<TSource, bool>)(x =>
                {
                    if ((object)x != (object)element)
                        return areTheSame(x, element);
                    return true;
                })))
                    source1.Add(element);
            }
            return (IEnumerable<TSource>)source1;
        }

        public static IEnumerable<TSource> SafeConcat<TSource>(
          this IEnumerable<TSource> source,
          IEnumerable<TSource> target,
          params IEnumerable<TSource>[] additionalTargets)
        {
            List<TSource> sourceList = new List<TSource>();
            if (source != null)
                sourceList.AddRange(source);
            if (target != null)
                sourceList.AddRange(target);
            if (additionalTargets != null)
            {
                foreach (IEnumerable<TSource> collection in ((IEnumerable<IEnumerable<TSource>>)additionalTargets).Where<IEnumerable<TSource>>((Func<IEnumerable<TSource>, bool>)(x => x != null)))
                    sourceList.AddRange(collection);
            }
            return (IEnumerable<TSource>)sourceList;
        }

        public static void AddRangeSafely<TSource>(
          this List<TSource> source,
          IEnumerable<TSource> newElements)
        {
            if (newElements == null)
                return;
            source.AddRange(newElements);
        }

        public static IUserIdentifier[] GetDistinctUserIdentifiers(
          IEnumerable<IUserIdentifier> targetUserIdentifiers)
        {
            return targetUserIdentifiers.Distinct<IUserIdentifier>((Func<IUserIdentifier, IUserIdentifier, bool>)((identifier1, identifier2) =>
            {
                bool flag1 = identifier1.Id == identifier2.Id && identifier1.Id != -1L;
                if (string.IsNullOrEmpty(identifier1.ScreenName) || string.IsNullOrEmpty(identifier2.ScreenName))
                    return flag1;
                bool flag2 = identifier1.ScreenName.ToLowerInvariant() == identifier2.ScreenName.ToLowerInvariant();
                return flag1 | flag2;
            })).ToArray<IUserIdentifier>();
        }
    }
}
