var FirebaseAuthPlugin = {
    signInWithProvider: function (provider, addAsProvider) {
        try {
            if (addAsProvider) {
                var currentUser = window.firebaseAuth.currentUser;
                if (currentUser) {
                    window.firebaseAuth.linkWithPopup(provider)
                        .catch(function (error) {
                            console.error("Account linking error:", error);
                        });
                } else {
                    console.error("Account linking error:", "No user is signed in");
                }
            } else {
                window.firebaseAuth.signInWithPopup(provider)
                    .catch(function (error) {
                        console.error("Sign in error:", error);
                    });
            }
        } catch (error) {
            console.error("Sign in error:", error);
        }
    },

    // Основные функции плагина
    InitializeAuth: function (config, objectName, signInCallback, signOutCallback, tokenCallback, telegramCallback, errorCallback) {
        try {
            // Сохраняем имя объекта и колбэки в window для всех функций
            window.firebaseAuthObjectName = UTF8ToString(objectName);
            window.signInCallback = UTF8ToString(signInCallback);
            window.signOutCallback = UTF8ToString(signOutCallback);
            window.tokenCallback = UTF8ToString(tokenCallback);
            window.telegramCallback = UTF8ToString(telegramCallback);
            window.errorCallback = UTF8ToString(errorCallback);

            // Флаг для отслеживания завершения проверки Telegram авторизации
            window.telegramAuthCheckCompleted = false;
            window.hasTelegramAuthData = false;
            window.initialized = false;

            // Инициализируем Firebase
            window.initializeFirebase(JSON.parse(UTF8ToString(config)));

            // Настраиваем отслеживание состояния аутентификации с учетом Telegram проверки
            window.firebaseAuth.onAuthStateChanged(function (user) {
                window.initialized = true;
                console.log("[FirebaseAuthPlugin] Initalized");
                if (user) {
                    console.log("[FirebaseAuthPlugin] Initalized");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signInCallback, JSON.stringify(user));
                }
                else {
                    if (window.telegramAuthCheckCompleted && !window.hasTelegramAuthData) {
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                    }
                }
            });
        } catch (error) {
            console.error("Error initializing Firebase Auth:", error);
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.errorCallback, "Error initializing Firebase Auth: " + error.message);
        }
    },

    // Check if Telegram authentication data is present in URL
    CheckForTelegramAuth: function () {
        console.log("[FirebaseAuthPlugin] CheckForTelegramAuth called");
        try {
            if (!window.hasTelegramAuthData && window.Telegram && window.Telegram.WebApp) {
                console.log("[FirebaseAuthPlugin] Telegram WebApp detected, checking for auth data");
                try {
                    var tgWebApp = window.Telegram.WebApp;

                    if (tgWebApp.initData) {
                        console.log("[FirebaseAuthPlugin] Telegram auth data found");
                        window.hasTelegramAuthData = true;
                        console.log("[FirebaseAuthPlugin] Sending Telegram callback to Unity:", window.telegramCallback);
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, tgWebApp.initData);
                    } else {
                        console.log("[FirebaseAuthPlugin] No Telegram initData found");
                    }
                    window.telegramAuthCheckCompleted = true;
                } catch (webAppError) {
                    console.error("[FirebaseAuthPlugin] Error processing Telegram WebApp data:", webAppError);
                }
            } else {
                console.log("[FirebaseAuthPlugin] Telegram WebApp not available or auth data already processed");
            }

            if (window.unityInstance && window.firebaseAuthObjectName) {
                if (window.initialized && !window.hasTelegramAuthData && !window.firebaseAuth.currentUser) {
                    console.log("[FirebaseAuthPlugin] No auth data found, sending sign-out to Unity");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                }

                console.log("[FirebaseAuthPlugin] Sending HandleTelegramAutoAuthComplete to Unity");
                window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
            }

        } catch (error) {
            console.error("[FirebaseAuthPlugin] Error checking Telegram auth:", error);
            // Don't throw error here, just continue

            // Still notify that checking is done even after error
            if (window.unityInstance && window.firebaseAuthObjectName) {
                // Отмечаем завершение проверки Telegram авторизации даже при ошибке
                window.telegramAuthCheckCompleted = true;
                window.hasTelegramAuthData = false;
                console.log("[FirebaseAuthPlugin] Error recovery: flags reset");

                // Если пользователь не авторизован в Firebase, вызываем signOut
                if (window.initialized && !window.firebaseAuth.currentUser) {
                    console.log("[FirebaseAuthPlugin] Error recovery: sending sign-out to Unity");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                }

                window.setTimeout(function () {
                    console.log("[FirebaseAuthPlugin] Error recovery: sending HandleTelegramAutoAuthComplete to Unity (delayed)");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
                }, 500);
            }
        }
    },

    InitializeTelegramAuth: function (botId) {
        console.log("[FirebaseAuthPlugin] InitializeTelegramAuth called with botId:", botId);
        try {
            console.log("[FirebaseAuthPlugin] Telegram init: " + botId);

            window.initializeTelegramAuth({ bot_id: botId },
                function (user) {
                    console.log("[FirebaseAuthPlugin] Telegram auth initialized, callback:", window.telegramCallback);
                    // Ensure we pass the user data as a parameter
                    var userData = user ? JSON.stringify(user) : "";
                    console.log("[FirebaseAuthPlugin] Sending Telegram auth success to Unity");
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.telegramCallback, userData);
                },
                function (error) {
                    console.error("[FirebaseAuthPlugin] Telegram auth error:", error);
                }
            );
        } catch (error) {
            console.error("[FirebaseAuthPlugin] Telegram initialization error:", error);
        }
    },

    SignInWithTokenAsync: function (token) {
        console.log("[FirebaseAuthPlugin] SignInWithTokenAsync called");
        var customToken = UTF8ToString(token);
        console.log("[FirebaseAuthPlugin] Attempting to sign in with token");

        try {
            window.firebaseAuth.signInWithCustomToken(customToken)
                .then(function (result) {
                    console.log("[FirebaseAuthPlugin] Sign-in successful. User UID:", result.user.uid);
                })
                .catch(function (error) {
                    console.error("[FirebaseAuthPlugin] Sign-in with token failed:", error);
                });
        } catch (error) {
            console.error("[FirebaseAuthPlugin] Error during token sign-in:", error);
        }
    },

    SignInWithGoogle: function (addProvider) {
        console.log("[FirebaseAuthPlugin] SignInWithGoogle called with addProvider:", addProvider);
        var provider = new window.firebaseAuth.GoogleAuthProvider();
        try {
            if (addProvider) {
                console.log("[FirebaseAuthPlugin] Attempting to link Google account");
                var currentUser = window.firebaseAuth.currentUser;
                if (currentUser) {
                    window.firebaseAuth.linkWithPopup(provider)
                        .then(function (result) {
                            console.log("[FirebaseAuthPlugin] Google account linking successful:", result.user.uid);
                        })
                        .catch(function (error) {
                            console.error("[FirebaseAuthPlugin] Account linking error:", error);
                        });
                } else {
                    console.error("[FirebaseAuthPlugin] Account linking error:", "No user is signed in");
                }
            } else {
                console.log("[FirebaseAuthPlugin] Starting Google sign-in");
                window.firebaseAuth.signInWithPopup(provider)
                    .then(function (result) {
                        console.log("[FirebaseAuthPlugin] Google sign-in successful:", result.user.uid);
                    })
                    .catch(function (error) {
                        console.error("[FirebaseAuthPlugin] Sign in error:", error);
                    });
            }
        } catch (error) {
            console.error("[FirebaseAuthPlugin] Sign in error:", error);
        }
    },

    SignInWithApple: function (addProvider) {
        console.log("[FirebaseAuthPlugin] SignInWithApple called with addProvider:", addProvider);
        var provider = new window.firebaseAuth.OAuthProvider('apple.com');
        try {
            if (addProvider) {
                console.log("[FirebaseAuthPlugin] Attempting to link Apple account");
                var currentUser = window.firebaseAuth.currentUser;
                if (currentUser) {
                    window.firebaseAuth.linkWithPopup(provider)
                        .then(function (result) {
                            console.log("[FirebaseAuthPlugin] Apple account linking successful:", result.user.uid);
                        })
                        .catch(function (error) {
                            console.error("[FirebaseAuthPlugin] Account linking error:", error);
                        });
                } else {
                    console.error("[FirebaseAuthPlugin] Account linking error:", "No user is signed in");
                }
            } else {
                console.log("[FirebaseAuthPlugin] Starting Apple sign-in");
                window.firebaseAuth.signInWithPopup(provider)
                    .then(function (result) {
                        console.log("[FirebaseAuthPlugin] Apple sign-in successful:", result.user.uid);
                    })
                    .catch(function (error) {
                        console.error("[FirebaseAuthPlugin] Sign in error:", error);
                    });
            }
        } catch (error) {
            console.error("[FirebaseAuthPlugin] Sign in error:", error);
        }
    },

    SignInWithTelegram: function () {
        console.log("[FirebaseAuthPlugin] SignInWithTelegram called");
        console.log("[FirebaseAuthPlugin] Starting Telegram sign in...");

        try {
            window.signInWithTelegram()
                .then(function (user) {
                    console.log("[FirebaseAuthPlugin] Telegram sign in success");
                    console.log("[FirebaseAuthPlugin] Sending Telegram sign-in success to Unity");
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.telegramCallback, JSON.stringify(user));
                })
                .catch(function (error) {
                    console.error("[FirebaseAuthPlugin] Telegram sign in error:", error);
                });
        } catch (error) {
            console.error("[FirebaseAuthPlugin] Error starting Telegram sign in:", error);
        }
    },

    GetCurrentUser: function () {
        console.log("[FirebaseAuthPlugin] GetCurrentUser called");
        var user = window.firebaseAuth.currentUser;
        console.log("[FirebaseAuthPlugin] Current user:", user ? "exists (UID: " + user.uid + ")" : "null");
        if (user) {
            return JSON.stringify(user);
        }
        return null;
    },

    GetIdToken: function (forceRefresh) {
        console.log("[FirebaseAuthPlugin] GetIdToken called with forceRefresh:", forceRefresh);
        console.log("[FirebaseAuthPlugin] Attempting to get ID token...");
        var user = window.firebaseAuth.currentUser;

        if (user) {
            console.log("[FirebaseAuthPlugin] User found, requesting token");
            user.getIdToken(forceRefresh)
                .then(function (idToken) {
                    console.log("[FirebaseAuthPlugin] ID token retrieved successfully");
                    console.log("[FirebaseAuthPlugin] Sending token to Unity via callback:", window.tokenCallback);
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.tokenCallback, idToken);
                })
                .catch(function (error) {
                    console.error("[FirebaseAuthPlugin] Error retrieving ID token:", error);
                });
        } else {
            console.error("[FirebaseAuthPlugin] ID token error:", "No user is currently signed in");
        }
    },

    SignOut: function () {
        console.log("[FirebaseAuthPlugin] SignOut called");
        console.log("[FirebaseAuthPlugin] Signing out user...");

        window.firebaseAuth.signOut()
            .then(function () {
                console.log("[FirebaseAuthPlugin] User signed out successfully");
            })
            .catch(function (error) {
                console.error("[FirebaseAuthPlugin] Error signing out:", error);
            });
    }
};

mergeInto(LibraryManager.library, FirebaseAuthPlugin);