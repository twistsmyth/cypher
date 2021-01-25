﻿// CYPCore by Matthew Hellyer is licensed under CC BY-NC-ND 4.0.
// To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-nd/4.0

using System.Threading.Tasks;

using Refit;

using CYPCore.Models;

namespace CYPCore.Services.Rest
{
    public interface IBlockRestAPI
    {
        [Get("/api/blocks/height")]
        Task<BlockHeight> Height();

        [Get("/api/blocks/range/{skip}/{take}")]
        Task<Block> Range(int skip, int take);
    }
}