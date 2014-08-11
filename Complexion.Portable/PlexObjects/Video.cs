using System;
using System.Collections.Generic;
using JimBobBennett.JimLib;
using JimBobBennett.JimLib.Collections;

namespace Complexion.Portable.PlexObjects
{
    public class Video : PlexObjectBase<Video>
    {
        [NotifyPropertyChangeDependency("Key")]
        [NotifyPropertyChangeDependency("HasIMDBLink")]
        [NotifyPropertyChangeDependency("HasTVDBLink")]
        [NotifyPropertyChangeDependency("UriSource")]
        [NotifyPropertyChangeDependency("Uri")]
        [NotifyPropertyChangeDependency("SchemeUri")]
        public string Guid { get; set; }

        public string Title { get; set; }
        public string Summary { get; set; }
        public string Tagline { get; set; }
        public string Art { get; set; }
        public string Thumb { get; set; }
        public long ViewOffset { get; set; }
        public long Duration { get; set; }
        public int Year { get; set; }

        public Player Player { get; set; }

        public ObservableCollectionEx<Role> Roles { get; set; }
        public ObservableCollectionEx<Genre> Genres { get; set; }
        public ObservableCollectionEx<Producer> Producers { get; set; }
        public ObservableCollectionEx<Writer> Writers { get; set; }
        public ObservableCollectionEx<Director> Directors { get; set; }

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
                    return new Uri(Guid.Replace("com.plexapp.agents.imdb://", "http://www.imdb.com/title/"));

                if (HasTVDBLink)
                {
                    var bit = Guid.Replace("com.plexapp.agents.thetvdb://", "");
                    var bits = bit.Split('/');

                    var series = bits[0];
                    return new Uri("http://www.thetvdb.com/?tab=series&id=" + series);
                }

                return null;
            }
        }

        private bool HasTVDBLink
        {
            get { return Guid.StartsWith("com.plexapp.agents.thetvdb://"); }
        }

        private bool HasIMDBLink
        {
            get { return Guid.StartsWith("com.plexapp.agents.imdb://", StringComparison.OrdinalIgnoreCase); }
        }

        public Uri SchemeUri
        {
            get
            {
                if (!HasIMDBLink)
                    return null;

                var imdbAppLink = Guid.Replace("com.plexapp.agents.imdb://", "imdb:///title/");
                var endLocation = imdbAppLink.IndexOf('?');
                if (endLocation > -1)
                    imdbAppLink = imdbAppLink.Substring(0, endLocation);

                imdbAppLink += "/";

                return new Uri(imdbAppLink);
            }
        }

        protected override bool OnUpdateFrom(Video newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => Title, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => Summary, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Guid, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Art, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Thumb, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => ViewOffset, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Duration, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = Player.UpdateFrom(newValue.Player) | isUpdated;
            isUpdated = Roles.UpdateToMatch(newValue.Roles, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return Guid; }
        }
    }
}
