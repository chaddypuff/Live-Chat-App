using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Collections.Generic;
using SignalRChat.Models;

namespace SignalRChat
{
    public class dbHelper
    {
        private static string connectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings["chatConnectionString"].ToString(); } }

        //------------- Dapper methods------------------
        private static IDbConnection Connection()
        {
            return new SqlConnection(connectionString);
        }
        /// <summary>
        /// The default type of CommandType is set to Text. You can explicitly pass CommandType.StoresProcedure to run sql.
        /// Note: To avoid any issue with Dapper methods, use of and interface as a type T is not recommended, T can be a class (only)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ProcNameOrSQL"></param>
        /// <param name="parameters"></param>
        /// <param name="_commandType"></param>
        /// <returns>FirstOrDefault of Type T</returns>
        public static T QueryFirstOrDefault<T>(string ProcNameOrSQL, object parameters = null, CommandType _commandType = CommandType.Text)
        {
            using (var connection = Connection())
            {
                return connection.Query<T>(ProcNameOrSQL, parameters, commandType: _commandType).FirstOrDefault();
            }
        }
        /// <summary>
        /// The default type of CommandType is set to Text. You can explicitly pass CommandType.StoresProcedure to run sql.
        /// Note: To avoid any issue with Dapper methods, use of and interface as a type T is not recommended, T can be a class (only)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ProcNameOrSQL"></param>
        /// <param name="parameters"></param>
        /// <param name="_commandType"></param>
        /// <returns>List of Type T</returns>
        public static List<T> Query<T>(string ProcNameOrSQL, object parameters = null, CommandType _commandType = CommandType.Text)
        {
            using (var connection = Connection())
            {
                return connection.Query<T>(ProcNameOrSQL, parameters, commandType: _commandType).ToList();
            }
        }
        /// <summary>
        /// The default type of CommandType is set to Text. You can explicitly pass CommandType.StoresProcedure to run sql.
        /// Note: To avoid any issue with Dapper methods, use of and interface as a type T is not recommended, T can be a class (only)
        /// </summary>
        /// <param name="ProcNameOrSQL"></param>
        /// <param name="parameters"></param>
        /// <param name="_commandType"></param>
        /// <returns>Id or number of affected rows</returns>
        public static int Execute(string ProcNameOrSQL, object parameters = null, CommandType _commandType = CommandType.Text)
        {
            using (var connection = Connection())
            {
                return connection.Execute(ProcNameOrSQL, parameters, commandType: _commandType);
            }
        }
        /// <summary>
        /// Returns Parent alongwith child elements
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TChild"></typeparam>
        /// <typeparam name="TParentKey"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parentKeySelector"></param>
        /// <param name="childSelector"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="_commandType"></param>
        /// <returns></returns>
        public static List<TParent> QueryParentChild<TParent, TChild, TParentKey>(string sql, Func<TParent, TParentKey> parentKeySelector,
                      Func<TParent, IList<TChild>> childSelector, object parameters = null, IDbTransaction transaction = null,
                      bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType _commandType = CommandType.Text)
        {
            Dictionary<TParentKey, TParent> cache = new Dictionary<TParentKey, TParent>();
            using (var connection = Connection())
            {
                connection.Query<TParent, TChild, TParent>(
                sql,
                (parent, child) =>
                {
                    if (!cache.ContainsKey(parentKeySelector(parent)))
                    {
                        cache.Add(parentKeySelector(parent), parent);
                    }

                    TParent cachedParent = cache[parentKeySelector(parent)];
                    IList<TChild> children = childSelector(cachedParent);
                    children.Add(child);
                    return cachedParent;
                },
                parameters as object, transaction, buffered, splitOn, commandTimeout, _commandType);
            }
            return cache.Values.ToList();
        }


        public static class Users
        {
            public static User GetByUserName(string username)
            {
                using (var con = Connection())
                {
                    return con.QueryFirstOrDefault<User>("SELECT TOP 1 * FROM Users WHERE Name=@username", new { username });
                }
            }
            public static User CheckEmailExistance(string email)
            {
                email = email.Trim();
                using (var con = Connection())
                {
                    return con.QueryFirstOrDefault<User>("SELECT TOP 1 * FROM Users WHERE Email=@email", new { email });
                }
            }
            public static long Create(User obj)
            {
                using (var con = Connection())
                {
                    return con.Insert(obj);
                }
            }
            public static bool Update(User obj)
            {
                using (var con = Connection())
                {
                    return con.Update(obj);
                }
            }

            internal static IEnumerable<User> GetAll()
            {
                using (var con = Connection())
                {
                    return con.GetAll<User>();
                }
            }
        }
        public static class UserChats
        {
            public static long Create(UserChat obj)
            {
                using (var con = Connection())
                {
                    return con.Insert(obj);
                }
            }

            public static IEnumerable<UserChat> GetAll()
            {
                using (var con = Connection())
                {
                    return con.GetAll<UserChat>();
                }
            }
        }

    }
}