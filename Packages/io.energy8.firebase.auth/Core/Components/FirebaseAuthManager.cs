using System;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Components
{
    /// <summary>
    /// Unity component for Firebase Authentication integration
    /// </summary>
    public class FirebaseAuthManager : MonoBehaviour
    {
        [Header("Auto Initialize")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnInitialized;
        public UnityEngine.Events.UnityEvent<string> OnUserSignedIn;
        public UnityEngine.Events.UnityEvent OnUserSignedOut;
        public UnityEngine.Events.UnityEvent<string> OnAuthError;

        private bool isInitialized = false;

        /// <summary>
        /// Current authenticated user
        /// </summary>
        public FirebaseUser CurrentUser => FirebaseAuth.CurrentUser;

        /// <summary>
        /// Whether user is signed in
        /// </summary>
        public bool IsSignedIn => CurrentUser != null;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private async void Start()
        {
            if (initializeOnStart)
            {
                await InitializeAsync();
            }
        }        private void OnEnable()
        {
            FirebaseAuth.OnAuthStateChanged += OnAuthStateChanged;
        }

        private void OnDisable()
        {
            FirebaseAuth.OnAuthStateChanged -= OnAuthStateChanged;
        }

        /// <summary>
        /// Initialize Firebase Auth
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (isInitialized)
                return true;

            try
            {
                await FirebaseAuth.InitializeAsync();
                isInitialized = true;
                OnInitialized?.Invoke();
                Debug.Log("[FirebaseAuthManager] Initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Initialization failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign in with email and password
        /// </summary>
        public async Task<bool> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var result = await FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                if (result?.User != null)
                {
                    Debug.Log($"[FirebaseAuthManager] User signed in: {result.User.Email}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Sign in failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create user with email and password
        /// </summary>
        public async Task<bool> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var result = await FirebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
                if (result?.User != null)
                {
                    Debug.Log($"[FirebaseAuthManager] User created: {result.User.Email}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] User creation failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign in anonymously
        /// </summary>
        public async Task<bool> SignInAnonymouslyAsync()
        {
            try
            {
                var result = await FirebaseAuth.SignInAnonymouslyAsync();
                if (result?.User != null)
                {
                    Debug.Log("[FirebaseAuthManager] User signed in anonymously");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Anonymous sign in failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out current user
        /// </summary>
        public async Task<bool> SignOutAsync()
        {
            try
            {
                await FirebaseAuth.SignOutAsync();
                Debug.Log("[FirebaseAuthManager] User signed out");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Sign out failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete current user
        /// </summary>
        public async Task<bool> DeleteUserAsync()
        {
            try
            {
                if (CurrentUser != null)
                {
                    await FirebaseAuth.DeleteUserAsync();
                    Debug.Log("[FirebaseAuthManager] User deleted");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] User deletion failed: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
                return false;
            }
        }

        private void OnAuthStateChanged(FirebaseUser user)
        {
            if (user != null)
            {
                OnUserSignedIn?.Invoke(user.Uid);
            }
            else
            {
                OnUserSignedOut?.Invoke();
            }
        }
    }
}
