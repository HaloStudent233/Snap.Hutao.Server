﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.Legacy;

/// <summary>
/// 统计服务
/// </summary>
public class StatisticsService
{
    /// <summary>
    /// 统计服务正在工作
    /// </summary>
    public const string Working = "StatisticsService.Working";
    private const int Partion = 256;

    // Compile queries that used multiple times to increase performance
    private static readonly Func<AppDbContext, long, IEnumerable<EntityRecord>> PartionQuery = EF.CompileQuery((AppDbContext context, long lastId) =>
        context.Records.AsNoTracking().OrderBy(r => r.PrimaryId).Where(r => r.PrimaryId > lastId).Take(Partion));

    private static readonly Func<AppDbContext, long, IEnumerable<EntityAvatar>> AvatarQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.Avatars.AsNoTracking().Where(a => a.RecordId == recordId));

    private static readonly Func<AppDbContext, long, EntitySpiralAbyss?> SpiralAbyssQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.SpiralAbysses.AsNoTracking().SingleOrDefault(s => s.RecordId == recordId));

    private static readonly Func<AppDbContext, long, IEnumerable<EntityFloor>> FloorQuery = EF.CompileQuery((AppDbContext context, long spiralAbyssId) =>
        context.SpiralAbyssFloors.AsNoTracking().Where(f => f.SpiralAbyssId == spiralAbyssId));

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<StatisticsService> logger;

    /// <summary>
    /// 构造一个新的统计服务
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="logger">日志器</param>
    public StatisticsService(AppDbContext appDbContext, IMemoryCache memoryCache, ILogger<StatisticsService> logger)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    /// <summary>
    /// 异步计算结果
    /// </summary>
    /// <returns>任务</returns>
    public async Task RunAsync()
    {
        StatisticsTracker tracker = new();

        using (memoryCache.Flag(Working))
        {
            using (await appDbContext.OperationLock.EnterAsync().ConfigureAwait(false))
            {
                ValueStopwatch stopwatch = ValueStopwatch.StartNew();
                await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
                tracker.CompleteTracking(appDbContext, memoryCache, stopwatch);
            }
        }
    }

    private void RunCore(StatisticsTracker tracker)
    {
        while (true)
        {
            List<EntityRecord> partialRecords = PartionQuery(appDbContext, tracker.LastId).ToList();

            Span<EntityRecord> partialRecordSpan = CollectionsMarshal.AsSpan(partialRecords);
            ref EntityRecord recordAtZero = ref MemoryMarshal.GetReference(partialRecordSpan);

            for (int i = 0; i < partialRecordSpan.Length; i++)
            {
                ref EntityRecord record = ref Unsafe.Add(ref recordAtZero, i);

                record.Avatars = AvatarQuery(appDbContext, record.PrimaryId).ToList();
                record.SpiralAbyss = SpiralAbyssQuery(appDbContext, record.PrimaryId);

                if (record.SpiralAbyss != null)
                {
                    record.SpiralAbyss.Floors = FloorQuery(appDbContext, record.SpiralAbyss.PrimaryId).ToList();
                }

                tracker.Track(record);
            }

            if (partialRecordSpan.Length < Partion)
            {
                break;
            }
        }
    }
}