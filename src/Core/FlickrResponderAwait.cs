﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace FlickrNet
{
    internal static partial class FlickrResponder
    {
        internal static async Task<string> GetDataResponseAsync(Flickr flickr, string baseUrl, IDictionary<string, string> parameters)
        {
            const string method = "POST";

            // If OAuth Access Token is set then add token and generate signature.
            if (!String.IsNullOrEmpty(flickr.OAuthAccessToken) && !parameters.ContainsKey("oauth_token"))
            {
                parameters.Add("oauth_token", flickr.OAuthAccessToken);
            }
            if (!String.IsNullOrEmpty(flickr.OAuthAccessTokenSecret) && !parameters.ContainsKey("oauth_signature"))
            {
                var sig = flickr.OAuthCalculateSignature(method, baseUrl, parameters, flickr.OAuthAccessTokenSecret);
                parameters.Add("oauth_signature", sig);
            }

            // Calculate post data, content header and auth header
            var data = OAuthCalculatePostData(parameters);
            var authHeader = OAuthCalculateAuthHeader(parameters);

            // Download data.
            try
            {
                return await DownloadDataAsync(method, baseUrl, data, PostContentType, authHeader);
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response == null) throw;

                if (response.StatusCode != HttpStatusCode.BadRequest &&
                    response.StatusCode != HttpStatusCode.Unauthorized) throw;

                var stream = response.GetResponseStream();
                if (stream == null) throw;

                using (var responseReader = new StreamReader(stream))
                {
                    var responseData = responseReader.ReadToEnd();
                    throw new OAuthException(responseData, ex);
                }
            }
        }

        internal static async Task<string> DownloadDataAsync(string method, string baseUrl, string data, string contentType, string authHeader)
        {
            var client = new WebClient();
            if (!String.IsNullOrEmpty(contentType)) client.Headers["Content-Type"] = contentType;
            if (!String.IsNullOrEmpty(authHeader)) client.Headers["Authorization"] = authHeader;

            if (method == "POST")
            {
                return await client.UploadStringTaskAsync(new Uri(baseUrl), data);
            }

            return await client.DownloadStringTaskAsync(new Uri(baseUrl));
        }
    }
}
