using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Toggl.Shared.Tests
{
    public sealed class PasswordTests
    {
        public sealed class TheIsValidProperty
        {
            [Fact, LogIfTooSlow]
            public void ReturnsFalseForEmptyPassword()
            {
                Password.Empty.IsValid.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("")]
            [InlineData("1")]
            [InlineData("pass")]
            [InlineData("12345")]
            [InlineData("    a")]
            [InlineData("\t a")]
            public void ReturnsFalseWhenPasswordIsShorterThan6Characters(
                string passwordString)
            {
                Password.From(passwordString).IsValid.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("123456")]
            [InlineData("qwerty1")]
            [InlineData("12345\t ")]
            public void ReturnsTrueWhenPasswordIsLongerThan6Characters(
                string passwordString)
            {
                Password.From(passwordString).IsValid.Should().BeTrue();
            }
        }

        public sealed class TheIsStrongProperty
        {
            [Fact, LogIfTooSlow]
            public void ReturnsFalseForEmptyOrNullPassword()
            {
                Password.Empty.IsStrong.Should().BeFalse();
                Password.From(null).IsStrong.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("")]
            [InlineData("1")]
            [InlineData("pAs1s")]
            [InlineData("1aB4567")]
            [InlineData("    aff")]
            [InlineData("\t axxX")]
            public void ReturnsFalseWhenPasswordIsShorterThan8Characters(
                string passwordString)
            {
                Password.From(passwordString).IsStrong.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("12345678999aa")]
            [InlineData("fdasfsf333qwerty1")]
            [InlineData("من أي وقت مضى الكلب القط من أي وقت مضىx11")]
            [InlineData("####@!@!@!@!@!@!123aaa")]
            public void ReturnsFalseWhenNoUppercasePresent(
                string passwordString)
            {
                Password.From(passwordString).IsStrong.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("12345678999AA")]
            [InlineData("SASASAS333SASQQQ1")]
            [InlineData("111212121212121X")]
            [InlineData("من أي وقت مضى الكلب القط من أي وقت مضى11X")]
            [InlineData("####@!@!@!@!@!@!123AAA")]
            public void ReturnsFalseWhenNoLowercasePresent(
                string passwordString)
            {
                Password.From(passwordString).IsStrong.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("aaKASJLAJSAKLJLKA")]
            [InlineData("fdLALSSKKSKSKS")]
            [InlineData("من أي وقت مضى الكلب القط من أي وقت مضىxxXX")]
            [InlineData("####@!@!@!@!@!@!aaAA")]
            public void ReturnsFalseWhenNoNumberPresent(
                string passwordString)
            {
                Password.From(passwordString).IsStrong.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData("من أي وقت مضى الكلب القط من أي وقت مضى1xX")]
            [InlineData("1Xxkldsjfklsdajfsdlk")]
            [InlineData("####@!@!@!@!@!@!1xX")]
            public void ReturnsTrueForPasswordsMatchingAllStrengthCriteria(
                string passwordString)
            {
                Password.From(passwordString).IsStrong.Should().BeTrue();
            }
        }


        public sealed class TheEmptyProperty
        {
            [Fact, LogIfTooSlow]
            public void ReturnsEmptyPassword()
            {
                Password.Empty.ToString().Should().Be("");
            }
        }

        public sealed class TheToStringMethod
        {
            [Property]
            public void ReturnsTheSameStringThatPasswordWasCreatedWith(
                NonEmptyString nonEmptyString)
            {
                var passwordString = nonEmptyString.Get;
                var password = Password.From(passwordString);

                password.ToString().Should().Be(passwordString);
            }
        }
    }
}
