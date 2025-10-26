using Models;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBL
{
    public class ListenerDB : BaseDB<Listener>
    {
        protected override string GetTableName()
        {
            return "users";
        }

        protected override string GetPrimaryKeyName()
        {
            return "userid";
        }

        protected override async Task<Listener> CreateModelAsync(object[] row)
        {
         return new Listener(int.Parse(row[0].ToString()), row[1].ToString());
        }

        public async Task<List<Listener>> GetAllAsync()
        {
            return (List<Listener>)await SelectAllAsync();
        }

        public async Task<Listener> GetListenerByPkAsync(int ListenerID)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("ListenerID", ListenerID.ToString());
            List<Listener> list = (List<Listener>)await SelectAllAsync(p);
            if (list.Count == 1)
                return list[0];
            else
                return null;
        }

        public async Task<Listener> InsertGetObjAsync(Listener Listener)
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            DateTime d = DateTime.Now;
            string dts = $"{d.Year}-{d.Month}-{d.Day} {d.Hour}:{d.Minute}:{d.Second}";
            fillValues.Add("datejoined", dts);
            Listener returnListener = (Listener)await base.InsertGetObjAsync(fillValues);
            return returnListener;
        }

        public async Task<int> UpdateAsync(Listener Listener, string password)
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            Dictionary<string, object> filterValues = new Dictionary<string, object>();
            fillValues.Add("password", password);
            filterValues.Add("ListenerID", Listener.userID.ToString());
            return await base.UpdateAsync(fillValues, filterValues);
        }

        public async Task<int> UpdateProfilePictureAsync(int userId, byte[] imageBytes)
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            Dictionary<string, object> filterValues = new Dictionary<string, object>();

            fillValues.Add("profilepicture", imageBytes);
            filterValues.Add("userid", userId);

            return await base.UpdateAsync(fillValues, filterValues);
        }


        public async Task<int> DeleteAsync(Listener Listener)
        {
            Dictionary<string, object> filterValues = new Dictionary<string, object>();
            filterValues.Add("ListenerID", Listener.userID.ToString());
            return await base.DeleteAsync(filterValues);
        }

        public async Task<Listener?> GetListenerByLoginAsync(string username, string password)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("username", username);
            p.Add("password", password);

            List<Listener> list = (List<Listener>)await SelectAllAsync(p);
            if (list.Count == 1)
                return list[0];
            else
                return null;
        }

    }
}
