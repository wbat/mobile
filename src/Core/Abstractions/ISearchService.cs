﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Models.View;

namespace Bit.Core.Abstractions
{
    public interface ISearchService
    {
        void ClearIndex();
        Task IndexCiphersAsync();
        bool IsSearchable(string query);
        Task<List<CipherView>> SearchCiphersAsync(string query, Func<CipherView, bool> filter = null,
            List<CipherView> ciphers = null, CancellationToken ct = default(CancellationToken));
        List<CipherView> SearchCiphersBasic(List<CipherView> ciphers, string query,
            CancellationToken ct = default(CancellationToken));
    }
}