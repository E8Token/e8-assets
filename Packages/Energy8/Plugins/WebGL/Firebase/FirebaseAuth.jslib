var firebaseAuth = {
    InitializeAuth: function (config, objectName, signInCallback, signOutCallback) {
        console.log("Initializing Firebase Auth...");

        window.initializeFirebase(JSON.parse(UTF8ToString(config)));
        window.firebaseAuthObjectName = UTF8ToString(objectName);

        console.log("Firebase initialized with object name: " + window.firebaseAuthObjectName);

        signInCallback = UTF8ToString(signInCallback);
        signOutCallback = UTF8ToString(signOutCallback);

        console.log("Callbacks assigned: signInCallback = " + signInCallback + ", signOutCallback = " + signOutCallback);

        window.firebaseAuth.onAuthStateChanged((user) => {
            if (user) {
                console.log("User signed in: " + user.uid);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, signInCallback, JSON.stringify(user));
            } else {
                console.log("User signed out.");
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, signOutCallback);
            }
        });
    },

    SignInByTokenAsync: function (token, callback, fallback) {
        console.log("Attempting to sign in with token...");

        var token = UTF8ToString(token);
        var callback = UTF8ToString(callback);
        var fallback = UTF8ToString(fallback);

        console.log("Token received: " + token);
        console.log("Callback function: " + callback);
        console.log("Fallback function: " + fallback);

        try {
            window.firebaseAuth.signInWithCustomToken(token).then(function (result) {
                console.log("Sign-in successful. User UID: " + result.user.uid);
                //window.unityInstance.SendMessage(window.firebaseAuthObjectName, callback, JSON.stringify(result.user));
            }).catch(function (error) {
                console.error("Sign-in failed: " + error);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, error);
            });
        } catch (error) {
            console.error("Error during sign-in: " + error.message);
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, error);
        }
    },
    GetIdToken: function (forceRefresh, callback, fallback) {
        console.log("Attempting to get ID token...");

        var callback = UTF8ToString(callback);
        var fallback = UTF8ToString(fallback);

        // Get the currently authenticated user
        var user = window.firebaseAuth.getCurrentUser();

        if (user) {
            console.log("User found, attempting to get ID token...");

            user.getIdToken(forceRefresh)
                .then(function (idToken) {
                    console.log("ID token retrieved successfully.");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, callback, idToken);
                })
                .catch(function (error) {
                    console.error("Error retrieving ID token: " + error.message);
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, error);
                });
        } else {
            console.log("No user signed in.");
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, "No user is currently signed in.");
        }
    },

    SignOut: function () {
        console.log("Signing out user...");
        window.firebaseAuth.signOut()
            .then(function () {
                console.log("User signed out successfully.");
            })
            .catch(function (error) {
                console.error("Error signing out: " + error.message);
            });
    }
}
mergeInto(LibraryManager.library, firebaseAuth);