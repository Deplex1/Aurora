using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DBL;
using Models;

namespace UnitTesting
{
    public class ListenerDBTests
    {
        private readonly ListenerDB listenerDB = new ListenerDB();

        public async Task RunAllTests()
        {
            Console.WriteLine("Starting ListenerDB CRUD Tests...\n");

            // 1️ Insert
            var insertedListener = await TestInsert();

            // 2️ Select (Get by PK)
            var fetchedListener = await TestGetByPk(insertedListener.userID);

            // 3️ Update password
            await TestUpdatePassword(insertedListener);

            // 4️ Update profile picture
            await TestUpdateProfilePicture(insertedListener);

            // 5️ Get by login (check if updated password works)
            await TestGetByLogin(insertedListener, "newPass_");

            // 6️ Delete
            await TestDelete(insertedListener);

            Console.WriteLine("\n✅ All tests completed!");
        }

        private async Task<Listener> TestInsert()
        {
            Console.WriteLine("➡️ Testing INSERT...");
            Listener listener = new Listener("Test_User", "Test_Email");
            string password = "test_pass";
            var inserted = await listenerDB.InsertGetObjAsync(listener, password);
            Console.WriteLine($"Inserted Listener ID: {inserted.userID}, Username: {inserted.username}");
            return inserted;
        }

        private async Task<Listener> TestGetByPk(int id)
        {
            Console.WriteLine("\nTesting GET BY PK...");
            var listener = await listenerDB.GetListenerByPkAsync(id);

            if (listener != null)
                Console.WriteLine($"Found Listener: {listener.username} (ID {listener.userID})");
            else
                Console.WriteLine("Listener not found.");

            return listener!;
        }

        private async Task TestUpdatePassword(Listener listener)
        {
            Console.WriteLine("\nTesting UPDATE PASSWORD...");
            string newPassword = "newPass_";

            int rowsAffected = await listenerDB.UpdateAsync(listener, newPassword);
            if (rowsAffected > 0)
                Console.WriteLine($"Password updated successfully for {listener.username}");
            else
                Console.WriteLine("Password update failed.");
        }

        private async Task TestUpdateProfilePicture(Listener listener)
        {
            Console.WriteLine("\nTesting UPDATE PROFILE PICTURE...");

            // Load sample image from local file (replace with an existing path)
            string imagePath = "C:\\Temp\\test-image.jpg";
            byte[] imageBytes = Array.Empty<byte>();

            if (File.Exists(imagePath))
            {
                imageBytes = await File.ReadAllBytesAsync(imagePath);
                int rows = await listenerDB.UpdateProfilePictureAsync(listener.userID, imageBytes);

                if (rows > 0)
                    Console.WriteLine($"Profile picture updated for user {listener.username}");
                else
                    Console.WriteLine("Failed to update profile picture.");
            }
            else
            {
                Console.WriteLine("Skipping profile picture update (no local test image found).");
            }
        }

        private async Task TestGetByLogin(Listener listener, string password)
        {
            Console.WriteLine("\nTesting GET BY LOGIN...");
            var result = await listenerDB.GetListenerByLoginAsync(listener.username, password);

            if (result != null)
                Console.WriteLine($"Login successful for {result.username}");
            else
                Console.WriteLine("Login failed (possibly wrong password or deleted user).");
        }

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
