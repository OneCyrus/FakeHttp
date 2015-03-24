﻿using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

namespace MockHttp
{
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly string _storeFolder;
        private readonly string _captureFolder;
        private readonly ResponseDeserializer _deserializer = new ResponseDeserializer();
        private readonly Func<string, string, bool> _paramFilter;

        public FileSystemResponseStore(string storeFolder)
            : this(storeFolder, storeFolder)
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder)
            : this(storeFolder, captureFolder, (key, value) => false)
        {

        }

        public FileSystemResponseStore(string storeFolder, Func<string, string, bool> paramFilter)
            : this(storeFolder, storeFolder, paramFilter)
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder, Func<string, string, bool> paramFilter)
        {
            if (paramFilter == null)
            {
                throw new ArgumentNullException("paramFilter");
            }

            _storeFolder = storeFolder;
            _captureFolder = captureFolder;
            _paramFilter = paramFilter;
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var query = request.RequestUri.NormalizeQuery(_paramFilter);

            // first try to find a file keyed to the http method and query
            var response = await _deserializer.DeserializeResponse(Path.Combine(_storeFolder, request.RequestUri.ToFilePath()), request.ToFileName(query));
            if (response != null)
            {
                return response;
            }

            // next just look for a default response based on just the http method
            response = await _deserializer.DeserializeResponse(Path.Combine(_storeFolder, request.RequestUri.ToFilePath()), request.ToShortFileName());
            if (response != null)
            {
                return response;
            }

            // otherwise return 404            
            return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    RequestMessage = request
                };
        }

        public async Task StoreResponse(HttpResponseMessage response)
        {
            var query = response.RequestMessage.RequestUri.NormalizeQuery(_paramFilter);

            var path = Path.Combine(_captureFolder, response.RequestMessage.RequestUri.ToFilePath());
            Directory.CreateDirectory(path);

            var fileName = response.RequestMessage.ToFileName(query);
            var info = new ResponseInfo()
            {
                Response = response,
                Query = query,
                ContentFileName = fileName + ".content.json"
            };

            var content = await response.Content.ReadAsStringAsync();
            using (var contentWriter = new StreamWriter(Path.Combine(path, info.ContentFileName), false))
            {
                contentWriter.Write(content);
            }

            var json = JsonConvert.SerializeObject(info, new HttpResponseMessageConverter());
            using (var responseWriter = new StreamWriter(Path.Combine(path, fileName + ".response.json"), false))
            {
                responseWriter.Write(json);
            }
        }
    }
}
