using System;
using UnityEngine;

public static class GoogleSignIn
{
    public static void SignIn(string webClientId, Action<string> onSuccess, Action<string> onError)
    {
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using var pluginInstance = new AndroidJavaObject("com.plugins.googlesignin.PluginInstance", activity, webClientId);
        pluginInstance.Call("signIn", new SignInCallback(onSuccess, onError));
    }

    private class SignInCallback : AndroidJavaProxy
    {
        Action<string> OnSuccessAction;
        Action<string> OnErrorAction;
        public SignInCallback(Action<string> onSuccess, Action<string> onError) : base("com.plugins.googlesignin.PluginInstance$SignInCallback")
        {   
            OnSuccessAction = onSuccess;
            OnErrorAction = onError;
        }

        public void onSuccess(string idToken)
        {
            Debug.Log("Google Sign-In success. ID Token: " + idToken);
            OnSuccessAction.Invoke(idToken);
        }

        public void onError(AndroidJavaObject exception)
        {
            Debug.LogError("Google Sign-In failed: " + exception.Call<string>("toString"));
            OnErrorAction.Invoke(exception.Call<string>("toString"));
        }
    }
}
