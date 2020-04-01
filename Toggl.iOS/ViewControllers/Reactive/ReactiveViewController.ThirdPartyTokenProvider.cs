using Foundation;
using Google.SignIn;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AuthenticationServices;
using Toggl.Core.Exceptions;
using Toggl.Core.Helper;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public abstract partial class ReactiveViewController<TViewModel> : ISignInDelegate, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        private const int cancelErrorCode = -5;

        private bool loggingIn;
        private Subject<ThirdPartyLoginInfo> tokenSubject = new Subject<ThirdPartyLoginInfo>();
        private ASAuthorizationAppleIdProvider appleIdProvider;

        public IObservable<ThirdPartyLoginInfo> GetLoginInfo(ThirdPartyLoginProvider provider)
        {
            switch (provider)
            {
                case ThirdPartyLoginProvider.Google: return getGoogleLoginInfo();
                case ThirdPartyLoginProvider.Apple: return getAppleLoginInfo();
                default: throw new NotSupportedException($"Cannot handle case {provider}");
            }
        }

        # region Google login

        private IObservable<ThirdPartyLoginInfo> getGoogleLoginInfo()
        {
            if (loggingIn)
                return tokenSubject.AsObservable();

            if (SignIn.SharedInstance.CurrentUser != null)
                SignIn.SharedInstance.SignOutUser();

            SignIn.SharedInstance.Delegate = this;
            SignIn.SharedInstance.PresentingViewController = this;
            SignIn.SharedInstance.SignInUser();
            loggingIn = true;

            return tokenSubject.AsObservable();
        }

        public void DidSignIn(SignIn signIn, GoogleUser user, NSError error)
        {
            if (error != null)
            {
                tokenSubject.OnError(new ThirdPartyLoginException(ThirdPartyLoginProvider.Google, error.Code == cancelErrorCode));
            }
            else
            {
                var token = user.Authentication.AccessToken;
                var loginInfo = new ThirdPartyLoginInfo(token);
                signIn.DisconnectUser();
                tokenSubject.CompleteWith(loginInfo);
            }

            tokenSubject = new Subject<ThirdPartyLoginInfo>();
            loggingIn = false;
        }

        [Export("signIn:presentViewController:")]
        public void PresentViewController(SignIn signIn, UIViewController viewController)
        {
            PresentViewController(viewController, true, null);
        }

        [Export("signIn:dismissViewController:")]
        public void DismissViewController(SignIn signIn, UIViewController viewController)
        {
            DismissViewController(true, null);
        }

        #endregion

        #region Apple sign in

        private IObservable<ThirdPartyLoginInfo> getAppleLoginInfo()
        {
            if (loggingIn)
                return tokenSubject.AsObservable();

            appleIdProvider = new ASAuthorizationAppleIdProvider();
            var request = appleIdProvider.CreateRequest();
            request.RequestedScopes = new[] { ASAuthorizationScope.Email, ASAuthorizationScope.FullName };
            var controller = new ASAuthorizationController(new[] { request });
            controller.Delegate = this;
            controller.PresentationContextProvider = this;
            controller.PerformRequests();
            loggingIn = true;

            return tokenSubject.AsObservable();
        }

        [Export("authorizationController:didCompleteWithAuthorization:")]
        private void authorizationControllerDidComplete(ASAuthorizationController controller, ASAuthorization authorization)
        {
            if (authorization.GetCredential<ASAuthorizationAppleIdCredential>() is ASAuthorizationAppleIdCredential appleIdCredential)
            {
                var jwtData = appleIdCredential.IdentityToken;
                var jwt = jwtData.ToString(NSStringEncoding.UTF8).ToString();

                // Try to get the fullname, send null if it's not possible so backend defaults to the email address
                var nameFormatter = new NSPersonNameComponentsFormatter();
                nameFormatter.Style = NSPersonNameComponentsFormatterStyle.Default;
                var fullname = nameFormatter.StringFor(appleIdCredential.FullName);
                fullname = string.IsNullOrEmpty(fullname) ? null : fullname;

                var loginInfo = new ThirdPartyLoginInfo(jwt, fullname);
                tokenSubject.CompleteWith(loginInfo);
            }

            finishAppleLogin();
        }

        [Export ("authorizationController:didCompleteWithError:")]
        public void authorizationControllerDidFail(ASAuthorizationController controller, NSError error)
        {
            var userCanceled = error.Code == (int)ASAuthorizationError.Canceled;
            tokenSubject.OnError(new ThirdPartyLoginException(ThirdPartyLoginProvider.Apple, userCanceled));
            finishAppleLogin();
        }

        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
            => View.Window;

        private void finishAppleLogin()
        {
            tokenSubject = new Subject<ThirdPartyLoginInfo>();
            loggingIn = false;
        }

        #endregion
    }
}
