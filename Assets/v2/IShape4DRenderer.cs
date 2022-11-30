namespace v2
{
    /// <summary>
    /// Generalized interface for anything that renders a 4D shape
    /// </summary>
    public interface IShape4DRenderer
    {

        public InterpolationBasedShape Shape { get; set; }
    }

}