using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Provides static classes to work with HTTP and HTTPS.
	/// </summary>
	public static class ProtocolHelper
    {
        /// <summary>
        /// Gets scheme which must be used to connect to the services (HTTP or HTTPS).
        /// </summary>
        public static string Scheme
        {
            get
            {
				return "http";
            }
        }

        /// <summary>
        /// Gets http security mode.
        /// </summary>
        public static BasicHttpSecurityMode SecurityMode
        {
            get
            {
                BasicHttpSecurityMode mode = BasicHttpSecurityMode.None;

                string scheme = ProtocolHelper.Scheme;
                if (scheme == Uri.UriSchemeHttps)
                {
                    mode = BasicHttpSecurityMode.Transport;
                }

                return mode;
            }
        }

        /// <summary>
        /// Set right scheme to given uri.
        /// </summary>
        /// <param name="uri">Uri to set scheme to.</param>
        /// <returns>Uri with scheme changed.</returns>
        public static string SetScheme(string uri)
        {
            string localUri = uri.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            if (localUri.StartsWith("http:", StringComparison.Ordinal))
            {
                string scheme = ProtocolHelper.Scheme;
                if (scheme == Uri.UriSchemeHttps)
                {
                    uri = scheme + uri.Substring(4);
                }
            }
            else if (localUri.StartsWith("https:", StringComparison.Ordinal))
            {
                string scheme = ProtocolHelper.Scheme;
                if (scheme == Uri.UriSchemeHttp)
                {
                    uri = scheme + uri.Substring(5);
                }
            }

            return uri;
        }
    }
}
