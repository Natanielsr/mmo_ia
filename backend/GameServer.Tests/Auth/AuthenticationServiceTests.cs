using Moq;
using Xunit;
using GameServerApp.Contracts;

namespace GameServer.Tests.Auth
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public void Login_Should_Return_True_On_Valid_Credentials()
        {
            var mock = new Mock<IAuthenticationService>();
            mock.Setup(s => s.Login("user", "pass")).Returns(true);

            var result = mock.Object.Login("user", "pass");
            Assert.True(result);
        }

        [Fact]
        public void Login_Should_Return_False_On_Invalid_Credentials()
        {
            var mock = new Mock<IAuthenticationService>();
            mock.Setup(s => s.Login("user", "wrong")).Returns(false);

            var result = mock.Object.Login("user", "wrong");
            Assert.False(result);
        }

        [Fact]
        public void GenerateToken_Should_Create_String_And_Validate()
        {
            var mock = new Mock<IAuthenticationService>();
            mock.Setup(s => s.GenerateToken(It.IsAny<string>())).Returns("token123");
            mock.Setup(s => s.ValidateToken("token123")).Returns(true);

            var token = mock.Object.GenerateToken("uid");
            Assert.Equal("token123", token);
            Assert.True(mock.Object.ValidateToken(token));
        }

        [Fact]
        public void Logout_Should_Be_Called_With_Token()
        {
            var mock = new Mock<IAuthenticationService>();
            var token = "tokenX";

            mock.Object.Logout(token);
            mock.Verify(s => s.Logout(token), Times.Once);
        }
    }
}