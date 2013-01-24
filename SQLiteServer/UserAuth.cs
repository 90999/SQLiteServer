using System;
using System.Collections.Generic;
using System.IO;

// Own
using Tools;

namespace SQLiteServer
{
	public class UserAuth
	{
		public struct UserItem {
			public string Rights;
			public string Username;
			public string Password;
		};

		static private List<UserItem> Users;

		// Costructor
		public UserAuth ()
		{
			Users = LoadUserList();
		}

		// Destructor
		~UserAuth ()
		{
			Users.Clear();
			Users = null;
		}

		// Load users from users.txt within exe directory
		private List<UserItem> LoadUserList ()
		{
			Users = new List<UserItem> ();
			string Line = "";

			try {

				StreamReader streamReader = new StreamReader (Path.Combine (Tools.System.GetProgramDir (), "users.txt"));
				while ((Line = streamReader.ReadLine()) != null) {
					Line = Line.Trim();
					if (Line != "") {
						string[] LineArr = Line.Split(':');
						for (int i = 0; i<LineArr.Length; i++) {
							LineArr[i] = LineArr[i].Trim();
						}
						LineArr[0] = LineArr[0].ToLower(); // RW -> rw
						LineArr[1] = LineArr[1].ToLower(); // LowerCase Username (Ignore Case)
						if ((LineArr.Length == 3) && ((LineArr[0] == "ro") || (LineArr[0] == "rw"))) {
							Users.Add(
								new UserItem() {
									Rights = LineArr[0],
									Username = LineArr[1],
									Password = LineArr[2]
								}
							);
						}
					}
				}
				streamReader.Close ();

			} catch {
			}
			return Users;
		}

		// Check if User/Password matches and give back accessrights
		public string CheckUserPassword (string AUser, string APassword)
		{
			for (int i = 0; i<Users.Count; i++) {
				if ((Users[i].Username == AUser.ToLower().Trim()) && (Users[i].Password == APassword)) {
					return Users[i].Rights;
				}
			}
			return "";
		}
	}
}

