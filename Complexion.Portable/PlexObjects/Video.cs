using System;
using System.Collections.Generic;
using JimBobBennett.JimLib.Extensions;

namespace Complexion.Portable.PlexObjects
{
    public class Video : PlexObjectBase<Video>
    {
        public string title { get; set; }
        public string summary { get; set; }
        public string tagline { get; set; }
        public string guid { get; set; }
        public string art { get; set; }
        public string thumb { get; set; }
        public long viewOffset { get; set; }
        public long duration { get; set; }
        public int year { get; set; }

        public Player Player { get; set; }
        public List<Role> Roles { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Producer> Producers { get; set; }
        public List<Writer> Writers { get; set; }
        public List<Director> Directors { get; set; }

        public string UriSource
        {
            get
            {
                if (HasIMDBLink)
                    return "IMDB";

                if (HasTVDBLink)
                    return "TheTVDB";

                return null;
            }
        }

        public Uri Uri
        {
            get
            {
                if (HasIMDBLink)
                    return new Uri(guid.Replace("com.plexapp.agents.imdb://", "http://www.imdb.com/title/"));

                if (HasTVDBLink)
                {
                    var bit = guid.Replace("com.plexapp.agents.thetvdb://", "");
                    var bits = bit.Split('/');

                    var series = bits[0];
                    return new Uri("http://www.thetvdb.com/?tab=series&id=" + series);
                }

                return null;
            }
        }

        private bool HasTVDBLink
        {
            get { return guid.StartsWith("com.plexapp.agents.thetvdb://"); }
        }

        private bool HasIMDBLink
        {
            get { return guid.StartsWith("com.plexapp.agents.imdb://", StringComparison.OrdinalIgnoreCase); }
        }

        public Uri SchemeUri
        {
            get
            {
                if (!HasIMDBLink)
                    return null;

                var imdbAppLink = guid.Replace("com.plexapp.agents.imdb://", "imdb:///title/");
                var endLocation = imdbAppLink.IndexOf('?');
                if (endLocation > -1)
                    imdbAppLink = imdbAppLink.Substring(0, endLocation);

                imdbAppLink += "/";

                return new Uri(imdbAppLink);
            }
        }

        protected override bool OnUpdateFrom(Video newValue)
        {
            var isUpdated = UpdateValue(() => title, newValue);
            isUpdated = UpdateValue(() => summary, newValue) | isUpdated;
            isUpdated = UpdateValue(() => guid, newValue) | isUpdated;
            isUpdated = UpdateValue(() => art, newValue) | isUpdated;
            isUpdated = UpdateValue(() => thumb, newValue) | isUpdated;
            isUpdated = UpdateValue(() => viewOffset, newValue) | isUpdated;
            isUpdated = UpdateValue(() => duration, newValue) | isUpdated;

            isUpdated = Player.UpdateFrom(newValue.Player) | isUpdated;
            isUpdated = Roles.UpdateToMatch(newValue.Roles, r => r.Key, UpdateRole) | isUpdated;

            return isUpdated;
        }

        private static bool UpdateRole(Role oldRole, Role newRole)
        {
            return oldRole.UpdateFrom(newRole);
        }

        public override string Key
        {
            get { return guid; }
        }
    }
}
