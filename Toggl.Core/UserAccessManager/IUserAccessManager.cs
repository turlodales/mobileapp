﻿using System;
using System.Reactive;
using Toggl.Core.Helper;
using Toggl.Networking;
using Toggl.Shared;

namespace Toggl.Core.Login
{
    public interface IUserAccessManager
    {
        IObservable<ITogglApi> UserLoggedIn { get; }
        IObservable<Unit> UserLoggedOut { get; }

        void OnUserLoggedOut();

        bool CheckIfLoggedIn();

        string GetSavedApiToken();

        void LoginWithSavedCredentials();

        IObservable<ITogglApi> ThirdPartyLogin(ThirdPartyLoginProvider provider, ThirdPartyLoginInfo loginInfo);
        IObservable<ITogglApi> Login(Email email, Password password);

        IObservable<Unit> ThirdPartySignUp(ThirdPartyLoginProvider provider, ThirdPartyLoginInfo loginInfo, bool termsAccepted, int countryId, string timezone);
        IObservable<Unit> SignUp(Email email, Password password, bool termsAccepted, int countryId, string timezone);

        IObservable<Unit> RefreshToken(Password password);

        IObservable<string> ResetPassword(Email email);
    }
}
