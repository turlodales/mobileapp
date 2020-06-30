using Toggl.Shared;

namespace Toggl.Core.UI.Parameters
{
    public sealed class SsoLinkParameters
    {
        public static SsoLinkParameters WithEmailAndConfirmationCode(Email email, string confirmationCode)
        {
            return new SsoLinkParameters
            {
                Email = email,
                ConfirmationCode = confirmationCode
            };
        }

        public Email Email { get; private set; }
        public string ConfirmationCode { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is SsoLinkParameters other &&
                   other.Email.ToString() == Email.ToString() &&
                   other.ConfirmationCode == ConfirmationCode;
        }
    }
}