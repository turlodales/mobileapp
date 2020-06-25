using System;
using Toggl.Shared;

namespace Toggl.Core.UI.Parameters
{
    public sealed class CredentialsParameter
    {
        public static CredentialsParameter Empty { get; } = new CredentialsParameter { Email = Email.Empty, Password = Password.Empty, IsForAccountLinking = false };

        public CredentialsParameter()
        {
            Email = Email.Empty;
            Password = Password.Empty;
            IsForAccountLinking = false;
        }

        public Email Email { get; set; }

        public Password Password { get; set; }

        public bool IsForAccountLinking { get; private set; }
        public string ConfirmationCode { get; private set; } = "";

        public static CredentialsParameter With(Email email, Password password, bool isForAccountLinking = false, string confirmationCode = "")
            => new CredentialsParameter { Email = email, Password = password, IsForAccountLinking = isForAccountLinking, ConfirmationCode = confirmationCode};

        public override bool Equals(object obj)
        {
            return obj is CredentialsParameter param &&
                   Email.ToString() == param.Email.ToString() &&
                   Password.ToString() == param.Password.ToString() &&
                   IsForAccountLinking == param.IsForAccountLinking &&
                   ConfirmationCode == param.ConfirmationCode;
        }
    }
}
