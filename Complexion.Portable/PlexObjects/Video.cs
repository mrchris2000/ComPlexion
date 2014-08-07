using System.Collections.Generic;

namespace Complexion.Portable.PlexObjects
{
    public class Video : PlexObjectBase<Video>
    {
        public string title { get; set; }
        public string summary { get; set; }
        public string guid { get; set; }
        public string art { get; set; }
        public string thumb { get; set; }
        public long viewOffset { get; set; }
        public long duration { get; set; }

        public Player Player { get; set; }
        public List<Role> Roles { get; set; }

        public string ImdbLink
        {
            get { return guid.Replace("com.plexapp.agents.imdb://", "http://www.imdb.com/title/"); }
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
