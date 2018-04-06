using ExactOnline.Client.OAuth;
using System;

namespace Switchboard.Connectors
{
    /// <summary>
    /// Refactored from the Exact Client SDK
    /// </summary>
	public class ExactConnectorHelper
	{
		private readonly string _clientId;
		private readonly string _clientSecret;
		private readonly Uri _callbackUrl;
		private readonly UserAuthorization _authorization;

		public string EndPoint
		{
			get
			{
				//return "https://start.exactonline.nl";
                return "https://start.exactonline.uk";
            }
		}

		public ExactConnectorHelper(string clientId, string clientSecret, Uri callbackUrl)
		{
			_clientId = clientId;
			_clientSecret = clientSecret;
			_callbackUrl = callbackUrl;
			_authorization = new UserAuthorization();
		}

		public string GetAccessToken()
		{
			UserAuthorizations.Authorize(_authorization, EndPoint, _clientId, _clientSecret, _callbackUrl);

			return _authorization.AccessToken;
		}

	}
}
