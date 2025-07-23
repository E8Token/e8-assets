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
                if (user) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signInCallback, JSON.stringify(user));
                } else {
                    if (window.telegramAuthCheckCompleted && !window.hasTelegramAuthData) {
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                    }
                }
            });

            _CheckForTelegramAuth();
        } catch (error) {
            console.error("Error initializing Firebase Auth:", error);
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.errorCallback, "Error initializing Firebase Auth: " + error.message);
        }
    },

    // Check if Telegram authentication data is present in URL
    CheckForTelegramAuth: function () {
        try {
            if (!window.hasTelegramAuthData && window.Telegram && window.Telegram.WebApp) {
                try {
                    var tgWebApp = window.Telegram.WebApp;

                    if (tgWebApp.initData) {
                        window.hasTelegramAuthData = true;
                        window.telegramAuthCheckCompleted = true;
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, tgWebApp.initData);
                    }
                } catch (webAppError) {
                    console.error("Error processing Telegram WebApp data:", webAppError);
                }
            }

            if (window.unityInstance && window.firebaseAuthObjectName) {
                if (window.initialized && !window.hasTelegramAuthData && !window.firebaseAuth.currentUser) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                }

                window.setTimeout(function () {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
                }, 500);
            }

        } catch (error) {
            console.error("Error checking Telegram auth:", error);
            // Don't throw error here, just continue

            // Still notify that checking is done even after error
            if (window.unityInstance && window.firebaseAuthObjectName) {
                // Отмечаем завершение проверки Telegram авторизации даже при ошибке
                window.telegramAuthCheckCompleted = true;
                window.hasTelegramAuthData = false;

                // Если пользователь не авторизован в Firebase, вызываем signOut
                if (window.initialized && !window.firebaseAuth.currentUser) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                }

                var self = this;
                window.setTimeout(function () {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
                }, 500);
            }
        }
    },

    InitializeTelegramAuth: function (botId) {
        try {
            console.log("Telegram init: " + botId);

            window.initializeTelegramAuth({ bot_id: botId },
                function (user) {
                    console.log("Telegram auth initialized" + window.telegramCallback);
                    // Ensure we pass the user data as a parameter
                    var userData = user ? JSON.stringify(user) : "";
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.telegramCallback, userData);
                },
                function (error) {
                    console.error("Telegram auth error:", error);
                }
            );
        } catch (error) {
            console.error("Telegram initialization error:", error);
        }
    },

    SignInWithTokenAsync: function (token) {
        var customToken = UTF8ToString(token);
        console.log("Attempting to sign in with token");

        try {
            window.firebaseAuth.signInWithCustomToken(customToken)
                .then(function (result) {
                    console.log("Sign-in successful. User UID:", result.user.uid);
                })
                .catch(function (error) {
                    console.error("Sign-in with token failed:", error);
                });
        } catch (error) {
            console.error("Error during token sign-in:", error);
        }
    },

    SignInWithGoogle: function (addProvider) {
        var provider = new window.firebaseAuth.GoogleAuthProvider();
        try {
            if (addProvider) {
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

    SignInWithApple: function (addProvider) {
        var provider = new window.firebaseAuth.OAuthProvider('apple.com');
        try {
            if (addProvider) {
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

    SignInWithTelegram: function () {
        console.log("Starting Telegram sign in...");

        try {
            window.signInWithTelegram()
                .then(function (user) {
                    console.log("Telegram sign in success");
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.telegramCallback, JSON.stringify(user));
                })
                .catch(function (error) {
                    console.error("Telegram sign in error:", error);
                });
        } catch (error) {
            console.error("Error starting Telegram sign in:", error);
        }
    },

    GetCurrentUser: function () {
        var user = window.firebaseAuth.currentUser;
        if (user) {
            return JSON.stringify(user);
        }
        return null;
    },

    GetIdToken: function (forceRefresh) {
        console.log("Attempting to get ID token...");
        var user = window.firebaseAuth.currentUser;

        if (user) {
            user.getIdToken(forceRefresh)
                .then(function (idToken) {
                    console.log("ID token retrieved successfully");
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.tokenCallback, idToken);
                })
                .catch(function (error) {
                    console.error("Error retrieving ID token:", error);
                });
        } else {
            console.error("ID token error:", "No user is currently signed in");
        }
    },

    SignOut: function () {
        console.log("Signing out user...");

        window.firebaseAuth.signOut()
            .then(function () {
                console.log("User signed out successfully");
            })
            .catch(function (error) {
                console.error("Error signing out:", error);
            });
    }
};

mergeInto(LibraryManager.library, FirebaseAuthPlugin);