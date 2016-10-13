using System.Collections.Generic;

namespace WebLoadTestUtils.Base
{
	public sealed class AutologinUsersStorage<T>
	{
		private readonly List<T> _users;
		private readonly int _totalUsersCount;
		private readonly object _usersLock = new object();
		private volatile int _lastSelectedUserIndex;

		public AutologinUsersStorage(List<T> users)
		{
			_users = users;
			_totalUsersCount = _users.Count;
		}

		public T GetNextUser()
		{
			int index = getNextUserIndex();
			return _users[index];
		}

		private int getNextUserIndex()
		{
			int lastSelectedUserIndex;
			lock (_usersLock)
			{
				if (_lastSelectedUserIndex >= _totalUsersCount - 1)
				{
					_lastSelectedUserIndex = 0;
				}
				else
				{
					_lastSelectedUserIndex++;
				}
				lastSelectedUserIndex = _lastSelectedUserIndex;
			}
			return lastSelectedUserIndex;
		}

		public int TotalUsersCount { get { return _totalUsersCount; } }
	}
}
