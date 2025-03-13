using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonFrame
{
    internal class YW605Reader
    {
        internal const byte SEARCHMODE_14443A = (byte)0x41;
        internal const byte SEARCHMODE_14443B = (byte)0x42;
        internal const byte SEARCHMODE_15693 = (byte)0x31;

        internal const byte REQUESTMODE_ALL = (byte)0x52;
        internal const byte REQUESTMODE_ACTIVE = (byte)0x26;

        internal const byte SAM_BOUND_9600 = (byte)0;
        internal const byte SAM_BOUND_38400 = (byte)1;

        internal const byte PASSWORD_A = (byte)0x60;
        internal const byte PASSWORD_B = (byte)0x61;

        //*******************************************DLL相关函数 ************************/


        /*    函数： YW_GetDLLVersion
         *    名称： 返回当前DLL的版本
         *    参数：无
         *  返回值：版本号
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_GetDLLVersion();


        /*    函数： DES
         *    名称： DES加解密函数
         *    参数：cModel： 加密或者解密 ， 0加密，1解密，对应常数ENCRYPT =0，DECRYPT = 1
                      pkey：加解密秘钥指针，8个字节
                    inData：要加解密的数据指针，8个字节
                    OutData: 经过加解密后的数据指针，8个字节
         *  返回值：无意义
        */
        [DllImport("YW60x.dll")]
        internal static extern int DES(byte cModel, byte[] pkey, byte[] inData, byte[] outData);


        /*    函数： DES3
         *    名称： 3DES加解密函数
         *    参数：cModel： 加密或者解密 ， 0加密，1解密，对应常数ENCRYPT =0，DECRYPT = 1
                      pkey：加解密秘钥指针，16个字节
                    inData：要加解密的数据指针，8个字节
                    OutData: 经过加解密后的数据指针，8个字节
         *  返回值：无意义
        */
        [DllImport("YW60x.dll")]
        internal static extern int DES3(byte cModel, byte[] pKey, byte[] pInData, byte[] pOutData);

        /*    函数： DES3_CBC
         *    名称： 带向量的3DES加解密函数
         *    参数：cModel： 加密或者解密 ， 0加密，1解密，对应常数ENCRYPT =0，DECRYPT = 1
                     pkey：加解密秘钥指针，8个字节
                     inData：要加解密的数据指针，8个字节
                     OutData: 经过加解密后的数据指针，8个字节
                     pIV:    向量指针，8个字节

         *  返回值：无意义
        */
        [DllImport("YW60x.dll")]
        internal static extern int DES3_CBC(byte cModel, byte[] pKey, byte[] pInData, byte[] pOutData, byte[] pIV);


        //*******************************************读写器相关函数 ************************/


        /*    函数： YW_ComInitial
         *    名称： 串口初始化函数
         *    参数：PortIndex： 串口号
                     Bound：通信波特率
         *  返回值：无意义
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ComInitial(int PortIndex, int Bound);

        /*    函数： YW_ComFree
         *    名称： 串口释放函数
         *    参数： 无
         *  返回值：>0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ComFree();


        /*    函数： YW_USBHIDInitial
         *    名称： 免驱USB端口初始化
         *    参数： 无
         *  返回值：>0为成功 ，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_USBHIDInitial();

        /*    函数： YW_USBHIDInitial
         *    名称： 免驱USB端口释放
         *    参数： 无
         *  返回值：>0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_USBHIDFree();


        /*    函数： YW_ComNewBound
         *    名称： 更改串口波特率
         *    参数： NewBound， 新的波特率
         *  返回值：>0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ComNewBound(int ReaderID, int NewBound);


        /*    函数： YW_SetReaderID
         *    名称： 设置读写器ID
         *    参数： OldID：读写器老的ID
                     NewID：读写器新的ID
         *  返回值：>0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SetReaderID(int OldID, int NewID);


        /*    函数： YW_GetReaderID
         *    名称： 读取读写器ID
         *    参数： ReaderID：读写器ID号，0为广播地址
         *  返回值：>=0为成功，并且为读写器ID ，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_GetReaderID(int ReaderID);

        /*    函数： YW_GetReaderVersion
         *    名称： 获取读写器的版本
         *    参数： ReaderID：读写器ID号，0为广播地址
         *  返回值：>=0为成功，并且为读写器版本 ，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_GetReaderVersion(int ReaderID);

        /*    函数： YW_GetReaderSerial
         *    名称： 获取读写器的序列号
         *    参数：     ReaderID：读写器ID号，0为广播地址
                     ReaderSerial：输出为读写器的序列号，8个字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_GetReaderSerial(int ReaderID, byte[] ReaderSerial);


        /*    函数： YW_GetReaderNo
         *    名称： 获取读写器的型号
         *    参数： ReaderID：读写器ID号，0为广播地址
                      ReadeNo：输出为读写器的型号，8个字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_GetReaderNo(int ReaderID, byte[] ReadeNo);



        /*    函数： YW_Buzzer
         *    名称： 蜂鸣器操作函数
         *    参数： ReaderID：读写器ID号，0为广播地址
                      Time_ON：蜂鸣器响时间，单位0.1s
                     Time_OFF：蜂鸣器不响时间，单位0.1s
                        Cycle：  蜂鸣器循环次数
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Buzzer(int ReaderID, int Time_ON, int Time_OFF, int Cycle);   //5



        /*    函数： YW_Led
         *    名称： LED灯操作函数
         *    参数： ReaderID：读写器ID号，0为广播地址
                     LEDIndex：选择要操作的LED灯
                      Time_ON： 灯亮的时间，单位0.1s
                     Time_OFF：灯不亮时间，单位0.1s
                        Cycle：   循环次数
                     LedIndexOn：最后要亮的灯
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Led(int ReaderID, int LEDIndex, int Time_ON, int Time_OFF, int Cycle, int LedIndexOn);    //6


        /*    函数： YW_LEDDisplay
         *    名称： LED显示器操作函数
         *    参数：  ReaderID：读写器ID号，0为广播地址
                     Alignment: 显示对其方式，01为左对齐，02为居中，03为右对齐
                       LEDText：  要显示的字符串
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_LEDDisplay(int ReaderID, int Alignment, byte[] LEDText);



        /*    函数： YW_AntennaStatus
         *    名称： 开启天线，在所有卡操作之前必须开启天线
         *    参数：  ReaderID：读写器ID号，0为广播地址
                       Status: true为开启天线， false为关闭天线
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AntennaStatus(int ReaderID, bool Status);


        /*    函数： YW_SearchCardMode
         *    名称： 寻卡模式设置
         *    参数：  ReaderID：读写器ID号，0为广播地址
                          Mode: 寻卡模式
                                SEARCHMODE_14443A       =    Byte(Char('A'));
                                SEARCHMODE_14443B       =    Byte(Char('B'));
                                SEARCHMODE_15693        =    Byte(Char('1'));
                                SEARCHMODE_ST           =    Byte(Char('S'));
                                SEARCHMODE_AT88RF020    =    Byte(Char('R'));
                                详细参见说明书
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SearchCardMode(int ReaderID, int Mode);


        //*******************************************ISO14443A卡片操作函数 ************************/



        /*    函数： YW_RequestCard
         *    名称： 寻卡TypeA卡
         *    参数：  ReaderID：读写器ID号，0为广播地址
                   RequestMode: 寻卡模式
                                所有卡  常数 REQUESTMODE_ALL=$52;
                                激活的卡 常数 REQUESTMODE_ACTIVE=$26;
                   CardType：输出卡类型
                              0x4400 -> Ultralight/UltraLight C /MifarePlus(7Byte UID)
                              0x4200 -> MifarePlus(7Byte UID)
                              0x0400 ->Mifare Mini/Mifare 1K (S50) /MifarePlus(4Byte UID)
                              0x0200->Mifare_4K(S70)/ MifarePlus(4Byte UID)
                              0x4403 ->Mifare_DESFire
                              0x0800 ->Mifare_Pro
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_RequestCard(int ReaderID, byte RequestMode, ref short CardType);


        /*    函数： YW_AntiCollide
         *    名称： 访冲突操作
         *    参数：  ReaderID：读写器ID号，0为广播地址
                   LenSNO:   输出卡号的长度
                      SNO：  输出卡号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AntiCollide(int ReaderID, ref byte LenSNO, byte[] SNO);



        /*    函数： YW_CardSelect
         *    名称： 选卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        LenSNO:   要选择卡卡号的长度
                           SNO：  要选择卡卡号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_CardSelect(int ReaderID, byte LenSNO, byte[] SNO);


        /*    函数： YW_KeyAuthorization
         *    名称： M1卡授权
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        KeyMode:  秘钥选择Key A或者Key B
                                   常数  PASSWORD_A           =    $60;
                                   常数  PASSWORD_B           =    $61;
                      BlockAddr：  要授权的块
                           Key：  秘钥字节指针，6字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_KeyAuthorization(int ReaderID, int KeyMode, int BlockAddr, byte[] Key);


        /*    函数： YW_ReadaBlock
         *    名称： 读取M1卡一个块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要读取的块号
                      LenData：  要读取的字节数，最大为16
                         Data：  数据指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ReadaBlock(int ReaderID, int BlockAddr, int LenData, byte[] pData);


        /*    函数： YW_WriteaBlock
         *    名称： 写入M1卡一个块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要写入的块号
                      LenData：  要读取的字节数，必须为16
                         Data：  数据指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_WriteaBlock(int ReaderID, int BlockAddr, int LenData, byte[] pData);


        /*    函数： YW_Purse_Initial
         *    名称： M1卡将某一块初始化钱包
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要初始化钱包的块号
                      IniValue：  钱包初始化值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Purse_Initial(int ReaderID, int BlockAddr, int IniMoney);


        /*    函数： YW_Purse_Read
         *    名称： 读取M1卡某个块的钱包值
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要初始化钱包的块号
                        Value：  钱包的当前值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Purse_Read(int ReaderID, int BlockAddr, ref int Money);  //16


        /*    函数： YW_Purse_Decrease
         *    名称： 对钱包进行减值操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要初始化钱包的块号
                     Decrement：  要减去的值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Purse_Decrease(int ReaderID, int BlockAddr, int Decrement);  //17


        /*    函数： YW_Purse_Decrease
         *    名称： 对钱包进行加值操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   要初始化钱包的块号
                     Charge：    要增加的值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Purse_Charge(int ReaderID, int BlockAddr, int Charge);  //18


        /*    函数： YW_Purse_Decrease
         *    名称： 对钱包进行Restor操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   钱包的块号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Restore(int ReaderID, int BlockAddr);


        /*    函数： YW_Purse_Decrease
         *    名称： 对钱包进行Transfer操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     BlockAddr:   钱包的块号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_Transfer(int ReaderID, int BlockAddr);



        /*    函数： YW_DownLoadKey
         *    名称： 下载秘钥到读写器中
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     KeyIndex:   秘钥的序号，从0-31
                         Key ：  秘钥指针，6个字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_DownLoadKey(int ReaderID, int KeyIndex, byte[] Key);


        /*    函数： YW_KeyDown_Authorization
         *    名称： 用读写器中的秘钥进行授权
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        KeyMode:  秘钥选择Key A或者Key B
                                   常数  PASSWORD_A           =    $60;
                                   常数  PASSWORD_B           =    $61;
                      BlockAddr：  要授权的块号
                       KeyIndex:   秘钥的序号，从0-31
         *  返回值：>=0为成功，其它失败

         */
        [DllImport("YW60x.dll")]
        internal static extern int YW_KeyDown_Authorization(int ReaderID, char KeyMode, int BlockAddr, byte KeyIndex);


        /*    函数： YW_CardHalt
         *    名称： 对M1卡进行Halt操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_CardHalt(int ReaderID);


        /*    函数： YW_AntiCollide_Level
         *    名称： 对M1卡进行n级防碰撞
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      Leveln：n级防碰撞，从1到3
                      LenSNO：卡号的长度
                        SNO：卡号指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AntiCollide_Level(int ReaderID, int Leveln, ref byte LenSNO, byte[] SNO);



        /*    函数： YW_SelectCard_Level
         *    名称： 对M1卡进行n级选卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      Leveln：n级防碰撞，从1到3
                         SAK：输出SAK
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SelectCard_Level(int ReaderID, int Leveln, byte[] SAK);


        /*    函数： YW_AntiCollideAndSelect
         *    名称： 对M1卡进行防碰撞并选卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                 MultiCardMode：对多张卡的处理方式
                                 00  返回多卡错误
                                 01  返回一张卡片
                       CardMem：返回卡的内存代码
                        SNOLen: 输出卡号的长度
                           SNO：卡的序列号        
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AntiCollideAndSelect(int ReaderID, byte MultiCardMode, ref byte CardMem, ref int SNLen, byte[] SN);


        /*    函数： YW_RequestAntiandSelect
         *    名称： 对M1卡寻卡，防碰撞并选卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                   RequestMode：寻卡模式
                                所有卡  常数 REQUESTMODE_ALL=$52;
                                激活的卡 常数 REQUESTMODE_ACTIVE=$26;
                 MultiCardMode：对多张卡的处理方式
                                 00  返回多卡错误
                                 01  返回一张卡片
                         ATQA ： ATQA
                          SAK ：SAK
                        SNOLen: 输出卡号的长度
                           SNO：卡的序列号        
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_RequestAntiandSelect(int ReaderID, int SearchMode, int MultiCardMode, ref short ATQA, byte[] SAK, ref byte LenSNO, byte[] SNO);


        /*    函数： YW_WriteM1MultiBlock
         *    名称： 对M1卡写多块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      StartBlock：开始块号
                      BlockNums： 要写得块数量
                        LenData： 要写得数据长度，16的倍数
                         pData：  要写得数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_WriteM1MultiBlock(int ReaderID, int StartBlock, int BlockNums, int LenData, byte[] pData);


        /*    函数： YW_ReadM1MultiBlock
         *    名称： 对M1卡读取多块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      StartBlock：开始块号
                      BlockNums： 要读取的块数量
                        LenData： 返回读取的数据长度
                         pData：  返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ReadM1MultiBlock(int ReaderID, int StartBlock, int BlockNums, ref int LenData, byte[] pData);


        //*******************************************UltraLight卡片操作函数 ************************/


        /*    函数： YW_UltraLightRead
         *    名称： Ultra Light 卡读取块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      BlockID ： 读取的块号
                         pData：  返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_UltraLightRead(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_UltraLightWrite
         *    名称： Ultra Light 卡写块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      BlockID ： 要写的块号
                         pData：  写入的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_UltraLightWrite(int ReaderID, int BlockID, byte[] pData);


        //*******************************************Type A CPU 卡片操作函数 ************************/


        /*    函数： YW_TypeA_Reset
         *    名称： Type A CPU卡复位
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                   RequestMode：寻卡模式
                                所有卡  常数 REQUESTMODE_ALL=$52;
                                激活的卡 常数 REQUESTMODE_ACTIVE=$26;
                 MultiCardMode：对多张卡的处理方式
                                 00  返回多卡错误
                                 01  返回一张卡片
                         rtLen: 复位返回数据的长度        
                         pData： 复位返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_TypeA_Reset(int ReaderID, byte Mode, byte MultiMode, ref int rtLen, byte[] pData);


        /*    函数： YW_TypeA_COS
         *    名称： Type A CPU卡执行COS命令
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        LenCOS：  COS命令的长度
                       Com_COS：  COS命令
                         rtLen:   执行COS后返回的数据长度
                         pData：  执行COS后返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_TypeA_COS(int ReaderID, int LenCOS, byte[] Com_COS, ref int rtLen, byte[] pData);


        //*******************************************Mifare Plus 卡片操作函数 ************************/


        /*    函数： YW_MFP_L0_WritePerso
         *    名称： Level 0 级写数据
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        Address： 要写数据的地址
                         pData：  要写入的数据，16字节
         *  返回值：>=0为成功，其它失败
        */

        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L0_WritePerso(int ReaderID, int Address, byte[] pData);


        /*    函数： YW_MFP_L0_CommitPerso
         *    名称： Level 0 级向Level 1 或者Level 3级切换
         *    参数：  ReaderID：  读写器ID号，0为广播地址
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L0_CommitPerso(int ReaderID);


        /*    函数： YW_MFP_SwitchToLevel
         *    名称：  向Level 2 或者Level 3级切换
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      DesLevel：  切换的目的层级
                     SwitchKey：  切换秘钥，16字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_SwitchToLevel(int ReaderID, int DesLevel, byte[] SwitchKey);


        /*    函数： YW_MFP_L3_Authorization
         *    名称： Mifare Plus 卡 Level 3级授权
         *    参数：  ReaderID：   读写器ID号，0为广播地址
                        KeyMode:   秘钥选择Key A或者Key B
                                   常数  PASSWORD_A           =    $60;
                                   常数  PASSWORD_B           =    $61;
                      BlockAddr：  要授权的块
                           Key：   秘钥字节指针，16字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Authorization(int ReaderID, int KeyMode, int BlockID, byte[] Key);


        /*    函数： YW_MFP_L3_Read
         *    名称： 读取Mifare Plus卡Level 3级数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     StartBlock:  要读取的开始块号
                      BlockNums： 要读取的块数量
                      DataLen：  读取数据返回的数据长度
                         pData：  数据指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Read(int ReaderID, int StartBlock, int BlockNums, ref int DataLen, byte[] pData);


        /*    函数： YW_MFP_L3_Write
         *    名称： 写Mifare Plus卡Level 3级数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     StartBlock:  要写入的开始块号
                      BlockNums： 要写入的块数量
                         pData：  数据指针，长度必须是16*BlockNums
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Write(int ReaderID, int StartBlock, int BlockNums, byte[] pData);


        /*    函数： YW_MFP_L3_Purse_Initial
         *    名称： 初始化Mifare Plus卡Level 3级的一个块为钱包
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   块号
                  InitialValue：  初始化钱包值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Purse_Initial(int ReaderID, int BlockID, int InitialValue);


        /*    函数： YW_MFP_L3_Purse_Read
         *    名称： 读取Mifare Plus卡Level 3级钱包的值
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   块号
                         Value：  读取的钱包值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Purse_Read(int ReaderID, int BlockID, ref int Value);


        /*    函数： YW_MFP_L3_Purse_Charge
         *    名称： 对Mifare Plus卡Level 3级钱包进行加值操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   块号
                         Value：  加的值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Purse_Charge(int ReaderID, int BlockID, int Value);

        /*    函数： YW_MFP_L3_Purse_Decrease
         *    名称： 对Mifare Plus卡Level 3级钱包进行减值操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   块号
                         Value：  减的值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Purse_Decrease(int ReaderID, int BlockID, int Value);


        /*    函数： YW_MFP_L3_Purse_Backup
         *    名称： 对Mifare Plus卡Level 3级钱包进行备份
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   块号
                    DesBlockID：  目标块号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_L3_Purse_Backup(int ReaderID, int BlockID, int DesBlockID);


        /*    函数： YW_MFP_Authorization_First
         *    名称： 对Mifare Plus卡Level 3级读写进行第一次授权
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                    AESKeyAddr:   授权地址
                        AESKey：  授权秘钥，16字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_Authorization_First(int ReaderID, int AESKeyAddr, byte[] AESKey);


        /*    函数： YW_MFP_Authorization_Follow
         *    名称： 对Mifare Plus卡Level 3级读写进行第二次授权
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                    AESKeyAddr:   授权地址
                        AESKey：  授权秘钥，16字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_Authorization_Follow(int ReaderID, int AESKeyAddr, byte[] AESKey);


        /*    函数： YW_MFP_CommonRead
         *    名称： 对Mifare Plus卡Level 3级通用读
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   数据块地址
                     BlockNums：  块数量
                       DataLen：  读出的数据长度
                         pData：  读出的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_CommonRead(int ReaderID, int BlockID, int BlockNums, ref int DataLen, byte[] pData);


        /*    函数： YW_MFP_CommonWrite
         *    名称： 对Mifare Plus卡Level 3级通用写
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID:   数据块地址
                     BlockNums：  块数量
                         pData：  要写入的数据，长度为16* BlockNums
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_MFP_CommonWrite(int ReaderID, int BlockID, int BlockNums, byte[] pData);




        //*******************************************ISO14443 Type B CPU卡片操作函数************************/


        /*    函数： YW_TypeB_Reset
         *    名称： Type B CPU卡复位
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                   RequestMode：寻卡模式
                                所有卡  常数 REQUESTMODE_ALL=$52;
                                激活的卡 常数 REQUESTMODE_ACTIVE=$26;
                         rtLen: 复位返回数据的长度
                         pData： 复位返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_TypeB_Reset(int ReaderID, byte Mode, ref int rtLen, byte[] pData);


        /*    函数： YW_TypeB_COS
         *    名称： Type B CPU卡执行COS命令
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                        LenCOS：  COS命令的长度
                       Com_COS：  COS命令
                       DataLen:   执行COS后返回的数据长度
                         pData：  执行COS后返回的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_TypeB_COS(int ReaderID, int LenCOS, byte[] Com_COS, ref int DataLen, byte[] pData);

        //*******************************************ISO14443 Type B AT88RF020 卡片操作函数************************/


        /*    函数： YW_AT88RF020_Check
         *    名称： AT88RF020卡秘钥认证
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                           Key：  认证秘钥，8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_Check(int ReaderID, byte[] Key);


        /*    函数： YW_AT88RF020_Read
         *    名称： AT88RF020卡读数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，8字节
         *  返回值：>=0为成功，其它失败
        */

        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_Read(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_AT88RF020_Write
         *    名称： AT88RF020卡写数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_Write(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_AT88RF020_Lock
         *    名称： AT88RF020卡锁数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      LockByte：  锁数据，4字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_Lock(int ReaderID, byte[] LockByte);


        /*    函数： YW_AT88RF020_Count
         *    名称： AT88RF020卡计数
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     Signature：  签名，6字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_Count(int ReaderID, byte[] Signature);


        /*    函数： YW_AT88RF020_DeSel
         *    名称： AT88RF020卡不选择
         *    参数：  ReaderID：  读写器ID号，0为广播地址
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_DeSel(int ReaderID);


        /*    函数： YW_AT88RF020_ReadMulti
         *    名称： AT88RF020卡读多块数据
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      BlockID：   开始块号
                    BlockNums：   块数量
                     LenData：   返回的数据的长度
                       pData：   返回的数据指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_ReadMulti(int ReaderID, int BlockID, int BlockNums, ref int LenData, byte[] pData);


        /*    函数： YW_AT88RF020_WriteMulti
         *    名称： AT88RF020卡读多块数据
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      BlockID：   开始块号
                    BlockNums：   块数量
                     LenData：   要写入数据的长度，数据长度为BlockNums*8
                       pData：   要写入的数据指针
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_AT88RF020_WriteMulti(int ReaderID, int BlockID, int BlockNums, int LenData, byte[] pData);


        //*******************************************ISO14443 Type B ST SR176，SR512，SRIX4K 卡片操作函数************************/


        /*    函数： YW_ST_Active
         *    名称： 激活ST系列卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      ChipID：   ST卡的ChipID
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ST_Active(int ReaderID, ref byte ChipID);


        /*    函数： YW_ST_DeActive
         *    名称： 使激活ST系列卡休眠
         *    参数：  ReaderID：  读写器ID号，0为广播地址
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ST_DeActive(int ReaderID);


        /*    函数： YW_SR176_GetUID
         *    名称： SR176卡获得UID
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                           UID：   SR176卡UID，8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_GetUID(int ReaderID, byte[] UID);


        /*    函数： YW_SR176_Read
         *    名称： SR176卡读块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，2字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_Read(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_SR176_Write
         *    名称： SR176卡写块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，2字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_Write(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_SR176_Lock
         *    名称： SR176卡锁数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      LockByte：  锁字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_Lock(int ReaderID, byte LockByte);


        /*    函数： YW_SR176_LockStatus
         *    名称： SR176卡读取锁状态
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      LockByte：  锁字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_LockStatus(int ReaderID, byte[] LockByte);


        /*    函数： YW_SR176_ReadMulti
         *    名称： SR176卡写块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                     BlockNums:   块数量
                      LenData：   返回读取的数据长度
                         pData：  数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_ReadMulti(int ReaderID, int BlockID, int BlockNums, ref int LenData, byte[] pData);


        /*    函数： YW_SR176_WriteMulti
         *    名称： SR176卡写块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                     BlockNums:   块数量
                      LenData：   要写入的数据长度
                         pData：  数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR176_WriteMulti(int ReaderID, int BlockID, int BlockNums, int LenData, byte[] pData);


        /*    函数： YW_SR512_GetUID
         *    名称： SR512卡获取UID
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                           UID：  卡的UID，8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_GetUID(int ReaderID, byte[] UID);


        /*    函数： YW_SR512_Read
         *    名称： SR512卡读数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，4字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_Read(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_SR512_Write
         *    名称： SR512卡写数据块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                         pData：  数据，4字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_Write(int ReaderID, int BlockID, byte[] pData);


        /*    函数： YW_SR512_Lock
         *    名称： SR512卡锁块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       LockByte：  锁块数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_Lock(int ReaderID, short LockByte);


        /*    函数： YW_SR512_LockStatus
         *    名称： SR512卡锁块信息
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       LockByte：  锁块数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_LockStatus(int ReaderID, ref short LockByte);


        /*    函数： YW_SR512_ReadMulti
         *    名称： SR512卡读块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                     BlockNums:   块数量
                      LenData：   返回读取的数据长度
                         pData：  数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_ReadMulti(int ReaderID, int BlockID, int BlockNums, ref int LenData, byte[] pData);


        /*    函数： YW_SR512_WriteMulti
         *    名称： SR512卡写块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                       BlockID：  块号
                     BlockNums:   块数量
                      LenData：   要写入的数据长度
                         pData：  数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SR512_WriteMulti(int ReaderID, int BlockID, int BlockNums, int LenData, byte[] pData);


        /*    函数： YW_SRIX4K_Check
         *    名称： SR512卡写块操作
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                           Key：  输入秘钥，6字节
                     Signature:   输出签名，3字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SRIX4K_Check(int ReaderID, byte[] Key, byte[] Signature);


        /*    函数： YW_TypeB_Sleep
         *    名称： YW_TypeB卡休眠
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                          PUPI：  卡片卡号，4字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_TypeB_Sleep(int ReaderID, byte[] PUPI);


        //*******************************************ISO15693 卡片操作函数************************/


        /*    函数： YW_ISO15693_Inventory
         *    名称： 15693卡寻卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         PData：  输出数据，Flag(1byte)+DSFID(1Byte)+UID(8Byte)
                          PLen：  数据长度
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Inventory(int ReaderID, byte[] PData, ref byte PLen);


        /*    函数： YW_ISO15693_Stay_Quiet
         *    名称： 15693卡休眠
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         PUID：   15693卡卡号,8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Stay_Quiet(int ReaderID, byte[] PUID);


        /*    函数： YW_ISO15693_Select
         *    名称： 15693卡选卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                         PUID：   15693卡卡号,8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Select(int ReaderID, byte Model, byte[] PUID);


        /*    函数： YW_ISO15693_Reset_To_Ready
         *    名称： 15693卡复位
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                         PUID：   15693卡卡号,8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Reset_To_Ready(int ReaderID, byte Model, byte[] PUID);


        /*    函数： YW_ISO15693_Read
         *    名称： 15693卡读块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：   15693卡卡号,8字节
                  StartBlockID:   开始块号
                     BlockNums:   块数量
                         PData:   读出数据
                         PLen:    读出数据的长度
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Read(int ReaderID, byte Model, byte[] PUID, byte StartBlockID, byte BlockNums, byte[] PData, ref byte PLen);


        /*    函数： YW_ISO15693_Write
         *    名称： 15693卡写块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：   15693卡卡号,8字节
                       BlockID:   要写入的块号
                       DataLen:    要写入数据的长度
                         PData:   要写入的数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Write(int ReaderID, byte Model, byte[] PUID, byte BlockID, byte DataLen, byte[] PData);


        /*    函数： YW_ISO15693_Lock_Block
         *    名称： 15693卡写块
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：   15693卡卡号,8字节
                       BlockID:   要锁的数据块
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Lock_Block(int ReaderID, byte Model, byte[] PUID, byte BlockID);


        /*    函数： YW_ISO15693_Write_AFI
         *    名称： 15693卡写AFI
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：   15693卡卡号,8字节
                          AFI:   AFI值
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Write_AFI(int ReaderID, byte Model, byte[] PUID, byte AFI);


        /*    函数： YW_ISO15693_Lock_AFI
         *    名称： 15693卡锁AFI
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：   15693卡卡号,8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Lock_AFI(int ReaderID, byte Model, byte[] PUID);


        /*    函数： YW_ISO15693_Write_DSFID
         *    名称： 15693卡锁AFI
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：  15693卡卡号,8字节
                         DSFID:   写DSFID

         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Write_DSFID(int ReaderID, byte Model, byte[] PUID, byte DSFID);


        /*    函数： YW_ISO15693_Lock_DSFID
         *    名称： 15693卡锁DSFID
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：  15693卡卡号,8字节
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Lock_DSFID(int ReaderID, byte Model, byte[] PUID);


        /*    函数： YW_ISO15693_Get_System_Information
         *    名称： 获取15693卡系统信息
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：  15693卡卡号,8字节
                         PData:   系统信息数据
                       DataLen:   数据长度
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Get_System_Information(int ReaderID, byte Model, byte[] PUID, byte[] PData, ref byte PLen);


        /*    函数： YW_ISO15693_Get_Block_Security
         *    名称： 获取15693卡块安全信息
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                         Model:   00按卡标志寻卡,01按卡地址寻卡
                          PUID：  15693卡卡号,8字节
                         PData:   块安全信息数据
                       DataLen:   数据长度
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Get_Block_Security(int ReaderID, byte Model, byte[] PUID, byte StartBlockID, byte BlockNums, byte[] PData, ref byte PLen);


        /*    函数： YW_ISO15693_Multi_Inventory
         *    名称： 15693卡访冲突寻卡
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     AFIEnable:   AFI是否有效
                          AFI：   AFI值
                         PData:   块安全信息数据
                       DataLen:   数据长度
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_ISO15693_Multi_Inventory(int ReaderID, byte AFIEnable, byte AFI, byte[] PData, ref byte PLen);




        /*    函数： YW_SAM_ResetBaud
         *    名称： SAM卡复位波特率设置
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                     SAMIndex:   SAM卡序号
                     BoundIndex:   波特率序号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SAM_ResetBaud(int ReaderID, int SAMIndex, int BaudIndex);


        /*    函数： YW_SAM_Reset
         *    名称： SAM卡复位
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      SAMIndex:   SAM卡序号
                       DataLen:   返回复位数据的长度
                         pData:   复位数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SAM_Reset(int ReaderID, int SAMIndex, ref int rtLen, byte[] pData);

        /*    函数： YW_SAM_Reset
         *    名称： SAM卡复位
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      SAMIndex:   SAM卡序号
                        LenCOS:   COS命令长度
                       Com_COS:   COS命令数据
                       DataLen:   返回复位数据的长度
                         pData:   复位数据
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SAM_COS(int ReaderID, int SAMIndex, int LenCOS, byte[] Com_COS, ref int rtLen, byte[] pData);


        /*    函数： YW_SAM_PPSBaud
         *    名称： SAM卡PPS波特率设置
         *    参数：  ReaderID：  读写器ID号，0为广播地址
                      SAMIndex:   SAM卡序号
                     BoundIndex:   波特率序号
         *  返回值：>=0为成功，其它失败
        */
        [DllImport("YW60x.dll")]
        internal static extern int YW_SAM_PPSBaud(int ReaderID, int SAMIndex, int BaudIndex);

    }
}
