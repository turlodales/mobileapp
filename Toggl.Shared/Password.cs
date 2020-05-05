﻿using System.Linq;

namespace Toggl.Shared
{
    public struct Password
    {
        private const int minimumLength = 6;
        private const int minimumStrongLength = 8;

        private readonly string password;

        public bool IsStrong { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(password);

        public int Length => password.Length;

        private Password(string password)
        {
            this.password = password;
            IsStrong = password != null && password.Length >= minimumStrongLength
                                        && password.Any(char.IsDigit)
                                        && password.Any(char.IsUpper)
                                        && password.Any(char.IsLower);
        }

        public override string ToString() => password;

        public static Password From(string password)
            => new Password(password);

        public static Password Empty { get; } = new Password("");
    }
}
