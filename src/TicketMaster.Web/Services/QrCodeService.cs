using Microsoft.IdentityModel.Tokens;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TicketMaster.Web.Services;

public class QrCodeService
{
    private readonly IConfiguration _config;
    public QrCodeService(IConfiguration config) => _config = config;

    public string GerarPayloadJwt(Guid ticketId, Guid eventId, string seatId, string userId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "ChaveSuperSecretaTicketMaster2026!"));
        var token = new JwtSecurityToken(
            issuer: "ticketmaster",
            audience: "ticket-validator",
            claims: new[]
            {
                new Claim("tid", ticketId.ToString()),
                new Claim("eid", eventId.ToString()),
                new Claim("sid", seatId),
                new Claim("uid", userId),
            },
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public byte[] GerarQrCodePng(string payload)
    {
        using var gen = new QRCodeGenerator();
        var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qr = new PngByteQRCode(data);
        return qr.GetGraphic(10);
    }
}
