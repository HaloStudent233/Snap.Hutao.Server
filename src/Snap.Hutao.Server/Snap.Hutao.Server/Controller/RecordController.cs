﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Controller.Helper;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Legacy;
using System.Collections.Concurrent;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 记录控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(GroupName = "SpiralAbyss")]
public class RecordController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, UploadToken> UidUploading = new();
    private readonly AppDbContext appDbContext;
    private readonly RankService rankService;
    private readonly ExpireService expireService;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的记录控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="rankService">排行服务</param>
    /// <param name="expireService">续期服务</param>
    /// <param name="memoryCache">内存缓存</param>
    public RecordController(AppDbContext appDbContext, RankService rankService, ExpireService expireService, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.rankService = rankService;
        this.expireService = expireService;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 上传记录
    /// </summary>
    /// <param name="record">记录</param>
    /// <param name="returningRank">返回排行结果</param>
    /// <returns>上传结果</returns>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] SimpleRecord record, [FromQuery] bool returningRank = false)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        string recordUid = record.Uid;
        if (appDbContext.BannedList.Any(banned => banned.Uid == recordUid))
        {
            return Model.Response.Response.Fail(ReturnCode.BannedUid, "Uid 已被数据库封禁");
        }

        if (!record.Validate())
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "无效的提交数据");
        }

        if (UidUploading.TryGetValue(record.Uid, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理");
        }

        if (!UidUploading.TryAdd(record.Uid, new()))
        {
            return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常");
        }

        RecordUploadResult result = await RecordHelper.SaveRecordAsync(appDbContext, rankService, expireService, record).ConfigureAwait(false);

        if (!UidUploading.TryRemove(record.Uid, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常");
        }

        if (returningRank)
        {
            Rank rank = await rankService.RetriveRankAsync(record.Uid).ConfigureAwait(false);
            return Response<Rank>.Success("获取排行数据成功", rank);
        }
        else
        {
            return result switch
            {
                RecordUploadResult.GachaLogExtented => Model.Response.Response.Success("数据提交成功，获赠 5 天祈愿记录上传服务时长"),
                RecordUploadResult.NotSnapHutao => Model.Response.Response.Success("数据提交成功，但不是由胡桃客户端发起，无法获赠祈愿记录上传服务时长"),
                RecordUploadResult.NotFirstAttempt => Model.Response.Response.Success("数据提交成功，但不是本期首次提交，无法获赠祈愿记录上传服务时长"),
                RecordUploadResult.NoUserNamePresented => Model.Response.Response.Success("数据提交成功，但未绑定胡桃账号，无法获赠祈愿记录上传服务时长"),
                _ => Model.Response.Response.Success("数据提交成功"),
            };
        }
    }

    /// <summary>
    /// 检查uid对应的记录是否存在
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>是否存在记录</returns>
    [HttpGet("Check")]
    public async Task<IActionResult> CheckAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid");
        }

        EntityRecord? record = appDbContext.Records.SingleOrDefault(r => r.Uid == uid);

        if (record != null)
        {
            long recordId = record.PrimaryId;
            EntitySpiralAbyss? spiralAbyss = await appDbContext.SpiralAbysses.SingleOrDefaultAsync(r => r.RecordId == recordId).ConfigureAwait(false);

            if (spiralAbyss != null)
            {
                return Response<bool>.Success("查询成功", true);
            }
        }

        return Response<bool>.Success("查询成功", false);
    }

    /// <summary>
    /// 获取排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    [HttpGet("Rank")]
    public async Task<IActionResult> RankAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid");
        }

        Rank rank = await rankService.RetriveRankAsync(uid).ConfigureAwait(false);
        return Response<Rank>.Success("获取排行数据成功", rank);
    }

    /// <summary>
    /// 并行上传占位
    /// </summary>
    private struct UploadToken
    {
    }
}