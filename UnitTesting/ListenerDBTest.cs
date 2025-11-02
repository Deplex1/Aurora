using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DBL;
using Models;



namespace UnitTesting
{
    internal class ListenerDBTests
    {
        private readonly ListenerDB listenerDB = new ListenerDB();

        public async Task RunAllTests()
        {
            Console.WriteLine("Starting ListenerDB Full Tests...\n");

            // 1. Insert new listener
            var insertedListener = await TestInsert();

            // 2. Save reset token
            string resetToken = Guid.NewGuid().ToString();
            await TestSaveResetToken(insertedListener.email, resetToken);

            // 3. Get listener by token
            await TestGetByResetToken(resetToken);

            // 4. Send reset email
            await TestSendResetEmail(insertedListener.email, resetToken);

            // 5. Clear reset token
            await TestClearResetToken(insertedListener.userID);

            Console.WriteLine("\nAll password-reset tests completed.\n");

            // 6. Delete listener from database
            await TestDelete(insertedListener);

            Console.WriteLine("\nAll tests finished successfully!");
            Console.ReadKey();
        }

        // Insert new test listener into the database
        private async Task<Listener> TestInsert()
        {
            Console.WriteLine("Testing INSERT...");
            Listener listener = new Listener("Reset_TestUser", "reset_test@example.com");
            string password = "reset_pass";

            var inserted = await listenerDB.InsertGetObjAsync(listener, password);
            if (inserted != null)
                Console.WriteLine($"Inserted Listener ID: {inserted.userID}, Username: {inserted.username}");
            else
                Console.WriteLine("Insert failed.");

            return inserted!;
        }

        // Save a reset token for a given email
        private async Task TestSaveResetToken(string email, string token)
        {
            Console.WriteLine("\nTesting SAVE RESET TOKEN...");
            int rows = await listenerDB.SaveResetTokenAsync(email, token, DateTime.Now.AddHours(1));

            if (rows > 0)
                Console.WriteLine($"Token saved successfully for {email}");
            else
                Console.WriteLine("Failed to save token.");
        }

        // Retrieve listener by their reset token
        private async Task TestGetByResetToken(string token)
        {
            Console.WriteLine("\nTesting GET BY RESET TOKEN...");
            var listener = await listenerDB.GetListenerByResetTokenAsync(token);

            if (listener != null)
                Console.WriteLine($"Found user with token: {listener.username} (ID {listener.userID})");
            else
                Console.WriteLine("No listener found for token.");
        }

        // Send a reset email using Resend API
        private async Task TestSendResetEmail(string email, string token)
        {
            Console.WriteLine("\nTesting SEND RESET EMAIL...");
            string resetLink = $"https://aurora.com/resetpassword?token={token}";

            try
            {
                await listenerDB.SendResetEmail(email, resetLink);
                Console.WriteLine($"Reset email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        // Clear the reset token and expiration fields
        private async Task TestClearResetToken(int listenerId)
        {
            Console.WriteLine("\nTesting CLEAR RESET TOKEN...");
            int rows = await listenerDB.ClearResetTokenAsync(listenerId);

            if (rows > 0)
                Console.WriteLine($"Cleared reset token for user ID {listenerId}");
            else
                Console.WriteLine("Failed to clear token.");
        }

        // Delete listener from the database
        private async Task TestDelete(Listener listener)
        {
            Console.WriteLine("\nTesting DELETE...");
            int rowsDeleted = await listenerDB.DeleteAsync(listener);

            if (rowsDeleted > 0)
                Console.WriteLine($"Deleted user {listener.username}");
            else
                Console.WriteLine("Delete failed.");
        }
    }
}
