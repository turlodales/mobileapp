using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Gms.Tasks;
using Toggl.Droid.Extensions;
using Toggl.Shared.Extensions;
using Object = Java.Lang.Object;


namespace Toggl.Droid.Activities
{
    public abstract partial class ReactiveActivity<TViewModel>
    {
        private const int googleSignInResult = 9002;
        private readonly object lockable = new object();

        private bool isLoggingIn;
        private GoogleSignInClient signInClient;
        private Subject<string> loginSubject = new Subject<string>();

        public IObservable<string> GetGoogleToken()
        {
            return signOutIfNeeded().SelectMany(getGoogleToken);

            IObservable<string> getGoogleToken(Unit _)
            {
                lock (lockable)
                {
                    if (isLoggingIn)
                    {
                        return loginSubject.AsObservable();
                    }

                    isLoggingIn = true;
                    loginSubject = new Subject<string>();

                    SignIn();
                    return loginSubject.AsObservable();
                }
            }

            IObservable<Unit> signOutIfNeeded()
            {
                var logoutSubject = new Subject<Unit>();
                var logoutCallback = new SignOutCallback(() => logoutSubject.CompleteWith(Unit.Default));
                signInClient.SignOut().AddOnCompleteListener(this, logoutCallback);
                return logoutSubject.AsObservable();
            }
        }

        private void SignIn()
        {
            var intent = signInClient.SignInIntent;
            StartActivityForResult(intent, googleSignInResult);
        }

        private void onGoogleSignInResult(Intent data)
        {
            lock (lockable)
            {
                var completedTask = GoogleSignIn.GetSignedInAccountFromIntent(data);
                try
                {
                    var account = completedTask.GetResult(JavaUtils.ToClass<ApiException>()) as GoogleSignInAccount;
                    loginSubject.OnNext(account.IdToken);
                    loginSubject.OnCompleted();
                }
                catch (ApiException e) {
                    loginSubject.OnError(e);
                }
                finally
                {
                    isLoggingIn = false;
                }
            }
        }

        private void initializeGoogleClient()
        {
            if (signInClient != null) return;

            var signInOptions = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken("{TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID}")
                .RequestEmail()
                .Build();

            signInClient = GoogleSignIn.GetClient(this, signInOptions);
        }

        private class SignOutCallback : Object, IOnCompleteListener
        {
            private Action callback;

            public SignOutCallback(Action callback)
            {
                this.callback = callback;
            }

            public void OnComplete(Task task)
            {
                callback();
            }
        }
    }
}
