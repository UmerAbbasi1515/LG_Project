using System.Data;

namespace LG_projects.Common.ListConvertor
{
    public interface IListConvertor
    {
        List<T> ConvertDataTable<T>(DataTable dt);
        T GetItem<T>(DataRow dr);
    }
}
