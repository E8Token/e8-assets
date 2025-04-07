var FirebaseAuthPlugin = {
    sendUnityMessage: function(callbackName, param) {
        if (param) {
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, callbackName, param);
        } else {
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, callbackName);
        }
    },

    handleError: function(context, error) {
        console.error(context + ":", error);
        
        // Конвертируем ошибку в строку если это объект
        let errorMessage = error;
        if (typeof error === 'object') {
            try {
                errorMessage = JSON.stringify(error);
            } catch (e) {
                errorMessage = error.toString();
            }
        }
        
        this._sendUnityMessage(window.errorCallback, errorMessage);
    },

    signInWithProvider: function(provider, addAsProvider) {
        try {
            if (addAsProvider) {
                const currentUser = window.firebaseAuth.currentUser;
                if (currentUser) {
                    window.firebaseAuth.linkWithPopup(provider)
                        .catch((error) => {
                            this._handleError("Account linking error", error);
                        });
                } else {
                    this._handleError("Account linking error", "No user is signed in");
                }
            } else {
                window.firebaseAuth.signInWithPopup(provider)
                    .catch((error) => {
                        this._handleError("Sign in error", error);
                    });
            }
        } catch (error) {
            this._handleError("Sign in error", error);
        }
    },

    // Основные функции плагина
    InitializeAuth: function(config, objectName, signInCallback, signOutCallback, tokenCallback, telegramCallback, errorCallback) {
        try {
            // Сохраняем имя объекта и колбэки в window для всех функций
            window.firebaseAuthObjectName = UTF8ToString(objectName);
            window.signInCallback = UTF8ToString(signInCallback);
            window.signOutCallback = UTF8ToString(signOutCallback);
            window.tokenCallback = UTF8ToString(tokenCallback);
            window.telegramCallback = UTF8ToString(telegramCallback);
            window.errorCallback = UTF8ToString(errorCallback);

            // Инициализируем Firebase
            window.initializeFirebase(JSON.parse(UTF8ToString(config)));
            console.log("Firebase initialized with object name:", window.firebaseAuthObjectName);

            // Настраиваем отслеживание состояния аутентификации
            window.firebaseAuth.onAuthStateChanged((user) => {
                if (user) {
                    console.log("User signed in:", user.uid);
                    this._sendUnityMessage(window.signInCallback, JSON.stringify(user));
                } else {
                    console.log("User signed out");
                    this._sendUnityMessage(window.signOutCallback);
                }
            });
        } catch (error) {
            console.error("Error initializing Firebase Auth:", error);
        }
    },

    InitializeTelegramAuth: function(botId) {
        try {
            const parsedBotId = botId ? JSON.parse(UTF8ToString(botId)) : 8114226239;
            const self = this; // Сохраняем ссылку на текущий 'this'
            
            console.log("Telegram init: " + parsedBotId);

            window.initializeTelegramAuth({ bot_id: parsedBotId },
                function(user) {
                    console.log("Telegram auth initialized" + window.telegramCallback);
                    self._sendUnityMessage(window.telegramCallback, JSON.stringify(user));
                },
                function(error) {
                    //self._handleError("Telegram auth error", error);
                }
            );
        } catch (error) {
            //Nthis._handleError("Telegram initialization error", error);
        }
    },

    SignInWithTokenAsync: function(token) {
        const customToken = UTF8ToString(token);
        console.log("Attempting to sign in with token");
        
        try {
            window.firebaseAuth.signInWithCustomToken(customToken)
                .then((result) => {
                    console.log("Sign-in successful. User UID:", result.user.uid);
                })
                .catch((error) => {
                    this._handleError("Sign-in with token failed", error);
                });
        } catch (error) {
            this._handleError("Error during token sign-in", error);
        }
    },

    SignInWithGoogle: function(addProvider) {
        const provider = new window.firebaseAuth.GoogleAuthProvider();
        this._signInWithProvider(provider, addProvider);
    },

    SignInWithApple: function(addProvider) {
        const provider = new window.firebaseAuth.OAuthProvider('apple.com');
        this._signInWithProvider(provider, addProvider);
    },

    SignInWithTelegram: function() {
        console.log("Starting Telegram sign in...");
        
        try {
            window.signInWithTelegram()
                .then((user) => {
                    console.log("Telegram sign in success");
                    this._sendUnityMessage(window.telegramCallback, JSON.stringify(user));
                })
                .catch((error) => {
                    this._handleError("Telegram sign in error", error);
                });
        } catch (error) {
            this._handleError("Error starting Telegram sign in", error);
        }
    },

    GetCurrentUser: function() {
        const user = window.firebaseAuth.currentUser;
        if (user) {
            return JSON.stringify(user);
        }
        return null;
    },

    GetIdToken: function(forceRefresh) {
        console.log("Attempting to get ID token...");
        const user = window.firebaseAuth.currentUser;
        
        if (user) {
            user.getIdToken(forceRefresh)
                .then((idToken) => {
                    console.log("ID token retrieved successfully");
                    this._sendUnityMessage(window.tokenCallback, idToken);
                })
                .catch((error) => {
                    this._handleError("Error retrieving ID token", error);
                });
        } else {
            this._handleError("ID token error", "No user is currently signed in");
        }
    },

    SignOut: function() {
        console.log("Signing out user...");
        
        window.firebaseAuth.signOut()
            .then(() => {
                console.log("User signed out successfully");
            })
            .catch((error) => {
                this._handleError("Error signing out", error);
            });
    }
};

mergeInto(LibraryManager.library, FirebaseAuthPlugin);