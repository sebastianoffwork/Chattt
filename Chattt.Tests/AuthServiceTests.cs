using Chattt.Data;
using Chattt.DTOs;
using Chattt.Entities;
using Chattt.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Chattt.Tests;

public class AuthServiceTests
{
    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IConfiguration GetMockConfiguration()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("Super_Secret_Key_For_Testing_Purpose_Only_12345");
        return mockConfig.Object;
    }

    [Fact]
    public async Task Register_Should_ReturnTokens_When_UserIsNew()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var config = GetMockConfiguration();
        var service = new AuthService(db, config);

        var dto = new RegisterDto("new_user", "password123");

        /*
         * Act.
         */
        var result = await service.RegisterAsync(dto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Error.Should().BeNull();

        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Username == "new_user");
        userInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_Should_Fail_When_UsernameExists()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new AuthService(db, GetMockConfiguration());

        db.Users.Add(new User
        {
            Username = "existing_user",
            PasswordHash = "some_hash"
        });
        await db.SaveChangesAsync();

        var dto = new RegisterDto("existing_user", "new_password");

        /*
         * Act.
         */
        var result = await service.RegisterAsync(dto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeFalse();
        result.AccessToken.Should().BeNull();
        result.Error.Should().Be("Username already exists");
    }

    [Fact]
    public async Task Login_Should_ReturnTokens_When_CredentialsAreCorrect()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new AuthService(db, GetMockConfiguration());

        await service.RegisterAsync(new RegisterDto("valid_user", "secret_pass"));

        var loginDto = new LoginDto("valid_user", "secret_pass");

        /*
         * Act.
         */
        var result = await service.LoginAsync(loginDto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().NotBeNull();
        result.RefreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_Should_Fail_When_UserDoesNotExist()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new AuthService(db, GetMockConfiguration());
        var loginDto = new LoginDto("ghost_user", "password");

        /*
         * Act.
         */
        var result = await service.LoginAsync(loginDto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_Should_Fail_When_PasswordIsWrong()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new AuthService(db, GetMockConfiguration());

        await service.RegisterAsync(new RegisterDto("user", "correct_password"));
        var loginDto = new LoginDto("user", "wrong_password");

        /*
         * Act.
         */
        var result = await service.LoginAsync(loginDto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task RefreshToken_Should_ReturnNewAccess_When_TokenIsValid()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new AuthService(db, GetMockConfiguration());

        await service.RegisterAsync(new RegisterDto("user", "pass"));
        var loginResult = await service.LoginAsync(new LoginDto("user", "pass"));

        var oldAccessToken = loginResult.AccessToken;
        var refreshToken = loginResult.RefreshToken;

        await Task.Delay(10);

        /*
         * Act.
         */
        var refreshResult = await service.RefreshTokenAsync(new RefreshTokenRequest(oldAccessToken!, refreshToken!));

        /*
         * Assert.
         */
        refreshResult.IsSuccess.Should().BeTrue();
        refreshResult.AccessToken.Should().NotBeNull();
        refreshResult.AccessToken.Should().NotBe(oldAccessToken);
        refreshResult.RefreshToken.Should().NotBeNull();

        var oldTokenInDb = await db.RefreshTokens.FirstAsync(t => t.Token == refreshToken);
        oldTokenInDb.RevokedOn.Should().NotBeNull();
    }
}