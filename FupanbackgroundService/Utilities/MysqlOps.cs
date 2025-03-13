using AutoScanFQCTest.DataModels;
using FupanBackgroundService.DataModels;
using FupanBackgroundService.Logics;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static FupanBackgroundService.DataModels.AVIProductInfo;

namespace FupanBackgroundService.Utilities
{
    public class MysqlOps
    {
        private static readonly object threadLock = new object();
        string connectionCertificate = "server=localhost;user=root;port=3306;password=123456;database=AutoScanFQCTest;SslMode=None;allowPublicKeyRetrieval=true;Pooling=true;Min Pool Size=5;Max Pool Size=50;";
        MySqlConnection m_connection = null;

        public MysqlOps()
        {

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
            string connStr = string.Format("server=localhost;user={0};port={1};password={2};database=AutoScanFQCTest;SslMode=None;allowPublicKeyRetrieval=true;Pooling=true;Min Pool Size=5;Max Pool Size=50;", strUser, nPort, strPassword);

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
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        string sql = $"USE {strDatabaseName}";

                        MySqlCommand cmd = new MySqlCommand(sql, m_connection);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(strDatabaseName + "数据库不存在！" + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 判断数据库是否存在
        public bool IsDatabaseExist(string strDatabaseName)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        // 检查数据库是否存在
                        string sql = $"SHOW DATABASES LIKE '{strDatabaseName}'";
                        MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                        if (cmd.ExecuteScalar() == null)
                            return false;
                        else
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("检查到数据库：" + strDatabaseName + "不存在！" + "\n错误信息：" + ex.Message);
                });
                return false;
            }
        }

        // 判断表是否存在
        public bool IsTableExist(string strDatabaseName, string strTableName)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        // Check if table exists
                        string sql = $"SHOW TABLES LIKE '{strTableName}'";
                        MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                        if (cmd.ExecuteScalar() == null)
                            return false;
                        else
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("检查到表：" + strTableName + "不存在！" + "\n错误信息：" + ex.Message);
                });
                return false;
            }
        }

        // 创建数据库
        public bool CreateDatabase(string strDatabaseName)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        // Create database
                        string sql = $"CREATE DATABASE {strDatabaseName}";
                        MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("创建数据库：" + strDatabaseName + "异常！" + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 创建表
        public bool CreateTable(string strDatabaseName, string strTableName, string strSqlCommand)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        if (String.IsNullOrEmpty(strSqlCommand))
                        {
                            return false;
                        }

                        MySqlCommand cmd = new MySqlCommand(strSqlCommand, m_connection);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("创建表：" + strTableName + "异常！" + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 执行查询
        public bool ExecuteQueryWithoutNeedingResult(string strQuery, ref string strError)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        MySqlCommand cmd = new MySqlCommand(strQuery, m_connection);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("执行无结果查询语句异常：" + strQuery + "\n错误信息：" + ex.Message);
                });

                strError = ex.Message;

                return false;
            }
        }

        // 向ProductInfo表添加一条记录
        public bool AddTrayInfoToTable(string strTableName, string strSqlCommand, AVITrayInfo trayInfo)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        using (var command = new MySqlCommand(strSqlCommand, m_connection))
                        {
                            command.Parameters.AddWithValue("@batch_id", trayInfo.BatchId);
                            command.Parameters.AddWithValue("@number_of_rows", trayInfo.Row);
                            command.Parameters.AddWithValue("@number_of_cols", trayInfo.Col);
                            command.Parameters.AddWithValue("@front", trayInfo.Front);
                            command.Parameters.AddWithValue("@mid", trayInfo.Mid);
                            command.Parameters.AddWithValue("@operator", trayInfo.Operator);
                            command.Parameters.AddWithValue("@operator_id", trayInfo.OperatorId);
                            command.Parameters.AddWithValue("@product_id", trayInfo.ProductId);
                            command.Parameters.AddWithValue("@resource", trayInfo.Resource);
                            command.Parameters.AddWithValue("@scan_code_mode", trayInfo.ScanCodeMode);
                            command.Parameters.AddWithValue("@set_id", trayInfo.SetId);
                            command.Parameters.AddWithValue("@total_pcs", trayInfo.TotalPcs);
                            command.Parameters.AddWithValue("@uuid", trayInfo.Uuid);
                            command.Parameters.AddWithValue("@work_area", trayInfo.WorkArea);
                            command.Parameters.AddWithValue("@full_status", trayInfo.Fullstatus);
                            command.Parameters.AddWithValue("@r1", trayInfo.r1);

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
                    if (ex.Message.Contains("Duplicate entry"))
                    {
                        Debugger.Log(0, null, string.Format("222222 该托盘信息已存在数据库中，无法插入！ex.Message = {0}", ex.Message));
                    }
                    else
                    {
                        MessageBox.Show("AddTrayInfoToTable_Nova异常：" + JsonConvert.SerializeObject(trayInfo) + "\n错误信息：" + ex.Message);
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
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
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
                            command.Parameters.AddWithValue("@r1", trayInfo.r1);
                            command.Parameters.AddWithValue("@r2", trayInfo.r2);
                            command.Parameters.AddWithValue("@r3", trayInfo.r3);
                            command.Parameters.AddWithValue("@r4", trayInfo.r4);
                            command.Parameters.AddWithValue("@r5", trayInfo.r5);
                            command.Parameters.AddWithValue("@r6", trayInfo.r6);
                            command.Parameters.AddWithValue("@r7", trayInfo.r7);
                            command.Parameters.AddWithValue("@r8", trayInfo.r8);
                            command.Parameters.AddWithValue("@r9", trayInfo.r9);
                            command.Parameters.AddWithValue("@r10", trayInfo.r10);

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
                    if (ex.Message.Contains("Duplicate entry"))
                    {
                        Debugger.Log(0, null, string.Format("222222 该托盘信息已存在数据库中，无法插入！ex.Message = {0}", ex.Message));
                    }
                    else
                    {
                        MessageBox.Show("AddTrayInfoToTable_Dock异常：" + JsonConvert.SerializeObject(trayInfo) + "\n错误信息：" + ex.Message);
                    }
                });

                return false;
            }
        }

        // 向Defect表添加一条记录
        public bool AddDefectToTable(string strTableName, string strSqlCommand, Defect defect, string strProductID)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
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
                            if (defect.aiDefectCode != null)
                            {
                                foreach (var code in defect.aiDefectCode)
                                {
                                    strDefectCode += code + ";";
                                }
                            }
                            command.Parameters.AddWithValue("@aiDefectCode", strDefectCode);
                            command.Parameters.AddWithValue("@aiGuid", defect.aiGuid);

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
                    MessageBox.Show("AddDefectToTable_Nova异常：" + JsonConvert.SerializeObject(defect) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 向Product表添加一条记录
        public bool AddProductToTable(string strTableName, string strSqlCommand, Product product, string strBatchID)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
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
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("AddProductToTable_Nova异常：" + JsonConvert.SerializeObject(product) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 向复判结果记录表添加一条记录
        public bool AddRecheckResultToTable(string strTableName, string strSqlCommand, Logics.RecheckResult recheckResult)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                        connection.Open();
                        using (var command = new MySqlCommand(strSqlCommand, m_connection))
                        {
                            command.Parameters.AddWithValue("@set_id", recheckResult.set_id);
                            command.Parameters.AddWithValue("@barcode", recheckResult.barcode);
                            command.Parameters.AddWithValue("@recheck_result", recheckResult.recheck_result);
                            command.Parameters.AddWithValue("@r1", recheckResult.r1);
                            command.Parameters.AddWithValue("@r2", recheckResult.r2);
                            command.Parameters.AddWithValue("@r3", recheckResult.r3);
                            command.Parameters.AddWithValue("@r4", recheckResult.r4);
                            command.Parameters.AddWithValue("@r5", recheckResult.r5);
                            command.Parameters.AddWithValue("@r6", recheckResult.r6);
                            command.Parameters.AddWithValue("@r7", recheckResult.r7);
                            command.Parameters.AddWithValue("@r8", recheckResult.r8);
                            command.Parameters.AddWithValue("@r9", recheckResult.r9);
                            command.Parameters.AddWithValue("@r10", recheckResult.r10);

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
                    MessageBox.Show("AddRecheckResultToTable异常：" + JsonConvert.SerializeObject(recheckResult) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 向Product表添加一条记录
        public bool AddProductToTable(string strTableName, string strSqlCommand, ProductInfo product)
        {
            try
            {
                lock (threadLock)
                {
                    using (var connection = new MySqlConnection(connectionCertificate))
                    {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@SetId", product.set_id);
                        command.Parameters.AddWithValue("@MachineId", product.machine_id);
                        command.Parameters.AddWithValue("@BarCode", product.barcode);
                        command.Parameters.AddWithValue("@PosCol", product.column);
                        command.Parameters.AddWithValue("@PosRow", product.row);
                        command.Parameters.AddWithValue("@bET", product.bET);
                        command.Parameters.AddWithValue("@MES_failure_msg", product.MES_failure_msg);
                        command.Parameters.AddWithValue("@is_ok_product", product.is_ok_product);
                        command.Parameters.AddWithValue("@inspect_date_time", product.inspect_date_time);
                        command.Parameters.AddWithValue("@region_area", product.region_area);
                        command.Parameters.AddWithValue("@mac_address", product.mac_address);
                        command.Parameters.AddWithValue("@ip_address", product.ip_address);
                        command.Parameters.AddWithValue("@sideA_image_path", product.sideA_image_path);
                        command.Parameters.AddWithValue("@sideB_image_path", product.sideB_image_path);
                        command.Parameters.AddWithValue("@sideC_image_path", product.sideC_image_path);
                        command.Parameters.AddWithValue("@sideD_image_path", product.sideD_image_path);
                        command.Parameters.AddWithValue("@sideE_image_path", product.sideE_image_path);
                        command.Parameters.AddWithValue("@sideF_image_path", product.sideF_image_path);
                        command.Parameters.AddWithValue("@sideG_image_path", product.sideG_image_path);
                        command.Parameters.AddWithValue("@sideH_image_path", product.sideH_image_path);
                        command.Parameters.AddWithValue("@all_image_paths", product.all_image_paths);
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
                }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 ex.Message {0}", ex.Message));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("AddProductToTable_Dock异常：" + JsonConvert.SerializeObject(product) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        // 向Defect表添加一条记录
        public bool AddDefectToTable(string strTableName, string strSqlCommand, DefectInfo defect)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
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
                            command.Parameters.AddWithValue("image_path", defect.image_path);
                        command.Parameters.AddWithValue("@aiCam", defect.aiCam);
                        command.Parameters.AddWithValue("@aiPos", defect.aiPos);
                        command.Parameters.AddWithValue("@aiImageIndex", defect.aiImageIndex);
                        command.Parameters.AddWithValue("@r1", defect.r1);
                        command.Parameters.AddWithValue("@r2", defect.r2);
                        command.Parameters.AddWithValue("@r3", defect.r3);
                        command.Parameters.AddWithValue("@r4", defect.r4);
                        command.Parameters.AddWithValue("@r5", defect.r5);
                        command.Parameters.AddWithValue("@r6", defect.r6);
                        command.Parameters.AddWithValue("@r7", defect.r7);
                        command.Parameters.AddWithValue("@r8", defect.r8);
                        command.Parameters.AddWithValue("@r9", defect.r9);
                        command.Parameters.AddWithValue("@r10", defect.r10);

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
                    MessageBox.Show("AddDefectToTable_Dock异常：" + JsonConvert.SerializeObject(defect) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        public bool AddSingleSideAIInfoToTable(string strTableName, string strSqlCommand, string side, MesAISingleSideAIInfo aiInfo)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    string deleteQuery = @"DELETE FROM " + strTableName + @" WHERE side = @side AND barcode = @barcode";
                    using (var command = new MySqlCommand(deleteQuery, m_connection))
                    {
                        command.Parameters.AddWithValue("@side", side);
                        command.Parameters.AddWithValue("@barcode", aiInfo.barCode);

                        command.ExecuteNonQuery();
                    }

                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@side", side);
                        command.Parameters.AddWithValue("@machine", aiInfo.machine);
                        command.Parameters.AddWithValue("@uuid", aiInfo.tray);
                        command.Parameters.AddWithValue("@barcode", aiInfo.barCode);
                        command.Parameters.AddWithValue("@aiResult", aiInfo.aiFinalRes);
                        command.Parameters.AddWithValue("@pointInfoJson", JsonConvert.SerializeObject(aiInfo.pointPosInfo));

                        command.ExecuteNonQuery();
                    }
                }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("AddSingleSideAIInfoToTable异常：" + JsonConvert.SerializeObject(aiInfo) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        public bool DeleteSingleSideAIInfoFromTableByBarcode(string strTableName, string strSqlCommand, string side, string barcode)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@side", side);
                        command.Parameters.AddWithValue("@barcode", barcode);

                        command.ExecuteNonQuery();
                    }
                }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("DeleteSingleSideAIInfoFromTableByBarcode异常：" + barcode + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        public bool DeleteSingleSideAIInfoFromTableByUUid(string strTableName, string strSqlCommand, string side, string uuid)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, connection))
                    {
                        command.Parameters.AddWithValue("@side", side);
                        command.Parameters.AddWithValue("@uuid", uuid);

                        command.ExecuteNonQuery();
                    }
                }
                }

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("DeleteSingleSideAIInfoFromTableByUUid异常：" + uuid + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }

        public string GetSingleSideUUidByBarcode(string strTableName, string strSqlCommand, string barcode)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@barcode", barcode);

                        using (var reader = command.ExecuteReader())
                        {
                            var result = "";
                            if (reader.Read())
                            {
                                result = reader["uuid"] as string;
                            }

                            return result;
                        }
                    }
                }
            }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("GetSingleSideUUidByBarcode异常：" + barcode + "\n错误信息：" + ex.Message);
                });

                return "";
            }
        }

        public string GetSingleSideAIInfoByUUid(string strTableName, string strSqlCommand, string uuid)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@uuid", uuid);

                        using (var reader = command.ExecuteReader())
                        {
                            var result = "";
                            while (reader.Read())
                            {
                                result += reader["uuid"] as string + "@" + reader["aiResult"] as string + "@" + reader["pointInfoJson"] as string + "\n";
                            }

                            return result;
                        }
                    }
                }
            }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("GetSingleSideAIInfoByUUid异常：" + uuid + "\n错误信息：" + ex.Message);
                });

                return "";
            }
        }

        public bool InsertMergeInfoInAIDataTable(string strTableName, string strSqlCommand, MesAIMergeAIInfo aiInfo)
        {
            try
            {
                lock (threadLock)
                {
                using (var connection = new MySqlConnection(connectionCertificate))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(strSqlCommand, m_connection))
                    {
                        command.Parameters.AddWithValue("@machine", aiInfo.machine);
                        command.Parameters.AddWithValue("@barcode", aiInfo.dmCode);
                        command.Parameters.AddWithValue("@aiFinalResult", aiInfo.aiFinalRes);

                        command.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("InsertMergeInfoInAIDataTable异常：" + JsonConvert.SerializeObject(aiInfo) + "\n错误信息：" + ex.Message);
                });

                return false;
            }
        }
    }
}
