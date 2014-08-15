using System;
using System.Collections.Generic;
using System.ComponentModel;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Mvvm;

namespace Complexion.Portable.PlexObjects
{
    public class Video : PlexObjectBase<Video>
    {
        private Player _player;

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

        [NotifyPropertyChangeDependency("Progress")]
        public double ViewOffset { get; set; }

        [NotifyPropertyChangeDependency("Progress")]
        public double Duration { get; set; }

        public double Progress
        {
            get { return Duration <= 0 ? 0 : ViewOffset/Duration; }
        }

        public string PlayerName { get { return Player.Title; } }
        
        public int Year { get; set; }
        public PlayerState State { get { return Player.State; } }

        public Player Player
        {
            get { return _player; }
            set
            {
                if (Equals(Player, value)) return;

                if (_player != null)
                    _player.PropertyChanged -= PlayerOnPropertyChanged;

                _player = value;

                if (_player != null)
                    _player.PropertyChanged += PlayerOnPropertyChanged;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this.ExtractPropertyInfo(() => _player.State).Name)
                RaisePropertyChanged(() => State);

            if (e.PropertyName == this.ExtractPropertyInfo(() => _player.Title).Name)
                RaisePropertyChanged(() => PlayerName);
        }

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
