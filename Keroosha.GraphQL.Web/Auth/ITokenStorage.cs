using System;
using System.Collections.Generic;
using System.Linq;
using Keroosha.GraphQL.Web.Config;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Keroosha.GraphQL.Web.Auth
{
    // TODO async version with less generic types (missing user info for JWT)
    public interface ITokenStorage
    {
        (T data, string token)? LoadToken<T>(string token, DateTimeOffset validTill);
        string CreateToken<T>(T data, DateTimeOffset validTill);
    }

    public static class TokenStorageExtensions
    {
        public static (T data, string token)? LoadToken<T>(this ITokenStorage storage, string token)
        {
            return storage.LoadToken<T>(token, DateTimeOffset.UtcNow.AddDays(1));
        }

        public static string CreateToken<T>(this ITokenStorage storage, T data)
        {
            return storage.CreateToken(data, DateTimeOffset.UtcNow.AddDays(1));
        }
    }

    internal class JwtTokenStorage : ITokenStorage
    {
        private readonly IJwtDecoder _decoder;
        private readonly IJwtEncoder _encoder;
        private readonly JWTConfiguration _options;

        public JwtTokenStorage(IOptions<TokenConfiguration> options)
        {
            var serializer = new JsonNetSerializer();
            var validator = new JwtValidator(serializer, new UtcDateTimeProvider());
            var urlEncoder = new JwtBase64UrlEncoder();
            var algorithm = new HMACSHA256Algorithm();

            _encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            _decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            _options = options.Value.JWT;
        }

        public (T data, string token)? LoadToken<T>(string token, DateTimeOffset validTill)
        {
            try
            {
                var decode = _decoder.Decode(token, _options.Key, true);
                var data = JsonConvert.DeserializeObject<dynamic>(decode);
                if (data is null || data["userdata"] is null) return null;

                var userdata = JsonConvert.DeserializeObject<T>(data["userdata"].ToString());
                return ((T)userdata, token);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CreateToken<T>(T data, DateTimeOffset validTill)
        {
            var payload = new Dictionary<string, object>
            {
                { "userdata", JsonConvert.SerializeObject(data) },
                { "exp", validTill.ToUnixTimeSeconds() }
            };
            return _encoder.Encode(payload, _options.Key);
        }
    }

    internal class InMemoryTokenStorage : ITokenStorage
    {
        private readonly DateTimeOffset _lastCleanup = DateTimeOffset.UtcNow;
        private readonly object _lock = new();

        private readonly Dictionary<string, TokenContainer> _tokens = new();

        public (T data, string token)? LoadToken<T>(string token, DateTimeOffset validTill)
        {
            Cleanup();
            lock (_lock)
            {
                if (!_tokens.TryGetValue(token, out var container))
                    return null;
                if (container.ValidTill < DateTimeOffset.UtcNow)
                {
                    _tokens.Remove(token);
                    return null;
                }

                return ((T)container.Data, container.Id);
            }
        }

        public string CreateToken<T>(T data, DateTimeOffset validTill)
        {
            lock (_lock)
            {
                var t = new TokenContainer(data, validTill);
                _tokens[t.Id] = t;
                return t.Id;
            }
        }

        private void Cleanup()
        {
            if ((DateTimeOffset.UtcNow - _lastCleanup).TotalMinutes > 5)
                lock (_lock)
                {
                    if ((DateTimeOffset.UtcNow - _lastCleanup).TotalMinutes > 5)
                        foreach (var kp in _tokens.ToList())
                            if (kp.Value.ValidTill < DateTimeOffset.UtcNow)
                                _tokens.Remove(kp.Key);
                }
        }

        private class TokenContainer
        {
            public TokenContainer(object data, DateTimeOffset validTill)
            {
                Id = Guid.NewGuid().ToString();
                Data = data;
                ValidTill = validTill;
            }

            public string Id { get; }
            public DateTimeOffset ValidTill { get; }
            public object Data { get; }
        }
    }
}