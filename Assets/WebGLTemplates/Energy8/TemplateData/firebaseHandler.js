import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-app.js';
import { getAuth, signInWithCustomToken, signInWithPopup, linkWithPopup, GoogleAuthProvider, OAuthProvider } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-auth.js';
import { getAnalytics, logEvent, setUserId, setUserProperties } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-analytics.js';

async function initializeFirebase(firebaseConfig) {
  document.cookie = "debug_mode=true";

  const app = initializeApp(firebaseConfig);
  const auth = getAuth(app);
  const analytics = getAnalytics(app);

  window.firebaseApp = app;
  window.firebaseAuth = {
    onAuthStateChanged: (callback) => auth.onAuthStateChanged(callback),
    signInWithCustomToken: (token) => signInWithCustomToken(auth, token),
    signOut: () => auth.signOut(),
    GoogleAuthProvider: GoogleAuthProvider,
    OAuthProvider: OAuthProvider,
    signInWithPopup: (provider) => signInWithPopup(auth, provider),
    linkWithPopup: (provider) => linkWithPopup(auth.currentUser, provider),
    get currentUser() { return auth.currentUser; }
  };

  window.firebaseAnalytics = {
    logEvent: (eventName, params) => logEvent(analytics, eventName, params),
    setUserId: (userId) => setUserId(analytics, userId),
    setUserProperties: (properties) => setUserProperties(analytics, properties),
    resetAnalyticsData: () => {
      // Сброс данных аналитики путем очистки идентификатора пользователя
      setUserId(analytics, null);
      console.log("Firebase Analytics: Reset analytics data");
    }
  };

  console.log("Firebase app, auth, and analytics initialized, methods available globally.");
}

export { initializeFirebase };

window.initializeFirebase = initializeFirebase;
