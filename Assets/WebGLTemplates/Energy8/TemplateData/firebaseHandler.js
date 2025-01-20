import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-app.js';
import { getAuth, signInWithCustomToken, signInWithPopup,linkWithPopup, GoogleAuthProvider, OAuthProvider } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-auth.js';

async function initializeFirebase(firebaseConfig) {
  const app = initializeApp(firebaseConfig);
  const auth = getAuth(app);

  window.firebaseApp = app;
  window.firebaseAuth = {
    onAuthStateChanged: (callback) => auth.onAuthStateChanged(callback),
    signInWithCustomToken: (token) => signInWithCustomToken(auth, token),
    signOut: () => auth.signOut(),
    GoogleAuthProvider: GoogleAuthProvider,
    getCurrentUser: () => auth.currentUser,
    OAuthProvider: OAuthProvider,
    signInWithPopup: (provider) => signInWithPopup(auth, provider),
    linkWithPopup: (provider) => linkWithPopup(auth.getCurrentUser(), provider)
  };

  console.log("Firebase app and auth initialized, methods available globally.");
}

export { initializeFirebase };

window.initializeFirebase = initializeFirebase;
