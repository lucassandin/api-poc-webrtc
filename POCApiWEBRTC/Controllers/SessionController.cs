using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using OpenTokSDK;
using POCApiWEBRTC.Infra;
using POCApiWEBRTC.Models;
using System.Net;

namespace POCApiWEBRTC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    private readonly Fixture _fixture;

    public SessionController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
        _fixture = new Fixture();
    }

    [HttpPost]
    [Route("/session/create")]
    public async Task<IActionResult> PostSessionPublisher()
    {
        try
        {
            var testes = _fixture.Create<SessionModel>();

            _contexto.Add(testes);
            _contexto.SaveChanges();

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet]
    [Route("/session/create/publisher")]
    public async Task<IActionResult> GetSessionPublisher([FromServices] IConfiguration configuration)
    {
        try
        {
            var apiKey = configuration.GetRequiredSection("API_KEY").Value;
            var secret = configuration.GetRequiredSection("API_SECRET").Value;

            return Ok(CreateSession(int.Parse(apiKey), secret, MediaMode.ROUTED, ArchiveMode.MANUAL, Role.PUBLISHER));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet]
    [Route("/session/create/subscriber/{sessionid}")]
    public async Task<ActionResult> GetSessionSubscriber([FromServices] IConfiguration configuration, string sessionid)
    {
        try
        {
            var apiKey = configuration.GetRequiredSection("API_KEY").Value;
            var secret = configuration.GetRequiredSection("API_SECRET").Value;

            return Ok(CreateSession(int.Parse(apiKey), secret, MediaMode.ROUTED, ArchiveMode.MANUAL, Role.SUBSCRIBER, sessionid));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    private SessionModel CreateSession(int apikey, string secret, MediaMode mediaMode, ArchiveMode archiveMode, Role papel, string sessionId = "")
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        var openTok = new OpenTok(apikey, secret);
        var session = openTok.CreateSession("", mediaMode, archiveMode);
        var inOneWeek = (DateTime.UtcNow.Add(TimeSpan.FromDays(7)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        var token = string.Empty;
        if (sessionId != "")
            token = openTok.GenerateToken(sessionId, papel, inOneWeek);
        else
            token = session.GenerateToken(papel, inOneWeek);

        return new SessionModel
        {
            Id = session.Id,
            ApiKey = session.ApiKey,
            ApiSecret = session.ApiSecret,
            Token = token
        };
    }
}