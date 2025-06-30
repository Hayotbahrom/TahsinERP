using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.Models
{
    public class BomIsParentProduct
    {
        public static  bool IsParentProduct(string childPno)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                if (childPno != null) 
                {
                    var bom = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == childPno).FirstOrDefault();
                    if (bom != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }

        }
    }
}