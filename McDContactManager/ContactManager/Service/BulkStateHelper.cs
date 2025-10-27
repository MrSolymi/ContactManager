using ContactManager.Model;

namespace ContactManager.Service;

public static class BulkStateHelper
{
    public static BulkState GetBulkState<T>(
        IEnumerable<T> items, 
        Func<T, bool?> selector)
    {
        var enumerable = items.ToList();
        if (enumerable.Count == 0)
            return BulkState.NoneSelected;

        bool anyNull = false, anyTrue = false, anyFalse = false;

        foreach (var v in enumerable.Select(selector))
        {
            if (v is null) anyNull = true;
            else if (v.Value) anyTrue = true;
            else anyFalse = true;
        }

        var kinds = (anyNull ? 1 : 0) + (anyTrue ? 1 : 0) + (anyFalse ? 1 : 0);
        if (kinds > 1) return BulkState.Mixed;
        if (anyTrue) return BulkState.AllTrue;
        return anyFalse ? BulkState.AllFalse : BulkState.AllNull;
    }
}