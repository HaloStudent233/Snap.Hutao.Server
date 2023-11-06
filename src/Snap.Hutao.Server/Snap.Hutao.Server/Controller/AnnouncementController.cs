﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 公告控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class AnnouncementController : ControllerBase
{
    private readonly AppDbContext appDbContext;

    public AnnouncementController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <summary>
    /// 获取胡桃公告
    /// </summary>
    /// <param name="locale">语言</param>
    /// <param name="excludedIds">排除的Id</param>
    /// <returns>公告信息</returns>
    [HttpPost("List")]
    public IActionResult List([FromQuery] string locale, [FromBody] HashSet<long> excludedIds)
    {
        long limit = (DateTimeOffset.Now - TimeSpan.FromDays(30)).ToUnixTimeSeconds();
        List<EntityAnnouncement> anns = appDbContext.Announcements
            .AsNoTracking()
            .OrderByDescending(ann => ann.LastUpdateTime)
            .Where(ann => ann.Locale == locale)
            .Where(ann => ann.LastUpdateTime >= limit)
            .ToList();

        string? userAgent = Request.Headers.UserAgent;
        Version version = !string.IsNullOrEmpty(userAgent) && userAgent.StartsWith("Snap Hutao/")
            ? new(userAgent!["Snap Hutao/".Length..])
            : new(0, 0, 0, 0);

        List<EntityAnnouncement> result = new();
        foreach (EntityAnnouncement ann in anns)
        {
            if (!string.IsNullOrEmpty(ann.MaxPresentVersion) && version > new Version(ann.MaxPresentVersion))
            {
                continue;
            }

            if (!excludedIds.Contains(ann.Id))
            {
                result.Add(ann);
            }
        }

        return Model.Response.Response<List<EntityAnnouncement>>.Success("获取公告成功", result);
    }
}