﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.Ignite.Core.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using Apache.Ignite.Core.Binary;
    using Apache.Ignite.Core.Client.Cache;
    using Apache.Ignite.Core.Client.Compute;
    using Apache.Ignite.Core.Client.Datastream;
    using Apache.Ignite.Core.Client.DataStructures;
    using Apache.Ignite.Core.Client.Services;
    using Apache.Ignite.Core.Client.Transactions;

    /// <summary>
    /// Main entry point for Ignite Thin Client APIs.
    /// You can obtain an instance of <see cref="IIgniteClient"/> through one of the
    /// <see cref="Ignition.StartClient()"/> overloads.
    /// <para />
    /// Instances of this class and all nested APIs are thread safe.
    /// </summary>
    public interface IIgniteClient : IDisposable
    {
        /// <summary>
        /// Gets the cache instance for the given name to work with keys and values of specified types.
        /// <para/>
        /// You can get instances of <see cref="ICacheClient{TK,TV}"/> of the same name,
        /// but with different key/value types.
        /// These will use the same named cache, but only allow working with entries of specified types.
        /// Attempt to retrieve an entry of incompatible type will result in <see cref="InvalidCastException"/>.
        /// Use <see cref="GetCache{TK,TV}"/> in order to work with entries of arbitrary types.
        /// </summary>
        /// <param name="name">Cache name.</param>
        /// <returns>Cache instance for given name.</returns>
        /// <typeparam name="TK">Cache key type.</typeparam>
        /// <typeparam name="TV">Cache value type.</typeparam>
        ICacheClient<TK, TV> GetCache<TK, TV>(string name);

        /// <summary>
        /// Gets existing cache with the given name or creates new one using template configuration.
        /// </summary>
        /// <typeparam name="TK">Cache key type.</typeparam>
        /// <typeparam name="TV">Cache value type.</typeparam>
        /// <param name="name">Cache name.</param>
        /// <returns>Existing or newly created cache.</returns>
        ICacheClient<TK, TV> GetOrCreateCache<TK, TV>(string name);

        /// <summary>
        /// Gets existing cache with the given name or creates new one using provided configuration.
        /// </summary>
        /// <typeparam name="TK">Cache key type.</typeparam>
        /// <typeparam name="TV">Cache value type.</typeparam>
        /// <param name="configuration">Cache configuration.</param>
        /// <returns>Existing or newly created cache.</returns>
        ICacheClient<TK, TV> GetOrCreateCache<TK, TV>(CacheClientConfiguration configuration);

        /// <summary>
        /// Dynamically starts new cache using template configuration.
        /// </summary>
        /// <typeparam name="TK">Cache key type.</typeparam>
        /// <typeparam name="TV">Cache value type.</typeparam>
        /// <param name="name">Cache name.</param>
        /// <returns>Existing or newly created cache.</returns>
        ICacheClient<TK, TV> CreateCache<TK, TV>(string name);

        /// <summary>
        /// Dynamically starts new cache using provided configuration.
        /// </summary>
        /// <typeparam name="TK">Cache key type.</typeparam>
        /// <typeparam name="TV">Cache value type.</typeparam>
        /// <param name="configuration">Cache configuration.</param>
        /// <returns>Existing or newly created cache.</returns>
        ICacheClient<TK, TV> CreateCache<TK, TV>(CacheClientConfiguration configuration);

        /// <summary>
        /// Gets the collection of names of currently available caches, or empty collection if there are no caches.
        /// </summary>
        /// <returns>Collection of names of currently available caches.</returns>
        ICollection<string> GetCacheNames();

        /// <summary>
        /// Gets Ignite cluster.
        /// </summary>
        /// <returns>Instance of <see cref="IClientCluster" /> interface.</returns>
        IClientCluster GetCluster();

        /// <summary>
        /// Destroys dynamically created (with <see cref="CreateCache{TK,TV}(string)"/> or
        /// <see cref="GetOrCreateCache{TK,TV}(string)"/>) cache.
        /// </summary>
        /// <param name="name">The name of the cache to stop.</param>
        void DestroyCache(string name);

        /// <summary>
        /// Gets Ignite binary services.
        /// </summary>
        /// <returns>Instance of <see cref="IBinary"/> interface</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Semantics.")]
        IBinary GetBinary();

        /// <summary>
        /// Gets Ignite transactions facade <see cref="ITransactionsClient"/>.
        /// <para /> Transactions are bound to the thread started the transaction. After that, each cache operation within this thread
        /// will belong to the corresponding transaction until the transaction is committed, rolled back or closed.
        /// <para /> Should not be used with async calls.
        /// </summary>
        ITransactionsClient GetTransactions();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Semantics.")]
        IgniteClientConfiguration GetConfiguration();

        /// <summary>
        /// Gets the current remote EndPoint.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Consistency with EndPoint class name.")]
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the current local EndPoint.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Consistency with EndPoint class name.")]
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets all active connections. Ignite Thin Client maintains connections to multiple server nodes when
        /// <see cref="IgniteClientConfiguration.EnablePartitionAwareness"/> is true.
        /// </summary>
        IEnumerable<IClientConnection> GetConnections();

        /// <summary>
        /// Gets the compute API.
        /// </summary>
        IComputeClient GetCompute();

        /// <summary>
        /// Gets the services API.
        /// </summary>
        IServicesClient GetServices();

        /// <summary>
        /// Gets a new instance of the data streamer associated with the given cache name.
        /// <para />
        /// Data streamer loads data efficiently into cache.
        /// See <see cref="IDataStreamerClient{TK,TV}"/> for more information.
        /// </summary>
        /// <param name="cacheName">Cache name.</param>
        /// <returns>Data streamer.</returns>
        IDataStreamerClient<TK, TV> GetDataStreamer<TK, TV>(string cacheName);

        /// <summary>
        /// Gets a new instance of the data streamer associated with the given cache name.
        /// <para />
        /// Data streamer loads data efficiently into cache.
        /// See <see cref="IDataStreamerClient{TK,TV}"/> for more information.
        /// </summary>
        /// <param name="cacheName">Cache name.</param>
        /// <param name="options">Data streamer options.</param>
        /// <returns>Data streamer.</returns>
        IDataStreamerClient<TK, TV> GetDataStreamer<TK, TV>(string cacheName, DataStreamerClientOptions options);

        /// <summary>
        /// Gets a new instance of the data streamer associated with the given cache name.
        /// <para />
        /// Data streamer loads data efficiently into cache.
        /// See <see cref="IDataStreamerClient{TK,TV}"/> for more information.
        /// </summary>
        /// <param name="cacheName">Cache name.</param>
        /// <param name="options">Data streamer options.</param>
        /// <returns>Data streamer.</returns>
        IDataStreamerClient<TK, TV> GetDataStreamer<TK, TV>(string cacheName, DataStreamerClientOptions<TK, TV> options);

        /// <summary>
        /// Gets an atomic long with the specified name.
        /// Creates a new atomic long if it does not exist and <paramref name="create"/> is true.
        /// </summary>
        /// <param name="name">Name of the atomic long.</param>
        /// <param name="initialValue">
        /// Initial value for the atomic long. Ignored if <paramref name="create"/> is false.
        /// </param>
        /// <param name="create">Flag indicating whether atomic long should be created if it does not exist.</param>
        /// <returns>Atomic long instance with the specified name,
        /// or null if it does not exist and <paramref name="create"/> is <c>false</c>.</returns>
        IAtomicLongClient GetAtomicLong(string name, long initialValue, bool create);

        /// <summary>
        /// Gets an atomic long with the specified name.
        /// Creates a new atomic long if it does not exist and <paramref name="create"/> is true.
        /// </summary>
        /// <param name="name">Name of the atomic long.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="initialValue">
        /// Initial value for the atomic long. Ignored if <paramref name="create"/> is false.
        /// </param>
        /// <param name="create">Flag indicating whether atomic long should be created if it does not exist.</param>
        /// <returns>Atomic long instance with the specified name,
        /// or null if it does not exist and <paramref name="create"/> is <c>false</c>.</returns>
        IAtomicLongClient GetAtomicLong(
            string name,
            AtomicClientConfiguration configuration,
            long initialValue,
            bool create);
    }
}
