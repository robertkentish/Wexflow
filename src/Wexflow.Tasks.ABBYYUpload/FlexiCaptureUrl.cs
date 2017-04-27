using System;

namespace Wexflow.Tasks.ABBYYUpload
{
    public static class FlexiCaptureUrl
    {
        public static string AbbyyFlexicaptureServerUrl(string serverNameOrIP)
        {
            string serverUrl = "http://" + serverNameOrIP + "/FlexiCapture12/Server/WebServices.dll?Handler=Version3";
            Uri validURI;
            bool checkResult = Uri.TryCreate(serverUrl, UriKind.Absolute, out validURI) && (validURI.Scheme == Uri.UriSchemeHttp || validURI.Scheme == Uri.UriSchemeHttps);

            if (checkResult)
            {
                return serverUrl;
            }
            else
            {
                throw new UriFormatException("Error in AbbyyFlexicapture plugin - Url class. " + serverNameOrIP + " does not appear to be a valid server name or address. Unable to form valid URL.");
            }
        }
    }
}
