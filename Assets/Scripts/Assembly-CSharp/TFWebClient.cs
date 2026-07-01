using System;
using System.Net;
using System.Runtime.CompilerServices;

public class TFWebClient : WebClient
{
    public delegate void OnNetworkError(object sender, WebException e);

    private const int TIMEOUT = 30000;

    private const string USER_AGENT = "Innertube Explorer v0.1";

    private static int _maxConnections = 2;

    public static bool OfflineMode = true;

    private CookieContainer cookies;

    public static int maxConnections
    {
        get
        {
            return _maxConnections;
        }
        set
        {
            _maxConnections = Math.Max(1, value);
            ServicePointManager.DefaultConnectionLimit = _maxConnections;
        }
    }

    [method: MethodImpl(32)]
    public event OnNetworkError NetworkError;

    public TFWebClient(CookieContainer cookieContainer)
    {
        cookies = cookieContainer;

        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
        }
        catch
        {
        }
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        if (OfflineMode)
        {
            TFUtils.WarnLog("[3DS OFFLINE MODE] Blocked request: " + address);
            return null;
        }

        HttpWebRequest httpWebRequest = base.GetWebRequest(address) as HttpWebRequest;

        if (httpWebRequest != null)
        {
            httpWebRequest.CookieContainer = cookies;
            httpWebRequest.Timeout = TIMEOUT;
            httpWebRequest.UserAgent = USER_AGENT;

            ServicePoint servicePoint = ServicePointManager.FindServicePoint(address);
            servicePoint.Expect100Continue = false;
            servicePoint.ConnectionLimit = _maxConnections;
        }

        return httpWebRequest;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
        if (OfflineMode)
        {
            return null;
        }

        try
        {
            return base.GetWebResponse(request);
        }
        catch (WebException ex)
        {
            TFUtils.WarnLog(string.Concat(
                request.RequestUri,
                ", Exception status: ",
                Enum.GetName(typeof(WebExceptionStatus), ex.Status)));

            if (this.NetworkError != null)
            {
                this.NetworkError(this, ex);
            }

            return null;
        }
    }
}