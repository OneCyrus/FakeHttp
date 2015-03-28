﻿using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    public sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly IReadonlyResponseStore _store;

        public MockHttpMessageHandler(IReadonlyResponseStore store)
        {
            _store = store;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _store.FindResponse(request);
        }
    }
}
