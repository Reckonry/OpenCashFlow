namespace OpenCashFlow.Application.Auth.Login;

public sealed record LoginCommand(string Username, string Password, bool RequireActiveAccount, int SessionMinutes);
