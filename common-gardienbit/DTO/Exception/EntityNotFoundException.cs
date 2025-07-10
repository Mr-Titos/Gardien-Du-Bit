namespace common_gardienbit.DTO.Exception
{
    public class EntityNotFoundException<T> : AbstractException
    {
        public EntityNotFoundException()
        {
            this.statusCode = 404;
            this.message = $"Entity {typeof(T).Name} not found";
        }
    }
}