using System;
using System.Threading.Tasks;
using DBL;
using Models;

namespace TestingApp
{
    internal class UserTest
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== USER TESTING START ===");
            Console.ResetColor();

            ListenerDB db = new ListenerDB();

            string email = "yarinkrupop@gmail.com";
            string username = "testuser";
            string password = "123456";
            string newPassword = "654321";

            // 1. Insert user
            Console.WriteLine("\n[1] Inserting new user...");
            Listener listener = new Listener
            {
                username = username,
                email = email
            };

            try
            {
                Listener inserted = await db.InsertGetObjAsync(listener, password);
                Console.WriteLine($"Inserted user ID: {inserted.userID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert failed (user may already exist): " + ex.Message);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 2. Login test
            Console.WriteLine("\n[2] Testing login...");
            Listener? loggedIn = await db.GetListenerByLoginAsync(username, password);

            if (loggedIn != null)
                Console.WriteLine($"Login successful for {loggedIn.username}");
            else
                Console.WriteLine("Login failed.");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 3. Send reset code
            Console.WriteLine("\n[3] Sending reset code...");
            Random random = new Random();
            string code = random.Next(100000, 999999).ToString();

            int saveResult = await db.SaveResetCodeAsync(email, code);
            Console.WriteLine(saveResult > 0 ? "Reset code saved in database." : "Failed to save reset code.");

            await db.SendResetCodeEmail(email, code);
            Console.WriteLine("Reset email sent.");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 4. Verify reset code manually
            Console.WriteLine("\n[4] Manually verifying reset code...");
            Listener? byEmail = await db.GetListenerByEmailAsync(email);

            if (byEmail != null)
            {
                Console.WriteLine($"Fetched user {byEmail.username}, checking reset code...");
                if (byEmail.reset_code == code)
                    Console.WriteLine("Reset code matches.");
                else
                    Console.WriteLine("Reset code does not match.");
            }
            else
            {
                Console.WriteLine("User not found.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 5. Change password
            Console.WriteLine("\n[5] Changing password...");
            int updateResult = await db.UpdatePasswordByEmailAsync(email, newPassword);
            Console.WriteLine(updateResult > 0 ? "Password updated successfully." : "Password update failed.");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 6. Login with new password
            Console.WriteLine("\n[6] Testing login with new password...");
            Listener? relogin = await db.GetListenerByLoginAsync(username, newPassword);

            if (relogin != null)
                Console.WriteLine($"Re-login successful for {relogin.username}");
            else
                Console.WriteLine("Re-login failed.");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // 7. Clear reset code
            Console.WriteLine("\n[7] Clearing reset code...");
            int clearResult = await db.ClearResetCodeAsync(email);
            Console.WriteLine(clearResult > 0 ? "Reset code cleared." : "Failed to clear reset code.");

            Console.WriteLine("\n=== USER TESTING COMPLETE ===");
        }
    }
}
