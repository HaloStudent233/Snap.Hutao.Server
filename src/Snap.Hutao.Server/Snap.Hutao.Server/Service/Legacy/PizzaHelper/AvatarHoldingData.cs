﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Service.Legacy.PizzaHelper;

internal sealed class AvatarHoldingData
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = default!;

    [JsonPropertyName("updateDate")]
    public string UpdateDate { get; set; } = default!;

    [JsonPropertyName("owningChars")]
    public List<int> OwningChars { get; set; } = default!;

    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = default!;
}
