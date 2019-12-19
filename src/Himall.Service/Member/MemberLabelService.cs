using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Entity;
using System.Transactions;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Service
{
    public class MemberLabelService : ServiceBase, IMemberLabelService
    {

        public void AddLabel(LabelInfo model)
        {
            Context.LabelInfo.Add(model);
            Context.SaveChanges();
        }

        public void DeleteLabel(LabelInfo model)
        {
            var label = Context.LabelInfo.FirstOrDefault(e => e.Id == model.Id);
            Context.LabelInfo.Remove(label);
            Context.SaveChanges();
        }

        public void UpdateLabel(LabelInfo model)
        {
            var label = Context.LabelInfo.FirstOrDefault(e => e.Id == model.Id);
            label.LabelName = model.LabelName;
            Context.SaveChanges();
        }

        public QueryPageModel<LabelInfo> GetMemberLabelList(LabelQuery model)
        {
            var result = Context.LabelInfo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(model.LabelName))
            {
                result = result.Where(item => item.LabelName.Contains(model.LabelName));
            }
            if (model.LabelIds != null)
            {
                result = result.Where(item => model.LabelIds.Contains(item.Id));
            }
            int total = 0;
            if (model.PageNo > 0 && model.PageSize > 0)
            {
                result = result.GetPage(d => d.OrderByDescending(o => o.Id), out total, model.PageNo, model.PageSize);
            }
            QueryPageModel<LabelInfo> pageModel = new QueryPageModel<LabelInfo> { Models = result.ToList(), Total = total };
            return pageModel;
        }


        public LabelInfo GetLabel(long id)
        {
            return Context.LabelInfo.FirstOrDefault(item => item.Id == id);
        }

        public bool CheckNameIsExist(string name)
        {
            return Context.LabelInfo.Any(e => e.LabelName == name);
        }
    }
}
