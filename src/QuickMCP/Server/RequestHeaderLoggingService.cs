using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QuickMCP.Server
{
    public class RequestHeaderLoggingService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RequestHeaderLoggingService> _logger;


        public RequestHeaderLoggingService(IHttpContextAccessor httpContextAccessor, ILogger<RequestHeaderLoggingService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string? GetToken()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return null;
            }


            return context.Request.Headers["Authorization"].FirstOrDefault();
        }
    }
}