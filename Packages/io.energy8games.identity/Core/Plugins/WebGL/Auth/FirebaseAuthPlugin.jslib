var FirebaseAuthPlugin = {
    InitializeAuth: function (config, objectName, signInCallback, signOutCallback, tokenCallback, telegramCallback, errorCallback) {
        window.firebaseAuthObjectName = UTF8ToString(objectName);
        window.signInCallback = UTF8ToString(signInCallback);
        window.signOutCallback = UTF8ToString(signOutCallback);
        window.tokenCallback = UTF8ToString(tokenCallback);
        window.telegramCallback = UTF8ToString(telegramCallback);
        window.errorCallback = UTF8ToString(errorCallback);

        window.initializeFirebase(JSON.parse(UTF8ToString(config)));
        window.initializeTelegramAuth({ bot_id: 8114226239 },
            function (user) {
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.telegramCallback, JSON.stringify(user));
            },
            function (error) {
                 window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                     window.errorCallback, JSON.stringify(error));
            });

        console.log("Firebase initialized with object name: " + window.firebaseAuthObjectName);

        window.firebaseAuth.onAuthStateChanged((user) => {
            if (user) {
                console.log("User signed in: " + JSON.stringify(user));
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.signInCallback, JSON.stringify(user));
            } else {
                console.log("User signed out.");
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.signOutCallback);
            }
        });
    },

    SignInWithTokenAsync: function (token) {
        console.log("Attempting to sign in with token...");

        var token = UTF8ToString(token);

        console.log("Token received: " + token);

        try {
            window.firebaseAuth.signInWithCustomToken(token).then(function (result) {
                console.log("Sign-in successful. User UID: " + result.user.uid);
            }).catch(function (error) {
                console.error("Sign-in failed: " + error);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.errorCallback, error);
            });
        } catch (error) {
            console.error("Error during sign-in: " + error.message);
            window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                window.errorCallback, error);
        }
    },

    SignInWithGoogle: function (addProvider) {
        const provider = new window.firebaseAuth.GoogleAuthProvider();

        if (addProvider) {
            const currentUser = window.firebaseAuth.currentUser;
            if (currentUser) {
                window.firebaseAuth.linkWithPopup(provider)
                    .catch((error) => {
                        console.error("Google account linking error:", error);
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                            window.errorCallback, JSON.stringify(error));
                    });
            }
        } else {
            window.firebaseAuth.signInWithPopup(provider)
                .catch((error) => {
                    console.error("Google sign in error:", error);
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                        window.errorCallback, JSON.stringify(error));
                });
        }
    },

    SignInWithApple: function (addProvider) {
        const provider = new window.firebaseAuth.OAuthProvider('apple.com');

        if (addProvider) {
            const currentUser = window.firebaseAuth.currentUser;
            if (currentUser) {
                window.firebaseAuth.linkWithPopup(provider)
                    .catch((error) => {
                        console.error("Apple account linking error:", error);
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                            window.errorCallback, JSON.stringify(error));
                    });
            }
        } else {
            window.firebaseAuth.signInWithPopup(provider)
                .catch((error) => {
                    console.error("Apple sign in error:", error);
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                        window.errorCallback, JSON.stringify(error));
                });
        }
    },

    SignInWithTelegram: function () {
        console.log("Starting Telegram sign in...");
        window.signInWithTelegram()
            .then(function (user) {
                console.log("Telegram sign in success:", user);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.telegramCallback, JSON.stringify(user));
            })
            .catch(function (error) {
                console.error("Telegram sign in error:", error);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.errorCallback, JSON.stringify(error));
            });
    },

    GetCurrentUser: function () {
        var user = window.firebaseAuth.currentUser;
        if (user) {
            return JSON.stringify(user);
        }
        return null;
    },

    InitializeTelegramAuth: function (botId) {
        window.initializeTelegramAuth({ bot_id: JSON.parse(UTF8ToString(botId)) },
            function (user) {
                console.log("Telegram auth initialized:", user);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.telegramCallback, JSON.stringify(user));
            },
            function (error) {
                console.error("Telegram auth error:", error);
                window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                    window.errorCallback, JSON.stringify(error));
            });
    },

    GetIdToken: function (forceRefresh) {
        console.log("Attempting to get ID token...");

        var user = window.firebaseAuth.currentUser;

        if (user) {
            console.log("User found, attempting to get ID token...");

            user.getIdToken(forceRefresh)
                .then(function (idToken) {
                    console.log("ID token retrieved successfully.");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.tokenCallback, idToken);
                })
                .catch(function (error) {
                    console.error("Error retrieving ID token: " + error.message);
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.errorCallback, error);
                });
        } else {
            console.log("No user signed in.");
            window.unityInstance.SendMessage(window.firebaseAuthObjectName,
                window.errorCallback, "No user is currently signed in.");
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
};

mergeInto(LibraryManager.library, FirebaseAuthPlugin);