using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Telegram.Bot.Types;

namespace CommunityBot.Services
{
    public class MediaGroupService : IMediaGroupService
    {
        private static readonly ConcurrentDictionary<string, (DateTime createDate, ConcurrentBag<IAlbumInputMedia> medias)> MediaGroups = new ConcurrentDictionary<string, (DateTime createDate, ConcurrentBag<IAlbumInputMedia> medias)>();

        public void AddMediaToGroup(string mediaGroupId, IAlbumInputMedia media)
        {
            MediaGroups.AddOrUpdate(mediaGroupId,
                id => (DateTime.Now, new ConcurrentBag<IAlbumInputMedia> {media}), 
                (id, medias) =>
                {
                    medias.medias.Add(media);
                    return medias;
                });
        }

        public void ClearOldMediaGroups()
        {
            var now = DateTime.Now;
            
            foreach (var (mediaGroupId, medias) in MediaGroups)
            {
                if (now - medias.createDate > TimeSpan.FromDays(1))
                {
                    MediaGroups.TryRemove(mediaGroupId, out _);
                }
            }
        }
        
        public async Task<IEnumerable<IAlbumInputMedia>?> GetMediaByGroupId(string mediaGroupId)
        {
            return MediaGroups.TryGetValue(mediaGroupId, out var medias) 
                ? medias.medias
                : null;
        }
    }
}