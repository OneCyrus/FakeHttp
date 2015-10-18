﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace FakeHttp
{
    public sealed class AutomaticHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store">The storage meachansim for responses</param>
        public AutomaticHttpClientHandler(IResponseStore store)
        {
            if (store == null) throw new ArgumentNullException("store");

            _store = store;
        }

        /// <summary>
        /// Override the base class to capture and store the response message
        /// if it doesn't already exist in storage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if the response exists in the store go get it from there
            if (await _store.ResponseExists(request))
            {
                return await _store.FindResponse(request);
            }

            // otherwise get it from the actual endpoint, store it and return it
            var response = await base.SendAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await _store.StoreResponse(response);

            return response;
        }
    }
}
