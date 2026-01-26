
namespace VNVTStore.API.Controllers.v1;

public record ExternalLoginRequest(
    string Provider, // "Google" or "Facebook"
    string Token // The access token from the provider
);
