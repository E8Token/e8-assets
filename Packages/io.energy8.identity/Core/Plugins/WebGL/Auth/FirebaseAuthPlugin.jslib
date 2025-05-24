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

            // Инициализируем Firebase
            window.initializeFirebase(JSON.parse(UTF8ToString(config)));
            console.log("Firebase initialized with object name:", window.firebaseAuthObjectName);

            // Настраиваем отслеживание состояния аутентификации
            window.firebaseAuth.onAuthStateChanged(function (user) {
                if (user) {
                    console.log("User signed in:", user.uid);
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signInCallback, JSON.stringify(user));
                } else {
                    console.log("User signed out");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.signOutCallback);
                }
            });

            // Check for Telegram authentication result in URL immediately
            var self = this;
            window.setTimeout(function () {
                _CheckForTelegramAuth();
            }, 100); // Сокращаем задержку для быстрой проверки
        } catch (error) {
            console.error("Error initializing Firebase Auth:", error);
            window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.errorCallback, "Error initializing Firebase Auth: " + error.message);
        }
    },

    // Check if Telegram authentication data is present in URL
    CheckForTelegramAuth: function () {
        console.log("Checking for Telegram auth in URL...");
        var foundData = false;
        try {
            // First check the URL params for standard Telegram Login Widget format
            var url = new URL(window.location.href);
            var searchParams = new URLSearchParams(url.search);

            // If Telegram auth params are present
            if (searchParams.has('id') && searchParams.has('first_name') && searchParams.has('hash')) {
                console.log("Found Telegram auth data in URL params");

                // Build the full query string
                var telegramData = url.search.substring(1); // remove the leading '?'

                // Send to Unity
                if (window.unityInstance && window.firebaseAuthObjectName) {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, telegramData);
                    console.log("Sent Telegram auth data to Unity:", telegramData);
                    foundData = true;
                } else {
                    console.error("Unity instance or object name not available for Telegram auth");
                }
            }

            // Check for Mini App format
            if (!foundData && window.Telegram && window.Telegram.WebApp) {
                console.log("Found Telegram WebApp data, processing...");

                try {
                    var tgWebApp = window.Telegram.WebApp;

                    if (tgWebApp.initDataUnsafe && tgWebApp.initDataUnsafe.user) {
                        var user = tgWebApp.initDataUnsafe.user;
                        // Add hash and auth_date to make compatible with standard format

                        // Избегаем использования оператора optional chaining (?.)
                        var hashValue = "";
                        if (tgWebApp.initData) {
                            var parts = tgWebApp.initData.split('&');
                            for (var i = 0; i < parts.length; i++) {
                                var part = parts[i];
                                if (part.indexOf('hash=') === 0) {
                                    var hashParts = part.split('=');
                                    if (hashParts.length > 1) {
                                        hashValue = hashParts[1];
                                    }
                                    break;
                                }
                            }
                        }
                        user.hash = hashValue;
                        user.auth_date = tgWebApp.initDataUnsafe.auth_date || Math.floor(Date.now() / 1000);

                        console.log("Extracted Telegram user from WebApp:", user);

                        // Build compatibility query string format for consistency
                        var params = new URLSearchParams();
                        params.append('id', user.id);
                        params.append('first_name', user.first_name || "");
                        params.append('last_name', user.last_name || "");
                        params.append('username', user.username || "");
                        params.append('photo_url', user.photo_url || "");
                        params.append('auth_date', user.auth_date);
                        params.append('hash', user.hash);

                        // Добавляем поля language_code и allows_write_to_pm
                        if (user.language_code) {
                            params.append('language_code', user.language_code);
                        }
                        if (typeof user.allows_write_to_pm !== 'undefined') {
                            params.append('allows_write_to_pm', user.allows_write_to_pm ? 'true' : 'false');
                        }

                        // Добавляем query_id если он доступен
                        if (tgWebApp.initDataUnsafe && tgWebApp.initDataUnsafe.query_id) {
                            params.append('query_id', tgWebApp.initDataUnsafe.query_id);
                            console.log("Added query_id to auth data:", tgWebApp.initDataUnsafe.query_id);
                        }

                        var telegramData = params.toString();
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, telegramData);
                        console.log("Sent Telegram WebApp data to Unity in standard format:", telegramData);
                        foundData = true;
                    } else if (tgWebApp.initData) {
                        // If we only have raw initData
                        window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, tgWebApp.initData);
                        console.log("Sent raw Telegram WebApp initData to Unity");
                        foundData = true;
                    }
                } catch (webAppError) {
                    console.error("Error processing Telegram WebApp data:", webAppError);
                }
            }

            // Then try using the haveTgAuthResult function from telegramAuthHandler.js if available
            if (!foundData && window.haveTgAuthResult && typeof window.haveTgAuthResult === 'function') {
                var tgAuthResult = window.haveTgAuthResult();
                if (tgAuthResult) {
                    console.log("Found Telegram auth data via haveTgAuthResult()");
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, window.telegramCallback, JSON.stringify(tgAuthResult));
                    foundData = true;
                }
            }

            if (!foundData) {
                console.log("No Telegram auth data found in the URL or WebApp");
            }

            // Always notify that checking is done, even if no data found
            if (window.unityInstance && window.firebaseAuthObjectName) {
                // Send a special completion event to notify Unity that the search process is finished
                var self = this;
                window.setTimeout(function () {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
                }, 500);
            }

            return foundData;
        } catch (error) {
            console.error("Error checking Telegram auth:", error);
            // Don't throw error here, just continue

            // Still notify that checking is done even after error
            if (window.unityInstance && window.firebaseAuthObjectName) {
                var self = this;
                window.setTimeout(function () {
                    window.unityInstance.SendMessage(window.firebaseAuthObjectName, "HandleTelegramAutoAuthComplete");
                }, 500);
            }
        }
        return false;
    },

    InitializeTelegramAuth: function (botId) {
        try {
            var parsedBotId = botId ? JSON.parse(UTF8ToString(botId)) : 8114226239;

            console.log("Telegram init: " + parsedBotId);

            window.initializeTelegramAuth({ bot_id: parsedBotId },
                function (user) {
                    console.log("Telegram auth initialized" + window.telegramCallback);
                    window.unityInstance.SendMessage("FirebaseWebGLAuthPlugin", window.telegramCallback, JSON.stringify(user));
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