using Histador.Data;
using Histador.Data.Entities;

namespace Histador.Services;

public class CurrentPlayerService
{
    private readonly IHttpContextAccessor _http;
    private readonly IConfiguration _config;
    private readonly HistadorDbContext _db;

    public CurrentPlayerService(IHttpContextAccessor http, IConfiguration config, HistadorDbContext db)
    {
        _http = http;
        _config = config;
        _db = db;
    }

    public Player? Get()
    {
        var user = _http.HttpContext?.User;
        var name = user?.Identity?.IsAuthenticated == true
            ? user.Identity.Name
            : _config["Dev:CurrentPlayer"];

        if (string.IsNullOrWhiteSpace(name)) return null;

        return _db.Players.FirstOrDefault(p => p.Username == name || p.DisplayName == name);
    }
}
