using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;

namespace CustomAuthApp
{
    public interface IApiKeyValidator
    {
        Task<bool> ValidateAsync(string apiKey);
    }

    public class MyApiKeyValidatorImpl : IApiKeyValidator
    {
        public Task<bool> ValidateAsync(string apiKey)
        {
            throw new NotImplementedException();
        }
    }

    public class ApiKeyAuthenticationOptions : AuthenticationOptions
    {
        public const string DefaultHeaderName = "Authorization";
        public string HeaderName { get; set; } = DefaultHeaderName;

        public ApiKeyAuthenticationOptions()
        {
            AuthenticationScheme = "apikey";
        }
    }
 
    public class ApiKeyAuthenticationMiddleware : AuthenticationMiddleware<ApiKeyAuthenticationOptions>
    {
        private IApiKeyValidator _validator;
        public ApiKeyAuthenticationMiddleware(
            IApiKeyValidator validator,  // custom dependency
            RequestDelegate next,
            IOptions<ApiKeyAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder)
        : base(next, options, loggerFactory, encoder)
        {
            _validator = validator;
        }
    
        protected override AuthenticationHandler<ApiKeyAuthenticationOptions> CreateHandler()
        {
            return new ApiKeyAuthenticationHandler(_validator);
        }
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyValidator _validator;

        public ApiKeyAuthenticationHandler(IApiKeyValidator validator)
        {
            _validator = validator;
        }
    
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            StringValues headerValue;
            
            if (!Context.Request.Headers.TryGetValue(Options.HeaderName, out headerValue))
            {
                return AuthenticateResult.Fail("Missing or malformed 'Authorization' header.");
            }
            
            var apiKey = headerValue.First();

            if (!await _validator.ValidateAsync(apiKey))
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }
            
            var identity = new ClaimsIdentity(Options.AuthenticationScheme);
            
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, Options.AuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}