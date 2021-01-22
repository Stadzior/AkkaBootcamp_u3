using System;
using Octokit;

namespace GithubActors
{
    /// <summary>
    /// Used to report on incremental progress.
    /// 
    /// Immutable.
    /// </summary>
    public class GithubProgressStats
    {
        public int ExpectedUsers { get; }
        public int UsersThusFar { get; }
        public int QueryFailures { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; }
        public TimeSpan Elapsed => ((EndTime ?? DateTime.UtcNow) - StartTime);
        public bool IsFinished => ExpectedUsers == UsersThusFar + QueryFailures;
        public int PoolSize { get; }

        public GithubProgressStats()
            => StartTime = DateTime.UtcNow;

        private GithubProgressStats(DateTime startTime, int expectedUsers, int usersThusFar, int queryFailures, DateTime? endTime, int poolSize)
        {
            EndTime = endTime;
            QueryFailures = queryFailures;
            UsersThusFar = usersThusFar;
            ExpectedUsers = expectedUsers;
            StartTime = startTime;
            PoolSize = poolSize;
        }

        /// <summary>
        /// Add <see cref="delta"/> users to the running total of <see cref="UsersThusFar"/>
        /// </summary>
        public GithubProgressStats UserQueriesFinished(int delta = 1)
            => Copy(usersThusFar: UsersThusFar + delta);

        /// <summary>
        /// Set the <see cref="ExpectedUsers"/> total
        /// </summary>
        public GithubProgressStats SetExpectedUserCount(int totalExpectedUsers) 
            => Copy(totalExpectedUsers);

        /// <summary>
        /// Add <see cref="delta"/> to the running <see cref="QueryFailures"/> total
        /// </summary>
        public GithubProgressStats IncrementFailures(int delta = 1) 
            => Copy(queryFailures: QueryFailures + delta);

        /// <summary>
        /// Update pool size to actual current pool size.
        /// </summary>
        /// <param name="poolSize">Current pool size</param>
        /// <returns></returns>
        public GithubProgressStats UpdatePoolSize(int poolSize)
            => Copy(poolSize: poolSize);

        /// <summary>
        /// Query is finished! Set's the <see cref="EndTime"/>
        /// </summary>
        public GithubProgressStats Finish()
            => Copy(endTime: DateTime.UtcNow);

        /// <summary>
        /// Creates a deep copy of the <see cref="GithubProgressStats"/> class
        /// </summary>
        public GithubProgressStats Copy(int? expectedUsers = null, int? usersThusFar = null, int? queryFailures = null, DateTime? startTime = null, DateTime? endTime = null, int? poolSize = null) 
            => new GithubProgressStats(startTime ?? StartTime, expectedUsers ?? ExpectedUsers, usersThusFar ?? UsersThusFar, queryFailures ?? QueryFailures, endTime ?? EndTime, poolSize ?? PoolSize);
    }
}