using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SupervisorProcessing.Utils
{
    /// <summary>
    /// Extension methods for <see cref="System.Collections.Concurrent.ConcurrentBag{T}"/>.
    /// </summary>
    public static class ConcurrentBagExtensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the <see cref="ConcurrentBag{T}"/>,
        /// </summary>
        /// <typeparam name="T">
        /// The type T of the values of the <see cref="ConcurrentBag{T}"/>.
        /// </typeparam>
        /// <param name="@this">
        /// The <see cref="ConcurrentBag{T}"/> to which the values shall be added.
        /// </param>
        /// <param name="collection_">
        /// The collection whose elements should be added to the end of the <see cref="ConcurrentBag{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null,
        /// if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException">values is null.</exception>
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> collection_)
        {
            // The given collection may not be null.
            if (collection_ == null)
                throw new ArgumentNullException(nameof(collection_));

            foreach (var item in collection_)
            {
                @this.Add(item);
            }
        }
    }
}