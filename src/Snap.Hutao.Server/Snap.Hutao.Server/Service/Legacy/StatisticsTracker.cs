﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Legacy.Primitive;

namespace Snap.Hutao.Server.Service.Legacy;

/// <summary>
/// 统计追踪器
/// </summary>
public class StatisticsTracker
{
    // 对应层满星 | 出场
    private readonly Map<AvatarId, int> level09PresentCounter = new();
    private readonly Map<AvatarId, int> level10PresentCounter = new();
    private readonly Map<AvatarId, int> level11PresentCounter = new();
    private readonly Map<AvatarId, int> level12PresentCounter = new();

    // 对应层满星 | 持有
    private readonly Map<AvatarId, int> level09RecordwithAvatarCounter = new();
    private readonly Map<AvatarId, int> level10RecordwithAvatarCounter = new();
    private readonly Map<AvatarId, int> level11RecordwithAvatarCounter = new();
    private readonly Map<AvatarId, int> level12RecordwithAvatarCounter = new();

    // 角色命座
    private readonly Map<AvatarId, Map<Constellation, int>> avatarConstellationCounter = new();

    // 角色持有
    private readonly Map<AvatarId, int> avatarHoldingCounter = new();

    // 角色 | 角色搭配
    private readonly Map<AvatarId, Map<AvatarId, int>> avatarAvatarBuildCounter = new();

    // 角色 | 武器搭配
    private readonly Map<AvatarId, Map<WeaponId, int>> avatarWeaponBuildCounter = new();

    // 角色 | 圣遗物搭配
    private readonly Map<AvatarId, Map<ReliquarySets, int>> avatarReliquaryBuildCounter = new();

    // 对应层满星 | 队伍出场
    private readonly Map<Team, int> level09Battle1TeamCounter = new();
    private readonly Map<Team, int> level09Battle2TeamCounter = new();
    private readonly Map<Team, int> level10Battle1TeamCounter = new();
    private readonly Map<Team, int> level10Battle2TeamCounter = new();
    private readonly Map<Team, int> level11Battle1TeamCounter = new();
    private readonly Map<Team, int> level11Battle2TeamCounter = new();
    private readonly Map<Team, int> level12Battle1TeamCounter = new();
    private readonly Map<Team, int> level12Battle2TeamCounter = new();

    // 仅用于角色数量统计
    private int totalRecordCounter;
    private int totalSpiralAbyssCounter;

    // 对应层总记录数
    private int level09TotalRecordCounter;
    private int level10TotalRecordCounter;
    private int level11TotalRecordCounter;
    private int level12TotalRecordCounter;

    /// <summary>
    /// 最后处理的Id
    /// </summary>
    public long LastId { get; private set; } = -1;

    /// <summary>
    /// 追踪记录
    /// </summary>
    /// <param name="record">记录</param>
    public void Track(EntityRecord record)
    {
        LastId = record.PrimaryId;

        ++totalRecordCounter;
        Map<AvatarId, EntityAvatar> holdingAvatars = new();

        // 遍历角色
        foreach (EntityAvatar avatar in record.Avatars)
        {
            holdingAvatars.Add(avatar.AvatarId, avatar);
            avatarHoldingCounter.Increase(avatar.AvatarId);
            avatarConstellationCounter.GetOrAdd(avatar.AvatarId, Maps.ForConstellation).Increase(avatar.ActivedConstellationNumber);
        }

        // 仅当深渊记录存在时，统计才有意义
        if (record.SpiralAbyss == null)
        {
            return;
        }

        ++totalSpiralAbyssCounter;

        // 深渊上场过的角色
        HashSet<AvatarId> recordPresentAvatars = new();

        // 遍历楼层
        foreach (EntityFloor floor in record.SpiralAbyss.Floors)
        {
            // 跳过非满星的数据
            if (floor.Star < 9)
            {
                continue;
            }

            HashSet<AvatarId> floorPresentAvatars = new();

            IncreaseCurrentFloorRecordCount(floor.Index);

            foreach (AvatarId holdAvatarId in holdingAvatars.Keys)
            {
                IncreaseCurrentFloorHoldingAvatarRecordCount(floor.Index, holdAvatarId);
            }

            foreach (SimpleBattle battle in floor.Levels.SelectMany(l => l.Battles))
            {
                // 递增队伍出现次数
                Team team = StatisticsHelper.AsTeam(battle.Avatars);
                IncreaseCurrentFloorUpDownTeamCount(floor.Index, battle.Index, team);
                IncreaseAvatarAvatarBuild(battle.Avatars);

                foreach (AvatarId avatarId in battle.Avatars)
                {
                    recordPresentAvatars.Add(avatarId);
                    floorPresentAvatars.Add(avatarId);
                }
            }

            foreach (AvatarId avatarId in floorPresentAvatars)
            {
                IncreaseCurrentFloorPresentAvatarCount(floor.Index, avatarId);
            }
        }

        foreach (AvatarId recordAvatarId in recordPresentAvatars)
        {
            EntityAvatar avatar = holdingAvatars[recordAvatarId];

            avatarWeaponBuildCounter.GetOrAdd(recordAvatarId, key => new()).Increase(avatar.WeaponId);
            avatarReliquaryBuildCounter.GetOrAdd(recordAvatarId, key => new()).Increase(avatar.ReliquarySet);
        }
    }

    /// <summary>
    /// 完成追踪并保存结果
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="stopwatch">停表</param>
    public void CompleteTracking(AppDbContext appDbContext, IMemoryCache memoryCache, Core.ValueStopwatch stopwatch)
    {
        int scheduleId = StatisticsHelper.GetScheduleId();

        // Overview
        {
            double totalTime = stopwatch.GetElapsedTime().TotalMilliseconds;

            Overview overview = new()
            {
                ScheduleId = scheduleId,
                RecordTotal = totalRecordCounter,
                SpiralAbyssTotal = totalSpiralAbyssCounter,
                SpiralAbyssFullStar = level12TotalRecordCounter,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                TimeTotal = totalTime,
                TimeAverage = totalRecordCounter / totalTime,
            };

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.Overview, overview);
        }

        // AvatarAppearanceRank
        {
            List<AvatarAppearanceRank> avatarAppearanceRanks = new()
            {
                StatisticsHelper.AvatarAppearanceRank(09, level09PresentCounter, level09TotalRecordCounter),
                StatisticsHelper.AvatarAppearanceRank(10, level10PresentCounter, level10TotalRecordCounter),
                StatisticsHelper.AvatarAppearanceRank(11, level11PresentCounter, level11TotalRecordCounter),
                StatisticsHelper.AvatarAppearanceRank(12, level12PresentCounter, level12TotalRecordCounter),
            };

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.AvatarAppearanceRank, avatarAppearanceRanks);
        }

        // AvatarUsageRank
        {
            List<AvatarUsageRank> avatarUsageRanks = new()
            {
                StatisticsHelper.AvatarUsageRank(09, level09PresentCounter, level09RecordwithAvatarCounter),
                StatisticsHelper.AvatarUsageRank(10, level10PresentCounter, level10RecordwithAvatarCounter),
                StatisticsHelper.AvatarUsageRank(11, level11PresentCounter, level11RecordwithAvatarCounter),
                StatisticsHelper.AvatarUsageRank(12, level12PresentCounter, level12RecordwithAvatarCounter),
            };

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.AvatarUsageRank, avatarUsageRanks);
        }

        // AvatarConstellationInfo
        {
            List<AvatarConstellationInfo> avatarConstellationInfos = avatarHoldingCounter.Select(kvp =>
            {
                AvatarId avatar = kvp.Key;
                Map<Constellation, int> constellationCounter = avatarConstellationCounter[avatar];
                return StatisticsHelper.AvatarConstellationInfo(avatar, totalRecordCounter, kvp.Value, constellationCounter);
            }).ToList();

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.AvatarConstellationInfo, avatarConstellationInfos);
        }

        // AvatarCollocation
        {
            List<AvatarCollocation> avatarCollocations = avatarAvatarBuildCounter.Select(kvp =>
            {
                AvatarId avatar = kvp.Key;

                Map<AvatarId, int> avatarBuildCounter = avatarAvatarBuildCounter[avatar];
                Map<WeaponId, int> weaponBuildCounter = avatarWeaponBuildCounter[avatar];
                Map<ReliquarySets, int> reliquaryBuildCounter = avatarReliquaryBuildCounter[avatar];

                return StatisticsHelper.AvatarCollocation(avatar, avatarBuildCounter, weaponBuildCounter, reliquaryBuildCounter);
            }).ToList();

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.AvatarCollocation, avatarCollocations);
        }

        // TeamAppearance
        {
            List<TeamAppearance> teamAppearances = new()
            {
                StatisticsHelper.TeamAppearance(09, level09Battle1TeamCounter, level09Battle2TeamCounter),
                StatisticsHelper.TeamAppearance(10, level10Battle1TeamCounter, level10Battle2TeamCounter),
                StatisticsHelper.TeamAppearance(11, level11Battle1TeamCounter, level11Battle2TeamCounter),
                StatisticsHelper.TeamAppearance(12, level12Battle1TeamCounter, level12Battle2TeamCounter),
            };

            StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.TeamAppearance, teamAppearances);
        }
    }

    /// <summary>
    /// 递增当前层记录数
    /// </summary>
    /// <param name="index">层</param>
    private void IncreaseCurrentFloorRecordCount(int index)
    {
        switch (index)
        {
            case 09: ++level09TotalRecordCounter; break;
            case 10: ++level10TotalRecordCounter; break;
            case 11: ++level11TotalRecordCounter; break;
            case 12: ++level12TotalRecordCounter; break;
        }
    }

    /// <summary>
    /// 递增当前层持有角色的记录数
    /// </summary>
    /// <param name="index">层</param>
    /// <param name="avatarId">角色</param>
    private void IncreaseCurrentFloorHoldingAvatarRecordCount(int index, AvatarId avatarId)
    {
        // 递增当前层且持有对应角色的记录数
        switch (index)
        {
            case 09: level09RecordwithAvatarCounter.Increase(avatarId); break;
            case 10: level10RecordwithAvatarCounter.Increase(avatarId); break;
            case 11: level11RecordwithAvatarCounter.Increase(avatarId); break;
            case 12: level12RecordwithAvatarCounter.Increase(avatarId); break;
        }
    }

    /// <summary>
    /// 递增当前层上下半队伍数
    /// </summary>
    /// <param name="floor">层</param>
    /// <param name="battle">上下半</param>
    /// <param name="team">队伍</param>
    private void IncreaseCurrentFloorUpDownTeamCount(int floor, int battle, Team team)
    {
        switch ((floor, battle))
        {
            case (09, 1): level09Battle1TeamCounter.Increase(team); break;
            case (09, 2): level09Battle2TeamCounter.Increase(team); break;
            case (10, 1): level10Battle1TeamCounter.Increase(team); break;
            case (10, 2): level10Battle2TeamCounter.Increase(team); break;
            case (11, 1): level11Battle1TeamCounter.Increase(team); break;
            case (11, 2): level11Battle2TeamCounter.Increase(team); break;
            case (12, 1): level12Battle1TeamCounter.Increase(team); break;
            case (12, 2): level12Battle2TeamCounter.Increase(team); break;
        }
    }

    /// <summary>
    /// 递增当前层
    /// </summary>
    /// <param name="index">层</param>
    /// <param name="avatarId">角色</param>
    private void IncreaseCurrentFloorPresentAvatarCount(int index, AvatarId avatarId)
    {
        switch (index)
        {
            case 09: level09PresentCounter.Increase(avatarId); break;
            case 10: level10PresentCounter.Increase(avatarId); break;
            case 11: level11PresentCounter.Increase(avatarId); break;
            case 12: level12PresentCounter.Increase(avatarId); break;
        }
    }

    /// <summary>
    /// 递增角色搭配
    /// </summary>
    /// <param name="team">队伍</param>
    private void IncreaseAvatarAvatarBuild(List<int> team)
    {
        foreach (int currentAvatar in team)
        {
            foreach (int otherAvatar in team.Where(avatar => avatar != currentAvatar))
            {
                avatarAvatarBuildCounter.GetOrAdd(currentAvatar, key => new()).Increase(otherAvatar);
            }
        }
    }
}