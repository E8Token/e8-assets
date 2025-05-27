// Firebase Auth WebGL Plugin JavaScript Implementation
// This file should be placed in WebGLTemplates or built into the final WebGL build

var FirebaseAuthPlugin = {
    // Firebase Auth instance
    firebaseAuth: null,
    
    // Unity callback methods
    unityInstance: null,
    
    // Initialize Firebase Auth
    FirebaseAuth_Initialize: function() {
        console.log('[FirebaseAuth] Initializing Firebase Auth...');
        
        // Initialize Firebase Auth if not already done
        if (typeof firebase !== 'undefined' && firebase.auth) {
            this.firebaseAuth = firebase.auth();
            console.log('[FirebaseAuth] Firebase Auth initialized successfully');
        } else {
            console.error('[FirebaseAuth] Firebase is not loaded or auth module is missing');
        }
    },
    
    // Sign in with email and password
    FirebaseAuth_SignInWithEmailAndPassword: function(email, password, callbackId) {
        var emailStr = UTF8ToString(email);
        var passwordStr = UTF8ToString(password);
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Signing in with email:', emailStr);
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'Firebase Auth not initialized');
            return;
        }
        
        this.firebaseAuth.signInWithEmailAndPassword(emailStr, passwordStr)
            .then((userCredential) => {
                var userJson = this.serializeUser(userCredential.user);
                this.sendSuccess(callbackIdStr, 'OnSignInSuccess', userJson);
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnSignInError', error.message);
            });
    },
    
    // Create user with email and password
    FirebaseAuth_CreateUserWithEmailAndPassword: function(email, password, callbackId) {
        var emailStr = UTF8ToString(email);
        var passwordStr = UTF8ToString(password);
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Creating user with email:', emailStr);
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnCreateUserError', 'Firebase Auth not initialized');
            return;
        }
        
        this.firebaseAuth.createUserWithEmailAndPassword(emailStr, passwordStr)
            .then((userCredential) => {
                var userJson = this.serializeUser(userCredential.user);
                this.sendSuccess(callbackIdStr, 'OnCreateUserSuccess', userJson);
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnCreateUserError', error.message);
            });
    },
    
    // Sign in anonymously
    FirebaseAuth_SignInAnonymously: function(callbackId) {
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Signing in anonymously');
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnSignInError', 'Firebase Auth not initialized');
            return;
        }
        
        this.firebaseAuth.signInAnonymously()
            .then((userCredential) => {
                var userJson = this.serializeUser(userCredential.user);
                this.sendSuccess(callbackIdStr, 'OnSignInSuccess', userJson);
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnSignInError', error.message);
            });
    },
    
    // Sign in with credential
    FirebaseAuth_SignInWithCredential: function(providerId, token, callbackId) {
        var providerIdStr = UTF8ToString(providerId);
        var tokenStr = UTF8ToString(token);
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Signing in with credential, provider:', providerIdStr);
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnSignInError', 'Firebase Auth not initialized');
            return;
        }
        
        // Create credential based on provider
        var credential;
        try {
            switch(providerIdStr) {
                case 'google.com':
                    credential = firebase.auth.GoogleAuthProvider.credential(tokenStr);
                    break;
                case 'facebook.com':
                    credential = firebase.auth.FacebookAuthProvider.credential(tokenStr);
                    break;
                default:
                    throw new Error('Unsupported provider: ' + providerIdStr);
            }
            
            this.firebaseAuth.signInWithCredential(credential)
                .then((userCredential) => {
                    var userJson = this.serializeUser(userCredential.user);
                    this.sendSuccess(callbackIdStr, 'OnSignInSuccess', userJson);
                })
                .catch((error) => {
                    this.sendError(callbackIdStr, 'OnSignInError', error.message);
                });
        } catch (error) {
            this.sendError(callbackIdStr, 'OnSignInError', error.message);
        }
    },
    
    // Sign out
    FirebaseAuth_SignOut: function(callbackId) {
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Signing out');
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnSignOutError', 'Firebase Auth not initialized');
            return;
        }
        
        this.firebaseAuth.signOut()
            .then(() => {
                this.sendSuccess(callbackIdStr, 'OnSignOutSuccess', 'success');
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnSignOutError', error.message);
            });
    },
    
    // Get current user
    FirebaseAuth_GetCurrentUser: function(callbackId) {
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Getting current user');
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnGetCurrentUserError', 'Firebase Auth not initialized');
            return;
        }
        
        var user = this.firebaseAuth.currentUser;
        var userJson = user ? this.serializeUser(user) : 'null';
        this.sendSuccess(callbackIdStr, 'OnGetCurrentUserSuccess', userJson);
    },
    
    // Delete user
    FirebaseAuth_DeleteUser: function(callbackId) {
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Deleting user');
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnDeleteUserError', 'Firebase Auth not initialized');
            return;
        }
        
        var user = this.firebaseAuth.currentUser;
        if (!user) {
            this.sendError(callbackIdStr, 'OnDeleteUserError', 'No user is currently signed in');
            return;
        }
        
        user.delete()
            .then(() => {
                this.sendSuccess(callbackIdStr, 'OnDeleteUserSuccess', 'success');
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnDeleteUserError', error.message);
            });
    },
    
    // Send password reset email
    FirebaseAuth_SendPasswordResetEmail: function(email, callbackId) {
        var emailStr = UTF8ToString(email);
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Sending password reset email to:', emailStr);
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnPasswordResetError', 'Firebase Auth not initialized');
            return;
        }
        
        this.firebaseAuth.sendPasswordResetEmail(emailStr)
            .then(() => {
                this.sendSuccess(callbackIdStr, 'OnPasswordResetSuccess', 'success');
            })
            .catch((error) => {
                this.sendError(callbackIdStr, 'OnPasswordResetError', error.message);
            });
    },
    
    // Connect to emulator
    FirebaseAuth_ConnectToEmulator: function(host, port, callbackId) {
        var hostStr = UTF8ToString(host);
        var callbackIdStr = UTF8ToString(callbackId);
        
        console.log('[FirebaseAuth] Connecting to emulator:', hostStr + ':' + port);
        
        if (!this.firebaseAuth) {
            this.sendError(callbackIdStr, 'OnEmulatorConnectError', 'Firebase Auth not initialized');
            return;
        }
        
        try {
            var url = 'http://' + hostStr + ':' + port;
            this.firebaseAuth.useEmulator(url);
            this.sendSuccess(callbackIdStr, 'OnEmulatorConnectSuccess', 'success');
        } catch (error) {
            this.sendError(callbackIdStr, 'OnEmulatorConnectError', error.message);
        }
    },
    
    // Helper methods
    serializeUser: function(user) {
        if (!user) return 'null';
        
        return JSON.stringify({
            uid: user.uid,
            email: user.email,
            displayName: user.displayName,
            photoURL: user.photoURL,
            emailVerified: user.emailVerified,
            isAnonymous: user.isAnonymous
        });
    },
    
    sendSuccess: function(callbackId, methodName, data) {
        if (typeof unityInstance !== 'undefined' && unityInstance.SendMessage) {
            unityInstance.SendMessage('FirebaseAuthPlugin', methodName, callbackId + '|' + data);
        } else {
            console.log('[FirebaseAuth] Unity instance not available, success:', methodName, data);
        }
    },
    
    sendError: function(callbackId, methodName, error) {
        if (typeof unityInstance !== 'undefined' && unityInstance.SendMessage) {
            unityInstance.SendMessage('FirebaseAuthPlugin', methodName, callbackId + '|' + error);
        } else {
            console.error('[FirebaseAuth] Unity instance not available, error:', methodName, error);
        }
    }
};

// Merge into LibraryManager
mergeInto(LibraryManager.library, FirebaseAuthPlugin);
