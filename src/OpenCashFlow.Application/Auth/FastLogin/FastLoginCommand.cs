namespace OpenCashFlow.Application.Auth.FastLogin;

public sealed record FastLoginCommand(string Pin, string CookieValue, int SessionMinutes);
