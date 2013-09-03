﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FlickrNet
{
    public partial class Flickr
    {

        internal T GetResponse<T>(IDictionary<string, string> parameters) where T : class, IFlickrParsable, new()
        {
            return new T();
        }

        internal async Task<T> GetResponseAsync<T>(IDictionary<string, string> parameters) where T : class, IFlickrParsable, new()
        {
            if( !parameters.ContainsKey("oauth_consumer_key"))
                parameters.Add("oauth_consumer_key", ApiKey);

            var result = await FlickrResponder.GetDataResponseAsync(this, BaseApiUrl, parameters);

            if (typeof(T).IsEnum)
            {
                return default(T);
            }
            if (typeof(T).GetInterface("FlickrNet.IFlickrParsable") != null)
            {
                using (var reader = new XmlTextReader(new StringReader(result)))
                {
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    if (!reader.ReadToDescendant("rsp"))
                    {
                        throw new XmlException("Unable to find response element 'rsp' in Flickr response");
                    }
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.LocalName == "stat" && reader.Value == "fail")
                            throw ExceptionHandler.CreateResponseException(reader);
                    }

                    reader.MoveToElement();
                    reader.Read();

                    var item = new T();
                    item.Load(reader);
                    return item;
                }
            }
            
            return default(T);
        }

        public Flickr(string apiKey, string sharedSecret)
        {
            ApiKey = apiKey;
            SharedSecret = sharedSecret;
        }

        public Flickr(string apiKey)
        {
            ApiKey = apiKey;
        }

        internal async Task GetResponseAsync(IDictionary<string, string> parameters)
        {
            await FlickrResponder.GetDataResponseAsync(this, BaseApiUrl, parameters);
        }

        internal string OAuthCalculateSignature(string method, string url, IDictionary<string, string> parameters,
                                              string tokenSecret)
        {
            var key = SharedSecret + "&" + tokenSecret;
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var sorted = new SortedList<string, string>();
            foreach (var pair in parameters)
            {
                sorted.Add(pair.Key, pair.Value);
            }

            var sb = new StringBuilder();
            foreach (var pair in sorted)
            {
                sb.Append(pair.Key);
                sb.Append("=");
                sb.Append(UtilityMethods.EscapeOAuthString(pair.Value));
                sb.Append("&");
            }

            sb.Remove(sb.Length - 1, 1);

            var baseString = method + "&" + UtilityMethods.EscapeOAuthString(url) + "&" +
                                UtilityMethods.EscapeOAuthString(sb.ToString());

            var hash = Sha1Hash(keyBytes, baseString);

            return hash;
        }

        internal static string Sha1Hash(byte[] key, string basestring)
        {
            var sha1 = new System.Security.Cryptography.HMACSHA1(key);

            var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(basestring));

            return Convert.ToBase64String(hashBytes);
        }

        public string UploadPicture(Stream stream, string filename, string videoUploadTest, string file, string videoTest, bool isFamily, bool isFriends, bool isPublic, ContentType contentType, SafetyLevel safetyLevel, HiddenFromSearch hiddenFromSearch)
        {
            throw new NotImplementedException();
        }
    }
}
