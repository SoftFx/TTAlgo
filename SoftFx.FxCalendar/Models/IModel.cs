namespace SoftFx.FxCalendar.Models
{
    public interface IModel<TEntity>
    {
        TEntity ConvertToEntity();
        void InitFromEntity(TEntity entity);
    }
}