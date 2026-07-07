namespace OpenCashFlow.Application.Auth.FastLogin;

public sealed record GenerateFastLoginCookieCommand(string Username, string Password, bool Enabled);
