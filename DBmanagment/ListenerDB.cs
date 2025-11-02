using Resend;
using Models;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace DBL
{
    
    public class ListenerDB : BaseDB<Listener>
    {
        private readonly string ApiKey = "re_7KMB4Fs1_LmXt9d3vG88d1hbayTMJpgFA";
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
         return new Listener(int.Parse(row[0].ToString()), row[1].ToString(), row[3].ToString(), row[4].ToString() , int.Parse(row[5].ToString()));
        }

        public async Task<List<Listener>> GetAllAsync()
        {
            return (List<Listener>)await SelectAllAsync();
        }

        public async Task<Listener> GetListenerByPkAsync(int userid)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("userid", userid.ToString());
            List<Listener> list = (List<Listener>)await SelectAllAsync(p);
            if (list.Count == 1)
                return list[0];
            else
                return null;
        }

        public async Task<Listener> InsertGetObjAsync(Listener listener, string password)
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            fillValues.Add("username", listener.username);
            fillValues.Add("email", listener.email);
            fillValues.Add("password", password);
            Listener returnListener = (Listener)await base.InsertGetObjAsync(fillValues);
            return returnListener;
        }

        public async Task<int> UpdateAsync(Listener Listener, string password)
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            Dictionary<string, object> filterValues = new Dictionary<string, object>();
            fillValues.Add("password", password);
            filterValues.Add("userid", Listener.userID.ToString());
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
            filterValues.Add("userid", Listener.userID.ToString());
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

        public async Task<Listener?> GetListenerByResetTokenAsync(string token) 
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("reset_token", token);
            List<Listener> list = (List<Listener>)await SelectAllAsync(p);
            if (list.Count == 1)
                return list[0];
            else
                return null;
        }
        public async Task<int> SaveResetTokenAsync(string email, string token, DateTime expiration) 
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            Dictionary<string, object> filterValues = new Dictionary<string, object>();

            fillValues.Add("reset_token", token);
            fillValues.Add("reset_expiration", expiration);
            filterValues.Add("email", email);

            return await base.UpdateAsync(fillValues, filterValues);
        }

        public async Task<int> ClearResetTokenAsync(int listenerId) 
        {
            Dictionary<string, object> fillValues = new Dictionary<string, object>();
            Dictionary<string, object> filterValues = new Dictionary<string, object>();
            fillValues.Add("reset_token", null);
            fillValues.Add("reset_expiration", null);
            filterValues.Add("userid", listenerId); 
            return await base.UpdateAsync(fillValues, filterValues);
        }



        public async Task SendResetEmail(string toEmail, string resetLink) 
        {
            IResend resend = ResendClient.Create(ApiKey);

            var resp = await resend.EmailSendAsync(new EmailMessage()
            {
                From = "onboarding@resend.dev",
                To = toEmail,
                Subject = "Reset Password request",
                HtmlBody = $@"
<html>
  <body style='
      font-family:Segoe UI, Roboto, sans-serif;
      background: radial-gradient(circle at 20% 20%, #0b1224, #070913 80%);
      color: #fff;
      margin: 0;
      padding: 40px;
      text-align: center;'>
    
    <div style='
        background: rgba(255,255,255,0.05);
        backdrop-filter: blur(15px);
        border-radius: 20px;
        padding: 40px;
        display: inline-block;
        box-shadow: 0 0 25px rgba(0,255,255,0.2);
        max-width: 500px;'>
      
      <h1 style='
          color: #00eaff;
          font-size: 28px;
          margin-bottom: 10px;'>
        Password Reset Request
      </h1>

      <p style='
          font-size: 16px;
          color: #cfd8dc;
          margin-bottom: 30px;'>
        Hey there 👋<br/>
        We received a request to reset your Aurora account password.
        Click the button below to choose a new one.
      </p>

      <a href='{resetLink}' style='
          background: linear-gradient(90deg, #00bfff, #00ffcc);
          color: #0a0a0a;
          text-decoration: none;
          padding: 14px 28px;
          border-radius: 12px;
          font-weight: 600;
          font-size: 16px;
          transition: 0.3s ease;
          display: inline-block;'>
        Reset My Password
      </a>

      <p style='
          font-size: 13px;
          color: #9ea7ad;
          margin-top: 30px;'>
        If you didn’t request a password reset, you can safely ignore this email.
      </p>

      <hr style='
          border: none;
          border-top: 1px solid rgba(255,255,255,0.1);
          margin: 25px 0;'/>

      <p style='font-size: 12px; color: #7a8594;'>
        © 2025 Aurora • All rights reserved
      </p>
    </div>
  </body>
</html>",
            });
        }
        private static async Task<string> ByteArrayToImageURL(byte[] imageBytes)
        {
            string base64Image = Convert.ToBase64String(imageBytes);
            return $"data:image/jpeg;base64,{base64Image}";
        }
    }
}
