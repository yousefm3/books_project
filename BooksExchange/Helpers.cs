﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using BooksExchange.Models;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.EnterpriseServices.Internal;
using System.Diagnostics;
using System.Web.Services.Description;

namespace BooksExchange
{
    public static class Helpers
    {
        
        static public async Task<string> GetCode(int length = 6)
        {
            string valid = "1234567890";
            string s = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        s += character;
                    }
                }
            }
            using (book_exchangeEntities db = new book_exchangeEntities())
            {
                VerifyCode UserCode = await db.VerifyCodes.FirstOrDefaultAsync(o => o.code == s);
                if (UserCode != null)
                    return await GetRandomString(64);
                else
                    return s;
            }
        }

        static public async Task<string> GetRandomString(int length = 64)
        {
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string s = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        s += character;
                    }
                }
            }
            using (book_exchangeEntities db = new book_exchangeEntities())
            {
                User UserToken = await db.Users.FirstOrDefaultAsync(o => o.token == s);
                if (UserToken != null)
                    return await GetRandomString(64);
                else
                    return s;
            }
        }
        public static string GetMD5Hash(string value)
        {
            try
            {
                MD5 md5Hasher = MD5.Create();
                byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString());
                }
                return sBuilder.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        static public bool NullOrEmpty(string[] Data)
        {
            try
            {
                //loop through the given array to see if it has value or not
                foreach (var item in Data)
                {
                    if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                    {
                        //if it doesn't have a value
                        return true;
                    }
                }
                //if they has a value
                return false;
            }
            catch (Exception)
            {

                return true;
            }
        }
        static public async Task<bool> EmailExist(string email)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    User use = await db.Users.FirstOrDefaultAsync(o => o.email == email);
                    if (use == null)
                        return false;
                    else return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<bool> MobileExist(string mobile)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    User use = await db.Users.FirstOrDefaultAsync(o => o.mobile == mobile);
                    if (use == null)
                        return false;
                    else return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<string> GetUserTokenByID(int id)
        {
            using (book_exchangeEntities db = new book_exchangeEntities())
            {
                User u = await db.Users.FindAsync(id);
                if (u == null)
                    return "0";
                else
                    return u.token;
            }
        }
        static public async Task<int> GetUserIDByToken(string token)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    User u = await db.Users.FirstOrDefaultAsync(o => o.token == token);
                    if (u == null)
                        return 0;
                    else
                        return u.id;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<int> CheckCity(string name)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    City city = await db.Cities.FirstOrDefaultAsync(o => o.name == name);
                    if (city != null)
                        return city.id;
                    else
                    {
                        City newCity = new City()
                        {
                            name = name,
                            created_at = DateTime.Now
                        };
                        db.Cities.Add(newCity);
                        if (await db.SaveChangesAsync() > 0)
                            return newCity.id;
                    }
                    return 0;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        static public async Task<bool> UserExist(string token)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    User user = await db.Users.FirstOrDefaultAsync(o => o.token == token);
                    if (user != null)
                        return true;
                    else return false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<bool> PostOwner(string token, int postID)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    int? id = await GetUserIDByToken(token);
                    if (id.Value != 0)
                    {
                        Post post = await db.Posts.FirstOrDefaultAsync(o => o.id == postID && o.user_id == id);
                        if (post != null)
                            return true;
                        else return false;
                    }
                    else
                        return false;

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        static public async Task<bool> haveEnoghBooksToRedeem(string token, int requested)
        {
            try
            {
                int? id = await GetUserIDByToken(token);
                if (id.Value != 0)
                {
                    using (book_exchangeEntities db = new book_exchangeEntities())
                    {
                        int count = await db.Posts.Where(o => o.user_id == id.Value && o.traded == true && o.redeemed == false).CountAsync();
                        if (count >= requested)
                            return true;
                        else
                            return false;
                    }
                }
                else
                    return false;

            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<bool> CheckVerifyCode(string email, string code, string password)
        {
            using (book_exchangeEntities db = new book_exchangeEntities())
            {
                VerifyCode verify = await db.VerifyCodes.Where(o => o.User.email == email && o.code == code).FirstOrDefaultAsync();
                if (verify == null)
                    return false;
                else
                {
                    int id = verify.user_id;
                    User user = await db.Users.FindAsync(id);
                    user.password = GetMD5Hash(password);
                    db.Entry(verify).State = EntityState.Deleted;
                    db.Entry(user).State = EntityState.Modified;
                    if (await db.SaveChangesAsync() > 0)
                        return true;
                    else
                        return false;
                }
            }
        }
        static public async Task<bool> IsAdmin(string token)
        {
            try
            {
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    User user = await db.Users.Where(o => o.token == token).FirstOrDefaultAsync();
                    if (user != null)
                        if (user.admin)
                            return true;
                    return false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<bool> SendEmail(string email, string code)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("Yourbook084@gmail.com");
                message.To.Add(new MailAddress(email));
                message.Subject = "Password reset code";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = "your password reset code is : " + code;
                message.Priority = MailPriority.Normal;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";//"mail.smart-mail.net"; //for gmail host  
                smtp.EnableSsl = false;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("Yourbook084@gmail.com", "mmm123.m");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.Send(message);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        static public async Task<List<object>> recommentionSysAsync(string token)
        {
            string pythonFilePath = @"C:\computer science\4th\project\2\books_project-main\books_project-main\books recommendation\main.py";
            string booksCsvPath = @"C:\computer science\4th\project\2\books_project-main\books_project-main\books recommendation\Books.csv";
            string usersCsvPath = @"C:\computer science\4th\project\2\books_project-main\books_project-main\books recommendation\Users.csv";
            string ratingsCsvPath = @"C:\computer science\4th\project\2\books_project-main\books_project-main\books recommendation\Ratings.csv";
            string[] additionalArgs = await FetchData.UserFavBooks(token);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "python";
            startInfo.Arguments = $"\"{pythonFilePath}\" \"{booksCsvPath}\" \"{usersCsvPath}\" \"{ratingsCsvPath}\"";

            // Append additional arguments to the command line
            foreach (string arg in additionalArgs)
            {
                startInfo.Arguments += $" \"{arg}\"";
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                // Read the output from the Python script
                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                List<Book_> books = new List<Book_>();
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts[0] == "")
                    {
                        break;
                    }
                    Book_ book = new Book_();
                    char charToRemove1 = '[';
                    char charToRemove2 = ']';
                    char charToRemove3 = '\'';

                    string result1 = parts[0].Replace(charToRemove1.ToString(), string.Empty);
                    string result2 = parts[3].Replace(charToRemove2.ToString(), string.Empty);
                    string result3 = result2.Replace(charToRemove3.ToString(), string.Empty);
                    string isbn = result1.Replace(charToRemove3.ToString(), string.Empty);
                    string title = parts[1].Replace(charToRemove3.ToString(), string.Empty);
                    string author = parts[2].Replace(charToRemove3.ToString(), string.Empty);
                    string finalTitle = title.Substring(1);
                    string finalAuthor = author.Substring(1);

                    book.ISBN = isbn;
                    book.Title = finalTitle;
                    book.Author = finalAuthor;
                    book.image = result3;
                    books.Add(book);
                }
                using (book_exchangeEntities db = new book_exchangeEntities())
                {
                    List<object> data = new List<object>();
                    foreach (Book_ book in books)
                    {
                        // Console.WriteLine("book ISBN is:" + book.ISBN + "\n" + "book Title is:" + book.Title + "\n" + "author is:" + book.Author + "\n" + "image url is:" + book.image);
                        object rate = new { rate = 0, amount = 0 };
                        object temp = new
                        {
                            id = book.ISBN,
                            title = book.Title,
                            image = book.image,
                            description = "Author:" + Environment.NewLine + book.Author + Environment.NewLine
                            + "ISBN:" + book.ISBN,
                            traded = false,
                            url = "-",
                            rate = rate
                        };
                        recommendtion r = new recommendtion()
                        {
                            description = "Author:"+ Environment.NewLine + book.Author + Environment.NewLine
                            + "ISBN:" +Environment.NewLine + book.ISBN,
                            image = book.image,
                            title = book.Title,
                            url = book.image,
                            user_id = await GetUserIDByToken(token)
                        };
                        db.recommendtions.Add(r);
                        if (!data.Contains(temp))
                            data.Add(temp);
                    }
                    await db.SaveChangesAsync();
                    return data;
                }
            }
        }
        static public async Task<bool> HaveRecommendtion(int id)
        {
            using (book_exchangeEntities db = new book_exchangeEntities())
            {
                recommendtion r = await db.recommendtions.FirstOrDefaultAsync(o => o.user_id == id);
                if (r == null)
                    return false;
                else
                    return true;
            }
        }
    }
    class Book_
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string image { get; set; }
    }
}