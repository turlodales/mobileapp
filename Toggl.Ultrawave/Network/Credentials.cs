﻿using System;
using System.Text;
using Toggl.Multivac;

namespace Toggl.Ultrawave.Network
{
    public sealed class Credentials
    {
        internal HttpHeader Header { get; }

        private Credentials(HttpHeader header)
        {
            Header = header;
        }

        public static Credentials None => new Credentials(HttpHeader.None);

        public static Credentials WithPassword(string email, string password)
        {
            Ensure.ArgumentIsNotNull(email, nameof(password));
            Ensure.ArgumentIsNotNull(password, nameof(password));

            var header = authorizationHeaderWithValue($"{email}:{password}");

            return new Credentials(header);
        }

        public static Credentials WithApiToken(string apiToken)
        {
            Ensure.ArgumentIsNotNull(apiToken, nameof(apiToken));

            var header = authorizationHeaderWithValue($"{apiToken}:api_token");

            return new Credentials(header);
        }

        private static HttpHeader authorizationHeaderWithValue(string authString)
        {
            Ensure.ArgumentIsNotNull(authString, nameof(authString));

            var authStringBytes = Encoding.UTF8.GetBytes(authString);
            var authHeader = Convert.ToBase64String(authStringBytes);

            return new HttpHeader("Authorization", authHeader, HttpHeader.HeaderType.Auth);
        }
    }
}
