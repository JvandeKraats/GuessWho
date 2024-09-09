using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace FetchInfo
{
    internal class CustomAuthProvider : IAuthenticationProvider
    {
        private readonly string _bearerToken;

        public CustomAuthProvider(string bearerToken)
        {
            _bearerToken = bearerToken;
        }

        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            if (request.Headers.ContainsKey("Authorization"))
            {
                request.Headers["Authorization"] = new List<string> { $"Bearer {_bearerToken}" };
            }
            else
            {
                request.Headers.Add("Authorization", new List<string> { $"Bearer {_bearerToken}" });
            }

            return Task.CompletedTask;
        }
    }
}
