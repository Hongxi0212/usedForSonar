using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class GeneralSetting : Window
    {
        MainWindow m_parent;

      private Dictionary<string, object> originalSettingRecords;
      private Dictionary<string, object> updatedSettingRecords;

        public GeneralSetting(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

         originalSettingRecords = new Dictionary<string, object>();
         updatedSettingRecords = new Dictionary<string,object>();

            // 厂区所在城市
            if (m_parent.m_global.m_strSiteCity == "苏州")
                combo_SiteCity.SelectedIndex = 0;
            else if (m_parent.m_global.m_strSiteCity == "盐城")
                combo_SiteCity.SelectedIndex = 1;
            else
                combo_SiteCity.SelectedIndex = 0;

         originalSettingRecords.Add(combo_SiteCity.Name, combo_SiteCity.SelectedIndex);

            // 产品大类
            if (m_parent.m_global.m_strProductType == "dock")
                combo_ProductFamily.SelectedIndex = 0;
            else if (m_parent.m_global.m_strProductType == "nova")
                combo_ProductFamily.SelectedIndex = 1;
            else
                combo_ProductFamily.SelectedIndex = 0;

            originalSettingRecords.Add(combo_ProductFamily.Name, combo_ProductFamily.SelectedIndex);

            // 提交MES的产品料号名
            textbox_ProductNameForMES.Text = m_parent.m_global.m_strProductName;
         originalSettingRecords.Add(textbox_ProductNameForMES.Name, textbox_ProductNameForMES.Text);

            // 提交MES的程序名
            textbox_ProgramNameForMES.Text = m_parent.m_global.m_strProgramName;
         originalSettingRecords.Add(textbox_ProgramNameForMES.Name, textbox_ProgramNameForMES.Text);

            // 复判结果MES提交地址url
            textbox_MESUrlForSubmittingRecheckResult.Text = m_parent.m_global.m_strMesDataUploadUrl;
         originalSettingRecords.Add(textbox_MESUrlForSubmittingRecheckResult.Name, textbox_MESUrlForSubmittingRecheckResult.Text);

            // MES校验地址url
            //textbox_MESValidationUrl.Text = m_parent.m_global.m_strMesCheckSampleProductUrl;
            textbox_MESValidationUrl.Text = m_parent.m_global.m_strMesValidationUrl;
         originalSettingRecords.Add(textbox_MESValidationUrl.Name, textbox_MESValidationUrl.Text);
            
            // AI复判地址url
            textbox_AIRecheckUrl.Text = m_parent.m_global.m_strMesAIDataUploadUrl;
         originalSettingRecords.Add(textbox_AIRecheckUrl.Name, textbox_AIRecheckUrl.Text);

            // 保留数据天数
            textbox_DataStorageDuration.Text = m_parent.m_global.m_nDataStorageDuration.ToString();
         originalSettingRecords.Add(textbox_DataStorageDuration.Name, textbox_DataStorageDuration.Text);

            // 登录工号时是否需要检查密码
            CheckBox_NeedToCheckPasswordWhenLogin.IsChecked = m_parent.m_global.m_bNeedToCheckPasswordWhenLogin;
         originalSettingRecords.Add(CheckBox_NeedToCheckPasswordWhenLogin.Name, CheckBox_NeedToCheckPasswordWhenLogin.IsChecked);

            // 读码器通讯本机服务端IP
            textbox_TCP_IP_address_for_communication_with_card_reader.Text = m_parent.m_global.m_strTCP_IP_address_for_communication_with_card_reader;
         originalSettingRecords.Add(textbox_TCP_IP_address_for_communication_with_card_reader.Name, textbox_TCP_IP_address_for_communication_with_card_reader.Text);

            // 读码器通讯本机服务端端口
            textbox_TCP_port_for_communication_with_card_reader.Text = m_parent.m_global.m_nTCP_port_for_communication_with_card_reader.ToString();
         originalSettingRecords.Add(textbox_TCP_port_for_communication_with_card_reader.Name, textbox_TCP_port_for_communication_with_card_reader.Text);

            // AI复判模式
            combo_RecheckModeWithAISystem.SelectedIndex = m_parent.m_global.m_nRecheckModeWithAISystem - 1;
         originalSettingRecords.Add(combo_RecheckModeWithAISystem.Name, combo_RecheckModeWithAISystem.SelectedIndex);

            // OK产品也进行AI复判
            CheckBox_RecheckOKProductWithAISystem.IsChecked = m_parent.m_global.m_bSubmitOKProductsToAI;
         originalSettingRecords.Add(CheckBox_RecheckOKProductWithAISystem.Name, CheckBox_RecheckOKProductWithAISystem.IsChecked);

            // 在复盘站显示中转的AI结果
            CheckBox_ShowAIResultFromTransferStation.IsChecked = m_parent.m_global.m_bShowAIResultFromTransferStation;
         originalSettingRecords.Add(CheckBox_ShowAIResultFromTransferStation.Name, CheckBox_ShowAIResultFromTransferStation.IsChecked);

            // 启用NG二次确认功能
            CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.IsChecked = m_parent.m_global.m_bSecondConfirmNGProductsBeforeSubmittingToMES;
         originalSettingRecords.Add(CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.Name, CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.IsChecked);

            // 提交MES前先检查已经跑过样品板
            CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.IsChecked = m_parent.m_global.m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES;
         originalSettingRecords.Add(CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.Name, CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.IsChecked);

            // 启用样品板卡控功能
            CheckBox_EnableSampleProductControl.IsChecked = m_parent.m_global.m_bEnableSampleProductControl;
         originalSettingRecords.Add(CheckBox_EnableSampleProductControl.Name, CheckBox_EnableSampleProductControl.IsChecked);

            // 启用一次NG数据上传
            CheckBox_EnableOriginalNGDataUploadToMES.IsChecked = m_parent.m_global.m_bEnableOriginalNGDataUploadToMES;
         originalSettingRecords.Add(CheckBox_EnableOriginalNGDataUploadToMES.Name, CheckBox_EnableOriginalNGDataUploadToMES.IsChecked);

            // 扫码查询时对NG产品进行MES校验
            CheckBox_EnableMESValidationForOriginalNGProducts.IsChecked = m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts;
         originalSettingRecords.Add(CheckBox_EnableMESValidationForOriginalNGProducts.Name, CheckBox_EnableMESValidationForOriginalNGProducts.IsChecked);

            // 是否显示不可复判项的图片
            CheckBox_ShowPictureOfUncheckableDefect.IsChecked = m_parent.m_global.m_bShowPictureOfUncheckableDefect;
         originalSettingRecords.Add(CheckBox_ShowPictureOfUncheckableDefect.Name, CheckBox_ShowPictureOfUncheckableDefect.IsChecked);

            // 只捞取有缺陷一面的图片
            CheckBox_RetrieveOnlyImagesOfDefectedSide.IsChecked = m_parent.m_global.m_bRetrieveOnlyImagesOfDefectedSide;
         originalSettingRecords.Add(CheckBox_RetrieveOnlyImagesOfDefectedSide.Name, CheckBox_RetrieveOnlyImagesOfDefectedSide.IsChecked);

            // 启用缺陷名称映射
            CheckBox_EnableDefectNameMapping.IsChecked = m_parent.m_global.m_bEnableDefectNameMapping;
         originalSettingRecords.Add(CheckBox_EnableDefectNameMapping.Name, CheckBox_EnableDefectNameMapping.IsChecked);

            // 如果图片不存在则不允许复判
            CheckBox_DisableRecheckIfImageIsNotExisting.IsChecked = m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting;
         originalSettingRecords.Add(CheckBox_DisableRecheckIfImageIsNotExisting.Name, CheckBox_DisableRecheckIfImageIsNotExisting.IsChecked);

            // 复判数据查询模式
            combo_RecheckDataQueryMode.SelectedIndex = Convert.ToInt32(m_parent.m_global.m_recheck_data_query_mode);
         originalSettingRecords.Add(combo_RecheckDataQueryMode.Name, combo_RecheckDataQueryMode.SelectedIndex);

            CheckBox_TestingMode.IsChecked = m_parent.m_global.m_bIsInTestingMode;
         originalSettingRecords.Add(CheckBox_TestingMode.Name, CheckBox_TestingMode.IsChecked);

            CheckBox_IsCardReaderEnabled.IsChecked = m_parent.m_global.m_bIsCardReaderEnabled;
         originalSettingRecords.Add(CheckBox_IsCardReaderEnabled.Name, CheckBox_IsCardReaderEnabled.IsChecked);

            textbox_RecheckTimeInterval.Text = m_parent.m_global.m_nRecheckInterval.ToString();
         originalSettingRecords.Add(textbox_RecheckTimeInterval.Name, textbox_RecheckTimeInterval.Text);
        }

        // 按钮：确定
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 厂区所在城市
                if (combo_SiteCity.SelectedIndex == 0)
                    m_parent.m_global.m_strSiteCity = "苏州";
                else if (combo_SiteCity.SelectedIndex == 1)
                    m_parent.m_global.m_strSiteCity = "盐城";
                else
                    m_parent.m_global.m_strSiteCity = "苏州";
            updatedSettingRecords.Add(combo_SiteCity.Name, combo_SiteCity.SelectedIndex);

                // 产品大类
                if (combo_ProductFamily.SelectedIndex == 0)
                    m_parent.m_global.m_strProductType = "dock";
                else if (combo_ProductFamily.SelectedIndex == 1)
                    m_parent.m_global.m_strProductType = "nova";
                else
                    m_parent.m_global.m_strProductType = "dock";
				updatedSettingRecords.Add(combo_ProductFamily.Name, combo_ProductFamily.SelectedIndex);

                // 提交MES的产品料号名
                m_parent.m_global.m_strProductName = textbox_ProductNameForMES.Text;
				updatedSettingRecords.Add(textbox_ProductNameForMES.Name, textbox_ProductNameForMES.Text);

                // 提交MES的程序名
                m_parent.m_global.m_strProgramName = textbox_ProgramNameForMES.Text;
				updatedSettingRecords.Add(textbox_ProgramNameForMES.Name, textbox_ProgramNameForMES.Text);

                // 复判结果MES提交地址url
                m_parent.m_global.m_strMesDataUploadUrl = textbox_MESUrlForSubmittingRecheckResult.Text;
				updatedSettingRecords.Add(textbox_MESUrlForSubmittingRecheckResult.Name, textbox_MESUrlForSubmittingRecheckResult.Text);

                // MES校验地址url
                //m_parent.m_global.m_strMesCheckSampleProductUrl = textbox_MESValidationUrl.Text;
                m_parent.m_global.m_strMesValidationUrl = textbox_MESValidationUrl.Text;
				updatedSettingRecords.Add(textbox_MESValidationUrl.Name, textbox_MESValidationUrl.Text);

                // AI复判地址url
                m_parent.m_global.m_strMesAIDataUploadUrl = textbox_AIRecheckUrl.Text;
				updatedSettingRecords.Add(textbox_AIRecheckUrl.Name, textbox_AIRecheckUrl.Text);

                // 保留数据天数
                m_parent.m_global.m_nDataStorageDuration = Convert.ToInt32(textbox_DataStorageDuration.Text);
				updatedSettingRecords.Add(textbox_DataStorageDuration.Name, textbox_DataStorageDuration.Text);

                // 登录工号时是否需要检查密码
                m_parent.m_global.m_bNeedToCheckPasswordWhenLogin = (bool)CheckBox_NeedToCheckPasswordWhenLogin.IsChecked;
				updatedSettingRecords.Add(CheckBox_NeedToCheckPasswordWhenLogin.Name, CheckBox_NeedToCheckPasswordWhenLogin.IsChecked);

                // 读码器通讯本机服务端IP
                m_parent.m_global.m_strTCP_IP_address_for_communication_with_card_reader = textbox_TCP_IP_address_for_communication_with_card_reader.Text;
				updatedSettingRecords.Add(textbox_TCP_IP_address_for_communication_with_card_reader.Name, textbox_TCP_IP_address_for_communication_with_card_reader.Text);

                // 读码器通讯本机服务端端口
                m_parent.m_global.m_nTCP_port_for_communication_with_card_reader = Convert.ToInt32(textbox_TCP_port_for_communication_with_card_reader.Text);
				updatedSettingRecords.Add(textbox_TCP_port_for_communication_with_card_reader.Name, textbox_TCP_port_for_communication_with_card_reader.Text);

                // AI复判模式
                m_parent.m_global.m_nRecheckModeWithAISystem = combo_RecheckModeWithAISystem.SelectedIndex + 1;
				updatedSettingRecords.Add(combo_RecheckModeWithAISystem.Name, combo_RecheckModeWithAISystem.SelectedIndex);

                // OK产品也进行AI复判
                m_parent.m_global.m_bSubmitOKProductsToAI = (bool)CheckBox_RecheckOKProductWithAISystem.IsChecked;
				updatedSettingRecords.Add(CheckBox_RecheckOKProductWithAISystem.Name, CheckBox_RecheckOKProductWithAISystem.IsChecked);

                // 在复盘站显示中转的AI结果
                m_parent.m_global.m_bShowAIResultFromTransferStation = (bool)CheckBox_ShowAIResultFromTransferStation.IsChecked;
				updatedSettingRecords.Add(CheckBox_ShowAIResultFromTransferStation.Name, CheckBox_ShowAIResultFromTransferStation.IsChecked);

                // 启用NG二次确认功能
                m_parent.m_global.m_bSecondConfirmNGProductsBeforeSubmittingToMES = (bool)CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.IsChecked;
				updatedSettingRecords.Add(CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.Name, CheckBox_SecondConfirmNGProductsBeforeSubmittingToMES.IsChecked);

                // 提交MES前先检查已经跑过样品板
                m_parent.m_global.m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES = (bool)CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.IsChecked;
				updatedSettingRecords.Add(CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.Name, CheckBox_CheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.IsChecked);

                // 启用样品板卡控功能
                m_parent.m_global.m_bEnableSampleProductControl = (bool)CheckBox_EnableSampleProductControl.IsChecked;
				updatedSettingRecords.Add(CheckBox_EnableSampleProductControl.Name, CheckBox_EnableSampleProductControl.IsChecked);

                // 启用一次NG数据上传
                m_parent.m_global.m_bEnableOriginalNGDataUploadToMES = (bool)CheckBox_EnableOriginalNGDataUploadToMES.IsChecked;
				updatedSettingRecords.Add(CheckBox_EnableOriginalNGDataUploadToMES.Name, CheckBox_EnableOriginalNGDataUploadToMES.IsChecked);

                // 扫码查询时对NG产品进行MES校验
                m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts = (bool)CheckBox_EnableMESValidationForOriginalNGProducts.IsChecked;
				updatedSettingRecords.Add(CheckBox_EnableMESValidationForOriginalNGProducts.Name, CheckBox_EnableMESValidationForOriginalNGProducts.IsChecked);

                // 是否显示不可复判项的图片
                m_parent.m_global.m_bShowPictureOfUncheckableDefect = (bool)CheckBox_ShowPictureOfUncheckableDefect.IsChecked;
				updatedSettingRecords.Add(CheckBox_ShowPictureOfUncheckableDefect.Name, CheckBox_ShowPictureOfUncheckableDefect.IsChecked);

                // 只捞取有缺陷一面的图片
                m_parent.m_global.m_bRetrieveOnlyImagesOfDefectedSide = (bool)CheckBox_RetrieveOnlyImagesOfDefectedSide.IsChecked;
				updatedSettingRecords.Add(CheckBox_RetrieveOnlyImagesOfDefectedSide.Name, CheckBox_RetrieveOnlyImagesOfDefectedSide.IsChecked);

                // 启用缺陷名称映射
                m_parent.m_global.m_bEnableDefectNameMapping = (bool)CheckBox_EnableDefectNameMapping.IsChecked;
				updatedSettingRecords.Add(CheckBox_EnableDefectNameMapping.Name, CheckBox_EnableDefectNameMapping.IsChecked);

                // 如果图片不存在则不允许复判
                m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting = (bool)CheckBox_DisableRecheckIfImageIsNotExisting.IsChecked;
				updatedSettingRecords.Add(CheckBox_DisableRecheckIfImageIsNotExisting.Name, CheckBox_DisableRecheckIfImageIsNotExisting.IsChecked);

                // 复判数据查询模式
                m_parent.m_global.m_recheck_data_query_mode = (RecheckDataQueryMode)combo_RecheckDataQueryMode.SelectedIndex;
				updatedSettingRecords.Add(combo_RecheckDataQueryMode.Name, combo_RecheckDataQueryMode.SelectedIndex);

                m_parent.m_global.m_bIsInTestingMode = (bool)CheckBox_TestingMode.IsChecked;
				updatedSettingRecords.Add(CheckBox_TestingMode.Name, CheckBox_TestingMode.IsChecked);

                m_parent.m_global.m_bIsCardReaderEnabled = (bool)CheckBox_IsCardReaderEnabled.IsChecked;
				updatedSettingRecords.Add(CheckBox_IsCardReaderEnabled.Name, CheckBox_IsCardReaderEnabled.IsChecked);

                m_parent.m_global.m_nRecheckInterval = Convert.ToInt32(textbox_RecheckTimeInterval.Text);
				updatedSettingRecords.Add(textbox_RecheckTimeInterval.Name, textbox_RecheckTimeInterval.Text);

            // 检查打开窗口时保存的settingRecords中，键值是否被更改
            var keyWithDifferentValues = originalSettingRecords
               .Where(kvp => updatedSettingRecords.ContainsKey(kvp.Key)
            && !Equals(kvp.Value, updatedSettingRecords[kvp.Key]))
               .ToDictionary(kvp => kvp.Key, kvp => new { originalValue = originalSettingRecords[ kvp.Key], updatedValue = updatedSettingRecords[kvp.Key] });

            var logInfo = "设置内容发生变动：";
            foreach(var kvp in keyWithDifferentValues) {
               logInfo += "\n设置项目：" + kvp.Key + "，原设置：" + kvp.Value.originalValue + "，新设置：" + kvp.Value.updatedValue;
            }
            logInfo += "\n设置变动时工号：" + m_parent.m_global.m_strCurrentOperatorID + "，变动时间：" + DateTime.Now.ToString();
            //m_parent.m_global.m_log_presenter.Log(logInfo);
            m_parent.m_global.m_log_presenter.SaveLogToFile(DateTime.Now.ToLongTimeString().ToString(), logInfo, true);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的数字！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 保存到配置文件
            m_parent.m_global.SaveConfigData("config.ini");

            // 弹出提示框
            MessageBox.Show("参数保存成功，软件自动关闭，重启软件后生效。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            // 提示保存成功，但需要强制退出程序，重启后才能生效
            MessageBox.Show(this, "保存成功，但需要重启软件，方能生效。\r\n请重启软件。", "提示", MessageBoxButton.OK);

            this.Close();
            m_parent.Close();
            Environment.Exit(0);
        }

        // 按钮：取消
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 复选框：OK产品也进行AI复判
        private void CheckBox_RecheckOKProductWithAISystem_Checked(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_bSubmitOKProductsToAI = true;
        }

        // 复选框：OK产品也进行AI复判
        private void CheckBox_RecheckOKProductWithAISystem_Unchecked(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_bSubmitOKProductsToAI = false;
        }
    }
}
