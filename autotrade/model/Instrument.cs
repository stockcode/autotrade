using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class Instrument : Entity, INotifyPropertyChanged
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 交易所代码
        /// </summary>
        public string ExchangeID { get; set; }

        /// <summary>
        /// 合约名称
        /// </summary>
        public string InstrumentName { get; set; }

        /// <summary>
        /// 合约在交易所的代码
        /// </summary>
        public string ExchangeInstID { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        public string ProductID { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        public TThostFtdcProductClassType ProductClass { get; set; }

        /// <summary>
        /// 交割年份
        /// </summary>
        public int DeliveryYear { get; set; }

        /// <summary>
        /// 交割月
        /// </summary>
        public int DeliveryMonth { get; set; }

        /// <summary>
        /// 市价单最大下单量
        /// </summary>
        public int MaxMarketOrderVolume { get; set; }

        /// <summary>
        /// 市价单最小下单量
        /// </summary>
        public int MinMarketOrderVolume { get; set; }

        /// <summary>
        /// 限价单最大下单量
        /// </summary>
        public int MaxLimitOrderVolume { get; set; }
        /// <summary>
        /// 限价单最小下单量
        /// </summary>
        public int MinLimitOrderVolume { get; set; }

        /// <summary>
        /// 合约数量乘数
        /// </summary>
        public int VolumeMultiple { get; set; }

        /// <summary>
        /// 最小变动价位
        /// </summary>
        public double PriceTick { get; set; }

        /// <summary>
        /// 创建日
        /// </summary>
        public string CreateDate { get; set; }

        /// <summary>
        /// 上市日
        /// </summary>        
        public string OpenDate { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>        
        public string ExpireDate { get; set; }

        /// <summary>
        /// 开始交割日
        /// </summary>        
        public string StartDelivDate { get; set; }

        /// <summary>
        /// 结束交割日
        /// </summary>        
        public string EndDelivDate { get; set; }

        /// <summary>
        /// 合约生命周期状态
        /// </summary>
        public TThostFtdcInstLifePhaseType InstLifePhase { get; set; }

        /// <summary>
        /// 当前是否交易
        /// </summary>
        public int IsTrading { get; set; }

        /// <summary>
        /// 持仓类型
        /// </summary>
        public TThostFtdcPositionTypeType PositionType { get; set; }

        /// <summary>
        /// 持仓日期类型
        /// </summary>
        public TThostFtdcPositionDateTypeType PositionDateType { get; set; }

        /// <summary>
        /// 多头保证金率
        /// </summary>
        public double LongMarginRatio { get; set; }

        /// <summary>
        /// 空头保证金率
        /// </summary>
        public double ShortMarginRatio { get; set; }

        /// <summary>
        /// 是否使用大额单边保证金算法
        /// </summary>
        public TThostFtdcMaxMarginSideAlgorithmType MaxMarginSideAlgorithm { get; set; }

        /// <summary>
        /// 基础商品代码
        /// </summary>        
        public string UnderlyingInstrID { get; set; }

        /// <summary>
        /// 执行价
        /// </summary>
        public double StrikePrice { get; set; }

        /// <summary>
        /// 期权类型
        /// </summary>
        public TThostFtdcOptionsTypeType OptionsType { get; set; }

        /// <summary>
        /// 合约基础商品乘数
        /// </summary>
        public double UnderlyingMultiple { get; set; }

        /// <summary>
        /// 组合类型
        /// </summary>
        public TThostFtdcCombinationTypeType CombinationType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
