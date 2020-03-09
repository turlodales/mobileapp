namespace Toggl.Core.Helper
{
    public struct ThirdPartyLoginInfo
    {
        public string Token { get; }
        public string Fullname { get; }

        public ThirdPartyLoginInfo(string token, string fullname = null)
        {
            Token = token;
            Fullname = fullname;
        }
    }
}
