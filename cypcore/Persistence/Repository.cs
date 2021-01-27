﻿// CYPCore by Matthew Hellyer is licensed under CC BY-NC-ND 4.0.
// To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-nd/4.0

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using FASTER.core;


namespace CYPCore.Persistence
{
    public class Repository<TEntity> : IRepository<TEntity>
    {
        private readonly IStoredbContext _storedbContext;
        private readonly ILogger _logger;

        private string _tableType;

        public Repository(IStoredbContext storedbContext, ILogger logger)
        {
            _storedbContext = storedbContext;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public void SetTableType(string table)
        {
            _tableType = table;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<TEntity> GetAsync(byte[] key)
        {
            TEntity entity = default;

            try
            {
                using var session = _storedbContext.Store.Database.NewSession(new StoreFunctions());

                var output = new StoreOutput();
                var blockKey = new StoreKey { tableType = _tableType, key = key };
                var readStatus = session.Read(ref blockKey, ref output);

                if (readStatus == Status.OK)
                {
                    entity = Helper.Util.DeserializeProto<TEntity>(output.value.value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.GetAsync >>>: {ex}");
            }

            return Task.FromResult(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<TEntity> PutAsync(TEntity entity, byte[] key)
        {
            TEntity entityPut = default;

            try
            {
                using var session = _storedbContext.Store.Database.NewSession(new StoreFunctions());

                var blockKey = new StoreKey { tableType = _tableType, key = key };
                var blockvalue = new StoreValue { value = Helper.Util.SerializeProto(entity) };

                var addStatus = session.Upsert(ref blockKey, ref blockvalue);
                if (addStatus == Status.OK)
                {
                    entityPut = entity;
                }

                session.CompletePending(true);
                _storedbContext.Store.Checkpoint().Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< StoredbContext.SaveOrUpdate >>>: {ex}");
            }

            return Task.FromResult(entityPut);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<int> CountAsync()
        {
            int count = 0;

            try
            {
                using var iterateAsync = CreateIterateAsync();
                count = iterateAsync.Iterate().CountAsync().Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.CountAsync >>>: {ex}");
            }

            return Task.FromResult(count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> WhereAsync(Func<TEntity, ValueTask<bool>> expression)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().WhereAwait(expression).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.WhereAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<TEntity> FirstOrDefaultAsync()
        {
            TEntity entity = default;

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entity = iterateAsync.Iterate().FirstOrDefaultAsync().Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.FirstOrDefaultAsync >>>: {ex}");
            }

            return Task.FromResult(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> expression)
        {
            TEntity entity = default;

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entity = iterateAsync.Iterate().FirstOrDefaultAsync(expression).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.FirstOrDefaultAsync(Func) >>>: {ex}");
            }

            return Task.FromResult(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<TEntity> LastOrDefaultAsync(Func<TEntity, bool> expression)
        {
            TEntity entity = default;

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entity = iterateAsync.Iterate().LastOrDefaultAsync(expression).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.LastOrDefaultAsync >>>: {ex}");
            }

            return Task.FromResult(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<TEntity> LastOrDefaultAsync()
        {
            TEntity entity = default;

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entity = iterateAsync.Iterate().LastOrDefaultAsync().Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.LastOrDefaultAsync >>>: {ex}");
            }

            return Task.FromResult(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        public Task<bool> DeleteAsync(StoreKey storeKey)
        {
            bool result = false;

            try
            {
                using var session = _storedbContext.Store.Database.NewSession(new StoreFunctions());
                var deleteStatus = session.Delete(ref storeKey);

                if (deleteStatus != Status.OK)
                    throw new Exception();

                result = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.DeleteAsync >>>: {ex}");
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> TakeAsync(int take)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().Take(take).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.TakeAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> RangeAsync(int skip, int take)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().Skip(skip).Take(take).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.RangeAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> TakeLastAsync(int n)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().TakeLast(n).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.TakeLastAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> TakeWhileAsync(Func<TEntity, ValueTask<bool>> expression)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().TakeWhileAwait(expression).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.TakeWhileAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> SelectAsync(Func<TEntity, ValueTask<TEntity>> selector)
        {
            var entities = Enumerable.Empty<TEntity>();

            try
            {
                using var iterateAsync = CreateIterateAsync();
                entities = iterateAsync.Iterate().SelectAwait(selector).ToEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError($"<<< Repository.SelectAsync >>>: {ex}");
            }

            return Task.FromResult(entities);
        }

        protected class IterateAsyncWrapper : IDisposable
        {
            private IFasterScanIterator<StoreKey, StoreValue> _iterator;
            private string _tableType;

            public IterateAsyncWrapper(IStoredbContext context, string tableType)
            {
                _iterator = context.Store.Database.Iterate();
                _tableType = tableType;
            }

            public async IAsyncEnumerable<TEntity> Iterate()
            {
                while (_iterator.GetNext(out RecordInfo recordInfo, out StoreKey storeKey, out StoreValue storeValue))
                {
                    if (storeKey.tableType == _tableType)
                    {
                        yield return Helper.Util.DeserializeProto<TEntity>(storeValue.value);
                    }
                }
            }

            public void Dispose()
            {
                _iterator.Dispose();
            }
        }

        protected IterateAsyncWrapper CreateIterateAsync()
        {
            return new (_storedbContext, _tableType);
        }
    }
}