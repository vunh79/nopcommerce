using Microsoft.AspNetCore.Http;
namespace Nop.Spa.Helpers
{
    /// <summary>
    /// Url helper
    /// </summary>
    public class UrlHelpers
    {
        /// <summary>
        /// Get Base Url
        /// </summary>
        /// <param name="request">HTTP request for the specified URL</param>
        /// <returns>Base Url</returns>
        public static string GetBaseUrl(HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        /// <summary>
        /// Get Absolute Uri
        /// </summary>
        /// <param name="request">HTTP request for the specified URL</param>
        /// <returns>Absolute Uri</returns>
        public static string GetAbsoluteUri(HttpRequest request)
        {
            var absoluteUri = string.Concat(
                       request.Scheme,
                       "://",
                       request.Host.ToUriComponent(),
                       request.PathBase.ToUriComponent(),
                       request.Path.ToUriComponent(),
                       request.QueryString.ToUriComponent());
            return absoluteUri;
        }

        /// <summary>
        /// Get Absolute Uri
        /// </summary>
        /// <param name="request">HTTP request for the specified URL</param>
        ///  /// <param name="path">Path of URL</param>
        /// <returns>Absolute Uri</returns>
        public static string GetAbsoluteUri(HttpRequest request, string path)
        {
            var absoluteUri = string.Concat(
                       request.Scheme,
                       "://",
                       request.Host.ToUriComponent(),
                       request.PathBase.ToUriComponent(),
                       path);
            return absoluteUri;
        }

    }
}
