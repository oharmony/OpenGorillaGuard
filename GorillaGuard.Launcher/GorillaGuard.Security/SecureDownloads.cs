using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GorillaGuard.Security;

internal static class SecureDownloads
{
	internal static HttpClient A()
	{
		return new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false,
			SslProtocols = (SslProtocols.Tls12 | SslProtocols.Tls13)
		})
		{
			Timeout = TimeSpan.FromMinutes(3.0)
		};
	}

	internal static async Task<HttpResponseMessage> A(HttpClient P_0, string P_1, CancellationToken P_2 = default(CancellationToken))
	{
		Uri address = new Uri(P_1, UriKind.Absolute);
		for (int redirects = 0; redirects <= 5; redirects++)
		{
			if (address.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(address.UserInfo))
			{
				throw new InvalidDataException(_"Downloads require HTTPS without embedded credentials, including every redirect.");
			}
			HttpResponseMessage httpResponseMessage = await P_0.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, P_2);
			HttpStatusCode statusCode = httpResponseMessage.StatusCode;
			if (((uint)(statusCode - 301) <= 2u || (uint)(statusCode - 307) <= 1u) ? true : false)
			{
				Uri location = httpResponseMessage.Headers.Location;
				httpResponseMessage.Dispose();
				if (location == null)
				{
					throw new InvalidDataException(_"Download redirect has no destination.");
				}
				address = new Uri(address, location);
				continue;
			}
			return httpResponseMessage;
		}
		throw new InvalidDataException(_"Download exceeded the redirect limit.");
	}
}
