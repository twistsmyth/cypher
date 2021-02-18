﻿// CYPCore by Matthew Hellyer is licensed under CC BY-NC-ND 4.0.
// To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-nd/4.0

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Dawn;
using CYPCore.Models;

namespace CYPCore.Persistence
{
    public class MemPoolRepository : Repository<MemPoolProto>, IMemPoolRepository
    {
        private readonly ILogger _logger;

        public MemPoolRepository(IStoreDb storeDb, ILogger logger)
            : base(storeDb, logger)
        {
            _logger = logger;

            SetTableName(StoreDb.MemoryPoolTable.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memPools"></param>
        /// <returns></returns>
        public async Task<List<MemPoolProto>> HasMoreAsync(MemPoolProto[] memPools)
        {
            Guard.Argument(memPools, nameof(memPools)).NotNull().NotEmpty();

            var moreBlocks = new List<MemPoolProto>();

            try
            {
                foreach (var next in memPools)
                {
                    var hasNext = await WhereAsync(x =>
                        new ValueTask<bool>(x.Block.Hash.Equals(next.Block.Hash)));

                    IEnumerable<(MemPoolProto nNext, MemPoolProto included)> enumerable()
                    {
                        foreach (var nNext in hasNext)
                        {
                            var included = moreBlocks
                                .FirstOrDefault(x => x.Block.Hash.Equals(nNext.Block.Hash)
                                                     && x.Block.Node == nNext.Block.Node
                                                     && x.Block.Round == nNext.Block.Round);

                            if (included is not null && included.Included)
                                continue;

                            if (included?.Block.Signature != null && included.Block.PublicKey != null)
                            {
                                yield return (nNext, included);
                            }
                        }
                    }

                    foreach (var (nNext, included) in enumerable())
                    {
                        if (included != null)
                            continue;

                        moreBlocks.Add(nNext);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"<<< MemPoolRepository.MoreAsync >>>: {e}");
            }

            return moreBlocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memPools"></param>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        public async Task IncludeAsync(MemPoolProto[] memPools, ulong currentNode)
        {
            Guard.Argument(memPools, nameof(memPools)).NotNull().NotEmpty();
            Guard.Argument(currentNode, nameof(currentNode)).NotNegative();

            try
            {
                foreach (var next in memPools.Where(x => x.Block.Node == currentNode))
                {
                    next.Included = true;
                    await PutAsync(next.ToIdentifier(), next);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"<<< BlockGraphRepository.IncludeAllAsync >>>: {e}");
            }

            return;
        }
    }
}