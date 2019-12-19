using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
    public class ShopShipperProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            CreateMap<Model.ShopShippersInfo, DTO.ShopShipper>();
            CreateMap<DTO.ShopShipper, Model.ShopShippersInfo>();

        }
    }
}
