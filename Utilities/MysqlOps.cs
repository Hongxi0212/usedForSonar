using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using AutoScanFQCTest.DataModels;
using MySql.Data.MySqlClient;

namespace AutoScanFQCTest.Utilities
{
    public class MysqlOps
    {
        public MainWindow m_parent;

        private static readonly object threadLock = new object();

        MySqlConnection m_connection = null;

        public MysqlOps(MainWindow parent)
        {
            m_parent = parent;
        }

        public bool IsMySQLServiceInstalled()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service.ServiceName.ToLower().StartsWith("mysql"))
                {
                    return true;
                }
            }
            return false;
        }

        // 连接到MySQL数据库
        public bool ConnectToMySQL(string strUser, string strPassword, int nPort)
        {
            string connStr = string.Format("server=localhost;user={0};port={1};password={2};SslMode=None;allowPublicKeyRetrieval=true;", strUser, nPort, strPassword);

            try
            {
                m_connection = new MySqlConnection(connStr);

                m_connection.Open();

                using (MySqlCommand cmd = m_connection.CreateCommand())
                {
                    cmd.CommandText = "SET global wait_timeout=1990000";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SET global interactive_timeout=10000000";
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
                // 无法连接到MySQL数据库
                return false;
            }
        }
        // 断开与MySQL数据库的连接
        public void DisconnectFromMySQL()
        {
            if (m_connection != null)
            {
                if (m_connection.State == System.Data.ConnectionState.Open)
                {
                    m_connection.Close();
                }
            }
        }

        // 连接到指定的数据库
        public bool ConnectToDatabase(string strDatabaseName)
        {
            try
            {
                string sql = $"USE {strDatabaseName}";

                MySqlCommand cmd = new MySqlCommand(sql, m_connection);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(strDatabaseName + "数据库不存在！");
                });

                return false;
            }
        }

        // 判断数据库是否存在
        public bool IsDatabaseExist(string strDatabaseName)
        {
            try
            {
                // 检查数据库是否存在
                string sql = $"SHOW DATABASES LIKE '{strDatabaseName}'";
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                if (cmd.ExecuteScalar() == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
                return false;
            }
        }

        // 判断表是否存在
        public bool IsTableExist(string strDatabaseName, string strTableName)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                // Check if table exists
                string sql = $"SHOW TABLES LIKE '{strTableName}'";
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                if (cmd.ExecuteScalar() == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
                return false;
            }
        }

        // 创建数据库
        public bool CreateDatabase(string strDatabaseName)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                // Create database
                string sql = $"CREATE DATABASE {strDatabaseName}";
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 创建表
        public bool CreateTable(string strDatabaseName, string strTableName, string strSqlCommand)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                MySqlCommand cmd = new MySqlCommand(strSqlCommand, m_connection);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        public bool TruncateTableData(string strTableName)
		{
			try
			{
				if (m_connection.State != System.Data.ConnectionState.Open)
				{
					return false;
				}

				string sql = $"TRUNCATE TABLE {strTableName}";

				MySqlCommand cmd = new MySqlCommand(sql, m_connection);
				cmd.ExecuteNonQuery();
				cmd.Dispose();

				return true;
			}
			catch (Exception ex)
			{
				Application.Current.Dispatcher.Invoke(() => {
					MessageBox.Show(ex.Message);
				});

				return false;
			}
		}

        // 删除数据表N天前的数据
        public bool DeleteTableData(string strTableName, int nDaysBefore)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                string sql = $"DELETE FROM {strTableName} WHERE create_time < NOW() - INTERVAL {nDaysBefore} DAY;";

                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        public bool DeleteTransferTableData(string tableName, int daysBefore)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                string sql = $"DELETE FROM {tableName} WHERE createTime < NOW() - INTERVAL {daysBefore} DAY;";

                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 向Defect表添加一条记录
        public bool AddDefectToTable(string strTableName, string strSqlCommand, Defect defect, string strProductID)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@id", defect.id);
                    command.Parameters.AddWithValue("@set_id", defect.SetId);
                    command.Parameters.AddWithValue("@uuid", defect.Uuid);
                    command.Parameters.AddWithValue("@product_id", strProductID);
                    command.Parameters.AddWithValue("@height", defect.Height);
                    command.Parameters.AddWithValue("@sn", defect.Sn);
                    command.Parameters.AddWithValue("@type", defect.Type);
                    command.Parameters.AddWithValue("@width", defect.Width);
                    command.Parameters.AddWithValue("@x", defect.X);
                    command.Parameters.AddWithValue("@y", defect.Y);
                    command.Parameters.AddWithValue("@channel_", defect.Channel);
                    command.Parameters.AddWithValue("@channelNum_", defect.ChannelNum);
                    command.Parameters.AddWithValue("@area", defect.Area);
                    command.Parameters.AddWithValue("@um_per_pixel", defect.um_per_pixel);
                    command.Parameters.AddWithValue("@aiResult", defect.aiResult);

                    string strDefectCode = string.Empty;
                    foreach (var code in defect.aiDefectCode)
                    {
                        strDefectCode += code + ";";
                    }
                    command.Parameters.AddWithValue("@aiDefectCode", strDefectCode);

                    command.Parameters.AddWithValue("@aiGuid", defect.aiGuid);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 向Defect表添加一条记录
        public bool AddDefectToTable(string strTableName, string strSqlCommand, DefectInfo defect)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@id", defect.ID);
                    command.Parameters.AddWithValue("@set_id", defect.set_id);
                    command.Parameters.AddWithValue("@product_id", defect.product_id);
                    command.Parameters.AddWithValue("@height", defect.height);
                    command.Parameters.AddWithValue("@type", defect.type);
                    command.Parameters.AddWithValue("@width", defect.width);
                    command.Parameters.AddWithValue("@area", defect.area);
                    command.Parameters.AddWithValue("@center_x", defect.center_x);
                    command.Parameters.AddWithValue("@center_y", defect.center_y);
                    command.Parameters.AddWithValue("@side", defect.side);
                    command.Parameters.AddWithValue("@light_channel", defect.light_channel);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 向Product表添加一条记录
        public bool AddProductToTable(string strTableName, string strSqlCommand, Product product, string strBatchID)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@SetId", product.SetId);
                    command.Parameters.AddWithValue("@Uuid", product.Uuid);
                    command.Parameters.AddWithValue("@BatchId", strBatchID);
                    command.Parameters.AddWithValue("@BarCode", product.BarCode);
                    command.Parameters.AddWithValue("@Image", product.Image);
                    command.Parameters.AddWithValue("@ImageA", product.ImageA);
                    command.Parameters.AddWithValue("@ImageB", product.ImageB);
                    command.Parameters.AddWithValue("@IsNull", product.IsNull);
                    command.Parameters.AddWithValue("@PanelId", product.PanelId);
                    command.Parameters.AddWithValue("@PosCol", product.PosCol);
                    command.Parameters.AddWithValue("@PosRow", product.PosRow);
                    command.Parameters.AddWithValue("@ShareimgPath", product.ShareimgPath);
                    command.Parameters.AddWithValue("@r1", product.r1);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 向Product表添加一条记录
        public bool AddProductToTable(string strTableName, string strSqlCommand, ProductInfo product)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@SetId", product.set_id);
                    command.Parameters.AddWithValue("@MachineId", product.machine_id);
                    command.Parameters.AddWithValue("@BarCode", product.barcode);
                    command.Parameters.AddWithValue("@PosCol", product.column);
                    command.Parameters.AddWithValue("@PosRow", product.row);
                    command.Parameters.AddWithValue("@bET", product.bET);
                    command.Parameters.AddWithValue("@sideA_image_path", product.sideA_image_path);
                    command.Parameters.AddWithValue("@sideB_image_path", product.sideB_image_path);
                    command.Parameters.AddWithValue("@sideC_image_path", product.sideC_image_path);
                    command.Parameters.AddWithValue("@sideD_image_path", product.sideD_image_path);
                    command.Parameters.AddWithValue("@sideE_image_path", product.sideE_image_path);
                    command.Parameters.AddWithValue("@sideF_image_path", product.sideF_image_path);
                    command.Parameters.AddWithValue("@sideG_image_path", product.sideG_image_path);
                    command.Parameters.AddWithValue("@sideH_image_path", product.sideH_image_path);
                    command.Parameters.AddWithValue("@image_path_for_MES_xiaochengxu_chatu", product.all_image_paths);
                    command.Parameters.AddWithValue("@r1", product.r1);
                    command.Parameters.AddWithValue("@r2", product.r2);
                    command.Parameters.AddWithValue("@r3", product.r3);
                    command.Parameters.AddWithValue("@r4", product.r4);
                    command.Parameters.AddWithValue("@r5", product.r5);
                    command.Parameters.AddWithValue("@r6", product.r6);
                    command.Parameters.AddWithValue("@r7", product.r7);
                    command.Parameters.AddWithValue("@r8", product.r8);
                    command.Parameters.AddWithValue("@r9", product.r9);
                    command.Parameters.AddWithValue("@r10", product.r10);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 向ProductInfo表添加一条记录
        public bool AddTrayInfoToTable(string strTableName, string strSqlCommand, AVITrayInfo trayInfo)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@BatchId", trayInfo.BatchId);
                    command.Parameters.AddWithValue("@Row", trayInfo.Row);
                    command.Parameters.AddWithValue("@Col", trayInfo.Col);
                    command.Parameters.AddWithValue("@Front", trayInfo.Front);
                    command.Parameters.AddWithValue("@Mid", trayInfo.Mid);
                    command.Parameters.AddWithValue("@Operator", trayInfo.Operator);
                    command.Parameters.AddWithValue("@OperatorId", trayInfo.OperatorId);
                    command.Parameters.AddWithValue("@ProductId", trayInfo.ProductId);
                    command.Parameters.AddWithValue("@Resource", trayInfo.Resource);
                    command.Parameters.AddWithValue("@ScanCodeMode", trayInfo.ScanCodeMode);
                    command.Parameters.AddWithValue("@SetId", trayInfo.SetId);
                    command.Parameters.AddWithValue("@TotalPcs", trayInfo.TotalPcs);
                    command.Parameters.AddWithValue("@Uuid", trayInfo.Uuid);
                    command.Parameters.AddWithValue("@WorkArea", trayInfo.WorkArea);
                    command.Parameters.AddWithValue("@Fullstatus", trayInfo.Fullstatus);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ex.Message.Contains("Duplicate entry"))
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息已存在数据库中，无法插入！ex.Message = {0}", ex.Message));
                        //m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息（trayInfo.SetId={0}）已存在数据库中，无法插入！", trayInfo.SetId));
                    }
                    else
                    {
                        MessageBox.Show(m_parent, ex.Message);
                    }
                });

                return false;
            }
        }

        // 向ProductInfo表添加一条记录
        public bool AddTrayInfoToTable(string strTableName, string strSqlCommand, TrayInfo trayInfo)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                using (var command = new MySqlCommand(strSqlCommand, m_connection))
                {
                    command.Parameters.AddWithValue("@Row", trayInfo.total_rows);
                    command.Parameters.AddWithValue("@Col", trayInfo.total_columns);
                    command.Parameters.AddWithValue("@Operator", trayInfo.Operator);
                    command.Parameters.AddWithValue("@OperatorId", trayInfo.operator_id);
                    command.Parameters.AddWithValue("@Resource", trayInfo.site);
                    command.Parameters.AddWithValue("@SetId", trayInfo.set_id);
                    command.Parameters.AddWithValue("@MachineId", trayInfo.machine_id);
                    command.Parameters.AddWithValue("@WorkArea", trayInfo.site);
                    command.Parameters.AddWithValue("@Site", trayInfo.site);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ex.Message.Contains("Duplicate entry"))
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息已存在数据库中，无法插入！ex.Message = {0}", ex.Message));
                        //m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息（trayInfo.SetId={0}）已存在数据库中，无法插入！", trayInfo.SetId));
                    }
                    else
                    {
                        MessageBox.Show(m_parent, ex.Message);
                    }
                });

                return false;
            }
        }

        // 向复判记录表添加一条记录
        public bool AddRecheckRecordToTable(string strSqlCommand, string jsonRecheckResult, string setID)
        {
			try
			{
				if (m_connection.State != System.Data.ConnectionState.Open)
				{
					return false;
				}

				using(var command = new MySqlCommand(strSqlCommand, m_connection))
				{
					command.Parameters.AddWithValue("@SetID", setID);
					command.Parameters.AddWithValue("@JsonData", jsonRecheckResult);

					command.ExecuteNonQuery();
				}

				return true;
			}
			catch(Exception ex)
			{
				Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

				Application.Current.Dispatcher.Invoke(() => {
					if(ex.Message.Contains("Duplicate entry"))
					{
						//m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息已存在数据库中，无法插入！ex.Message = {0}", ex.Message));
                        m_parent.m_global.m_log_presenter.Log(string.Format("该托盘信息（setid={0}）已存在数据库中，无法插入！", setID));
                    }
					else
					{
						MessageBox.Show(m_parent, ex.Message);
					}
				});

				return false;
			}
			return false;
        }

        // 向statistics_by_defects_表添加复判缺陷记录
        public bool AddDataToDefectsStatisticsTable(AVITrayInfo trayInfo, List<RecheckResult[]> list_recheck_flags_by_defect)
        {
            if (m_connection.State != System.Data.ConnectionState.Open)
            {
                return false;
            }

            try
            {
                string strTableOfStatisticsByDefects = "defects_statistics_" + trayInfo.Mid;

                // 如果strTables文本包含"-"，替换为"_"，因为数据库表名不允许包含"-"
                strTableOfStatisticsByDefects = strTableOfStatisticsByDefects.Replace("-", "_");

                // 遍历托盘中的每个产品
                for (int i = 0; i < trayInfo.Products.Count; i++)
                {
                    Product product = trayInfo.Products[i];

                    int nColumns = trayInfo.Col;
                    int nRows = trayInfo.Row;

                    // 获取该产品的行列信息
                    int col = product.PosCol - 1;
                    int row = product.PosRow - 1;

                    // 获取该产品的复判结果
                    RecheckResult[] recheckResults = list_recheck_flags_by_defect[i];

                    for (int n = 0; n < recheckResults.Length; n++)
                    {
                        Defect defect = product.Defects[n];

						// 获取该缺陷的数据和复判结果
						DefectStatistics defectRecord = new DefectStatistics();

                        defectRecord.MachineId = trayInfo.Mid;
                        defectRecord.ProductId = trayInfo.ProductId;
                        defectRecord.TrayId = trayInfo.SetId;
                        defectRecord.Barcode = product.BarCode;
                        defectRecord.DefectType = defect.Type;
                        defectRecord.TimeBlock = DateTime.Now;

                        switch (recheckResults[n])
                        {
                        case RecheckResult.OK:
                            defectRecord.RecheckOKCount = 1;
							defectRecord.RecheckNGCount = 0;
							break;
                        case RecheckResult.NG:
                            defectRecord.RecheckNGCount = 1;
							defectRecord.RecheckOKCount = 0;
							break;
                        }

                        // 构造插入数据库的SQL语句
                        string query = @"
                            INSERT INTO " + strTableOfStatisticsByDefects + @" (machine_id, product_id, tray_id, barcode, defect_type, time_block, ng_count, ok_count) 
                            VALUES (@MachineId, @ProductId, @TrayId, @Barcode, @DefectType, DATE_FORMAT(@TimeBlock, '%Y-%m-%d %H:00:00'), @RecheckNGCount, @RecheckOKCount)
                            ON DUPLICATE KEY UPDATE ng_count = ng_count + VALUES(ng_count), ok_count = ok_count + VALUES(ok_count);";

                        using (var command = new MySqlCommand(query, m_connection))
                        {
                            command.Parameters.AddWithValue("@MachineId", defectRecord.MachineId);
                            command.Parameters.AddWithValue("@ProductId", defectRecord.ProductId);
                            command.Parameters.AddWithValue("@TrayId", defectRecord.TrayId);
                            command.Parameters.AddWithValue("@Barcode", defectRecord.Barcode);
                            command.Parameters.AddWithValue("@DefectType", defectRecord.DefectType);
                            command.Parameters.AddWithValue("@TimeBlock", defectRecord.TimeBlock);
                            command.Parameters.AddWithValue("@RecheckNGCount", defectRecord.RecheckNGCount);
                            command.Parameters.AddWithValue("@RecheckOKCount", defectRecord.RecheckOKCount);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                for (int i = 0; i < trayInfo.products_with_unrecheckable_defect.Count; i++)
                {
                    Product product = trayInfo.products_with_unrecheckable_defect[i];

                    int col = product.PosCol - 1;
                    int row = product.PosRow - 1;

                    for (int j = 0; j < product.Defects.Count; j++)
                    {
                        Defect defect = product.Defects[j];

                        // 获取该缺陷的数据和复判结果
                        DefectStatistics defectRecord = new DefectStatistics();

                        defectRecord.MachineId = trayInfo.Mid;
                        defectRecord.ProductId = trayInfo.BatchId;
                        defectRecord.TrayId = trayInfo.SetId;
                        defectRecord.Barcode = product.BarCode;
                        defectRecord.DefectType = defect.Type;
                        defectRecord.TimeBlock = DateTime.Now;
                        defectRecord.RecheckNGCount = 1;
                        defectRecord.RecheckOKCount = 0;

                        // 构造插入数据库的SQL语句
                        string query = @"
                            INSERT INTO " + strTableOfStatisticsByDefects + @" (machine_id, product_id, tray_id, barcode, defect_type, time_block, ng_count, ok_count) 
                            VALUES (@MachineId, @ProductId, @TrayId, @Barcode, @DefectType, DATE_FORMAT(@TimeBlock, '%Y-%m-%d %H:00:00'), @RecheckNGCount, @RecheckOKCount)
                            ON DUPLICATE KEY UPDATE ng_count = ng_count + VALUES(ng_count), ok_count = ok_count + VALUES(ok_count);";

                        using (var command = new MySqlCommand(query, m_connection))
                        {
                            command.Parameters.AddWithValue("@MachineId", defectRecord.MachineId);
                            command.Parameters.AddWithValue("@ProductId", defectRecord.ProductId);
                            command.Parameters.AddWithValue("@TrayId", defectRecord.TrayId);
                            command.Parameters.AddWithValue("@Barcode", defectRecord.Barcode);
                            command.Parameters.AddWithValue("@DefectType", defectRecord.DefectType);
                            command.Parameters.AddWithValue("@TimeBlock", defectRecord.TimeBlock);
                            command.Parameters.AddWithValue("@RecheckNGCount", defectRecord.RecheckNGCount);
                            command.Parameters.AddWithValue("@RecheckOKCount", defectRecord.RecheckOKCount);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        public bool AddDataToDefectsStatisticsTable(TrayInfo trayInfo, List<RecheckResult[]> listRecheckFlags_defect)
        {
            if (m_connection.State != System.Data.ConnectionState.Open)
            {
                return false;
            }

            try
            {
                string strTableOfStatisticsByDefects = "defects_statistics_" + trayInfo.machine_id;

                // 如果strTables文本包含"-"，替换为"_"，因为数据库表名不允许包含"-"
                strTableOfStatisticsByDefects = strTableOfStatisticsByDefects.Replace("-", "_");

                // 遍历托盘中的每个产品
                for (int i = 0; i < trayInfo.products.Count; i++)
                {
                    ProductInfo product = trayInfo.products[i];

                    // 获取该产品的行列信息
                    int col = product.column - 1;
                    int row = product.row - 1;

                    // 获取该产品的复判结果
                    RecheckResult[] recheckResults = listRecheckFlags_defect[i];

                    for (int n = 0; n < recheckResults.Length; n++)
                    {
                        DefectInfo defect = product.defects[n];

						// 获取该缺陷的数据和复判结果
						DefectStatistics defectRecord = new DefectStatistics();

                        defectRecord.MachineId = trayInfo.machine_id;
                        defectRecord.ProductId = trayInfo.r4;
                        if (m_parent.m_global.m_strProductSubType == "glue_check")
                        {
                            defectRecord.ProductId = trayInfo.r7.Split('_')[0];
                        }
                        defectRecord.TrayId = trayInfo.set_id;
                        defectRecord.Barcode = product.barcode;
						defectRecord.DefectType = defect.type;
						defectRecord.TimeBlock = DateTime.Now;

						switch (recheckResults[n])
						{
						case RecheckResult.OK:
							defectRecord.RecheckOKCount = 1;
							defectRecord.RecheckNGCount = 0;
							break;
						case RecheckResult.NG:
							defectRecord.RecheckNGCount = 1;
							defectRecord.RecheckOKCount = 0;
							break;
						}

						// 构造插入数据库的SQL语句
						string query = @"
                            INSERT INTO " + strTableOfStatisticsByDefects + @" (machine_id, product_id, tray_id, barcode, defect_type, time_block, ng_count, ok_count) 
                            VALUES (@MachineId, @ProductId, @TrayId, @Barcode, @DefectType, DATE_FORMAT(@TimeBlock, '%Y-%m-%d %H:00:00'), @RecheckNGCount, @RecheckOKCount)
                            ON DUPLICATE KEY UPDATE ng_count = ng_count + VALUES(ng_count), ok_count = ok_count + VALUES(ok_count);";

                        using (var command = new MySqlCommand(query, m_connection))
						{
							command.Parameters.AddWithValue("@MachineId", defectRecord.MachineId);
                            command.Parameters.AddWithValue("@ProductId", defectRecord.ProductId);
                            command.Parameters.AddWithValue("@TrayId", defectRecord.TrayId);
                            command.Parameters.AddWithValue("@Barcode", defectRecord.Barcode);
							command.Parameters.AddWithValue("@DefectType", defectRecord.DefectType);
							command.Parameters.AddWithValue("@TimeBlock", defectRecord.TimeBlock);
							command.Parameters.AddWithValue("@RecheckNGCount", defectRecord.RecheckNGCount);
							command.Parameters.AddWithValue("@RecheckOKCount", defectRecord.RecheckOKCount);

							command.ExecuteNonQuery();
						}
					}
                }

                #region 添加不可复判项的代码，不需要添加这部分复判数据，已注释
                // 遍历不可复判项
                for (int i = 0; i < trayInfo.products_with_unrecheckable_defect.Count; i++)
                {
                    ProductInfo product = trayInfo.products_with_unrecheckable_defect[i];

                    int col = product.column - 1;
                    int row = product.row - 1;

                    for (int j = 0; j < product.defects.Count; j++)
                    {
                        DefectInfo defect = product.defects[j];

                        // 获取该缺陷的数据和复判结果
                        DefectStatistics defectRecord = new DefectStatistics();

                        defectRecord.MachineId = trayInfo.machine_id;
                        defectRecord.ProductId = trayInfo.r4;
                        defectRecord.TrayId = trayInfo.set_id;
                        defectRecord.Barcode = product.barcode;
                        defectRecord.DefectType = defect.type;
                        defectRecord.TimeBlock = DateTime.Now;
                        defectRecord.RecheckNGCount = 1;
                        defectRecord.RecheckOKCount = 0;

                        // 构造插入数据库的SQL语句
                        string query = @"
				                        INSERT INTO " + strTableOfStatisticsByDefects + @" (machine_id, product_id, tray_id, barcode, defect_type, time_block, ng_count, ok_count) 
				                        VALUES (@MachineId, @ProductId, @TrayId, @Barcode, @DefectType, DATE_FORMAT(@TimeBlock, '%Y-%m-%d %H:00:00'), @RecheckNGCount, @RecheckOKCount)
				                        ON DUPLICATE KEY UPDATE ng_count = ng_count + VALUES(ng_count), ok_count = ok_count + VALUES(ok_count);";

                        using (var command = new MySqlCommand(query, m_connection))
                        {
                            command.Parameters.AddWithValue("@MachineId", defectRecord.MachineId);
                            command.Parameters.AddWithValue("@ProductId", defectRecord.ProductId);
                            command.Parameters.AddWithValue("@TrayId", defectRecord.TrayId);
                            command.Parameters.AddWithValue("@Barcode", defectRecord.Barcode);
                            command.Parameters.AddWithValue("@DefectType", defectRecord.DefectType);
                            command.Parameters.AddWithValue("@TimeBlock", defectRecord.TimeBlock);
                            command.Parameters.AddWithValue("@RecheckNGCount", defectRecord.RecheckNGCount);
                            command.Parameters.AddWithValue("@RecheckOKCount", defectRecord.RecheckOKCount);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                #endregion

                // 遍历OK项
                for (int i = 0; i < trayInfo.OK_products.Count; i++)
                {
                    // 获取该缺陷的数据和复判结果
                    DefectStatistics defectRecord = new DefectStatistics();

                    defectRecord.MachineId = trayInfo.machine_id;
                    defectRecord.ProductId = trayInfo.r4;
                    defectRecord.TrayId = trayInfo.set_id;
                    defectRecord.Barcode = "OKProduct";
                    defectRecord.DefectType = "OKProductHasNoDefects";
                    defectRecord.TimeBlock = DateTime.Now;
                    defectRecord.RecheckNGCount = 0;
                    defectRecord.RecheckOKCount = 1;

                    // 构造插入数据库的SQL语句
                    string query = @"
                            INSERT INTO " + strTableOfStatisticsByDefects + @" (machine_id, product_id, tray_id, barcode, defect_type, time_block, ng_count, ok_count) 
                            VALUES (@MachineId, @ProductId, @TrayId, @Barcode, @DefectType, DATE_FORMAT(@TimeBlock, '%Y-%m-%d %H:00:00'), @RecheckNGCount, @RecheckOKCount)
                            ON DUPLICATE KEY UPDATE ok_count = ok_count + VALUES(ok_count);";

                    using (var command = new MySqlCommand(query, m_connection))
                    {
                        command.Parameters.AddWithValue("@MachineId", defectRecord.MachineId);
                        command.Parameters.AddWithValue("@ProductId", defectRecord.ProductId);
                        command.Parameters.AddWithValue("@TrayId", defectRecord.TrayId);
                        command.Parameters.AddWithValue("@Barcode",defectRecord.Barcode);
                        command.Parameters.AddWithValue("@DefectType", defectRecord.DefectType);
                        command.Parameters.AddWithValue("@TimeBlock", defectRecord.TimeBlock);
                        command.Parameters.AddWithValue("@RecheckNGCount", defectRecord.RecheckNGCount);
                        command.Parameters.AddWithValue("@RecheckOKCount", defectRecord.RecheckOKCount);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 写入数据库
        public bool WriteMeasureResultsToTable(DetectionResultEntry result, string strDatabaseName, string strTableName)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                strTableName = "t" + strTableName;

                string query = $"INSERT INTO {strTableName} (ID, INDEX, MachineID, ResultFlag, DefectType, Barcode, Result, Time, DefectArea)"
                    + $"VALUES (@ID, @INDEX, @MachineID, @ResultFlag, @DefectType, @Barcode, @Result, @Time, @DefectArea)";

                using (var command = new MySqlCommand(query, m_connection))
                {
                    command.Parameters.AddWithValue("@ID", result.m_nID);
                    command.Parameters.AddWithValue("@INDEX", result.m_nIndex);
                    command.Parameters.AddWithValue("@MachineID", result.m_nMachineID);
                    command.Parameters.AddWithValue("@ResultFlag", result.m_nResultFlag);
                    command.Parameters.AddWithValue("@DefectType", result.m_nDefectType);
                    command.Parameters.AddWithValue("@Barcode", result.m_strBarcode);
                    command.Parameters.AddWithValue("@Result", result.m_strResult);
                    command.Parameters.AddWithValue("@Time", result.m_strTime);
                    command.Parameters.AddWithValue("@DefectArea", result.m_dbDefectArea);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 执行查询
        public bool ExecuteQueryWithoutNeedingResult(string strQuery, ref string strError)
        {
            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    strError = "数据库尚未连接！";
                    return false;
                }

                MySqlCommand cmd = new MySqlCommand(strQuery, m_connection);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                strError = ex.Message;

                return false;
            }
        }

        // 获取所有的数据表名
        public bool GetAllTableNames(string strDatabaseName, List<string> list_table_names)
        {
            List<string> tableNames = new List<string>();

            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                string sql = "SHOW TABLES";
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tableNames.Add(rdr[0].ToString().Substring(1));
                }
                rdr.Close();

                list_table_names = tableNames;

                return tableNames.Count > 0;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
            }

            return false;
        }

        // 获取指定表中的所有数据，并填充到DataGridView中
        public bool FillDataGridViewWithTableContent(System.Windows.Forms.DataGridView gridview, ref List<int> list_IDs, ref List<string> list_names,
            ref List<double> list_values, string strTableName, string strDatabaseName)
        {
            // 创建用于存储检索结果的List
            List<int> ids = new List<int>();
            List<string> strNames = new List<string>();
            List<DateTime> measureTimes = new List<DateTime>();
            List<double> dbStandardValues = new List<double>();
            List<double> dbUpperValues = new List<double>();
            List<double> dbLowerValues = new List<double>();
            List<double> dbMeasurementValues = new List<double>();

            try
            {
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    return false;
                }

                strTableName = "t" + strTableName;

                string sql = $"SELECT * FROM {strTableName}";
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ids.Add(rdr.GetInt32(0));
                    strNames.Add(rdr.GetString(1));
                    measureTimes.Add(rdr.GetDateTime(2));
                    dbStandardValues.Add(rdr.GetDouble(3));
                    dbUpperValues.Add(rdr.GetDouble(4));
                    dbLowerValues.Add(rdr.GetDouble(5));
                    dbMeasurementValues.Add(rdr.GetDouble(6));

                    list_IDs.Add(rdr.GetInt32(0));
                    list_names.Add(rdr.GetString(1));
                    list_values.Add(rdr.GetDouble(3));
                    list_values.Add(rdr.GetDouble(4));
                    list_values.Add(rdr.GetDouble(5));
                    list_values.Add(rdr.GetDouble(6));
                }
                rdr.Close();

                // 将检索结果填充到DataGridView中
                for (int i = 0; i < ids.Count; i++)
                {
                    gridview.Rows.Add(ids[i], strNames[i], dbStandardValues[i], dbUpperValues[i], dbLowerValues[i], dbMeasurementValues[i], measureTimes[i]);
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

                return false;
            }
        }

        // 获取指定表中的所有数据，并填充到string中，ts为查询耗时
        public bool QueryTableData(string strTableName, string strQuerySQL, ref string strQueryResult, ref TimeSpan ts)
        {
            try
            {
                lock (threadLock)
                {
                // 检查连接有效性
                if (m_connection.State != System.Data.ConnectionState.Open)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(string.Format("数据库尚未连接！"));
                    });

                    return false;
                }

                // 创建一个 Stopwatch 实例
                Stopwatch stopwatch = new Stopwatch();

                // 开始计时
                stopwatch.Start();

                MySqlCommand cmd = new MySqlCommand(strQuerySQL, m_connection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        strQueryResult += rdr[i].ToString() + "@";
                    }
                    strQueryResult += "\n";
                }

                // 停止计时
                stopwatch.Stop();

                // 获取运行时间
                ts = stopwatch.Elapsed;

                rdr.Close();

                return true;
            }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("数据表查询异常："+ex.Message);
                });

                return false;
            }
        }

    }
}
