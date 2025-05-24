# Firebase Web Analytics Integration Guide

This document explains how to integrate Firebase Analytics into your WebGL build.

## 1. Setup Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create a new project or use an existing one
3. Add a web app to your Firebase project
4. Follow the setup instructions to get your Firebase configuration

## 2. Update WebGL Template

Add the Firebase JavaScript SDK to your WebGL template (`index.html`). The template is typically located at `Assets/WebGLTemplates/YourTemplate/index.html`.

Add the following scripts before the closing `</body>` tag:

```html
<!-- Firebase SDKs -->
<script src="https://www.gstatic.com/firebasejs/9.22.0/firebase-app-compat.js"></script>
<script src="https://www.gstatic.com/firebasejs/9.22.0/firebase-auth-compat.js"></script>
<script src="https://www.gstatic.com/firebasejs/9.22.0/firebase-analytics-compat.js"></script>

<!-- Initialize Firebase -->
<script>
  var firebaseConfig = {
    apiKey: "YOUR_API_KEY",
    authDomain: "YOUR_AUTH_DOMAIN",
    projectId: "YOUR_PROJECT_ID",
    storageBucket: "YOUR_STORAGE_BUCKET",
    messagingSenderId: "YOUR_MESSAGING_SENDER_ID",
    appId: "YOUR_APP_ID",
    measurementId: "YOUR_MEASUREMENT_ID"
  };
  
  // Initialize Firebase
  firebase.initializeApp(firebaseConfig);
  
  // Initialize Analytics
  firebase.analytics();
</script>
```

Replace the placeholders with your actual Firebase configuration.

## 3. Enable Analytics in Unity

1. Make sure you have the Analytics service properly injected into your IdentityService
2. The analytics provider will be automatically initialized when IdentityService is initialized

## 4. Verify Analytics Events

1. Deploy your WebGL build to a web server
2. Visit your Firebase Console > Analytics dashboard
3. Check for events coming from your application
4. Note that there may be a delay before events appear in the dashboard

## 5. Custom Events

You can log custom events using the AnalyticsService:

```csharp
// Simple event
analyticsService.LogEvent("event_name");

// Event with parameters
var parameters = new Dictionary<string, object>
{
    { "parameter_name", "parameter_value" },
    { "score", 100 }
};
analyticsService.LogEvent("custom_event", parameters);
```

## 6. Debug Mode

To enable debug mode for Firebase Analytics, add the following code to your initialization script:

```javascript
firebase.analytics().setAnalyticsCollectionEnabled(true);
```

You can also check the browser console for logs from the Firebase Analytics JavaScript SDK.
