//===============================================================================
// Microsoft Premier Support for Developers
// Azure Active Directory Authentication Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;

namespace AzureADOpenID.Library
{
    public class NaiveSQLCache : TokenCache
    {
        private static ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string _userObjectId = string.Empty;
        string _cacheId = string.Empty;

        public NaiveSQLCache(string userId)
        {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            _cacheLock.EnterReadLock();
            this.Deserialize(GetCache(_cacheId));
            _cacheLock.ExitReadLock();
        }

        public void Persist()
        {
            _cacheLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            this.HasStateChanged = false;

            // Reflect changes in the persistent store
            SetCache(_cacheId, this.Serialize());
            _cacheLock.ExitWriteLock();
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            DeleteCache(_cacheId);
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                Persist();
            }
        }

        private byte[] GetCache(string cacheId)
        {
            byte[] tokens = null;
            const string command = "SELECT Tokens FROM dbo.TokenCache WHERE CacheId = @CacheId";
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlHelper _sqlHelper = new SqlHelper();

            parameters.Add(new SqlParameter("@CacheId", cacheId));

            try
            {
                SqlDataReader reader = _sqlHelper.ExecuteDataReader(command, System.Data.CommandType.Text, ref parameters);
                while(reader.Read())
                {
                    tokens = Convert.FromBase64String(reader["Tokens"].ToString());
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_sqlHelper != null) _sqlHelper.Close();
            }

            return tokens;
        }

        private void SetCache(string cacheId, byte[] tokens)
        {
            const string command = "MERGE dbo.TokenCache AS target USING (SELECT @CacheId, @Tokens) AS source (CacheId, Tokens) ON (target.CacheId = source.CacheId) WHEN MATCHED THEN UPDATE SET Tokens = source.Tokens WHEN NOT MATCHED THEN INSERT (CacheId, Tokens) VALUES (source.CacheId, source.Tokens);";
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlHelper _sqlHelper = new SqlHelper();

            parameters.Add(new SqlParameter("@CacheId", cacheId));
            parameters.Add(new SqlParameter("@Tokens", Convert.ToBase64String(tokens)));

            try
            {
                _sqlHelper.Execute(command, System.Data.CommandType.Text, ref parameters);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_sqlHelper != null) _sqlHelper.Close();
            }
        }

        private void DeleteCache(string cacheId)
        {
            const string command = "DELETE FROM dbo.TokenCache WHERE CacheId = @CacheId";
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlHelper _sqlHelper = new SqlHelper();

            parameters.Add(new SqlParameter("@CacheId", cacheId));

            try
            {
                _sqlHelper.Execute(command, System.Data.CommandType.Text, ref parameters);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_sqlHelper != null) _sqlHelper.Close();
            }
        }
    }
}
