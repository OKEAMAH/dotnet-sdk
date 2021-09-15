﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api.Objects;

namespace DiscordBotsList.Api.Adapter.Discord.Net
{
    internal class SubmissionAdapter : IAdapter
    {
        protected AuthDiscordBotListApi api;
        protected IDiscordClient client;

        protected DateTime lastTimeUpdated;
        protected TimeSpan updateTime;

        public SubmissionAdapter(AuthDiscordBotListApi api, IDiscordClient client, TimeSpan updateTime)
        {
            this.client = client;
            this.updateTime = updateTime;
        }

        public event Action<string> Log;

        public virtual async Task RunAsync()
        {
            if (DateTime.Now > lastTimeUpdated + updateTime)
            {
                await api.UpdateStats(
                    (await client.GetGuildsAsync()).Count
                );

                lastTimeUpdated = DateTime.Now;
                SendLog("Submitted stats to Top.gg!");
            }
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        protected void SendLog(string msg)
        {
            Log?.Invoke(msg);
        }
    }

    internal class ShardedSubmissionAdapter : SubmissionAdapter, IAdapter
    {
        public ShardedSubmissionAdapter(AuthDiscordBotListApi api, DiscordShardedClient client, TimeSpan updateTime)
            : base(api, client, updateTime)
        {
        }

        public override async Task RunAsync()
        {
            if (DateTime.Now > lastTimeUpdated + updateTime)
            {
                await api.UpdateStats(
                    0,
                    (client as DiscordShardedClient).Shards.Count,
                    (client as DiscordShardedClient).Shards.Select(x => x.Guilds.Count).ToArray()
                );

                lastTimeUpdated = DateTime.Now;
                SendLog("Sent stats to Top.gg!");
            }
        }
    }
}
