using AutoScanFQCTest.DialogWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels {
	public class FupanUser {
		public UserType UserType;
		public string UserName;
		public string Password;
	}

	// 用户类型
	public enum UserType {
		管理员 = 0,
		工程师 = 1,
		技术员 = 2,
		操作员 = 3
	}
}