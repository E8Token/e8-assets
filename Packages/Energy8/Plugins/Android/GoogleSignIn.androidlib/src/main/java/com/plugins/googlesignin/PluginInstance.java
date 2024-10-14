package com.plugins.googlesignin;

import android.app.Activity;
import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Base64;

import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialException;

import com.google.android.libraries.identity.googleid.GetGoogleIdOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.UUID;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public class PluginInstance {

    public Activity mainActivity;
    public String webClientId;

    public PluginInstance(Activity activity, String id) {
        mainActivity = activity;
        webClientId = id;
    }

    public void signIn(SignInCallback callback) {
        Context context = mainActivity.getApplicationContext();
        CredentialManager credentialManager = CredentialManager.create(context);
        String nonce = generateNonce();

        GetGoogleIdOption getGoogleIdOption = new GetGoogleIdOption.Builder()
                .setFilterByAuthorizedAccounts(false)
                .setServerClientId(webClientId)
                .setNonce(nonce)
                .build();

        GetCredentialRequest getCredentialRequest = new GetCredentialRequest.Builder()
                .addCredentialOption(getGoogleIdOption)
                .build();

        Executor executor = Executors.newSingleThreadExecutor();

        credentialManager.getCredentialAsync(
                mainActivity,
                getCredentialRequest,
                null,
                executor,
                new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                    @Override
                    public void onResult(GetCredentialResponse result) {
                        GoogleIdTokenCredential idToken = GoogleIdTokenCredential.createFrom(result.getCredential().getData());
                        new Handler(Looper.getMainLooper()).Post<T>(() -> callback.onSuccess(idToken.getIdToken()));
                    }

                    @Override
                    public void onError(GetCredentialException e) {
                        new Handler(Looper.getMainLooper()).Post<T>(() -> callback.onError(e));
                    }
                }
        );
    }

    public interface SignInCallback {
        void onSuccess(String idToken);
        void onError(Exception e);
    }

    static String generateNonce() {
        String rawNonce = UUID.randomUUID().toString();
        byte[] bytes = rawNonce.getBytes(StandardCharsets.UTF_8);
        MessageDigest md;
        try {
            md = MessageDigest.getInstance("SHA-256");
        } catch (NoSuchAlgorithmException e) {
            throw new RuntimeException(e);
        }
        byte[] digest = md.digest(bytes);
        return Base64.encodeToString(digest, Base64.DEFAULT).trim();
    }
}
