﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebHookReceiverApi.Middleware;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApiTests;

public class ApiKeyAuthMiddlewareTests
{
    private readonly Mock<IOptions<ApiKeySettings>> _mockOptions;
    private readonly Mock<ILogger<ApiKeyAuthMiddleware>> _mockLogger;
    private readonly ApiKeySettings _apiKeySettings;
    private readonly string _validApiKey = "test-api-key";

    public ApiKeyAuthMiddlewareTests()
    {
        _apiKeySettings = new ApiKeySettings { ApiKey = _validApiKey };
        _mockOptions = new Mock<IOptions<ApiKeySettings>>();
        _mockOptions.Setup(x => x.Value).Returns(_apiKeySettings);
        _mockLogger = new Mock<ILogger<ApiKeyAuthMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WithValidApiKey_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/webhook";
        context.Request.Method = "POST";
        context.Request.Headers["X-API-Key"] = _validApiKey;

        var nextDelegateCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthMiddleware(next, _mockLogger.Object, _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextDelegateCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status code
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKey_Returns401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/webhook";
        context.Request.Method = "POST";
        context.Request.Headers["X-API-Key"] = "invalid-key";

        var nextDelegateCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthMiddleware(next, _mockLogger.Object, _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextDelegateCalled);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingApiKey_Returns401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/webhook";
        context.Request.Method = "POST";
        // No API key header

        var nextDelegateCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthMiddleware(next, _mockLogger.Object, _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextDelegateCalled);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ForSignalRHub_SkipsAuthentication()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/notificationHub";
        context.Request.Method = "GET";
        // No API key header

        var nextDelegateCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthMiddleware(next, _mockLogger.Object, _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextDelegateCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status code
    }

    [Fact]
    public async Task InvokeAsync_ForGetRequest_SkipsAuthentication()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/webhook";
        context.Request.Method = "GET";
        // No API key header

        var nextDelegateCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthMiddleware(next, _mockLogger.Object, _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextDelegateCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status code
    }
}
