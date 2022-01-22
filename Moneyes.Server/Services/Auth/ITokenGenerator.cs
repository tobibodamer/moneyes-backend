﻿using System.Security.Claims;

namespace Moneyes.Server.Services
{
    /// <summary>
    /// Interface for generating token.
    /// </summary>
    public interface ITokenGenerator
    {
        /// <summary>
        /// Generates jwt token.
        /// </summary>
        /// <param name="secretKey">The secret key for security.</param>
        /// <param name="issuer">The issuer.</param>
        /// <param name="audience">The audience.</param>
        /// <param name="expires">The expire time.</param>
        /// <param name="claims"><see cref="IEnumerable{T}"/></param>
        /// <returns>Generated token.</returns>
        (string Token, string Id) Generate(string secretKey, string issuer, string audience, double expires,
            IEnumerable<Claim>? claims = null);
    }}