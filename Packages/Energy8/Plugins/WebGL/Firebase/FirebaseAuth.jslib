var firebaseAuth = {
    Initialize: function (config, objectName, signInCallback, signOutCallback) {
        window.initializeFirebase(JSON.parse(UTF8ToString(config)));
        window.firebaseAuthObjectName = UTF8ToString(objectName);
        signInCallback = UTF8ToString(signInCallback);
        signOutCallback = UTF8ToString(signOutCallback);
        window.firebaseAuth.onAuthStateChanged((user) => {
            if (user) {
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, signInCallback, JSON.stringify(user));
            } else {
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, signOutCallback);
            }
        });
    },
    SignInByTokenAsync: function (token, callback, fallback) {
        var token = UTF8ToString(token);
        var callback = UTF8ToString(callback);
        var fallback = UTF8ToString(fallback);

        try {
            window.firebaseAuth.signInWithCustomToken(token).then(function (result) {
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, callback, result.user.uid);
            }).catch(function (error) {
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
            });

        } catch (error) {
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },
    GetIdToken: function (forceRefresh, callback, fallback) {
        var callback = UTF8ToString(callback);
        var fallback = UTF8ToString(fallback);

        // Get the currently authenticated user
        var user = window.firebaseAuth.getCurrentUser();

        if (user) {
            user.getIdToken(forceRefresh)
                .then(function (idToken) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, callback, idToken);
                })
                .catch(function (error) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
                });
        } else {
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, fallback, "No user is currently signed in.");
        }
    },
    SignOut: function() {
        window.firebaseAuth.signOut()
    }
}

mergeInto(LibraryManager.library, firebaseAuth);