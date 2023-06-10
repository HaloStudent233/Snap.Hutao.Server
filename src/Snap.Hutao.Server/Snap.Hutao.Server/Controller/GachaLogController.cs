﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Model.Response;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 祈愿记录控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(GroupName = "GachaLog")]
public class GachaLogController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public GachaLogController(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    [HttpGet("Statistics/CurrentEventStatistics")]
    public IActionResult CurrentEventStatistics()
    {
        return GetStatistics<GachaEventStatistics>(GachaStatistics.GachaEventStatistics);
    }

    [HttpGet("Statistics/Distribution/AvatarEvent")]
    public IActionResult AvatarEventDistribution()
    {
        return GetStatistics<GachaDistribution>(GachaStatistics.AvaterEventGachaDistribution);
    }

    [HttpGet("Statistics/Distribution/WeaponEvent")]
    public IActionResult WeaponEventDistribution()
    {
        return GetStatistics<GachaDistribution>(GachaStatistics.WeaponEventGachaDistribution);
    }

    [HttpGet("Statistics/Distribution/Standard")]
    public IActionResult StandardDistribution()
    {
        return GetStatistics<GachaDistribution>(GachaStatistics.StandardGachaDistribution);
    }

    /// <summary>
    /// 获取 Uid 列表
    /// </summary>
    /// <returns>Uid 列表</returns>
    [Authorize]
    [HttpGet("Uids")]
    public async Task<IActionResult> GetUidsAsync()
    {
        int userId = this.GetUserId();

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "未开通祈愿记录上传服务或已到期");
        }

        List<string> uids = await appDbContext.GachaItems
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .Select(x => x.Uid)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);

        return Response<List<string>>.Success("获取 Uid 成功", uids);
    }

    /// <summary>
    /// 获取各个卡池对应的最后Id
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>各个卡池对应的最后Id</returns>
    [Authorize]
    [HttpGet("EndIds")]
    public async Task<IActionResult> GetEndIdsAsync([FromQuery(Name = "Uid")] string uid)
    {
        int userId = this.GetUserId();

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "未开通祈愿记录上传服务或已到期");
        }

        EndIds endIds = new();
        foreach (GachaConfigType type in EndIds.QueryTypes)
        {
            EntityGachaItem? item = appDbContext.GachaItems
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == type)
                .OrderByDescending(i => i.Id)
                .FirstOrDefault();

            endIds.Add(type, item?.Id ?? 0L);
        }

        return Response<EndIds>.Success(userId.ToString(), endIds);
    }

    /// <summary>
    /// 获取小于 End Id 的祈愿记录
    /// </summary>
    /// <param name="uidAndEndIds">数据</param>
    /// <returns>祈愿记录</returns>
    [Authorize]
    [HttpPost("Retrieve")]
    public async Task<IActionResult> RetrieveAsync([FromBody] UidAndEndIds uidAndEndIds)
    {
        int userId = this.GetUserId();

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "未开通祈愿记录上传服务或已到期");
        }

        string uid = uidAndEndIds.Uid;
        EndIds endIds = uidAndEndIds.EndIds;
        List<SimpleGachaItem> gachaItems = new();

        foreach ((string type, long endId) in endIds)
        {
            GachaConfigType configType = Enum.Parse<GachaConfigType>(type);
            long exactEndId = endId == 0 ? long.MaxValue : endId;

            List<EntityGachaItem> items = await appDbContext.GachaItems
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == configType)
                .Where(i => i.Id < exactEndId)
                .ToListAsync()
                .ConfigureAwait(false);

            AppendEntitiesToModels(items, gachaItems);
        }

        return Response<List<SimpleGachaItem>>.Success(userId.ToString(), gachaItems);
    }

    /// <summary>
    /// 上传祈愿记录
    /// </summary>
    /// <param name="gachaData">祈愿数据</param>
    /// <returns>上传成功</returns>
    [Authorize]
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] UidAndItems gachaData)
    {
        int userId = this.GetUserId();

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "当前胡桃账号未开通祈愿记录上传服务，或服务已到期");
        }

        string uid = gachaData.Uid;

        try
        {
            List<EntityGachaItem> entities = new();
            AppendModelsToEntities(gachaData.Items, entities, userId, gachaData.Uid, true);
            int count = await appDbContext.GachaItems.AddRangeAndSaveAsync(entities).ConfigureAwait(false);

            return Model.Response.Response.Success($"上传了 {gachaData.Items.Count} 条数据，存储了 {count} 条数据");
        }
        catch
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogDatabaseOperationFailed, "数据异常，无法保存至云端");
        }
    }

    /// <summary>
    /// 删除祈愿记录
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>响应</returns>
    [Authorize]
    [HttpGet("Delete")]
    public async Task<IActionResult> DeleteAsync([FromQuery(Name = "Uid")] string uid)
    {
        int userId = this.GetUserId();

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            return Model.Response.Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "当前胡桃账号未开通祈愿记录上传服务，或服务已到期");
        }

        int count = await appDbContext.GachaItems
            .Where(i => i.UserId == userId)
            .Where(i => i.Uid == uid)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        return Model.Response.Response.Success($"删除了 {count} 条记录");
    }

    private static void AppendEntitiesToModels(List<EntityGachaItem> entities, List<SimpleGachaItem> models)
    {
        foreach (ref EntityGachaItem item in CollectionsMarshal.AsSpan(entities))
        {
            SimpleGachaItem simple = new()
            {
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
                Id = item.Id,
            };

            models.Add(simple);
        }
    }

    private static void AppendModelsToEntities(List<SimpleGachaItem> models, List<EntityGachaItem> entites, int userId, string uid, bool isTrusted)
    {
        foreach (ref SimpleGachaItem item in CollectionsMarshal.AsSpan(models))
        {
            EntityGachaItem entity = new()
            {
                UserId = userId,
                Uid = uid,
                Id = item.Id,
                IsTrusted = isTrusted,
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
            };

            entites.Add(entity);
        }
    }

    private static T? FromCacheOrDb<T>(AppDbContext appDbContext, IMemoryCache memoryCache, string name)
        where T : class
    {
        if (memoryCache.TryGetValue(name, out object? data))
        {
            return (T)data!;
        }

        GachaStatistics? statistics = appDbContext.GachaStatistics
            .SingleOrDefault(s => s.Name == name);

        if (statistics != null)
        {
            T? tdata = JsonSerializer.Deserialize<T>(statistics.Data);
            memoryCache.Set(name, tdata);

            return tdata;
        }
        else
        {
            return null;
        }
    }

    private IActionResult GetStatistics<T>(string name)
        where T : class
    {
        T? data = FromCacheOrDb<T>(appDbContext, memoryCache, name);

        return Response<T>.Success("获取祈愿统计数据成功", data!);
    }

    private async Task<bool> CanUseGachaLogServiceAsync(int userId)
    {
        HutaoUser? user = await appDbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId)
            .ConfigureAwait(false);

        if (user != null)
        {
            return user.IsLicensedDeveloper || user.GachaLogExpireAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        else
        {
            return false;
        }
    }
}