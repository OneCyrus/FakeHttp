﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// A serializatoin frie3ndly wrapper around <see cref="System.Net.Http.HttpResponseMessage"/>
    /// </summary>
    public sealed class ResponseInfo
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ResponseInfo()
        {
            ResponseHeaders = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
            ContentHeaders = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The query string from the request that generated the response (used to key the response for future reference)
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The name of the associated serialized content file
        /// </summary>
        public string ContentFileName { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; }

        /// <summary>
        /// The content headers
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpResponseMessage"/> from the object's state
        /// </summary>
        /// <returns>The <see cref="System.Net.Http.HttpResponseMessage"/></returns>
        public HttpResponseMessage CreateResponse()
        {
            var response = new HttpResponseMessage(StatusCode);
            foreach (var kvp in ResponseHeaders)
            {
                response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return response;
        }

        /// <summary>
        /// Creates an <see cref="System.Net.Http.HttpContent"/> object from a stream, setting content headers
        /// </summary>
        /// <param name="stream">The content stream</param>
        /// <returns>The conent object</returns>
        public HttpContent CreateContent(Stream stream)
        {
            var content = new StreamContent(stream);
            foreach (var kvp in ContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return content;
        }
    }
}
