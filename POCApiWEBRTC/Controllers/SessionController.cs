using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTokSDK;
using POCApiWEBRTC.Infra;
using POCApiWEBRTC.Models;
using System.Net;

namespace POCApiWEBRTC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    [HttpGet]
    [Route("/getall")]
    public async Task<IActionResult> GetSessions(
        [FromServices] ApplicationDbContext _contexto)
    {
        try
        {
            var sessions = await _contexto.Session.ToListAsync();

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet]
    [Route("/get/{sessionid}")]
    public async Task<IActionResult> GetSessions(
        [FromServices] ApplicationDbContext _contexto, 
        string sessionid)
    {
        try
        {
            var session = await _contexto.Session.Where(x => x.Id == sessionid).FirstOrDefaultAsync();

            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost]
    [Route("/publisher")]
    public async Task<IActionResult> Publisher(
        [FromServices] IConfiguration configuration, 
        [FromServices] ApplicationDbContext _contexto)
    {
        try
        {
            var apiKey = int.Parse(configuration.GetRequiredSection("API_KEY").Value!);
            var secret = configuration.GetRequiredSection("API_SECRET").Value;
            var createSession = CreateSession(apiKey, secret!, MediaMode.ROUTED, ArchiveMode.ALWAYS, Role.PUBLISHER);

            _contexto.Add(createSession);
            var isSave = await _contexto.SaveChangesAsync();

            if (isSave > 0)
                return Ok(createSession);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost]
    [Route("/subscriber/{sessionid}")]
    public async Task<ActionResult> Subscriber(
        [FromServices] IConfiguration configuration,
        [FromServices] ApplicationDbContext _contexto,
        string sessionid)
    {
        try
        {
            var apiKey = int.Parse(configuration.GetRequiredSection("API_KEY").Value!);
            var secret = configuration.GetRequiredSection("API_SECRET").Value;

            var findSession = await _contexto.Session.Where(x => x.Id == sessionid).FirstOrDefaultAsync();

            if (findSession != null)
            {
                var openTok = new OpenTok(apiKey, secret);
                var inOneWeek = (DateTime.UtcNow.Add(TimeSpan.FromDays(7)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var subscriberToken = openTok.GenerateToken(findSession.Id, Role.SUBSCRIBER, inOneWeek);

                findSession.Token = subscriberToken;

                return Ok(findSession);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpDelete]
    [Route("/delete/all")]
    public async Task<ActionResult> DeleteAll([FromServices] ApplicationDbContext _contexto)
    {
        try
        {
            var sessions = await _contexto.Session.ToListAsync();

            _contexto.RemoveRange(sessions);
            await _contexto.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    private SessionModel CreateSession(int apikey, string secret, MediaMode mediaMode, ArchiveMode archiveMode, Role papel)
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        var openTok = new OpenTok(apikey, secret);
        var session = openTok.CreateSession("", mediaMode, archiveMode);
        var inOneWeek = (DateTime.UtcNow.Add(TimeSpan.FromDays(7)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        var token = session.GenerateToken(papel, inOneWeek);

        return new SessionModel
        {
            Id = session.Id,
            ApiKey = session.ApiKey,
            ApiSecret = session.ApiSecret,
            Token = token
        };
    }
}