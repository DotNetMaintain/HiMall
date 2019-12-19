using NetRube.Data;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Db
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public partial class Record<T>
        {
            private bool? _enableLazyLoad = null;

            /// <summary>
            /// 是否开启延迟加载
            /// </summary>
            [ResultColumn]
            public bool EnableLazyLoad
            {
                get
                {
                    if (!_enableLazyLoad.HasValue)
                        _enableLazyLoad = DbFactory.Default.EnableLazyLoad;
                    return _enableLazyLoad.Value;
                }
            }

            /// <summary>
            /// 是否开启关联属性(false则关联属性为null或空集合)
            /// </summary>
            [ResultColumn]
            public bool IgnoreReference { get; set; }
        }
    }
}
