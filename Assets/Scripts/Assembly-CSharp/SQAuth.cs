using MiniJSON;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SQAuth
{
    public static bool g_reassignID;
    private string currentNonce;
    private RuntimePlatform platform;
    public SQAuth(RuntimePlatform platform)
    {
        this.platform = platform;
    }
    public bool IsAuthenticated()
    {
        return currentNonce != null;
    }

    public void AuthUser(Session session, TFServer.JsonResponseHandler callback, bool doFacebookAuth, string fbAccessToken)
    {
        g_reassignID = false;

        // 1. Fetch the cryptographic nonce from the server
        if (currentNonce == null)
        {
            session.Server.PreAuth(delegate (Dictionary<string, object> data, HttpStatusCode status)
            {
                if (status != HttpStatusCode.OK || data == null)
                {
                    callback((Dictionary<string, object>)Json.Deserialize(TFServer.NETWORK_ERROR_JSON), status);
                    return;
                }

                try
                {
                    Dictionary<string, object> dictionary = (Dictionary<string, object>)data["data"];
                    currentNonce = (string)dictionary["nonce"];
                    ExecuteServerLogin(session, callback);
                }
                catch (KeyNotFoundException)
                {
                    callback((Dictionary<string, object>)Json.Deserialize(TFServer.NETWORK_ERROR_JSON), status);
                }
            });
        }
        else
        {
            ExecuteServerLogin(session, callback);
        }
    }

    private void ExecuteServerLogin(Session session, TFServer.JsonResponseHandler callback)
    {
        // 2. Use the local Device ID as the universal player identity
        string playerId = TFUtils.DeviceID;

        TFUtils.DebugLog("Sending login payload to server...");

        // Even though it's called GcLogin (GameCenter), we pass our device ID 
        // to fulfill whatever schema the backend endpoint requires.
        session.Server.GcLogin(playerId, playerId, currentNonce, callback);
        session.Username = playerId;
    }
}