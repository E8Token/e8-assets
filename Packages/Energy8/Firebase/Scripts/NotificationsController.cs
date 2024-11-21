using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
#else
using Firebase.Messaging;
#endif

namespace Energy8.Firebase
{
    public static class NotificationsController
    {
        static readonly Logger logger = new(null, "NotificationsController", new Color(0.88f, 0.43f, 027f));
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        public static void Initialize()
        {
            FirebaseMessaging.TokenReceived += (object sender, TokenReceivedEventArgs token) => logger.Log("Received Registration Token: " + token.Token);
            FirebaseMessaging.MessageReceived += (object sender, MessageReceivedEventArgs e) => logger.Log("Received a new message from: " + e.Message.From);
            logger.Log("Initialized");
        }
#endif
    }
}